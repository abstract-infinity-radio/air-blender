module Air.Blender.Mixer

open FSharp.Osc

type Mixer(clientIp: string, port: int) =
    let Client = new OscUdpClient(clientIp, port)

    static member PathPrefix = "/airmix/"

    member this.Run message = Client.SendMessage message |> ignore

    member this.Cd(path: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}cd"
              arguments = [ OscString path ] }

    member this.Play(track: string, file: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/play"
              arguments = [ OscString file ] }

    member this.Play(track: string, file: string, start: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/play"
              arguments = [ OscString file; OscString start ] }

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

    member this.Fade(track: string, level: float32, duration: int) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/fader"
              arguments = [ OscFloat32 level; OscInt32 duration ] }

    member this.Fade(track: string, level: float32, duration: int, curve: string) =
        this.Run
            { addressPattern = $"{Mixer.PathPrefix}{track}/fader"
              arguments = [ OscFloat32 level; OscInt32 duration; OscString curve ] }
