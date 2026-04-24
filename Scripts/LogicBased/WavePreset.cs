using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class WavePreset : Resource
{
    [Export] public SpawnPreset[] Spawns;
    [Export] public string[] UsedSpawners;
    [Export] public int WaveMoney;
}
