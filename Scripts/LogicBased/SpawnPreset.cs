using System;
using Godot;

[GlobalClass]
public partial class SpawnPreset : Resource
{
    [Export] public int amount;
    [Export] public float wait;
    [Export] public float SpawnTime; 
    [Export] public PackedScene EnemyScene;
    [Export] public string SpawnerName;
}
