module Todo.Server 

open Shared
open Elmish
open Todo.Types
open Fable.Remoting.Client

// =================================
// Create typed proxy for server api
// =================================

let api : ITodoProtocol = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodoProtocol>

// ===========================================
// Create Elmish commands that map server calls to client messages,
// these messages will be propagated into the Elmish dispatch loop,
// This makes for a clean api when dealing with the State of the app 
// see the `update` function
// ==========================================

let loadAllTodos() =
    Cmd.ofAsync 
      api.allTodos ()
      TodoItemsLoaded
      LoadTodoItemsFailure

let deleteTodo id = 
    // the server call being successful, 
    // does not mean that the operation has succeeded.
    // Map server result to client msg 
    let successCallback = function
        | Deleted -> LoadTodoItems
        | DeleteError error -> DeleteTodoFailure error

    // Error callback for other errors that could happen:
    // - Network error
    // - Server exceptions
    let errorCallback (_: exn) = 
        DeleteTodoFailure DeleteNotSuccesful
    
    Cmd.ofAsync
      api.deleteTodo id
      // delete CALL => reload all items
      successCallback
      errorCallback

let addTodo text = 
    Cmd.ofAsync
      api.addTodo (Description(text))
      (function 
        | Some addedTodo -> TodoAdded addedTodo
        | None -> AddTodoFailed)
      (fun ex -> AddTodoFailed)


let toggleCompleted id = 
    Cmd.ofAsync
      api.toggleCompleted id
      // success callback
      (function 
        | Updated -> LoadTodoItems
        | UpdateError error -> ToggleCompletedFailure error)
      // error callback
      (fun _ -> ToggleCompletedFailure UpdateNotSuccesful) 