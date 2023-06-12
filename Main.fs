open System
open Argu

open Air.Blender.Data
open Air.Blender.Mixer
open Air.Blender.Blender

let defaultCliArguments =
    {| Mixer_Host = "127.0.0.1"
       Mixer_Port = 3000
       Library_Path = "/var/air/data/library"
       Audio_Headers_Path = "/var/air/data/library" |}

type CliArguments =
    | [<Unique; EqualsAssignmentOrSpaced>] Mixer_Host of mixerHost: string
    | [<Unique; EqualsAssignmentOrSpaced>] Mixer_Port of mixerPort: int
    | [<Unique; EqualsAssignmentOrSpaced>] Library_Path of libraryPath: string
    | [<Unique; EqualsAssignmentOrSpaced>] Audio_Headers_Path of audioHeadersPath: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Mixer_Host _ -> $"specify a mixer host or an IP address. Default is '{defaultCliArguments.Mixer_Host}'."
            | Mixer_Port _ -> $"specify a mixer port. Default is '{defaultCliArguments.Mixer_Port}'."
            | Library_Path _ -> $"specify a library path. Default is '{defaultCliArguments.Library_Path}'."
            | Audio_Headers_Path _ ->
                $"specify a path to audio headers (when running on a computer without the full library). Default is '{defaultCliArguments.Audio_Headers_Path}'."

let config =
    { Liner =
        { MinDuration = 1_000
          MaxDuration = 30_000
          FadeDuration = 5_000 }
      Loop =
        { MinDuration = 100
          MaxDuration = 2_000
          MinTotalDuration = 5_000
          MaxTotalDuration = 10_000
          FadeDuration = 20_000 }
      SlidingLoop =
        { MinLoopDuration = 500
          MaxLoopDuration = 2_000
          MinLoopStep = 100
          MaxLoopStep = 500
          MinTotalDuration = 10_000
          MaxTotalDuration = 20_000 } }

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<CliArguments>(programName = "air-blender")
        let options = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

        let library =
            loadLibrary
                (options.GetResult(Audio_Headers_Path, defaultValue = defaultCliArguments.Audio_Headers_Path))
                (options.GetResult(Library_Path, defaultValue = defaultCliArguments.Library_Path))

        let mixer =
            Mixer(
                options.GetResult(Mixer_Host, defaultValue = defaultCliArguments.Mixer_Host),
                options.GetResult(Mixer_Port, defaultValue = defaultCliArguments.Mixer_Port)
            )

        mixer.Init()

        let audioBook = "AIRC_1"

        for i in [ 1..8 ] do
            trackAgent mixer config (string i) library[audioBook] |> ignore

        Async.RunSynchronously(async { do Console.ReadKey() |> ignore })

        mixer.Stop("all")

    with e ->
        printfn "%s" e.Message

    0
