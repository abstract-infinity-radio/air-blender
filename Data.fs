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

type Time =
    | Time of int

    member this.Value =
        let (Time value) = this
        value

module Time =

    let toStringHMS (duration: Time) =
        let (Time durationInMilliseconds) = duration
        let totalSeconds = durationInMilliseconds / 1000
        let milliseconds = durationInMilliseconds % 1000
        let hours = totalSeconds / 3600
        let minutes = (totalSeconds - (hours * 60)) / 60
        let seconds = totalSeconds - (hours * 3600) - (minutes * 60)
        $"{hours}:{minutes}:{seconds}.{milliseconds}"

type AudioHeader = { Filename: string; Duration: Time }

let readAudioHeaders audioHeadersFilename bookPath =

    let headers =
        IO.File.ReadLines audioHeadersFilename
        |> String.concat "\n"
        |> Deserialize<Map<string, AudioHeaderDto>>

    match headers.Head with
    | Success headers -> headers.Data
    | Error error -> failwith $"Errror parsing audio headers from {audioHeadersFilename}: {error}"
    |> Map.toList
    |> List.map (fun (key, value) ->
        { Filename = IO.Path.Join(bookPath, key)
          Duration = value.duration_ms |> int |> Time })

type Library = Map<string, AudioHeader list>

let loadLibrary audioHeadersPath libraryPath : Library =
    IO.Directory.GetDirectories audioHeadersPath
    |> Array.map (fun bookPath ->
        IO.DirectoryInfo(bookPath).Name,
        readAudioHeaders (IO.Path.Join(bookPath, "audioheaders.yaml")) (IO.Path.Join(libraryPath, IO.DirectoryInfo(bookPath).Name)))
    |> Map.ofArray
