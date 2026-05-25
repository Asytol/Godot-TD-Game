using Godot;
using System;

public partial class Material : Area2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public string MaterialType;
	public bool PickedUp = false;
}
