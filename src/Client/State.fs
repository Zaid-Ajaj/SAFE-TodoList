module Todo.State

open Todo.Types
open Elmish 

let initialState() = 
    let initState = {
        TodoItems = []
        NewTodoDescription = None
        Visibility = All
    }

    initState, Cmd.ofMsg LoadTodoItems


let update (msg: Msg) (prevState: State) = 
    match msg with
    | SetNewTextDescription text ->
        let nextState = { prevState with NewTodoDescription = Some text }
        nextState, Cmd.none

    | LoadTodoItems -> 
        prevState, Server.loadAllTodos()

    | AddTodo ->
        match prevState.NewTodoDescription with
        | None -> prevState, Cmd.none
        | Some text -> prevState, Server.addTodo text

    | TodoAdded todoItem -> 
        let nextTodoItems = List.append prevState.TodoItems [todoItem] 
        let nextState = { prevState with TodoItems = nextTodoItems; NewTodoDescription = None }
        nextState, Cmd.none
            
    | TodoItemsLoaded items -> 
        let nextState = { prevState with TodoItems = items }
        nextState, Cmd.none

    | ToggleCompleted id -> 
        prevState, Server.toggleCompleted id

    | DeleteTodo id ->
        prevState, Server.deleteTodo id

    | _ -> prevState, Cmd.none