using Godot;
using System;

public partial class Tower_base : Area2D
{
	private CollisionShape2D my_collider;
	[Export] public float base_range = 80;
	public float range = 80;
	[Export] public float base_damage = 80;
	public float damage = 80;
	//
	[Export] public float Cooldown = 2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		range = base_range; damage = base_damage;


		my_collider = this.GetNode<CollisionShape2D>("CollisionShape2D");
		CircleShape2D circle_collider = my_collider.Shape as CircleShape2D;

		circle_collider.Radius = range;

		this.BodyEntered += OnBodyEntered;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Add_range(float addon){
		this.range += addon;
	}
	public void Multiply_range(float multiplier){
		this.range *= multiplier;
	}
	public void Add_damage(float addon){
		this.damage += addon;
	}
	public void Multiply_damage(float multiplier){
		this.damage *= multiplier;
	}

	private void OnBodyEntered(Node2D body)
	{
		
	}
}
