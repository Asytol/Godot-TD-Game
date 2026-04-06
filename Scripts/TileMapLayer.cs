using Godot;
using System.Numerics;
using System;
using System.ComponentModel;

public partial class TileMapLayer : Godot.TileMapLayer
{
	[Export] public bool Place_tiles = false;
	[Export] public Vector2I current_atlas = new Vector2I(0,0);


	private bool mouse_down = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	

		GD.Print(this.GetUsedCells());
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton)
		{
			mouse_down = !mouse_down;
			if (Place_tiles && mouse_down)
			{
				Godot.Vector2 MousePos = eventMouseButton.Position;
				Godot.Vector2 LocalPos = this.ToLocal(MousePos);
				Vector2I TilePos = this.LocalToMap(LocalPos);

				this.SetCell(TilePos, sourceId:0,current_atlas,0);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
