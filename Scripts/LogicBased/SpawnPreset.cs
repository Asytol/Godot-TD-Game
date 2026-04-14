using Godot;
using System;

public class SpawnPreset
{
    public int amount;
    public float wait;
    public float SpawnTime;
    public string EnemyScene;
    public string SpawnerName;
    public SpawnPreset(int amount,float wait, float SpawnTime,string EnemyScene,string SpawnerName="Spawner")
    {
        this.amount = amount;
        this.wait = wait;
        this.SpawnTime = SpawnTime;
        this.EnemyScene = EnemyScene;
        this.SpawnerName = SpawnerName;
    }
}
