#r @"packages/build/FAKE/tools/FakeLib.dll"
open System.Threading.Tasks

open System

open Fake

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  tool
  |> ProcessHelper.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"

let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
  let result =
    ExecProcess (fun info ->
      info.FileName <- cmd
      info.WorkingDirectory <- workingDir
      info.Arguments <- args) TimeSpan.MaxValue
  if result <> 0 then failwithf "'%s %s' failed" cmd args

Target "Clean" DoNothing


Target "InstallClient" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" __SOURCE_DIRECTORY__
  printfn "Yarn version:"
  run "npm" "--version" __SOURCE_DIRECTORY__
  run "npm" "install" __SOURCE_DIRECTORY__
  run dotnetCli "restore" clientPath
)

Target "RestoreServer" (fun () -> 
  run dotnetCli "restore" serverPath
)

Target "Build" (fun () ->
  run dotnetCli "build" serverPath
  run dotnetCli "fable webpack -- -p" clientPath
)

Target "BuildParallel" <| fun _ ->
  [ async { run dotnetCli "build" serverPath }
    async { run dotnetCli "fable webpack -- -p" clientPath } ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

Target "RunServer" <| fun _ ->
  run dotnetCli "run" serverPath


Target "RunDevMode" (fun () ->
  let server = async {
    run dotnetCli "watch run" serverPath
  }
  let client = async {
    run dotnetCli "fable webpack-dev-server" clientPath
  }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://localhost:8080" |> ignore
  }

  [ server; client; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Target "RunProdMode" DoNothing


"Clean"
  ==> "InstallClient"
  ==> "Build"

"Build"
  ==> "RunServer"
  ==> "RunProdMode"

"InstallClient"
  ==> "RestoreServer"
  ==> "RunDevMode"

RunTargetOrDefault "Build"