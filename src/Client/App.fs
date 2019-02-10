module Todo.App

open Elmish
open Elmish.React
open Elmish.Debug
open Elmish.HMR

// ====================================================
// Construct the Elmish application using the three parts 
// - State.initialState
// - State.update
// - View.render
// ====================================================

Program.mkProgram 
  State.initialState 
  State.update 
  View.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
