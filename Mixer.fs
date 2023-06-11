module Air.Blender.Mixer

open FSharp.Osc

type Mixer(clientIp: string, port: int) =
    let client = new OscUdpClient(clientIp, port)

    let mailbox =
        MailboxProcessor.Start(fun inbox ->
            let rec loop () =
                async {
                    let! command = inbox.Receive()
                    printfn $"Sending OSC command: {command}"
                    client.SendMessage command |> ignore
                    return! loop ()
                }

            loop ())

    static member PathPrefix = "/airmix/"

    member this.Run message = mailbox.Post message

    member this.Cd(path: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}cd"
              arguments = [ OscString path ] }

    member this.Stereo(path: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}stereo"
              arguments = [ OscString path ] }

    member this.Mono(path: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}mono"
              arguments = [ OscString path ] }

    member this.Globepan() =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}globepan"
              arguments = [] }

    member this.Play(track: string, file: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/play"
              arguments = [ OscString file ] }

    member this.Play(track: string, file: string, start: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/play"
              arguments = [ OscString file; OscString start ] }

    member this.Play(track: string, file: string, start: string, stop: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/play"
              arguments = [ OscString file; OscString start; OscString stop ] }

    member this.Loop(track: string, file: string, start, stop) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/loop"
              arguments = [ OscString file; OscString start; OscString stop ] }

    member this.Stop(track: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/stop"
              arguments = [] }

    member this.Mute(track: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/mute"
              arguments = [] }

    member this.Unmute(track: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/unmute"
              arguments = [] }

    member this.Fade(track: string, level: float32) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/fader"
              arguments = [ OscFloat32 level ] }

    member this.Fade(track: string, level: float32, duration: int) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/fader"
              arguments = [ OscFloat32 level; OscInt32 duration ] }

    member this.Fade(track: string, level: float32, duration: int, curve: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/fader"
              arguments = [ OscFloat32 level; OscInt32 duration; OscString curve ] }

    member this.Init() =
        // this.Stop("all")
        this.Mono("all")
        this.Fade("all", 1.0f)
        this.Globepan()
        this.Unmute("all")
