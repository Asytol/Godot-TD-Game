using Godot;
using System.Collections.Generic;
using System.Numerics;
using System;

public partial class TowerBase : Area2D
{
	private CollisionShape2D my_collider;
	[Export] public float base_range = 80;
	public float range = 80;
	//
	public List<Node2D> entities_in_area = new List<Node2D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		my_collider = this.GetNode<CollisionShape2D>("CollisionShape2D");
		CircleShape2D circle_collider = my_collider.Shape as CircleShape2D;

		circle_collider.Radius = range;

		this.BodyEntered += OnBodyEntered;
		this.BodyExited += OnBodyExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print("added"+body.Name);
		if (body is CollisionObject2D collider && collider.CollisionLayer == 2)
		{
			entities_in_area.Add(body);
		}
	}
	private void OnBodyExited(Node2D body)
	{
		GD.Print("removed"+body.Name);
		if (body is CollisionObject2D collider && collider.CollisionLayer == 2)
		{
			entities_in_area.Remove(body);
		}
	}

}
