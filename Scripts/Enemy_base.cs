using Godot;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System;

public partial class Enemy_base : RigidBody2D
{
	[Export] public Line2D this_line;
	private float og_line_width;
	[Export] public int Speed = 20;
	[Export] public float Max_health = 100;
	[Export] public float health = 100;
	[Export] public float wall_damage = 0.5f;
	[Export] public bool stunned = true;	

	private PathFinder pathFinder;
	[Export] public TileMapLayer tilemap;
	private bool path_updated = false;
	private const int cellsize=16;
	private Godot.Vector2I finish_position;

	List<PathNode> path = new List<PathNode>();
	[Export] public int PathFinding_delay = 15;
	private int current_pathfinding_delay = 0;
	public override void _PhysicsProcess(double delta)
	{
		current_pathfinding_delay++;

		if (stunned == false)
		{
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
		
		walk_along_nodes(path);
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

		this_line.SetPointPosition(0,new Godot.Vector2(og_line_width * (health/Max_health),0));
	}
	public void Knockback(Godot.Vector2 Direction, float force){
		float ex_force = Max_health;
		if (Max_health != health){ex_force = Max_health/health;}
		ApplyImpulse(Direction * force * ex_force);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Vector2 temp_pos = GetNode<Area2D>("/root/Main_test_scene/Finish").Position;
		finish_position = new Vector2I(Mathf.RoundToInt(temp_pos.X/cellsize),//->
		Mathf.RoundToInt(temp_pos.Y/cellsize)); //<-
		finish_position = finish_position.Abs();

		pathFinder = new PathFinder(finish_position.X+10,finish_position.Y+10, tilemap); 
	}

	private async void walk_along_nodes(List<PathNode> nodes){
		path_updated = false;

		Godot.Vector2 Velocity = new Godot.Vector2(0,0);
		while (path_updated == false){
			for (int i = 0; i < nodes.Count; i++){

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
						if (script.Area_damageI(nodes[i].tilemap_position, wall_damage * (float)GetPhysicsProcessDeltaTime()) == true){break;}
						await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					}
					nodes[i].is_obstruction = false;
				}

				while ((distance > 4) && path_updated == false){
					if (distance > distance2){break;}
					if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}

					Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
					GlobalPosition += Velocity;

					distance = GlobalPosition.DistanceTo(cell_positon);
					distance2 = GlobalPosition.DistanceTo(cell_positon2);

					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}
}
