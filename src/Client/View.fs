module Todo.View

open Shared
open Todo.Types
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props


let divider =  span [ Style [ MarginLeft 5; MarginRight 5 ] ] [ ]

let renderTodo (item: Todo) dispatch = 
    let toggleText = if item.Completed then "Actually, Not Yet!" else "Complete"
    let dispatchToggle = OnClick (fun _ -> dispatch (ToggleCompleted item.Id))
    let dispatchDelete = OnClick (fun _ -> dispatch (DeleteTodo item.Id))
    
    let todoStyle = 
      match item.Completed with
      | true ->  Style [ Color "red"; FontSize 19; Padding 5; TextDecoration "line-through"]
      | false ->  Style [ Color "green"; FontSize 19; Padding 5 ]

    div 
      [ todoStyle ] 
      [ str item.Description
        br [ ] 
        button [ dispatchToggle ] [ str toggleText ]
        divider
        button [ dispatchDelete ] [ str "Delete" ] ]


let addTodo (state: State) dispatch = 
  let textValue = defaultArg state.NewTodoDescription ""
  div 
    [ Style [Padding 5] ] 
    [ str "Add Todo"
      divider
      input [ DefaultValue textValue
              OnChange (fun ev -> dispatch (SetNewTextDescription (!!ev.target?value)))] 
      divider
      button [ OnClick (fun _ -> dispatch AddTodo) ] [ str "Add Todo" ] ] 
 
let render  (state: State) dispatch = 
    let sortedTodos = 
      state.TodoItems 
      |> List.sortBy (fun todo -> todo.DateAdded) 
      |> List.map (fun todo -> renderTodo todo dispatch)

    div 
     [ ]
     [ yield addTodo state dispatch
       yield br [] 
       yield! sortedTodos ]