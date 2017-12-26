open System.IO
open System.Net
open Suave
open Shared
open Fable.Remoting.Suave


let clientPath = Path.Combine("..", "Client", "dist") |> Path.GetFullPath 
let port = 8085us

let config =
  { defaultConfig with 
      homeFolder = Some clientPath
      bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

let webApp = 
  let todoProtocol = WebApp.createUsingInMemoryStorage()
  WebApp.seedIntitialData(todoProtocol)
  FableSuaveAdapter.webPartWithBuilderFor todoProtocol Route.builder

let webPart =
  choose  
   [ webApp
     Files.browseHome ]

startWebServer config webPart