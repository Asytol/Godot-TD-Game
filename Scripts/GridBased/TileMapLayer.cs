using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Godot;
using static System.Net.Mime.MediaTypeNames;

public partial class TileMapLayer : Godot.TileMapLayer
{
	//Building and ui

	//local saved tiles 
	public static bool PlaceTiles = true;
	public static bool PlaceBuildings = false;
	[Export] public Terrain_tile_button TileScript;
	[Export] public Build_tile BuildScript;
	private bool hidden = false;
	//
    private TextureRect TowerContainer;
    private TextureRect TileContainer;

    private string dir_path = "res://Scenes/Terrain_tiles/";
	//Grids and cells
	private bool mouse_down = false;

	private static Grid_class<Tile_node> grid;
	public const int width = 72;
	public const int height = 41;
	private const int cellsize = 16;
	public static int money = 0;
	public static Label MoneyNum;

    private Sprite2D TileSignifier;
    private GpuParticles2D Particles2D;

    [Signal]
    public delegate void CustomTileChangedEventHandler();
    //Towers
    private bool ButtonPressed;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TowerContainer = GetNode<TextureRect>("%TowerContainer");
        TileContainer = GetNode<TextureRect>("%TileContainer");
        //Get all TextureButton signals
        if (TileSignifier == null) { TileSignifier = GetNode<Sprite2D>("%TileSignifier"); }
        Particles2D = TileSignifier.GetChild<GpuParticles2D>(1);

        foreach (MarginContainer container in TileContainer.GetChild<VBoxContainer>(0).GetChildren())
        {
            TextureButton child = container.GetChild<TextureButton>(0);
            child.Connect("SendTileInfo",new Callable(this, nameof(ChangeCurrentTile)));
        }
        foreach (MarginContainer container in TowerContainer.GetChild<VBoxContainer>(0).GetChildren())
        {
			TextureButton child = container.GetChild<TextureButton>(0);
            child.Connect("SendBuildInfo", new Callable(this, nameof(ChangeCurrentBuilding)));
            ChangeCurrentBuilding(child);
        }


        MoneyNum = GetNode<Label>("%MoneyNum");
		grid = new Grid_class<Tile_node>(width,height,cellsize,new Godot.Vector2(0,0), (Grid_class<Tile_node> g, int x, int y) => new Tile_node(x:x,y:y));

		foreach (Vector2I cell in GetUsedCells()){
			Tile_node Tile = grid.GetGridObject(cell.X,cell.Y);
			if (Tile != null)
			{
				Tile.health = (float)GetCellTileData(cell).GetCustomData("health");
				Tile.breakable = (bool)GetCellTileData(cell).GetCustomData("breakable");
			}
		}

		//Ui and building
		Vector2I position = new Vector2I(cellsize,cellsize);
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton)
		{
			mouse_down = !mouse_down;
			if (mouse_down && LevelHandler.RoundOver && !ButtonPressed && ((int)eventMouseButton.ButtonIndex) == 1)
			{
				if (PlaceTiles)
				{
					if (money >= TileScript.cost)
					{
						Godot.Vector2 MousePos = eventMouseButton.Position;
						CreateTile(MousePos,TileScript.SourceId);
					}
                }
                else if (PlaceBuildings)
				{
					if (money >= BuildScript.cost)
					{
						CreateBuilding(eventMouseButton.Position);
					}
				}
            }
            if (mouse_down && LevelHandler.RoundOver && !ButtonPressed && ((int)eventMouseButton.ButtonIndex) == 2)
            {
                if (PlaceTiles)
                {
                	Godot.Vector2 MousePos = eventMouseButton.Position;
                    SellTile(MousePos);
                }
                else if (PlaceBuildings)
                {
                	Godot.Vector2 MousePos = eventMouseButton.Position;
                    SellBuilding(MousePos);
                }
            }
        }
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			TileSignifier.GlobalPosition = ((Vector2I)eventMouseMotion.Position/cellsize)*cellsize;
        }
        ButtonPressed = false;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (LevelHandler.RoundOver)
		{
			if (hidden)
			{
				hidden = false;
				TileSignifier.Visible = true;
                TowerContainer.Visible = true;
                TileContainer.Visible = true;
            }
		}
		else
		{
			if (!hidden)
			{
				hidden = true;
                TileSignifier.Visible = false;
                TowerContainer.Visible = false;
                TileContainer.Visible = false;
            }
		}
    }
    public override void _Draw()
    {
        //DrawCircle(TileSignifier.GlobalPosition,);
    }


    private void CreateTile(Godot.Vector2 GlobalPosition, int sourceId){
		Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
		Vector2I TilePos = LocalToMap(LocalPos);

		Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
        if (GetCellSourceId(TilePos) == -1 && !Tile.occupied)
        {
            NewParticles(Particles2D);

			money -= TileScript.cost;
			MoneyNum.Text = money.ToString();
			SetCell(TilePos,sourceId,TileScript.AtlasCoordinates/cellsize,0);
			if (Tile != null)
			{
				Tile.health = (float)GetCellTileData(TilePos).GetCustomData("health");
				Tile.breakable = (bool)GetCellTileData(TilePos).GetCustomData("breakable");
            }
            Tile.occupied = true;
            Tile.IndentedMoney = Mathf.RoundToInt(TileScript.cost * 0.75f);
            EmitSignal("CustomTileChanged");
            //UpdateTerrain(TilePos,3);
        }
	}
	private void CreateBuilding(Godot.Vector2 GlobalPosition)
    {
        Godot.Vector2I Position = ((Vector2I)GlobalPosition / cellsize) * cellsize;
        Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
        Vector2I TilePos = LocalToMap(LocalPos);
        Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
        //if (grid.GetGridObject(Position.X,Position.Y))
        if (GetCellSourceId(TilePos) == -1 || (!Tile.occupied && (bool)GetCellTileData(TilePos).GetCustomData("Buildable")))
        {
            Particles2D.Restart();

			money -= BuildScript.cost;
			GD.Print(BuildScript.cost);
			MoneyNum.Text = money.ToString();

			Node2D Instance = BuildScript.building.Instantiate<Node2D>();
			AddChild(Instance);
            Instance.GlobalPosition = new Godot.Vector2(Position.X + cellsize / 2, Position.Y + cellsize / 2);
            Tile.occupied = true;
            Tile.BuildingPointer = (Area2D)Instance;
            Tile.IndentedMoney = Mathf.RoundToInt(BuildScript.cost * 0.75f);
        }
	}

	private void UpdateTerrain(Vector2I MidPoint,int size)
	{ //Tried doing terrain here but it didn't work altoo well, my bad gng
		Godot.Collections.Array<Vector2I> array = []; 
		array.Resize(size*size);

		int total = 0;
		for (int x = -1; x < size-1; x++){
			for (int y = -1; x < size-1; x++){
				array[total] = new Vector2I(MidPoint.X - x,MidPoint.Y - y);
				total++;
			}
		}
		int TerrainSet = GetCellTileData(MidPoint).TerrainSet;
		int Terrain = GetCellTileData(MidPoint).Terrain;
		SetCellsTerrainConnect(array,TerrainSet,Terrain);
	}

	public bool Damage_tile(Godot.Vector2 GlobalPosition,float damage)
	{
		Godot.Vector2 LocalPos = this.ToLocal(GlobalPosition);
		Vector2I TilePos = this.LocalToMap(LocalPos);
		Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
		Tile.health -= damage;

		if (Tile.health < 0 && Tile.breakable == true){
            SetCell(TilePos, -1, Godot.Vector2I.Zero, -1);
            //EmitSignal("CustomTileChanged");
            Tile.occupied = false;
            Particles2D.Restart();
            return true;
        }
		return false;
	}
	public bool Damage_tileI(Godot.Vector2I TilePos,float damage)
	{
		Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
		Tile.health -= damage;

		if (Tile.health < 0 && Tile.breakable){
            SetCell(TilePos, -1, Godot.Vector2I.Zero, -1);
            //EmitSignal("CustomTileChanged");
            Tile.occupied = false;
            Particles2D.Restart();
            return true;
		}
		return false;
	}

	public void Heal_tile(Godot.Vector2 GlobalPosition,float health)
	{
		Godot.Vector2 LocalPos = this.ToLocal(GlobalPosition);
		Vector2I TilePos = this.LocalToMap(LocalPos);

		Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
		Tile.health += health;
	}


	public bool Area_damageI(Godot.Vector2I TilePos, float damage)
	{
		float raw_dmg = damage;
        //Left
        for (int x = -1; x < 2; x++){
            for (int y = -1; y < 2; y++){
                if (x == 0 && y == 0) { continue; }
                Tile_node Tile = grid.GetGridObject(TilePos.X + x,TilePos.Y + y);
				Tile.health -= damage;
				if (Tile.health < 0 && Tile.breakable){
                    SetCell(new Vector2I(TilePos.X + x, TilePos.Y + y), -1, Godot.Vector2I.Zero, -1);
                    //EmitSignal("CustomTileChanged");
                    Particles2D.Restart();
                    Tile.occupied = false;
                }
            }
        }

		return Damage_tileI(TilePos, raw_dmg);
    }

    private void SellTile(Godot.Vector2 GlobalPosition)
    {
		Godot.Vector2 LocalPos = this.ToLocal(GlobalPosition);
        Vector2I TilePos = this.LocalToMap(LocalPos);

        if (GetCellSourceId(TilePos) != -1)
        {
            Particles2D.Restart();
        	money += grid.GetGridObject(TilePos.X,TilePos.Y).IndentedMoney;
            Damage_tileI(TilePos, int.MaxValue);
            MoneyNum.Text = money.ToString();
        }
    }
    private void SellBuilding(Godot.Vector2 GlobalPosition)
    {
		Godot.Vector2 LocalPos = this.ToLocal(GlobalPosition);
        Vector2I TilePos = this.LocalToMap(LocalPos);

        if (grid.GetGridObject(TilePos.X, TilePos.Y).BuildingPointer == null) { return; }
        Particles2D.Restart();
        money += grid.GetGridObject(TilePos.X,TilePos.Y).IndentedMoney;
        grid.GetGridObject(TilePos.X, TilePos.Y).BuildingPointer.QueueFree();
        MoneyNum.Text = money.ToString();
    }


    //Tiles
    private void ChangeCurrentBuilding(TextureButton button)
	{
		PlaceTiles = false;
		PlaceBuildings = true;
        BuildScript = button as Build_tile;

        Node2D temp = BuildScript.building.Instantiate<Node2D>();
        TowerBase BuildingScript = temp as TowerBase;

        Sprite2D Sprite = TileSignifier.GetChild<Sprite2D>(0);
        Sprite.Scale = new Godot.Vector2(BuildingScript.base_range, BuildingScript.base_range) / new Godot.Vector2(64,64);



        TileSignifier.Texture = button.TextureNormal;
        ButtonPressed = true;
    }

    private void ChangeCurrentTile(TextureButton button)
    {
        PlaceTiles = true;
        PlaceBuildings = false;
        TileScript = button as Terrain_tile_button;
        TileSignifier.GetChild<Sprite2D>(0).Visible = false;

        TileSignifier.Texture = button.TextureNormal;
        ButtonPressed = true;
    }

    private void NewParticles(GpuParticles2D GPUParticles2D)
	{
		Node node = GPUParticles2D.Duplicate();
        GpuParticles2D NewParticle = (GpuParticles2D)node;
        NewParticle.GlobalPosition = GPUParticles2D.GlobalPosition;
		AddChild(NewParticle);
		NewParticle.Restart();
		NewParticle.Emitting = true;
		NewParticle.Connect("finished",new Callable(NewParticle, nameof(QueueFree)));
	}
}


/*
⠀⠀⠀⠀⠀⠀⠀⠀⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣷⣿⣿⣿⣿⣿⠏⢀⣴⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⠀⠀⠀⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿⣿⡿⢁⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⠀⠀⠀⠀⠀⠀⢠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠉⠉⠀⠈⠉⠉⠉⠉⠙⠻⠿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⠀⠀⠀⠀⠀⠀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠿⠿⣿⣿⣿⣿⣿⣿
⠀⠀⠀⠀⠀⢠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⢡⣴⣶⣦⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠻⣿⣿⣿⣿
⠀⠀⠀⠀⠀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⢋⣠⣶⣿⣿⡟⡛⣛⣿⣦⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣿⣿
⠀⠀⠀⠀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⠀⠘⠿⠿⠿⢿⣿⣿⣿⣿⣿⣿⣶⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿
⠀⠀⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠟⠛⠛⠁⠀⢛⡠⠀⠀⠀⢀⠈⢿⣿⣿⣿⠛⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿
⠀⠀⠀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⢁⣤⣶⣿⣿⣿⣿⣿⣿⣥⡄⠀⠙⠂⠀⠀⢻⡟⢉⣺⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⣄⣈⠉⠛⠛⠛⠛⠟⠁⣿⣿⢶⣶⣦⣄⣀⣈⣿⣿⣬⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠭⣿⣿⣷⣤⣤⣶⣤⡿⠻⢷⣦⣤⣉⣻⣿⣶⣿⣿⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣿⢿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠰⣶⣶⣄⡀⠉⠻⢿⣿⣿⣭⡅⠀⠐⠛⠉⠉⠉⠁⠀⠈⠙⠂⢠⡄⠀⠀⠀⠀⠀⠀⠀⠀⢀⡿⢋⣼
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣄⡉⠉⠉⠙⠓⠶⠾⢿⣿⣿⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣈⣤⣤⡦⠈⠀⢀⣠⡴⡴⠋⣠⣿⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠈⠂⠄⠀⠀⠀⠀⠀⠙⠛⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢁⣠⣴⣿⡿⢋⠔⣡⣾⣿⣿⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⣿⡿⢋⠔⣡⣾⣿⣿⣿⣿⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏⣿⣿⣿⣿⡐⠆⣠⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⣠⣄⠀⣿⠏⠀⢁⣾⣿⣿⣿⣿⣿⣿⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠁⠿⠛⠻⠋⢀⣼⠋⢠⣶⡆⠀⠀⠀⠀⠀⠀⠀⠀⠓⠸⠧⢼⡿⠀⠋⠀⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿
⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⡿⠟⢁⣴⠀⣠⣴⣶⡿⠃⠀⣿⣿⣿⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣴⣶⣿⣷⣄⠻⢿⣿⣿⣿⣿⣿⣿⣿
⠀⠀⢸⣿⣿⣿⣟⡿⠟⠋⣀⣴⣿⠏⢸⣿⣿⡏⠀⣠⣾⣿⣿⣿⣿⣿⣶⣦⣤⣤⠀⠀⣀⣴⣾⣿⡿⠟⠋⠉⣭⡙⢶⣌⠛⢡⣼⢿⣿⣿
⠀⠀⢸⣿⣿⡿⣿⡇⢠⣶⣿⡿⠉⠀⣼⣿⢿⡀⢾⣿⣿⣿⣿⣿⡿⠿⢿⡿⠟⣋⣴⣾⡿⠟⣉⣀⣀⣤⣷⡇⠘⣿⢦⠍⠳⣤⠁⠸⠯⢍
⠀⠀⠸⠯⠿⣷⣾⡇⠈⠛⠋⡄⢀⣾⣿⣿⣿⣷⣄⠋⣿⠟⢛⣩⠴⠂⣡⣴⣿⣿⠟⠁⣠⡾⣿⣿⣿⣿⣿⣷⡀⣿⡄⣆⠰⠎⣷⣄⠀⣸
⠀⠀⠀⠀⠀⠀⠉⠀⠀⠀⠉⠀⣸⣿⣿⣿⣿⣿⣿⣷⣦⣤⣭⣴⣾⣿⣿⡿⠋⠠⣶⣶⠚⣓⣀⣿⣿⣿⣿⣿⣧⡘⢿⣿⡇⢦⠜⢿⡆⠫
⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏⠀⠐⠿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣄⠱⢸⣦⠈⣿⣦
⠀⠀⠀⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
*/