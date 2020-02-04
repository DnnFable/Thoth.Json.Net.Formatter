module Tests.Formatter

open Newtonsoft.Json
open Expecto
open Util.Testing
open Thoth.Json.Net
open System.IO

let toStream (s:string) =
   let stream = new MemoryStream()
   let writer = new StreamWriter(stream)
   writer.Write s
   writer.Flush ()
   stream.Position <- int64 0 
   stream

let fromStream (stream:Stream) : string =
   stream.Position <- int64 0 
   use reader = new StreamReader(stream)
   reader.ReadToEnd()

let encode<'T> (value: 'T) : string = 
    let formatter =Formatter()
    let t = typeof<'T>
    let stream = new MemoryStream()
    async 
        {
          do! formatter.WriteToStreamAsync (t, value, stream, null, null) |> Async.AwaitTask
        } 
    |>Async.RunSynchronously
    fromStream stream

let inline decode<'T> (json: string) :'T=
    let formatter =Formatter()
    let t = typeof<'T>
    
    async
     {
       use stream = toStream json
       return! formatter.ReadFromStreamAsync (t, stream, null, null)|> Async.AwaitTask 
     } 
    |> Async.RunSynchronously
    |> unbox


type MyUnion = Foo of int | Bar

let tests : Test =
    testList "Thoth.Json.Converter" [

        testList "Decoding" [
            testCase "works for int32" <| fun _ ->               
                let json = "2"
                decode json
                |> equal 2
            testCase "works for bool" <| fun _ ->               
                let json = "true"
                decode json
                |> equal true
            testCase "works for string" <| fun _ ->               
                let json = "\"fable\""
                decode json
                |> equal "fable"
            testCase "works for option None" <| fun _ ->               
                let json = "null"
                decode<string option > json
                |> equal None
            testCase "works for option" <| fun _ ->               
                let json = "\"fable\""
                decode json
                |> equal (Some "fable")
            testCase "works for enum" <| fun _ ->               
                let json = "2"
                decode json
                |> equal (System.DayOfWeek.Tuesday)
            testCase "works for simple union" <| fun _ ->               
                let json = "\"Bar\""
                decode json
                |> equal (Bar)
            testCase "works for union" <| fun _ ->               
                let json = "[\"Foo\",42]"
                decode json
                |> equal (Foo 42)
        ]
        testList "Encoding" [
            testCase "works for int32" <| fun _ ->               
                let json = "2"
                encode 2
                |> equal json
            testCase "works for bool" <| fun _ ->               
                let json = "true"
                encode true
                |> equal json
            testCase "works for string" <| fun _ ->               
                let json = "\"fable\""
                encode "fable"
                |> equal json
            testCase "works for option None" <| fun _ ->               
                let json = "null"
                encode<string option> None
                |> equal json
            testCase "works for option" <| fun _ ->               
                let json = "\"fable\""
                encode "fable"
                |> equal json
            testCase "works for enum" <| fun _ ->               
                let json = "2"
                decode json
                |> equal (System.DayOfWeek.Tuesday)
            testCase "works for simple union" <| fun _ ->               
                let json = "\"Bar\""
                encode Bar
                |> equal json
            testCase "works for union" <| fun _ ->               
                let json = "[\"Foo\",42]"
                encode (Foo 42)
                |> equal json
        ]
    ]
