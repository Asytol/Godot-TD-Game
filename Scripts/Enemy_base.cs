using Godot;
using System;

public partial class Enemy_base : RigidBody2D
{
	
	public const int Speed = 200;
	public float health = 100;
	
	public override void _PhysicsProcess(double delta)
	{
		
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
}
