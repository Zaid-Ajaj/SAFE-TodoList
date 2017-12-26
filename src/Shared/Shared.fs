module Shared

open System

type Counter = int

module Route =
  /// Defines how routes are generated on server and mapped from client
  let builder typeName methodName = 
    sprintf "/api/%s/%s" typeName methodName


type TodoId = int

[<CLIMutable>]
type Todo = {
  Id: TodoId // auto-incremented
  Description: string
  Completed: bool
  DateAdded: DateTime
}

let defaultTodo() = 
  { Id = 0 // Id is auto-incremented because of the CLIMutable attribute
    Description = ""
    Completed = false
    DateAdded = DateTime.Now }

type TodoError = 
  | TodoDoesNotExist
  | InsertNotSuccessful
  | UpdateNotSuccesful
  | DeleteNotSuccesful

type DeleteResult = 
  | Deleted 
  | DeleteError of TodoError

type UpdateResult = 
  | Updated 
  | UpdateError of TodoError

type Description = Description of string

/// A type that specifies the communication protocol for client and server
/// Every record field must have the type : 'a -> Async<'b> where 'a can also be `unit`
/// Add more such fields, implement them on the server and they be directly available on client
type ITodoProtocol =
  { allTodos : unit -> Async<List<Todo>>
    addTodo : Description -> Async<Option<Todo>>
    toggleCompleted : TodoId -> Async<UpdateResult>
    deleteTodo : TodoId -> Async<DeleteResult>  }
