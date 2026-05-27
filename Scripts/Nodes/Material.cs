using Godot;
using System;

public partial class Material : Area2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public string MaterialType;
	public bool PickedUp = false;
	private float InterpolationTime;
	private float CurrentTime;

	public async void InterpolateToPosition(Vector2 Position, Sprite2D SpriteReference=null)
	{
		float distance = GlobalPosition.DistanceTo(Position);
		while (distance > 1)
		{
			float delta = (float)GetProcessDeltaTime();
			CurrentTime += delta;
			distance = GlobalPosition.DistanceTo(Position);
			float speed = distance/InterpolationTime-CurrentTime;	
			GlobalPosition += speed * GlobalPosition.DirectionTo(Position) * delta;
		}
		GlobalPosition = Position;
		if (SpriteReference != null)
		{
			SpriteReference.Visible = true;	
		}
	}
}
