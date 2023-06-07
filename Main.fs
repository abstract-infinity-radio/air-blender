open Air.Blender.Mixer

let Mixer = Mixer("10.0.1.123", 3000)

// Mixer.Cd("/var/air/data/library")
Mixer.Play("1", "test.wav")
// Mixer.Loop("1", "test.wav", "0:10.000", "0:11")
// Mixer.Unmute("1")
// Mixer.Unmute("1")
// Mixer.Stop("1")
// Mixer.Fade("1", 1.0f, 2000, "sin")
