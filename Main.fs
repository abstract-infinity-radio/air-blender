open System

open Air.Blender.Data
open Air.Blender.Mixer

let rnd = new Random()
let libraryPath = @"/var/air/data/library"

let mixer = Mixer("10.0.1.123", 3000)

mixer.Stop("all")
mixer.Mono("all")
mixer.Fade("all", 1.0f)
mixer.Unmute("all")

// Mixer.Loop("1", "test.wav", "0:10.000", "0:11")

// module Timers =

//     let createTimer (interval: int) (handler: Timers.ElapsedEventArgs -> unit) =
//         let timer = new Timers.Timer()
//         timer.Enabled <- true
//         timer.Interval <- interval
//         timer.AutoReset <- false
//         timer.Elapsed.Add handler

//     let playRandomTrack () =
//         let audio = audioHeaders[rnd.Next(audioHeaders.Length)]
//         printfn $"Playing {audio.Filename}: {Duration.toStringHMS audio.Duration}"
//         Mixer.Play("1", audio.Filename)

//     let rec handler (x: Timers.ElapsedEventArgs) =
//         printfn $"Timer elapsed {x.SignalTime}"
//         playRandomTrack ()
//         createTimer 5000 handler

//     let timer = createTimer 5000 handler

//     // Event.add (fun (event: Timers.ElapsedEventArgs) -> printfn $"{event.SignalTime}") timer

//     Console.ReadLine() |> ignore

let trackAgent (track: string) (audioHeaders: AudioHeader list) =
    let rnd = new Random()
    let mixer = Mixer("10.0.1.123", 3000)

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                mixer.Fade(track, 0f)

                let audio = audioHeaders[rnd.Next(audioHeaders.Length - 1)]
                let start = rnd.Next(audio.Duration.Value)
                let duration = rnd.Next(min (audio.Duration.Value - start) 5000, min (audio.Duration.Value - start) 20000)
                // let duration = rnd.Next(10000, 20000)
                do printfn $"Playing {audio.Filename} on track {track} (duration=audio.Duration.Value) at {start}ms for {duration}ms"
                do mixer.Play(track, audio.Filename, Duration.toStringHMS (Duration start))

                mixer.Fade(track, 1.0f, 2000, "sin")
                do! Async.Sleep(2000)

                do! Async.Sleep(duration)

                mixer.Fade(track, 0f, 2000, "sin")
                do! Async.Sleep(2000)

                return! loop ()
            }

        loop ())

let library = loadLibrary "var/library"

let audioBook = "AIRC_1"

mixer.Cd(IO.Path.Join(libraryPath, audioBook))

for i in [ 1..4 ] do
    // printfn $"Starting agent for track {i}"
    trackAgent (string i) library[audioBook] |> ignore

Async.RunSynchronously(async { do Console.ReadKey() |> ignore })
mixer.Stop("all")
