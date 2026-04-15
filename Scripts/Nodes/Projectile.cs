using Godot;
using static Godot.TextServer;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System;

public partial class Projectile : Area2D
{
	//Decay and innactivity
	private float current_time = 0;
	[Export] public float Inactive_time = 0.5f;
    [Export] public float current_inact_time = 0;
	[Export] public float Decay_time = 3;


	[Export] public float speed = 30; //. ,
	[Export] public float damage = 10;
	[Export] public float StunDuration = 0.3f;
	private Godot.Vector2 Direction; 

	private List<int> ActiveLayers = new List<int>();
	//
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
	}
	public void instantiate(Godot.Vector2 direction, float speed=30, float Extra_spread = 0,List<int> ExLayers = null, List<int> IncLayers=null){
		
		if (Extra_spread != 0){
			direction = add_spread(direction,Extra_spread);
		}
		this.Direction = direction.Normalized();
		this.Rotation = Direction.Angle() + Mathf.Pi/2;
		this.speed = speed;
		if (IncLayers == null){IncLayers = new List<int>{2};}
		this.ActiveLayers = IncLayers;
		foreach (int layer in ActiveLayers)
		{
			CollisionLayer += (uint)layer;
		}
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
		GlobalPosition += Direction * speed * (float)delta*10;
		current_time += (float)delta;
		if (current_time > Decay_time)
		{
			QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is CollisionObject2D collider){
			foreach(int layer in ActiveLayers){
				if (collider.CollisionLayer == layer){
					if (body is Enemy_base enemy){
						enemy.Damage(damage,StunDuration,Name);
						Godot.Vector2 Direction = (body.Position - this.Position).Normalized();
						enemy.Knockback(Direction,1f);
						break;
					}
				}
			}
		}
	}
}
