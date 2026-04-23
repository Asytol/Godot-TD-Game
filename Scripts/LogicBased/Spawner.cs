using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class Spawner : Node2D
{
    [Export] public float global_wait = 3;
    public double current_time;
    public List<SpawnPreset> EnemyClumps = new List<SpawnPreset>();
    private PackedScene current_enemy;

    private PathFinder pathFinder;
    public List<PathNode> path = new List<PathNode>();
    private const int cellsize=16;
	private const int mapheight = 41;
    private const int mapwidth = 73;
    private Godot.Vector2I finish_position;

    private TileMapLayer tilemap;
    public override void _Ready()
    {
        tilemap = GetNode<TileMapLayer>("%TileMap");
        tilemap.Connect("CustomTileChanged", new Callable(this, nameof(ForceReCalculatePath)));
        pathFinder = new PathFinder(mapwidth, mapheight, tilemap);

        Godot.Vector2 temp_pos = GetTree().Root.GetChild(1).GetNode<Area2D>("%Finish").Position;
        finish_position = new Vector2I(Mathf.RoundToInt(temp_pos.X/cellsize),//->
		Mathf.RoundToInt(temp_pos.Y/cellsize)); //<-
        finish_position = finish_position.Abs();

        ForceReCalculatePath();
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
            GD.Print("instantiated enemy"); //Maybe load from levelhandler instead to have on loadtime
            Node instance = preset.EnemyScene.Instantiate();
            AddChild(instance);
            Node2D obj = instance as Node2D;
            if (obj is Enemy_base script)
            {
                script.ExternallySetPath(path);
                script.path_updated = true;
            }
            obj.GlobalPosition = GlobalPosition;
            GD.Print(obj.GlobalPosition);
            await ToSignal(GetTree().CreateTimer(preset.SpawnTime), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
    private void ForceReCalculatePath()
    {
        GD.Print("bang");
        pathFinder.GetGrid().GetXY(new Godot.Vector2 (0,0),out int x, out int y);
		Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));

        path = pathFinder.FindPath(position.X, position.Y, finish_position.X, finish_position.Y);
        QueueRedraw();
    }
    public override void _Draw(){
		for (int i = 1; i < path.Count; i++){
			DrawLine(new Godot.Vector2(path[i -1].x, path[i-1].y)*16 -this.GlobalPosition+new Godot.Vector2(8,8), new Godot.Vector2(path[i].x, path[i].y)*16-this.GlobalPosition+new Godot.Vector2(8,8),Colors.Red, 1.0f);
		}
	}

}
