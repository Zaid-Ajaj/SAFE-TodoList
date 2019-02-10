#r @"packages/build/FAKE/tools/FakeLib.dll"
open System.Threading.Tasks

open System

open Fake

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName
let cwd = __SOURCE_DIRECTORY__
let npm = "npm"

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  tool
  |> ProcessHelper.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = "node"

let mutable dotnetCli = "dotnet"

let run fileName args workingDir =
    printfn "CWD: %s" workingDir
    let fileName, args =
        if isUnix
        then fileName, args else "cmd", ("/C " + fileName + " " + args)
    let ok =
        execProcess (fun info ->
             info.FileName <- fileName
             info.WorkingDirectory <- workingDir
             info.Arguments <- args) TimeSpan.MaxValue
    if not ok then failwith (sprintf "'%s> %s %s' task failed" workingDir fileName args)

Target "Clean" DoNothing


Target "InstallClient" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" cwd
  printfn "Yarn version:"
  run "yarn" "--version" cwd
  run "yarn" "install" cwd
)

Target "RestoreServer" (fun () -> 
  run dotnetCli "restore" serverPath
)

Target "Build" (fun () ->
  // build server
  run dotnetCli "build" serverPath
  // build client
  run npm "run build" cwd
)

Target "BuildParallel" <| fun _ ->
  [ async { run dotnetCli "build" serverPath }
    async { run npm "run build" cwd } ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

Target "RunServer" <| fun _ ->
  run dotnetCli "run" serverPath


Target "RunDevMode" (fun () ->
  let server = async { run dotnetCli "watch run" serverPath }
  let client = async { run npm "start" cwd }
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