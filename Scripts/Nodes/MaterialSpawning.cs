using Godot;
using System;
using System.Security.Cryptography;

public partial class MaterialSpawning : Node2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] private Vector2 BoxExtents = new Vector2(16,16);
	[Export] private float SpawnTime;
	private float CurrentTime;
	[Export] private PackedScene MaterialScene;
	private Godot.RandomNumberGenerator RndNumber = new Godot.RandomNumberGenerator();


	[Export] Godot.TileMapLayer Tilemap;
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CurrentTime += (float)delta;
		int AmountOfRetries = 0;
		if (CurrentTime > SpawnTime)
		{
			start:
			float x = RndNumber.RandfRange(-BoxExtents.X/2,BoxExtents.X/2);
			float y = RndNumber.RandfRange(-BoxExtents.Y/2,BoxExtents.Y/2);
			Godot.Vector2 LocalPos = ToLocal(new Vector2(x,y));
        	Vector2I TilePos = Tilemap.LocalToMap(LocalPos);
			if (Tilemap.GetCellSourceId(TilePos) != -1)
			{
				if (AmountOfRetries <= 3)
				{
					AmountOfRetries += 1;
					goto start;	
				}
			}

			CurrentTime = 0;
			Node2D instance = MaterialScene.Instantiate<Node2D>();
			AddChild(instance);
			instance.Position = new Vector2(x,y);
		}
	}
}
