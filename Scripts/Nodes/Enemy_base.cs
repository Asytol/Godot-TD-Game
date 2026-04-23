using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Godot;

public partial class Enemy_base : RigidBody2D
{
	private AnimatedSprite2D Sprite;
	[Export] public Line2D this_line;
	[Export] private Label DamageDisplay;
	private GpuParticles2D Particles2D;
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	
	private float og_line_width;
	[Export] public int Speed = 1;
	private Godot.Vector2 GlobalVelocity;
	[Export] public float Max_health = 100;
	[Export] public float health = 100;
	[Export] public float wall_damage = 0.5f;
	[Export] public bool stunned = false;
	private float StunDuration = 2;
	[Export] public float StunResistance = 1;
	[Export] public float ExtraIFrames = 0.2f;
	private float C_StunDuration = 0;
	[Export] public float RotationSpeed = 5;
	[Export] public int MoneyDrops;
	private PathFinder pathFinder;
	[Export] public TileMapLayer tilemap;
	public bool path_updated = false;
	private const int cellsize=16;
	private const int mapheight = 41;
	private const int mapwidth = 73;

	private Godot.Vector2I finish_position;

	List<PathNode> path = new List<PathNode>();
	[Export] public int PathFinding_delay = 30;
	private int current_pathfinding_delay = 0;

	private bool KillingSelf = false;

	public List<I_frame_obj> IFrameList = new List<I_frame_obj> {};

	public override void _Ready()
	{
		Sprite = GetChild<AnimatedSprite2D>(0);
		DamageDisplay = GetNode<Label>("%DamageDisplay");
		Particles2D = GetNode<GpuParticles2D>("%Particle");
		//Child(0) is sceene transitioner :crying_emoji:
		if (tilemap == null) { tilemap = GetTree().Root.GetChild(1).GetNode<TileMapLayer>("%TileMap"); }

		if (this_line == null){this_line = GetNode<Line2D>("Line2D"); }
		og_line_width = this_line.Points[0].X;
		this_line.Visible = false;

		Godot.Vector2 temp_pos = GetTree().Root.GetChild(1).GetNode<Area2D>("%Finish").Position;
		finish_position = new Vector2I(Mathf.RoundToInt(temp_pos.X/cellsize),//->
		Mathf.RoundToInt(temp_pos.Y/cellsize)); //<-
		finish_position = finish_position.Abs();

		pathFinder = new PathFinder(mapwidth,mapheight, tilemap);

		GetNode<Area2D>("Area2D").AreaEntered += OnAreaEntered;
		GetNode<Area2D>("Area2D").BodyEntered += OnBodyEntered;
		GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;
		ForceReCalculatePath();
	}
	public override void _PhysicsProcess(double delta)
	{
		UpdateIFrameList((float) delta);
		current_pathfinding_delay++;

		if (!stunned)
		{
			if (GlobalRotation != 0) { StandStraight(); }
			GlobalPosition += GlobalVelocity;
			Sprite.Play("Walking");
		}
		else{
			C_StunDuration += (float)delta;
			Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));
			if (tilemap.GetCellSourceId(position) == 1)
			{
				if (!KillingSelf){KillYourself();}
			}

			if (C_StunDuration > StunDuration){
				stunned = false;
				C_StunDuration = 0;
				StandStraight();
			}
		}

		if (path_updated)
		{
			GD.Print(path.Count);
			WalkAlongNodes(path);
		}
	}

	public void Damage(float damage,float StunDuration,StringName name)
	{
		if (CheckIFrames(name)) { return; } //in list
		this_line.Visible = true;
		DamageDisplay.Text = damage.ToString();
		Godot.Vector2 ParticleVelocity = new Godot.Vector2(rng.RandfRange(-10, 10), 10);
		Particles2D.EmitParticle(this.Transform, ParticleVelocity, Colors.Black, Colors.Black, 2);
		Particles2D.GetChild<GpuParticles2D>(0).Restart();
		IFrameList.Add(new I_frame_obj(name,StunDuration+ExtraIFrames));
		health -= damage;
		if (health <= 0 && !KillingSelf) { KillYourself(); }
		Sprite.Play("default");

		this_line.SetPointPosition(0,new Godot.Vector2(og_line_width * (health/Max_health),0));

		if (this.StunDuration - C_StunDuration < StunDuration/StunResistance && StunDuration != 0)
		{this.StunDuration = StunDuration/StunResistance; stunned = true;}
	}
	public void Knockback(Godot.Vector2 Direction, float force){
		float ex_force = Max_health;
		if (health != 0){ex_force = Max_health/health;}
		ApplyImpulse(Direction * force * ex_force);
	}
	private void UpdateIFrameList(float deltaTime){
		for (int i = 0; i < IFrameList.Count; i++){
			IFrameList[i].C_Duration += deltaTime;
			if (IFrameList[i].C_Duration > IFrameList[i].Duration){
				IFrameList.RemoveAt(i);
			}
		}
	}
	public bool CheckIFrames(StringName name){
		foreach (I_frame_obj obj in IFrameList){
			if (obj.name == name){
				return true;
			}
		}
		return false;
	}

	// Called when the node enters the scene tree for the first time.
	private async void StandStraight()
	{
		Tween tween = CreateTween();

		float time = 2;
		float StartPoint = 0;
		if(RotationDegrees > 180){StartPoint = 180;}
		time *= Mathf.Abs(StartPoint-RotationDegrees)/90;

		tween.TweenProperty(this,"rotation",Mathf.DegToRad(0),time);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	private async void WalkAlongNodes(List<PathNode> nodes){
		path_updated = false;
		for (int i = 0; i < nodes.Count; i++){
			if (path_updated){break;}
			Godot.Vector2 Velocity = Godot.Vector2.Zero;

			Godot.Vector2 cell_positon = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize+8);
			Godot.Vector2 cell_positon2 = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize+8);
			float distance = GlobalPosition.DistanceTo(cell_positon);
			float distance2 = GlobalPosition.DistanceTo(cell_positon2);
			if (distance > distance2){continue;}
			if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){continue;}
			//
			if (nodes[i].is_obstruction){
				TileMapLayer script = tilemap as TileMapLayer;
				Godot.Vector2I TileMapPosition = nodes[i].tilemap_position;
				GlobalVelocity = Godot.Vector2.Zero;
				while (true){
					if (script.Area_damageI(TileMapPosition, wall_damage*(float)GetPhysicsProcessDeltaTime()) == true){break;}
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
				nodes[i].is_obstruction = false;
			}

			while ((distance > 4) && !path_updated){
				if (distance > distance2){break;}
				if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}

				Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
				GlobalVelocity = Velocity;

				distance = GlobalPosition.DistanceTo(cell_positon);
				distance2 = GlobalPosition.DistanceTo(cell_positon2);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
	private void OnAreaEntered(Node2D body)
	{
	}
	private void OnBodyEntered(Node2D body)
	{
	}
	private void OnBodyExited(Node2D body)
	{
	}

	private async void KillYourself()
	{
		KillingSelf = true;
		GetTree().Root.GetChild<LevelHandler>(1).OnEnemyDeath();
		Tween tween = CreateTween();

		AngularVelocity = 4000;
		tween.TweenProperty(this, "scale",Godot.Vector2.Zero,1);
		await ToSignal(tween, Tween.SignalName.Finished);
		QueueFree();
	}
	private void ForceReCalculatePath()
	{
		pathFinder.GetGrid().GetXY(new Godot.Vector2 (0,0),out int x, out int y);
		Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));

		path = pathFinder.FindPath(position.X,position.Y,finish_position.X,finish_position.Y);
		path_updated = true;
	}


	public void ExternallySetPath(List<PathNode> path)
	{
		this.path = path;
	}

	private void NewParticles(GpuParticles2D GPUParticles2D)
	{
		Node node = GPUParticles2D.Duplicate();
		GpuParticles2D NewParticle = (GpuParticles2D)node;
		NewParticle.GlobalPosition = GPUParticles2D.GlobalPosition;
		AddChild(NewParticle);
		NewParticle.Restart();
		NewParticle.Emitting = true;
		NewParticle.Connect("finished",new Callable(NewParticle, nameof(QueueFree)));
	}
}
