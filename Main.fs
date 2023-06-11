open System

open Air.Blender.Data
open Air.Blender.Mixer

let rnd = new Random()

let fadeDuration = 5_000
let minDuration = 1_000
let maxDuration = 30_000

let minLoopDuration = 100
let maxLoopDuration = 2_000
let minLoopTotalDuration = 5_000
let maxLoopTotalDuration = 10_000
let loopFadeDuration = 20_000

// 7P9
let mixer = Mixer("10.0.1.123", 3000)
let libraryPath = @"/var/air/data/library"

// RTV
// let mixer = Mixer("172.18.30.39", 3000)
// let libraryPath = @"/home/gregor/temp/airfront"

let library = loadLibrary "var/library"
let audioBook = "AIRC_2"

let playLinear track (audioHeaders: AudioHeader list) =
    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let start = rnd.Next(audio.Duration.Value - (minDuration + 2 * fadeDuration))
        let duration = rnd.Next(minDuration, min (audio.Duration.Value - start) maxDuration)

        mixer.Fade(track, 0f)

        mixer.Play(
            track,
            audio.Filename,
            Time.toStringHMS (Time start)
        // Time.toStringHMS (Time(start + duration + 2 * fadeDuration))
        )

        mixer.Fade(track, 1.0f, fadeDuration, "sin")
        do! Async.Sleep(fadeDuration)

        do! Async.Sleep(duration)

        mixer.Fade(track, 0f, fadeDuration, "sin")
        do! Async.Sleep(fadeDuration)
    }

let playLoop track (audioHeaders: AudioHeader list) =
    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let start = rnd.Next(audio.Duration.Value - minLoopDuration)

        let duration =
            rnd.Next(minLoopDuration, min (audio.Duration.Value - start) maxLoopDuration)

        mixer.Fade(track, 0f)

        mixer.Loop(track, audio.Filename, Time.toStringHMS (Time start), Time.toStringHMS (Time(start + duration)))

        mixer.Fade(track, 1f, fadeDuration, "sin")
        do! Async.Sleep(rnd.Next(loopFadeDuration))

        do! Async.Sleep(rnd.Next(minLoopTotalDuration, maxLoopTotalDuration))

        mixer.Fade(track, 0f, fadeDuration, "sin")
        do! Async.Sleep(rnd.Next(loopFadeDuration))
    }

let playSlidingLoop track (audioHeaders: AudioHeader list) =
    async {
        let audio = audioHeaders[rnd.Next(audioHeaders.Length)]

        let loopLength = rnd.Next(500, 2_000)
        let loopStep = rnd.Next(100, 500)
        let loopWindow = rnd.Next(10_000, 20_000)

        let start = rnd.Next(audio.Duration.Value)

        mixer.Fade(track, 1f)

        for i in [ 0 .. (loopWindow / loopStep) ] do
            mixer.Play(track, audio.Filename, Time.toStringHMS (Time(start + i * loopStep)))

            do! Async.Sleep(loopLength)
    }

let trackAgent (track: string) (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                do!
                    match rnd.Next(10) with
                    | i when i <= 7 -> playLinear track audioHeaders
                    | i when i <= 8 -> playSlidingLoop track audioHeaders
                    | _ -> playLoop track audioHeaders

                return! loop ()
            }

        loop ())

mixer.Init()
mixer.Cd(IO.Path.Join(libraryPath, audioBook))

for i in [ 1..8 ] do
    trackAgent (string i) library[audioBook] |> ignore

Async.RunSynchronously(async { do Console.ReadKey() |> ignore })
mixer.Stop("all")
