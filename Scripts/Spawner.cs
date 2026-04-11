using Godot;
using System.Collections.Generic;
using System;

public partial class Spawner : Node2D
{
    [Export] public float global_wait = 20;
    public double current_time;
    public List<SpawnPreset> enemy_clumps = new List<SpawnPreset>();
    private PackedScene current_enemy;

    public override void _Ready()
    {
        //Just testing my system with this line
        enemy_clumps.Add(new SpawnPreset(5,2,1,GD.Load<PackedScene>("res://Scenes/Lizard_enemy.tscn")));

        foreach (SpawnPreset enemy in enemy_clumps)
        {
            spawn_clump(enemy);
        }
    }

    public override void _Process(double delta)
    {
    }

    private async void spawn_clump(SpawnPreset preset)
    {
        await ToSignal(GetTree().CreateTimer(preset.wait), SceneTreeTimer.SignalName.Timeout);

        for (int i = 0; i < preset.amount; i++)
        {
            GD.Print("instantiated enemy");
            Node instance = preset.enemy.Instantiate();
            AddChild(instance);
            Node2D obj = instance as Node2D;
            obj.GlobalPosition = GlobalPosition;
            GD.Print(obj.GlobalPosition);
            await ToSignal(GetTree().CreateTimer(preset.SpawnTime), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

}
