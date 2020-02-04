module Thoth.Json.Net
open System.Net.Http.Formatting
open System.IO
open Newtonsoft.Json
open Thoth.Json.Net
open System.Threading.Tasks
open System

let private writeToStream (intendation:int) (stream:Stream) (token:JsonValue)  =
    let format = if intendation = 0 then Formatting.None else Formatting.Indented
    let writer = new StreamWriter (stream) 
    let jsonWriter = new JsonTextWriter(
                            writer,
                            Formatting = format,
                            Indentation = intendation )
    token.WriteTo(jsonWriter)
    jsonWriter.Flush()

type Formatter (?isCamelCase : bool, ?extra : ExtraCoders, ?intendation : int) as __ =
    inherit JsonMediaTypeFormatter()

    do base.UseDataContractJsonSerializer <- false
    
    let failIfNull (o) t =  if isNull o then raise (ArgumentNullException ("Should not be Null: " + t))

    override __.ReadFromStreamAsync (t, readStream, _, _) = 
        async {
            failIfNull t "Type"
            failIfNull readStream "ReadStream" 
            let decoder = Decode.Auto.LowLevel.generateDecoderCached(t, ?isCamelCase=isCamelCase, ?extra=extra)
            use jsonReader = new StreamReader(readStream)
            let! json = jsonReader.ReadToEndAsync() |> Async.AwaitTask
            let obj = Decode.unsafeFromString decoder json 
            return obj } 
        |> Async.StartAsTask

    override __.WriteToStreamAsync (t, value, writeStream, _, _) =
        failIfNull t "Type"
        failIfNull writeStream "WriteStream" 
        let encoder = Encode.Auto.LowLevel.generateEncoderCached(t, ?isCamelCase=isCamelCase, ?extra=extra)
        encoder value |> writeToStream (defaultArg  intendation 0) writeStream 
        Task.FromResult writeStream :> Task
     
    new() = Formatter(false)
    new(intendation) = Formatter(?isCamelCase =Some false, ?intendation = Some intendation)