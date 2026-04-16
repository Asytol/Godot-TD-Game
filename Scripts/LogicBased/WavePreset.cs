using Godot;
using System.Collections.Generic;
using System;

public class WavePreset
{
    public List<SpawnPreset> SpawnPresets = new List<SpawnPreset>();
    public int WaveMoney;
    public WavePreset(List<SpawnPreset> SpawnPresets,int WaveMoney=200)
    {
        this.SpawnPresets = SpawnPresets;
        this.WaveMoney = WaveMoney;
    }
}
