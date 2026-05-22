using Godot;
using System;
using System.Collections.Generic;

public partial class FriendlyLizardScript : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;



	//Path finding
	private PathFinder pathFinder;
	List<PathNode> path = new List<PathNode>();
	[Export] public Godot.TileMapLayer tilemap;
	private bool path_updated = false;
	private bool Walking;
	private const int cellsize=16;
	private Godot.Vector2I finish_position;

	private const int mapheight = 41;
	private const int mapwidth = 73;


	private Vector2 GlobalVelocity;

    public override void _Ready()
    {
		if (tilemap == null) { tilemap = GetTree().Root.GetChild(1).GetNode<Godot.TileMapLayer>("%TileMap"); }
		pathFinder = new PathFinder(mapwidth,mapheight, tilemap);

		ForceReCalculatePath();
    }
	public override void _PhysicsProcess(double delta)
	{
		Velocity = GlobalVelocity*Speed;
		MoveAndSlide();
	}

	private void ForceReCalculatePath()
	{
		pathFinder.GetGrid().GetXY(new Godot.Vector2 (0,0),out int x, out int y);
		Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));

		path = pathFinder.FindPath(position.X,position.Y,finish_position.X,finish_position.Y);
		GD.Print(path);
		path_updated = true;
	}
	public void MoveTowardsPos(Vector2 Position)
	{
		finish_position = new Vector2I(Mathf.RoundToInt(Position.X/cellsize),//->
		Mathf.RoundToInt(Position.Y/cellsize)); //<-

		ForceReCalculatePath();
		WalkAlongNodes(path);
	}

	public async void WalkAlongNodes(List<PathNode> nodes){
		path_updated = false;
		for (int i = 0; i < nodes.Count; i++){
			if (path_updated){break;}
			Godot.Vector2 Velocity = Godot.Vector2.Zero;
			Godot.Vector2 cell_positon = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            Godot.Vector2 cell_positon2 = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            GD.Print(cell_positon);
            float distance = GlobalPosition.DistanceTo(cell_positon);
            float distance2 = GlobalPosition.DistanceTo(cell_positon2);
            
            if (distance > distance2){continue;}
			if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){continue;}
			//
            while ((Mathf.Abs(GlobalPosition.X - cell_positon.X) > 1 || Mathf.Abs(GlobalPosition.Y - cell_positon.Y) > 1) && !path_updated)
            {
                if (distance > distance2) { break; }
                //if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}
                Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
				GlobalVelocity = Velocity;

				distance = GlobalPosition.DistanceTo(cell_positon);
				distance2 = GlobalPosition.DistanceTo(cell_positon2);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
		GlobalVelocity = Vector2.Zero;
		//BrokenTotem.DrawList.Remove(new Vector2I(nodes[nodes.Count].x,nodes[nodes.Count].y));
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
}
