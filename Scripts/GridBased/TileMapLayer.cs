using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Godot;
using static System.Net.Mime.MediaTypeNames;
using static Godot.GD;

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

    public static bool HoveringOnSumShit = false;


    private List<Vector2I> BuildQueue = new List<Vector2I>();
    private bool Escaping;

    [Signal]
    public delegate void CustomTileChangedEventHandler();
    //Towers
    private bool ButtonPressed;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TowerContainer = GetTree().Root.GetChild(1).GetNode<TextureRect>("%TowerContainer");
        TileContainer = GetTree().Root.GetChild(1).GetNode<TextureRect>("%TileContainer");
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


        MoneyNum = GetTree().Root.GetChild(1).GetNode<Label>("%MoneyNum");
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
                    Vector2I obj = new Vector2I(Mathf.FloorToInt(eventMouseButton.Position.X / 16), Mathf.FloorToInt(eventMouseButton.Position.Y / 16));
                    BuildQueue.Add(obj);
                }
                //Buildings
                if (PlaceBuildings)
                {
                    if (money >= BuildScript.cost)
                    {
                        CreateBuilding(eventMouseButton.Position);
                    }
                }
            }
            if (!mouse_down && LevelHandler.RoundOver && !ButtonPressed && ((int)eventMouseButton.ButtonIndex) == 1)
            {
                ProcessBuildQueue();
                Escaping = false;
            }

            // bing bang boom
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

        //
        if (@event is InputEventMouseMotion eventMouseMotion)
		{
            TileSignifier.GlobalPosition = ((Vector2I)eventMouseMotion.Position / cellsize) * cellsize;

            if (mouse_down && PlaceTiles && !Escaping)
            {
                Vector2I obj = new Vector2I(Mathf.FloorToInt(eventMouseMotion.Position.X / 16), Mathf.FloorToInt(eventMouseMotion.Position.Y / 16));
                if (!BuildQueue.Contains(obj))
                {
                    BuildQueue.Add(obj);
                }
                else
                {
                    if (BuildQueue[^1] != obj)
                    {
                        BuildQueue.RemoveRange(BuildQueue.IndexOf(obj), BuildQueue.Count - BuildQueue.IndexOf(obj));
                    }
                }
                int index = BuildQueue.IndexOf(obj);
                if (!CheckIfFuckerGotNeighbours(index))
                {
                    //ConnectPaths(index);
                }
                QueueRedraw();
            }
            else
            {
                BuildQueue.Clear();
                QueueRedraw();
            }
        }
        ButtonPressed = false;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Exit"))
        {
            Escaping = true;
        }

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
        foreach (Vector2I tile in BuildQueue)
        {
            DrawTexture(TileSignifier.Texture,tile*16,Color.Color8(255,255,255,150));
        }
    }


    private async void CreateTile(Godot.Vector2 GlobalPosition, int sourceId)
    {
        if (HoveringOnSumShit) { return; }
        Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
        Vector2I TilePos = LocalToMap(LocalPos);

        Tile_node Tile = grid.GetGridObject(TilePos.X, TilePos.Y);
        if (GetCellSourceId(TilePos) == 2) {SetCell(TilePos,-1);}//shrubs
        else if (GetCellSourceId(TilePos) != -1) { return; }

        if (!Tile.occupied)
        {
            NewParticles(Particles2D);

            money -= TileScript.cost;
            MoneyNum.Text = money.ToString();
            SetCell(TilePos, sourceId, TileScript.AtlasCoordinates / cellsize, 0);
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
    private async void CreateTileI(Godot.Vector2I TilePos, int sourceId)
    {
        if (HoveringOnSumShit) { return; }


        Tile_node Tile = grid.GetGridObject(TilePos.X, TilePos.Y);

        if (GetCellSourceId(TilePos) == 2) {SetCell(TilePos,-1);}//shrubs
        else if (GetCellSourceId(TilePos) != -1) { return; }
        if (!Tile.occupied)
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
        if (HoveringOnSumShit){return;}
        Godot.Vector2I Position = ((Vector2I)GlobalPosition / cellsize) * cellsize;
        Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
        Vector2I TilePos = LocalToMap(LocalPos);
        Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
        //if (grid.GetGridObject(Position.X,Position.Y))
        if (GetCellSourceId(TilePos) == 2){SetCell(TilePos,-1);}//shrubage

        if ((GetCellSourceId(TilePos) == -1 && !Tile.occupied) || (!Tile.occupied && (bool)GetCellTileData(TilePos).GetCustomData("Buildable")))
        {
            Particles2D.Restart();

			money -= BuildScript.cost;
			GD.Print(BuildScript.cost);
			MoneyNum.Text = money.ToString();

			Node2D Instance = BuildScript.building.Instantiate<Node2D>();
			AddChild(Instance);
            Instance.GlobalPosition = new Godot.Vector2(Position.X + cellsize / 2, Position.Y + cellsize / 2);
            Tile.occupied = true;
            GD.Print(Tile.occupied);
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
        NewParticle.Connect("finished", new Callable(NewParticle, nameof(QueueFree)));
    }

    private void ProcessBuildQueue()
    {
        if (!PlaceTiles) { return; }

        if (money >= TileScript.cost * BuildQueue.Count)
        {
            foreach (Vector2I TilePos in BuildQueue)
            {
                CreateTileI(TilePos, TileScript.SourceId);
            }
        }
    }

    private bool CheckIfFuckerGotNeighbours(int index)
    {
        if (index < 2){ return true; }
        return Mathf.Abs(BuildQueue[index - 1].X - BuildQueue[index].X) <= 1 && Mathf.Abs(BuildQueue[index - 1].Y - BuildQueue[index].Y) <= 1;
    }

    private void ConnectPaths(int index)
    {
        Godot.Vector2 Direction = new Godot.Vector2(BuildQueue[index].X,BuildQueue[index].Y).DirectionTo(BuildQueue[index - 1]);

        int DistX = Mathf.Abs(BuildQueue[index].X - BuildQueue[index - 1].X);
        int DistY = Mathf.Abs(BuildQueue[index].Y - BuildQueue[index - 1].Y);

        int stepX = Math.Sign(BuildQueue[index - 1].X - BuildQueue[index].X);
        int stepY = Math.Sign(BuildQueue[index - 1].Y - BuildQueue[index].Y);

        List<Vector2I> TempList = new List<Vector2I>();
        if (DistX == DistY)
        {
            for (int i = 1; i < DistX; i++)
            {
                TempList.Add(new Vector2I(
                    BuildQueue[index].X + (int)(i*Direction.X),
                    BuildQueue[index].Y + (int)(i*Direction.Y)));
            }
        }
        else if (DistX > DistY)
        {
            for (int x = 1; x < DistX; x++)
            {
                float t = (float)x / DistX;
                TempList.Add(new Vector2I(
                    BuildQueue[index].X + x * stepX,
                    BuildQueue[index].Y + Mathf.FloorToInt(DistY * t) * stepY
                ));
            }
        }
        else
        {
            for (int y = 1; y < DistY; y++)
            {
                float t = (float)y / DistY;
                TempList.Add(new Vector2I(
                    BuildQueue[index].X + Mathf.FloorToInt(DistX * t) * stepX,
                    BuildQueue[index].Y + y * stepY
                ));
            }
        }

        BuildQueue.InsertRange(index, TempList);
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
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
   ⣀⣤⣤⣶⣿⠿⣿⣿⣿⣿⣿⣿⣙⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠀⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⡻⣻⠿⠿⡿⣦⢸⣷⣤⡞⢿
*/