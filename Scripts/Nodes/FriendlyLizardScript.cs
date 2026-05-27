using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class FriendlyLizardScript : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;



	//Path finding
	private PathFinder pathFinder;
	List<PathNode> path = new List<PathNode>();
	[Export] public Godot.TileMapLayer tilemap;
	private bool path_updated = false;
	private bool Walking;
	private const int cellsize=16;
	private Godot.Vector2I finish_position;

	private const int mapheight = 41;
	private const int mapwidth = 73;


	public Vector2 GlobalVelocity;

	private bool QueueCoroutineDeletion;
	private int CoroutineAmount = 0;

	[Export] private AnimatedSprite2D Sprite;
	[Export] private Sprite2D MaterialSprite;
	private string CarriedMaterial = "";

	private bool Colliding;
	private bool RunningAway;
	private int FriendsInside;
	private List<Node2D> ListOfFriends = new List<Node2D>();

    public override void _Ready()
    {
		if (tilemap == null) { tilemap = GetTree().Root.GetChild(1).GetNode<Godot.TileMapLayer>("%TileMap"); }
		pathFinder = new PathFinder(mapwidth,mapheight, tilemap);

		MaterialSprite.Visible = false;
		GetNode<Sprite2D>("SelectionCircle").Visible = false;

		GetNode<Area2D>("Area2D").AreaEntered += PickingUpMaterial;
		GetNode<Area2D>("TinyDetector").AreaEntered += CollidedWithFriendly;
		GetNode<Area2D>("TinyDetector").AreaExited += ExitingFriendly;

		Sprite.Play("default");

		ForceReCalculatePath();
    }
	public override void _PhysicsProcess(double delta)
	{
		if (Colliding == false)
		{
			Velocity = GlobalVelocity*Speed;	
		}
		else if (ListOfFriends.Count != 0){ Velocity = -GlobalPosition.DirectionTo(ListOfFriends[0].GlobalPosition) * Speed;}
		MoveAndSlide();
	}

	private void ForceReCalculatePath()
	{
		pathFinder.GetGrid().GetXY(new Godot.Vector2 (0,0),out int x, out int y);
		Vector2I position = new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/cellsize),Mathf.FloorToInt(this.GlobalPosition.Y/cellsize));

		path = pathFinder.FindPath(position.X,position.Y,finish_position.X,finish_position.Y);
		GD.Print(path);
		path_updated = true;
	}
	public void MoveTowardsPos(Vector2 Position)
	{
		finish_position = new Vector2I(Mathf.RoundToInt(Position.X/cellsize),//->
		Mathf.RoundToInt(Position.Y/cellsize)); //<-

		ForceReCalculatePath();
		if (!QueueCoroutineDeletion)
		{
			QueueCoroutineCreation();	
		}
	}

	private async void QueueCoroutineCreation()
	{
		QueueCoroutineDeletion = true;
		while (CoroutineAmount != 0)
		{
			GD.Print("Coroutines not deleted");
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		QueueCoroutineDeletion = false;
		if (path != null)
		{
			WalkAlongNodes(path);	
		}
	}

	public async void WalkAlongNodes(List<PathNode> nodes){
		path_updated = false;
		CoroutineAmount++;
		int CoroutineNum = CoroutineAmount;
		Sprite.Play("Walking");

		for (int i = 0; i < nodes.Count; i++){
			if (QueueCoroutineDeletion)
			{
				goto end;
			}
			Godot.Vector2 Velocity = Godot.Vector2.Zero;
			Godot.Vector2 cell_positon = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            Godot.Vector2 cell_positon2 = new Godot.Vector2(nodes[i].x * cellsize, nodes[i].y * cellsize) + new Godot.Vector2(8,8);
            float distance = GlobalPosition.DistanceTo(cell_positon);
            float distance2 = GlobalPosition.DistanceTo(cell_positon2);

			if(GlobalPosition.X-cell_positon.X > 0)
			{
				Sprite.FlipH = true;
			}
			else if(GlobalPosition.X-cell_positon.X < 0)
			{
				Sprite.FlipH = false;
			}

            if (distance > distance2){continue;}
			if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){continue;}
			//
            while ((Mathf.Abs(GlobalPosition.X - cell_positon.X) > 1 || Mathf.Abs(GlobalPosition.Y - cell_positon.Y) > 1) && !path_updated)
            {
				if (QueueCoroutineDeletion)
				{
					goto end;
				}
                if (distance > distance2) { break; }
                //if (Mathf.Abs(GlobalPosition.X - cell_positon2.X) < cellsize && Mathf.Abs(GlobalPosition.Y - cell_positon2.Y) < cellsize){break;}
                Velocity = GlobalPosition.DirectionTo(cell_positon) * Speed * (float)GetPhysicsProcessDeltaTime();
				GlobalVelocity = Velocity;

				distance = GlobalPosition.DistanceTo(cell_positon);
				distance2 = GlobalPosition.DistanceTo(cell_positon2);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
		GlobalVelocity = Vector2.Zero;
		//BrokenTotem.DrawList.Remove(new Vector2I(nodes[nodes.Count].x,nodes[nodes.Count].y));
		end:
		CoroutineAmount--;
		Sprite.Play("default");
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	private void PickingUpMaterial(Node2D material)
	{
		if (material is Material script && CarriedMaterial == "")
		{
			if (script.PickedUp != true)
			{
				script.PickedUp = true;
				MaterialSprite.Visible = true;
				MaterialSprite.Texture = material.GetNode<Sprite2D>("Sprite2D").Texture;
				CarriedMaterial = (material as Material).MaterialType;
				material.QueueFree();	
			}
		}
		if (material is BrokenTotem && CarriedMaterial != "")
		{
			if (CarriedMaterial == "Wood")
			{
				(material as BrokenTotem).DepositTree();
			}
			MaterialSprite.Visible = false;
			CarriedMaterial = "";
		}
	}
	private void CollidedWithFriendly(Node2D body)
	{
		FriendsInside++;
		Colliding = true;
		if (body.GetParent() is FriendlyLizardScript script)
		{
			if (script.GlobalVelocity == Vector2.Zero)
			{
				CollidedWithFriendly2(body);
			}
			else if (script.RunningAway == false)
			{
				RunningAway = true;
				ListOfFriends.Add(body);		
			}
		}
	}
	private void ExitingFriendly(Node2D body)
	{
		ListOfFriends.Remove(body);
		FriendsInside--;
		if (FriendsInside <= 0)
		{
			RunningAway = false;
			Colliding = false;	
		}
	}
	private async void CollidedWithFriendly2(Node2D body)
	{
		while (CoroutineAmount != 0)
		{
			QueueCoroutineDeletion = true;	
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		GlobalVelocity = Vector2.Zero;
		Sprite.Play("default");
		QueueCoroutineDeletion = false;
	}
}
