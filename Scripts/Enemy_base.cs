using Godot;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System;

public partial class Enemy_base : RigidBody2D
{
	[Export] public Line2D this_line;
	private float og_line_width;
	[Export] public int Speed = 200;
	[Export] public float Max_health = 100;
	[Export] public float health = 100;
	

	private PathFinder pathFinder;
	[Export] public TileMapLayer tilemap;
	List<PathNode> path = new List<PathNode>();
	public override void _PhysicsProcess(double delta)
	{
		pathFinder.GetGrid().GetXY(new Godot.Vector2 (10,10),out int x, out int y);
		Vector2I position = new Vector2I (Mathf.RoundToInt(this.GlobalPosition.X),Mathf.RoundToInt(this.GlobalPosition.Y));

		path = pathFinder.FindPath(0,0,10,10);
	}
	public override void _Draw(){
		for (int i = 1; i < path.Count; i++){
			GD.Print("coordinates: "+path[i].x+" "+path[i].y);
			DrawLine(new Godot.Vector2(path[i -1].x, path[i-1].y), new Godot.Vector2(path[i].x, path[i].y),Colors.Green, 1.0f);
		} 
	}

	public void Damage(float damage)
	{
		health -= damage;

		this_line.SetPointPosition(0,new Godot.Vector2(og_line_width * (health/Max_health),0));
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pathFinder = new PathFinder(20,20, tilemap);
	}
}
