using Godot;
using System;

public class SpawnPreset
{
    public int amount;
    public float wait;
    public float SpawnTime;
    public PackedScene enemy;
    public SpawnPreset(int amount,float wait, float SpawnTime,PackedScene enemy)
    {
        this.amount = amount;
        this.wait = wait;
        this.SpawnTime = SpawnTime;
        this.enemy = enemy;
    }
}
