using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Godot;

public partial class TileMapLayer : Godot.TileMapLayer
{
	//Building and ui
	[Export] private TextureRect terrain_texture;

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

	private Grid_class<Tile_node> grid;
	[Export] public int width = 72;
	[Export] public int height = 41;
	private const int cellsize = 16;
	public static int money = 0;
	public Label MoneyNum;

	private Sprite2D TileSignifier;


	//Towers
	TextureButton Ballista;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        TowerContainer = GetNode<TextureRect>("%TowerContainer");
        TileContainer = GetNode<TextureRect>("%TileContainer");
        //Get all TextureButton signals
        if (TileSignifier == null){TileSignifier = GetNode<Sprite2D>("%TileSignifier");}

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
        //GetNode<TextureButton>("%Ballista").


        MoneyNum = GetNode<Label>("%MoneyNum");
		grid = new Grid_class<Tile_node>(width,height,cellsize,new Godot.Vector2(0,0), (Grid_class<Tile_node> g, int x, int y) => new Tile_node(g,x,y));

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
			if (mouse_down && LevelHandler.RoundOver)
			{
				if (PlaceTiles)
				{
					if (money >= TileScript.cost)
					{
						Godot.Vector2 MousePos = eventMouseButton.Position;
						CreateTile(MousePos,TileScript.SourceId);
					}
                }
                if (PlaceBuildings)
				{
					if (money >= BuildScript.cost)
					{
						CreateBuilding(eventMouseButton.Position);
					}
				}
            }
		}
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			TileSignifier.GlobalPosition = ((Vector2I)eventMouseMotion.Position/cellsize)*cellsize;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (LevelHandler.RoundOver)
		{
			if (hidden)
			{
				hidden = false;
				terrain_texture.Visible = true;
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
                terrain_texture.Visible = false;
                TileSignifier.Visible = false;
                TowerContainer.Visible = false;
                TileContainer.Visible = false;
            }
		}
	}

	private void CreateTile(Godot.Vector2 GlobalPosition, int sourceId){
		Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
		Vector2I TilePos = LocalToMap(LocalPos);

		if (GetCellSourceId(TilePos) == -1)
		{
			money -= TileScript.cost;
			MoneyNum.Text = money.ToString();
			SetCell(TilePos,sourceId,TileScript.AtlasCoordinates/cellsize,0);
			Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
			if (Tile != null)
			{
				Tile.health = (float)GetCellTileData(TilePos).GetCustomData("health");
				Tile.breakable = (bool)GetCellTileData(TilePos).GetCustomData("breakable");
			}
			//UpdateTerrain(TilePos,3);
		}
	}
	private void CreateBuilding(Godot.Vector2 GlobalPosition)
	{
		money -= BuildScript.cost;
		GD.Print(BuildScript.cost);
		MoneyNum.Text = money.ToString();
		Godot.Vector2I Position = ((Vector2I)GlobalPosition/cellsize)*cellsize;

		Node2D Instance = BuildScript.building.Instantiate<Node2D>();
		AddChild(Instance);
		Instance.GlobalPosition = new Godot.Vector2(Position.X + cellsize/2, Position.Y + cellsize/2);
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
		Tile.health -= damage * (float)GetPhysicsProcessDeltaTime();

		if (Tile.health < 0 && Tile.breakable == true){
			SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);
			return true;
		}
		return false;
	}
	public bool Damage_tileI(Godot.Vector2I TilePos,float damage)
	{
		Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
		Tile.health -= damage;

		if (Tile.health < 0 && Tile.breakable == true){
			SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);
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
		for (int i = -1; i < 2; i++){
			//left
			Tile_node Tile = grid.GetGridObject(TilePos.X-1,TilePos.Y+i);
			Tile.health -= damage;
			if (Tile.health < 0 && Tile.breakable == true){
				SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);}
			//less left
			Tile = grid.GetGridObject(TilePos.X+1,TilePos.Y+i);
			Tile.health -= damage;
			if (Tile.health < 0 && Tile.breakable == true){
				SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);}
		}
		//up and down
		Tile_node Tile_2 = grid.GetGridObject(TilePos.X,TilePos.Y+1);
		Tile_2.health -= damage;
		if (Tile_2.health < 0 && Tile_2.breakable == true){
			SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);}

		Tile_2 = grid.GetGridObject(TilePos.X,TilePos.Y-1);
		Tile_2.health -= damage;
		if (Tile_2.health < 0 && Tile_2.breakable == true){
			SetCell(TilePos,-1,Godot.Vector2I.Zero,-1);}

		return Damage_tileI(TilePos, raw_dmg);
	}


	//Tiles
	private void ChangeCurrentBuilding(TextureButton button)
	{
		PlaceTiles = false;
		PlaceBuildings = true;
		BuildScript = button as Build_tile;

		TileSignifier.Texture = button.TextureNormal;
    }

    private void ChangeCurrentTile(TextureButton button)
    {
        PlaceTiles = true;
        PlaceBuildings = false;
        TileScript = button as Terrain_tile_button;

        TileSignifier.Texture = button.TextureNormal;
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
*/