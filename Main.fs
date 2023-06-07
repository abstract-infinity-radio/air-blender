open Air.Blender.Data
open Air.Blender.Mixer
open Legivel.Serialization

// let Mixer = Mixer("10.0.1.123", 3000)

// Mixer.Mono("all")
// Mixer.Fade("all", 1.0f)
// Mixer.Unmute("all")

// // Mixer.Cd("/var/air/data/library")
// // Mixer.Fade("1", 1.0f, 5000, "sin")
// // Mixer.Loop("1", "test.wav", "0:10.000", "0:11")
// // Mixer.Mute("1")
// // Mixer.Unmute("8")
// // Mixer.Stop("1")

// Mixer.Play("1", "test.wav")
// Mixer.Play("2", "test.wav")

// Mixer.Fade("2", 0.5f, 5000, "sin")

let audioHeaders =
    readAudioHeaders "var/audioheaders.yaml"
    |> List.iter (fun header ->
        let Duration milliseconds = header.Duration
        printfn $"Filename: {header.Filename} ; Duration: {header.Duration.ToString()}")

printfn $"{audioHeaders}"

// match audioHeaders with
// | Ok headers -> headers
// |> printfn "%A"
