# Thoth.Json.Net.Formatter

`Thoth.Json.Net.Formatter` is a JsonMediaTypeFormatter which can be used with Asp.Net WebApi to handle requests from `Thoth.Fetch` and uses `Thoth.Json.Net` for encoding and decoding.

## Example for using the formatter within your service
```fsharp
type FableFormatterAttribute() =
    inherit System.Attribute()
    interface IControllerConfiguration with
        member __.Initialize((controllerSettings : HttpControllerSettings), _) =
            controllerSettings.Formatters.Clear()
            controllerSettings.Formatters.Add <| Thoth.Json.Net.Formatter()

[<FableFormatter>]
type MyServiceController () =
    inherit ApiController ()
```