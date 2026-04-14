using Godot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public partial class LevelHandler : Node
{
	private WavePreset[] ThisLevel;
	public int CurrentWave;
	[Export] public int MaxWaves = 4;
	//[Export] private WavePreset[] WavePresets;

	[Signal]
	public delegate void StartGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("%Start").ButtonUp += OnStart;
		//if/or is sadly faster than reflection ):
		if (Name.ToString() == "Main_test_scene")
		{
			ThisLevel = LevelDictionary.Main_test_scene;
		}
		else if (CantFindIsDigitFunction(Name.ToString()[-1]))
		{
			ThisLevel = LevelDictionary.Levels[Name.ToString()[-1]];
		}
	}
	private bool CantFindIsDigitFunction(char c)
	{
		for (int i = 0; i < 10; i++)
		{
			if (c == (char)i)
			{
				return true;
			}
		}
		return false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnStart()
	{
		foreach (SpawnPreset preset in ThisLevel[CurrentWave].SpawnPresets)
		{
			Node2D SpawnNode = GetNode<Node2D>($"%{preset.SpawnerName}");
			Spawner script = SpawnNode as Spawner;
			script.StartSpawn(preset);
		}
	}
}
