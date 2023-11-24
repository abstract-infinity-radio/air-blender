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
    | [<Unique>] Audio_Books of audioBook: string list

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Mixer_Host _ -> $"specify a mixer host or an IP address. Default is '{defaultCliArguments.Mixer_Host}'."
            | Mixer_Port _ -> $"specify a mixer port. Default is '{defaultCliArguments.Mixer_Port}'."
            | Library_Path _ -> $"specify a library path. Default is '{defaultCliArguments.Library_Path}'."
            | Audio_Headers_Path _ ->
                $"specify a path to audio headers (when running on a computer without the full library). Default is '{defaultCliArguments.Audio_Headers_Path}'."
            | Audio_Books _ -> $"specify a list library audio books to play from. Default is all audio books."

let config =
    { AudioTracks = 8
      Liner =
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

        let audioHeaders =
            match options.GetResult(Audio_Books, defaultValue=[]) with
            | [] -> library.Values |> Seq.collect id |> Seq.toList
            | audioBooks -> audioBooks |> List.collect (fun audioBook -> library[audioBook])

        // Stop playing when Ctrl+C is pressed
        Console.CancelKeyPress.AddHandler(fun _ _ ->
            mixer.Stop("all")
            Threading.Thread.Sleep 100)

        printfn "Press 'r' to restart or 'q' to quit."

        let createTrackAgents () =
            [ 1..config.AudioTracks ]
            |> List.map (fun i ->
                let cts = new Threading.CancellationTokenSource()
                (trackAgent cts mixer config (string i) audioHeaders), cts)

        let rec loop trackAgents =
            match Console.ReadKey(true).KeyChar with
            | 'q'
            | 'Q' -> mixer.Stop("all")
            | 'r'
            | 'R' ->
                printfn "Restarting..."

                // Cancel all track agents, initialize the mixer and create a new set of track agents
                trackAgents
                |> List.iter (fun (agent, (cts: Threading.CancellationTokenSource)) -> cts.Cancel())

                mixer.Init()
                loop (createTrackAgents ())

            | _ -> loop (trackAgents)

        loop (createTrackAgents ())

    with e ->
        printfn "%s" e.Message

    0
