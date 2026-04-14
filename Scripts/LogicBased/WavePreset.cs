using Godot;
using System.Collections.Generic;
using System;

public class WavePreset
{
    public List<SpawnPreset> SpawnPresets = new List<SpawnPreset>();
    public WavePreset(List<SpawnPreset> SpawnPresets)
    {
        this.SpawnPresets = SpawnPresets;
    }
}
