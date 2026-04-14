using Godot;
using System.Collections.Generic;
using System;

public partial class Spawner : Node2D
{
    [Export] public float global_wait = 3;
    public double current_time;
    public List<SpawnPreset> EnemyClumps = new List<SpawnPreset>();
    private PackedScene current_enemy;

    public override void _Ready()
    {
        //Just testing my system with this line
    }

    public override void _Process(double delta)
    {
    }
    public void ChangeEnemyClumps(List<SpawnPreset> EnemyClumps)
    {
        this.EnemyClumps = EnemyClumps;
    }
    public async void StartSpawn(SpawnPreset EnemyClump)
    {
        await ToSignal(GetTree().CreateTimer(global_wait), SceneTreeTimer.SignalName.Timeout);
        
        spawn_clump(EnemyClump);
    }

    private async void spawn_clump(SpawnPreset preset)
    {
        await ToSignal(GetTree().CreateTimer(preset.wait), SceneTreeTimer.SignalName.Timeout);

        for (int i = 0; i < preset.amount; i++)
        {
            GD.Print("instantiated enemy");
            PackedScene enemy = GD.Load<PackedScene>(preset.EnemyScene); //Maybe load from levelhandler instead to have on loadtime
            Node instance = enemy.Instantiate();
            AddChild(instance);
            Node2D obj = instance as Node2D;
            obj.GlobalPosition = GlobalPosition;
            GD.Print(obj.GlobalPosition);
            await ToSignal(GetTree().CreateTimer(preset.SpawnTime), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

}
