using Godot;
using GodotPlugins.Game;
using static Godot.TextServer;
using System.Collections.Generic;
using System.Numerics;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class MainCharacter : RigidBody2D
{
	[Export] public CollisionShape2D this_collider;
	[Export] public int Jump_force = -400;
	[Export] public int speed = 40;

	[Export] public float health = 100;

	//Weapon
	[Export] public Area2D Weapon_body;
	[Export] public CollisionPolygon2D Weapon_collider;
	[Export] public Node2D Weapon_node;
	
	public bool Grounded = true;

	public List<I_frame_obj> I_frame_list = new List<I_frame_obj> {};
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (this_collider == null){this_collider = this.GetNode<CollisionShape2D>("CollisionShape2D");}


		Weapon_node = this.GetNode<Node2D>("Weapon_node");
		if (Weapon_body == null){
			//idk, we should prob change it to generic name so it just gets whatever, idk
			Weapon_body = this.GetNode<Area2D>("Reg_slash");		
		}

		Weapon_collider = Weapon_body.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		Weapon_collider.Disabled = true;
	}

	public override void _PhysicsProcess(double delta)
	{ 
		if (Input.IsActionJustPressed("jump"))
		{
			LinearVelocity += new Godot.Vector2(0,Jump_force);
			//ApplyImpulse(new Vector2(0, Jump_force));
		}
		
		if (Input.IsActionPressed("right"))
		{
			LinearVelocity += new Godot.Vector2(speed,0);
		}
		if (Input.IsActionPressed("left"))
		{
			LinearVelocity += new Godot.Vector2(-speed,0);
		} 
		
		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;	
		Godot.Vector2 Position = new Godot.Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + this_collider.Shape.GetRect().Size.Y / 2);
		var query = PhysicsRayQueryParameters2D.Create(Position,new Godot.Vector2(Position.X, Position.Y + 2),1);
		var result = spaceState.IntersectRay(query);
		if (result != null){
			Grounded = true; }
		else{ Grounded = false;}
		
		UpdateIFrameList((float) delta);
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton)
		{
			GD.Print("clicked");
			Weapon_script script = (Weapon_script)Weapon_body;
			if (script != null)
			{
				GD.Print("Not null");
				if (script.available == true)
				{	
					GD.Print("attacked");
					float Direction = GetAngleTo(eventMouseButton.Position);
					Weapon_body.Rotation = Direction - Mathf.Pi;
					script.Summon_weapon(this as MainCharacter);
				}
			}
		}
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			float Direction = GetAngleTo(eventMouseMotion.Position);
			Weapon_body.Rotation = Direction - Mathf.Pi;
		}
	}


	private void UpdateIFrameList(float deltaTime){
		for (int i = 0; i < I_frame_list.Count; i++){
			I_frame_list[i].C_Duration += deltaTime;
			if (I_frame_list[i].C_Duration > I_frame_list[i].Duration){
				I_frame_list.RemoveAt(i);
			}
		}
	}
	public bool CheckIFrames(StringName name){
		foreach (I_frame_obj obj in I_frame_list){
			if (obj.name == name){
				return true;
			}
		}
		return false;
	}
}

public class I_frame_obj
{ 
	public string name;
	public float Duration;
	public float C_Duration = 0;

	public I_frame_obj(string obj2_name, float Duration)
	{
		this.name = obj2_name; 
	}
}
