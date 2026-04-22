using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Godot;
using static Godot.TextServer;

public partial class Projectile : Area2D
{
	//Decay and innactivity
	private float current_time = 0;
	[Export] public float Inactive_time = 0.5f;
    [Export] public float current_inact_time = 0;
	[Export] public float Decay_time = 3;

	[Export] public float BaseSpeed = 30; //. ,
	public float speed = 30; //. ,
	[Export] public float damage = 10;
    [Export] public float StunDuration = 0;

    [Export] public int Pierce = 2;
	[Export] private PackedScene Shrapnell;
	[Export] private int ShrapnellAmount;
	[Export] private float ShrapnellSpread;
    private int HitCounter;
    public Godot.Vector2 Direction; 

	private List<int> ActiveLayers = new List<int>();
	//
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        this.BodyEntered += OnBodyEntered;
        if (ActiveLayers.Count == 0){CollisionLayer += this.CollisionMask;}
    }
	public void instantiate(Godot.Vector2 direction, float SpeedMultiplier=1, float Extra_spread = 0,float damage = int.MaxValue,List<int> ExLayers = null, List<int> IncLayers=null){
		if (Extra_spread != 0){
			direction = add_spread(direction,Extra_spread);
		}
		this.Direction = direction.Normalized();
		this.Rotation = Direction.Angle() + Mathf.Pi/2;
        this.speed = BaseSpeed * SpeedMultiplier;
        if (damage != int.MaxValue){this.damage = damage;}
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
			if (body is Enemy_base enemy){
				GD.Print(Name);
				GD.Print(damage);
				enemy.Damage(damage,StunDuration,Name);
				Godot.Vector2 Direction = (body.Position - this.Position).Normalized();
				enemy.Knockback(Direction, 1f);
				HitCounter++;
				if (Shrapnell != null){
					float ExtraSpread = Direction.Angle();
					for (int i = 0; i < ShrapnellAmount; i++){
						ExtraSpread += ShrapnellSpread;
						InstantiateShrapnell(ExtraSpread);
					}
				}
				if (HitCounter == Pierce){QueueFree();}
			}
		}
    }
    private void InstantiateShrapnell(float ExtraSpread)
	{
        Node2D instance = Shrapnell.Instantiate<Node2D>();
		GetParent().CallDeferred("add_child", instance);
		if (instance is Projectile projectile)
		{
			Godot.Vector2 ExtraDirection = new Godot.Vector2(Mathf.Cos(ExtraSpread), Mathf.Sin(ExtraSpread));
			projectile.instantiate((Direction + ExtraDirection).Normalized());
		}
		instance.CallDeferred(Node2D.MethodName.SetGlobalPosition, this.GlobalPosition);
    }
}
