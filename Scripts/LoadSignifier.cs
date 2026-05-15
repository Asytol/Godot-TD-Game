using Godot;
using System;

public partial class LoadSignifier : Node
{
	// Called when the node enters the scene tree for the first time.

	[Signal]
	public delegate void FinishedLoadingEventHandler();
	public override void _Ready()
	{
		SceneTransitioner.FinishedLoading = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
