using Godot;
using static Godot.TextServer;
using System.Collections.Generic;
using System.Numerics;
using System;

public partial class Projectile : Area2D
{
	//Decay and innactivity
	private float current_time = 0;
	[Export] public float Inactive_time = 0.5f;
    [Export] public float current_inact_time = 0;
	[Export] public float Decay_time = 3;


	[Export] public float speed;
	[Export] public float damage;
	private Godot.Vector2 Direction; 

	//
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	public void instantiate(Godot.Vector2 direction, float speed=30, float Extra_spread = 0, List<int> ex_layers = null){
		
		if (Extra_spread != 0){
			direction = add_spread(direction,Extra_spread);
		}
		this.Direction = direction.Normalized();
		this.Rotation = Direction.Angle() + 90;
		this.speed = speed;

	}
	private Godot.Vector2 add_spread(Godot.Vector2 direction,float Extra_spread){
        float Current_angle = Mathf.RadToDeg(Mathf.Atan2(direction.Y,direction.X));
        float Angle_to_add = 0 + rng.RandfRange(-Extra_spread, Extra_spread);
        float rad_angle = Mathf.DegToRad((Current_angle + Angle_to_add));
        return new Godot.Vector2(Mathf.Cos(rad_angle), Mathf.Sin(rad_angle));
    }
	private async void temp_exc_layers(List<int> layers, float duration){

		foreach (int layer in layers)
		{
			this.CollisionLayer -= (uint)layer;	
		}
		await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
		foreach (int layer in layers)
		{
			this.CollisionLayer += (uint)layer;	
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition += Direction * speed * (float)delta;
	}
}
