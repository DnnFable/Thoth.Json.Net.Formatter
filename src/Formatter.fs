namespace Thoth.Json.Net
open System.Net.Http.Formatting
open System.Text
open System.IO
open Newtonsoft.Json
open Thoth.Json.Net
open System.Net.Http.Headers
open System.Threading.Tasks

module Formatter =
    module Helper =
        let writeToStream (intendation:int) (stream:Stream) (token:JsonValue)  =
            let format = if intendation = 0 then Formatting.None else Formatting.Indented
            let writer = new StreamWriter (stream) 
            let jsonWriter = new JsonTextWriter(
                                    writer,
                                    Formatting = format,
                                    Indentation = intendation )
            token.WriteTo(jsonWriter)
            jsonWriter.Flush()

type Formatter (?isCamelCase : bool, ?extra : ExtraCoders, ?intendation : int) as __ =
    inherit MediaTypeFormatter()

    do __.SupportedMediaTypes.Add (MediaTypeHeaderValue "application/json")
       __.SupportedEncodings.Add (UTF8Encoding( false, true))
       __.SupportedEncodings.Add (UnicodeEncoding(false, true, true))
    
    override __.CanReadType t =
        if isNull t then failwith "Not null"
        true 

    override __.CanWriteType t =
        if isNull t then failwith "Not null"
        true 
       
    override __.ReadFromStreamAsync (t, readStream, _, _) = 
        async {
            let decoder = Decode.Auto.LowLevel.generateDecoderCached(t, ?isCamelCase=isCamelCase, ?extra=extra)
            use jsonReader = new StreamReader(readStream)
            let! json = jsonReader.ReadToEndAsync() |> Async.AwaitTask
            let obj = Decode.unsafeFromString decoder json 
            return obj } 
        |> Async.StartAsTask

    override __.WriteToStreamAsync (t, value, writeStream, _, _) =
        let encoder = Encode.Auto.LowLevel.generateEncoderCached(t, ?isCamelCase=isCamelCase, ?extra=extra)
        encoder value |> Formatter.Helper.writeToStream (defaultArg  intendation 0) writeStream 
        Task.FromResult writeStream :> Task
     
    new() = Formatter(false)
    new(intendation) = Formatter(?isCamelCase =Some false, ?intendation = Some intendation)