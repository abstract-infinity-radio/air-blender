# AIR-Blender

Project AIR Audio Blender

## Audio server

Fajli v enonivojski direktorijski strukturi.

Vsak direktorij ima dva metapodatkovna fajla:

1. Human editable general metadata

Polja so:

- Naslov
- Avtor
- Datum
- Seznam audio fajlov

2. Computer generated audio metadata

Analize so shranjene za vsako analizo posebej, za vsak file posebej. Fajl analize je oblike:

start stop data

Primer:

metadata.json

track-1.waw
track-1.anal.tempo
track-1.anal.density

track-2.aiff
track-1.anal.density

## Komunikacija

Client:
localhost:3000/airmix/channel/controller/arguments

Server:
localhost:3001/airmon/
