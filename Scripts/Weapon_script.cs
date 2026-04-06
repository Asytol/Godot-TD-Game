using Godot;
using GodotPlugins.Game;
using Microsoft.VisualBasic;
using static Godot.TextServer;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

public partial class Weapon_script : Area2D
{
	private CollisionPolygon2D my_collider;
	public bool available = false;

	//Duration and stuff
	[Export] public float Standard_Damage = 1;
	[Export] public float Damage = 1;
	[Export] public float Duration = 1;
	[Export] public float Cooldown = 2;
	[Export] public float Size = 1;
	[Export] public float Force = 1000;

	private MainCharacter parent_script;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
		
		available = true;
		Damage = Standard_Damage;
		if (my_collider == null)
		{
			my_collider = this.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
			my_collider.Disabled = true;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
	}
	public void Add_damage(float damage)
	{
		this.Damage += damage;	
	}
	public void Add_dmg_multiplier(float multiplier)
	{
		this.Damage *= multiplier;
	}
	public void Add_size(float size)
	{
		Size += size;
	}

	public void Summon_weapon(MainCharacter script)
	{
		Summon_weapon2();
		this.parent_script = script;
	}

	private async void Summon_weapon2()
	{
		available = false;
		my_collider.Disabled = false;

		//Builtin timer
		await ToSignal(GetTree().CreateTimer(Duration), SceneTreeTimer.SignalName.Timeout);

		my_collider.Disabled = true;
		GD.Print("Weapon disabled");
		//Builtin timer
		await ToSignal(GetTree().CreateTimer(Cooldown), SceneTreeTimer.SignalName.Timeout);

		GD.Print("Weapon available");
		available = true;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is CollisionObject2D collider && collider.CollisionLayer == 2){
			if (parent_script.CheckIFrames(body.Name) == false)
			{
				parent_script.I_frame_list.Add(new I_frame_obj(body.Name, 1));

				GD.Print("Weapon_collided, w/ enemy");
				if (body is RigidBody2D rb)
				{
					Vector2 Direction = (body.Position - this.Position).Normalized();
					rb.LinearVelocity += Direction * Force;
					GD.Print("New velocity is:"+rb.LinearVelocity);
				}
				Enemy_base script = body as Enemy_base;
				script.health -= Damage;	
			}
		}
	} 
}
