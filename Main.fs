open System

open Air.Blender.Data
open Air.Blender.Mixer

let rnd = new Random()

let minDuration = 5 * 1000
// let maxDuration = 10 * 60 * 1000
let maxDuration = 20 * 1000

// 7P9
// let mixer = Mixer("10.0.1.123", 3000)
// let libraryPath = @"/var/air/data/library"

// RTV
let mixer = Mixer("172.18.30.39", 3000)
let libraryPath = @"/home/gregor/temp/airfront"

let library = loadLibrary "var/library"
let audioBook = "AIRC_2"

// Mixer.Loop("1", "test.wav", "0:10.000", "0:11")

let trackAgent (track: string) (audioHeaders: AudioHeader list) =
    let rnd = new Random()

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let audio = audioHeaders[rnd.Next(audioHeaders.Length - 1)]
                let start = rnd.Next(audio.Duration.Value)

                let duration =
                    rnd.Next(
                        min (audio.Duration.Value - start) minDuration,
                        min (audio.Duration.Value - start) maxDuration
                    )

                mixer.Fade(track, 0f)

                mixer.Play(
                    track,
                    audio.Filename,
                    Duration.toStringHMS (Duration start),
                    Duration.toStringHMS (Duration(start + duration))
                )

                mixer.Fade(track, 1.0f, 2000, "sin")
                do! Async.Sleep(2000)

                do! Async.Sleep(duration)

                mixer.Fade(track, 0f, 2000, "sin")
                do! Async.Sleep(2000)

                return! loop ()
            }

        loop ())

mixer.Init()
mixer.Cd(IO.Path.Join(libraryPath, audioBook))

for i in [ 1..4 ] do
    trackAgent (string i) library[audioBook] |> ignore

Async.RunSynchronously(async { do Console.ReadKey() |> ignore })
mixer.Stop("all")
