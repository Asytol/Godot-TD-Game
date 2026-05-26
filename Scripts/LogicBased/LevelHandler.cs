using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

public partial class LevelHandler : Node
{
	[Export] WavePreset[] Waves;
	public static int CurrentWave;
	public static bool RoundOver = true;
	public static int EnemiesAlive = 0;
    //[Export] private WavePreset[] WavePresets;

    private TextureRect CutSceneHandler;
	private Label RoundText;

	private bool Rebirthed = false;
	private Node2D OriginalSpawner;


	[Export] private Node2D Totem;

	private Control UiNodes;

    [Signal]
	public delegate void StartGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UiNodes = GetTree().Root.GetChild(1).GetNode<Control>("%Ui");

		CurrentWave = 0;	
        UiNodes.GetNode<Button>("%Start").ButtonUp += OnStart;
        GD.Print("got start");
        RoundText = UiNodes.GetNode<Label>("%RoundText");
        //if/or is sadly faster than reflection ):
        TileMapLayer.money += Waves[0].WaveMoney;
		TileMapLayer.MoneyNum.Text = Waves[0].WaveMoney.ToString();

		OriginalSpawner = GetNode<Node2D>("%Spawner");
        foreach (string SpawnerName in Waves[0].UsedSpawners)
        {
            Node2D SpawnNode = GetNodeOrNull<Node2D>($"%{SpawnerName}");
            if (SpawnNode == null) {continue;}
            Spawner script = SpawnNode as Spawner;
            script.DrawPath = true;
			script.QueueRedraw();
        }

        RoundOver = true;

		CutSceneHandler = GetNodeOrNull<TextureRect>("%Cutscene");
		if (CutSceneHandler != null)
		{
			CutSceneHandler.Connect("CutsceneFinished", new Callable(this, nameof(ResumeScene)));
			//Getting the DONT TOUCH node, yeah im touching it. But only i can, not anyone else.
			GetNode<Node2D>("%2DNodes").PropagateCall("set_process", [false]);
			GetNode<Node2D>("%2DNodes").Visible = false;
			GetNode<Control>("ControlNodes").Visible = false;
        }

        RoundText.Text = "Round" + "\n" + $"0/{Waves.Length}";

		if (Totem == null)
		{
			Totem = GetNode<Node2D>("%Finish");
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
		if (Rebirthed)
		{
			
		}
	}
	public void OnEnemyDeath(Vector2 Position)
	{
		EnemiesAlive--;
		Finish script = Totem as Finish;
		script.EmitSoul(Position);
		if (EnemiesAlive == 0)
        {
        	DisableAndEnableSpawners();
            RoundOver = true;
			if (CurrentWave < Waves.Length)
			{
				TileMapLayer.money += Waves[CurrentWave].WaveMoney;
			}
			else
			{
				UiNodes.GetNode<CenterContainer>("%WinMenu").Visible = true;	
			}
            TileMapLayer.MoneyNum.Text = TileMapLayer.money.ToString();
        }

	}

	private void OnStart()
	{
		if (RoundOver && CurrentWave < Waves.Length)
        {
            foreach (SpawnPreset preset in Waves[CurrentWave].Spawns)
			{
				EnemiesAlive += preset.amount;
                Node2D SpawnNode = GetNodeOrNull<Node2D>($"%{preset.SpawnerName}");
                if (SpawnNode == null) {continue;}
                Spawner script = SpawnNode as Spawner;
				script.StartSpawn(preset);
			}
			RoundOver = false;
            CurrentWave++;
            RoundText.Text = "Round" + "\n" + $"{CurrentWave}/{Waves.Length}";
        }
	}

	private void ResumeScene()
	{
		GetNode<Node2D>("%2DNodes").PropagateCall("set_process", [true]);
		GetNode<Node2D>("%2DNodes").Visible = true;
		GetNode<Control>("ControlNodes").Visible = true;
	}
    private void DisableAndEnableSpawners()
    {
        foreach (string SpawnerName in Waves[CurrentWave-1].UsedSpawners)
		{
			Node2D SpawnNode = GetNodeOrNull<Node2D>($"%{SpawnerName}");
			if (SpawnNode == null) {continue;}
			Spawner script = SpawnNode as Spawner;
            script.DrawPath = false;
            script.SomeoneIsFat = false;
        }
		if (CurrentWave < Waves.Length)
		{
			foreach (string SpawnerName in Waves[CurrentWave].UsedSpawners)
			{
				Node2D SpawnNode = GetNodeOrNull<Node2D>($"%{SpawnerName}");
				if (SpawnNode == null) {continue;}
				Spawner script = SpawnNode as Spawner;
				script.DrawPath = true;
				script.QueueRedraw();
			}
		}
		if (CurrentWave < Waves.Length)
		{
			foreach (SpawnPreset spawn in Waves[CurrentWave].Spawns)
			{
				if (CheckIfSomeoneIsFat(spawn.EnemyScene))
				{
					Node2D SpawnNode = GetNodeOrNull<Node2D>($"%{spawn.SpawnerName}");
					if (SpawnNode == null) {continue;}
					Spawner script = SpawnNode as Spawner;
					script.SomeoneIsFat = true;
					script.QueueRedraw();
					break;
				}
			}	
		}
    }
	private bool CheckIfSomeoneIsFat(PackedScene Enemy)
	{
		Node2D Instance = Enemy.Instantiate<Node2D>();
		return Instance is FatEnemy_base;
	}

	private void InstantiateCombinedSpawner()
	{
		SpawnPreset[] CombinedSpawns = Waves[CurrentWave - 1].Spawns;
	}

	private void GetUsedSpawners(SpawnPreset[] waves)
	{
		List<string> Spawners = new List<string>{waves[0].SpawnerName};

		for (int i = 1; i < waves.Length; i++)
		{
			if (!Spawners.Contains(waves[i].SpawnerName))
			{
				Spawners.Add(waves[i].SpawnerName);
			}
		}
	}
}