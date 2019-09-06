module Tests.Main

open Expecto
open Util.Testing

[<EntryPoint>]
let main args =
    testList "All" [ Formatter.tests ]
    |> runTestsWithArgs defaultConfig args
