using Godot;
using System;

public partial class Enemy_base : RigidBody2D
{
	[Export] public Line2D this_line;
	private float og_line_width;
	[Export] public int Speed = 200;
	[Export] public float Max_health = 100;
	[Export] public float health = 100;
	
	public override void _PhysicsProcess(double delta)
	{
		
	}

	public void Damage(float damage)
	{
		health -= damage;

		this_line.SetPointPosition(0,new Vector2(og_line_width * (Max_health / health),0));
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health = Max_health;
		if (this_line == null) {this_line = this.GetNode<Line2D>("Line2D");}
		og_line_width = this_line.Points[0].X;
	}
}
