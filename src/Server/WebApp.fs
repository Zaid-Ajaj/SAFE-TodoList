module WebApp

open Shared
open System.IO
open LiteDB
open LiteDB.FSharp
open LiteDB.FSharp.Extensions

let toAsync x = async { return x }

let createUsing (db: LiteDatabase) : ITodoProtocol = 

    let todos = db.GetCollection<Todo>("todos")

    let app : ITodoProtocol = {

        allTodos = fun () ->
            todos.FindAll()
            |> List.ofSeq
            |> toAsync

        addTodo = fun (Description(text)) ->
            let nextTodo = { defaultTodo() with Description = text }
            todos.Insert(nextTodo)
            |> todos.TryFindById
            |> toAsync

        toggleCompleted = fun id ->
            let todoId = BsonValue(id)
            match todos.TryFindById(todoId) with
            | None -> UpdateError TodoDoesNotExist
            | Some existingTodo ->
                let updatedTodo = { existingTodo with Completed = not existingTodo.Completed }
                match todos.Update(todoId, updatedTodo) with
                | false -> UpdateError UpdateNotSuccesful
                | true -> Updated 
            |> toAsync

        deleteTodo = fun id -> 
            let todoId = BsonValue(id)
            match todos.TryFindById(todoId) with
            | None -> DeleteError TodoDoesNotExist
            | Some _ ->
                match todos.Delete(todoId) with
                | true -> Deleted
                | false -> DeleteError DeleteNotSuccesful
            |> toAsync
    }

    app 

let createUsingInMemoryStorage() : ITodoProtocol = 
    // In memory collection
    let memoryStream = new MemoryStream()
    let bsonMapper = FSharpBsonMapper()
    let inMemoryDatabase = new LiteDatabase(memoryStream, bsonMapper)
    createUsing inMemoryDatabase

let seedIntitialData (todos: ITodoProtocol) = 
    [ "Learn F#"
      "Learn Fable"
      "Build Awesome Apps!" ]
    |> List.map (Description >> todos.addTodo)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore