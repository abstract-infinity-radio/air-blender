module Air.Blender.Blender

open System

open Air.Blender.Data
open Air.Blender.Mixer

type LinerConfig =
    { MinDuration: int
      MaxDuration: int
      FadeDuration: int }

type LoopConfig =
    { MinDuration: int
      MaxDuration: int
      MinTotalDuration: int
      MaxTotalDuration: int
      FadeDuration: int }

type SlidingLoopConfig =
    { MinLoopDuration: int
      MaxLoopDuration: int
      MinLoopStep: int
      MaxLoopStep: int
      MinTotalDuration: int
      MaxTotalDuration: int }

type Config =
    { Liner: LinerConfig
      Loop: LoopConfig
      SlidingLoop: SlidingLoopConfig }

let playLinear (mixer: Mixer) (config: LinerConfig) track (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let start =
            rnd.Next(audio.Duration.Value - (config.MinDuration + 2 * config.FadeDuration))

        let duration =
            rnd.Next(config.MinDuration, min (audio.Duration.Value - start) config.MaxDuration)

        mixer.Fade(track, 0f)

        mixer.Play(track, audio.Filename, Time.toStringHMS (Time start))

        mixer.Fade(track, 1.0f, config.FadeDuration, "sin")
        do! Async.Sleep(config.FadeDuration)

        do! Async.Sleep(duration)

        mixer.Fade(track, 0f, config.FadeDuration, "sin")
        do! Async.Sleep(config.FadeDuration)
    }

let playLoop (mixer: Mixer) (config: LoopConfig) track (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let start = rnd.Next(audio.Duration.Value - config.MinDuration)

        let duration =
            rnd.Next(config.MinDuration, min (audio.Duration.Value - start) config.MaxDuration)

        mixer.Fade(track, 0f)

        mixer.Loop(track, audio.Filename, Time.toStringHMS (Time start), Time.toStringHMS (Time(start + duration)))

        mixer.Fade(track, 1f, config.FadeDuration, "sin")
        do! Async.Sleep(rnd.Next(config.FadeDuration))

        do! Async.Sleep(rnd.Next(config.MinTotalDuration, config.MaxTotalDuration))

        mixer.Fade(track, 0f, config.FadeDuration, "sin")
        do! Async.Sleep(rnd.Next(config.FadeDuration))
    }

let playSlidingLoop (mixer: Mixer) (config: SlidingLoopConfig) track (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let start = rnd.Next(audio.Duration.Value)

        let loopDuration = rnd.Next(config.MinLoopDuration, config.MaxLoopDuration)
        let loopStep = rnd.Next(config.MinLoopStep, config.MaxLoopStep)
        let totalDuration = rnd.Next(config.MinTotalDuration, config.MaxTotalDuration)

        mixer.Fade(track, 1f)

        for i in [ 0 .. (totalDuration / loopStep) ] do
            mixer.Play(track, audio.Filename, Time.toStringHMS (Time(start + i * loopStep)))

            do! Async.Sleep(loopDuration)
    }

let trackAgent (mixer: Mixer) (config: Config) (track: string) (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                do!
                    match rnd.Next(10) with
                    | i when i <= 7 -> playLinear mixer config.Liner track audioHeaders
                    | i when i <= 8 -> playSlidingLoop mixer config.SlidingLoop track audioHeaders
                    | _ -> playLoop mixer config.Loop track audioHeaders

                return! loop ()
            }

        loop ())
