using Godot;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System;

public partial class Enemy_base : RigidBody2D
{
	[Export] public Line2D this_line;
	private float og_line_width;
	[Export] public int Speed = 1;
	private Godot.Vector2 GlobalVelocity;
	[Export] public float Max_health = 100;
	[Export] public float health = 100;
	[Export] public float wall_damage = 0.5f;
	[Export] public bool stunned = false;
	[Export] public float StunDuration = 2;
	private float C_StunDuration = 0;
	[Export] public float RotationSpeed = 5;
	[Export] public int MoneyDrops;
	private PathFinder pathFinder;
	[Export] public TileMapLayer tilemap;
	private bool path_updated = false;
	private const int cellsize=16;
	private Godot.Vector2I finish_position;

	List<PathNode> path = new List<PathNode>();
	[Export] public int PathFinding_delay = 15;
	private int current_pathfinding_delay = 0;

	private bool InKillZone = false;

	public override void _Ready()
	{
		if (tilemap == null){tilemap = GetTree().GetRoot().GetNode<TileMapLayer>("Main_test_scene/TileMap");}
		if (this_line == null){this_line = GetNode<Line2D>("Line2D"); }
		og_line_width = this_line.Points[0].X;

		Godot.Vector2 temp_pos = GetNode<Area2D>("/root/Main_test_scene/Finish").Position;
		finish_position = new Vector2I(Mathf.RoundToInt(temp_pos.X/cellsize),//->
		Mathf.RoundToInt(temp_pos.Y/cellsize)); //<-
		finish_position = finish_position.Abs();

		pathFinder = new PathFinder(finish_position.X+10,finish_position.Y+10, tilemap); 

		GetNode<Area2D>("Area2D").AreaEntered += OnAreaEntered;
		GetNode<Area2D>("Area2D").BodyEntered += OnBodyEntered;
		GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;
	}
	public override void _PhysicsProcess(double delta)
	{
		current_pathfinding_delay++;

		if (!stunned)
		{
			Rotation = 0;
			GlobalPosition += GlobalVelocity;
			if (current_pathfinding_delay == PathFinding_delay){
				pathFinder.GetGrid().GetXY(new Godot.Vector2 (10,10),out int x, out int y);
				Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));

				//What the actual fuck, why does the code break when the total width compared to the end position is smaller than 10! what! wtf!
				path = pathFinder.FindPath(position.X,position.Y,finish_position.X,finish_position.Y);
				path_updated = true;
				//-
				current_pathfinding_delay = 0;
			}
		}
		else{
			C_StunDuration += (float)delta;
			if (InKillZone){KillYourself();}
			if (C_StunDuration > StunDuration){
				stunned = false;
				C_StunDuration = 0;
				StandStraight();
			}
		}

		if (path_updated)
		{
			WalkAlongNodes(path);
		}
		QueueRedraw();
	}
	public override void _Draw(){
		for (int i = 1; i < path.Count; i++){
			DrawLine(new Godot.Vector2(path[i -1].x, path[i-1].y)*16 -this.GlobalPosition+new Godot.Vector2(8,8), new Godot.Vector2(path[i].x, path[i].y)*16-this.GlobalPosition+new Godot.Vector2(8,8),Colors.Red, 1.0f);
		} 
	}

	public void Damage(float damage)
	{
		health -= damage;
		if (health <= 0){KillYourself();}

		this_line.SetPointPosition(0,new Godot.Vector2(og_line_width * (health/Max_health),0));
		stunned = true;
	}
	public void Knockback(Godot.Vector2 Direction, float force){
		float ex_force = Max_health;
		if (health != 0){ex_force = Max_health/health;}
		ApplyImpulse(Direction * force * ex_force);
	}

	// Called when the node enters the scene tree for the first time.
	private async void StandStraight()
	{
		ConstantTorque = 0;
		int Direction = -1;
		if (Mathf.Abs(GlobalRotationDegrees) > 180){
			Direction *= -1;
		}
		if (Rotation < 0){
			Direction *= -1;
		}

		float Degrees = Rotation;
		while(Mathf.Abs(GlobalRotationDegrees) > 4 && Mathf.Abs(GlobalRotationDegrees) < 356)
		{
			Degrees += RotationSpeed * (float)GetPhysicsProcessDeltaTime();
			Rotation = Degrees;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		ConstantTorque = 0;
		Rotation = 0;
	}
	private async void WalkAlongNodes(List<PathNode> nodes){
		path_updated = false;

		Godot.Vector2 Velocity = new Godot.Vector2(0,0);
		for (int i = 0; i < nodes.Count; i++){
			if (path_updated){break;}
			Velocity = Godot.Vector2.Zero;

			Godot.Vector2 cell_positon = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize+8);
			Godot.Vector2 cell_positon2 = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize+8);
			float distance = GlobalPosition.DistanceTo(cell_positon);
			float distance2 = GlobalPosition.DistanceTo(cell_positon2);
			if (distance > distance2){continue;}
			if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){continue;}
			//
			if (nodes[i].is_obstruction){
				TileMapLayer script = tilemap as TileMapLayer;
				while (true){
					if (script.Area_damageI(nodes[i].tilemap_position, wall_damage) == true){break;}
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
				nodes[i].is_obstruction = false;
			}

			while ((distance > 4) && path_updated == false){
				if (distance > distance2){break;}
				if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}

				Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
				GlobalVelocity = Velocity;

				distance = GlobalPosition.DistanceTo(cell_positon);
				distance2 = GlobalPosition.DistanceTo(cell_positon2);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
	private void OnAreaEntered(Node2D body)
	{
		if (body is CollisionObject2D col)
		{
			GD.Print(col.CollisionLayer);
		}
		if (body is CollisionObject2D collider && collider.CollisionLayer == 5 && stunned)
		{ //Layermask collision layer 3 is holes, so we slaughter the lizards and other fuckers
			KillYourself();
		}
	}
	private void OnBodyEntered(Node2D body)
	{
		if (body is CollisionObject2D collider && collider.CollisionLayer == 5)
		{ //Layermask collision layer value 5 is holes, so we slaughter the lizards and other fuckers
			InKillZone = true;
		}
	}
	private void OnBodyExited(Node2D body)
	{
		if (body is CollisionObject2D collider && collider.CollisionLayer == 5)
		{ 
			InKillZone = false;
		}
	}

	private async void KillYourself()
	{
		/*
		this.SetProcess(false);
		while (this.Scale.X > 0.1 && this.Scale.Y > 0.1)
		{
			this.Scale = new Godot.Vector2(this.Scale.X - 0.1f, this.Scale.Y - 0.1f);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}*/
		QueueFree();
	}
}
