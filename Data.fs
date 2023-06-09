module Air.Blender.Data

open System
open Legivel.Serialization

type AudioHeaderDto =
    { bitrate: int
      channels: int
      duration: string
      duration_ms: float
      encoding: string
      samplerate: int
      ``type``: string }

type Duration =
    | Duration of int

    member this.Value =
        let (Duration value) = this
        value

module Duration =

    let toStringHMS (duration: Duration) =
        let (Duration durationInMilliseconds) = duration
        let totalSeconds = durationInMilliseconds / 1000
        let milliseconds = durationInMilliseconds % 1000
        let hours = totalSeconds / 3600
        let minutes = (totalSeconds - (hours * 60)) / 60
        let seconds = totalSeconds - (hours * 3600) - (minutes * 60)
        $"{hours}:{minutes}:{seconds}.{milliseconds}"

type AudioHeader =
    { Filename: string; Duration: Duration }

let readAudioHeaders filename =
    let headers =
        IO.File.ReadLines filename
        |> String.concat "\n"
        |> Deserialize<Map<string, AudioHeaderDto>>

    match headers.Head with
    | Success headers -> headers.Data
    | Error error -> failwith $"Errror parsing audio headers from {filename}: {error}"
    |> Map.toList
    |> List.map (fun (key, value) ->
        { Filename = key
          Duration = value.duration_ms |> int |> Duration })

type Library = FSharp.Collections.Map<string, AudioHeader list>

let loadLibrary path : Library =
    IO.Directory.GetDirectories path
    |> Array.map (fun path -> IO.DirectoryInfo(path).Name, readAudioHeaders $"{path}/audioheaders.yaml")
    |> Map.ofArray
