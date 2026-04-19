using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

public partial class LevelHandler : Node
{
	[Export] WavePreset[] Waves;
	public static int CurrentWave;
	[Export] public int MaxWaves = 4;
	public static bool RoundOver = true;
	public static int EnemiesAlive = 0;
	//[Export] private WavePreset[] WavePresets;

	[Signal]
	public delegate void StartGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		GetNode<Button>("%Start").ButtonUp += OnStart;
		//if/or is sadly faster than reflection ):
		TileMapLayer.money += Waves[0].WaveMoney;
		TileMapLayer.MoneyNum.Text = Waves[0].WaveMoney.ToString();

		RoundOver = true;
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
	public void OnEnemyDeath()
	{
		EnemiesAlive--;
		if (EnemiesAlive == 0)
		{
			RoundOver = true;
            TileMapLayer.money += Waves[CurrentWave].WaveMoney;
            TileMapLayer.MoneyNum.Text = TileMapLayer.money.ToString();
        }
	}

	private void OnStart()
	{
		if (RoundOver && CurrentWave <= Waves.Length)
		{
			foreach (SpawnPreset preset in Waves[CurrentWave].Spawns)
			{
				EnemiesAlive += preset.amount;
				Node2D SpawnNode = GetNode<Node2D>($"%{preset.SpawnerName}");
				Spawner script = SpawnNode as Spawner;
				script.StartSpawn(preset);
			}
			RoundOver = false;
			CurrentWave++;
		}
	}


	//Getting nodes
}
