open System.IO
open System.Net
open Suave
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Suave


let clientPath = Path.Combine("..", "Client", "dist") |> Path.GetFullPath 
let port = 8085us

let config =
  { defaultConfig with 
      homeFolder = Some clientPath
      bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

let webApi = 
  let todoProtocol = WebApp.createUsingInMemoryStorage()
  WebApp.seedIntitialData(todoProtocol)
  Remoting.createApi()
  |> Remoting.fromValue todoProtocol
  |> Remoting.withRouteBuilder Route.builder
  |> Remoting.buildWebPart  

let webApp = choose  [ webApi; Files.browseHome ]
    
startWebServer config webApp