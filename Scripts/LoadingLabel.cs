using Godot;
using System;

public partial class LoadingLabel : RichTextLabel
{
	[Export] private float TimeBetweenDots = 1;
	private float CurrentTime = 0;
	private int CurrentDot;
	private int CharacterCount;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CharacterCount = GetTotalCharacterCount();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CurrentTime += (float)delta;
		if (CurrentTime > TimeBetweenDots)
		{
			if (CurrentDot == 3)
			{
				CurrentDot = 0;
			}
			else
			{
				CurrentDot++;
			}
			VisibleCharacters = CharacterCount - CurrentDot;
			CurrentTime = 0;
		}
	}
}
