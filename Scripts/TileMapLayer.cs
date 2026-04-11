using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System;
using System.ComponentModel;
using System.IO;

public partial class TileMapLayer : Godot.TileMapLayer
{
	//Building and ui
	[Export] private TextureRect terrain_texture;

	//tiles 
	[Export] public bool Place_tiles = false;
	[Export] public Vector2I current_atlas = new Vector2I(0,0);
	[Export] public int current_SourceId = 0;

	private List<Build_tile> BuildTiles = new List<Build_tile>();
	private List<Sprite2D> TerrainTiles = new List<Sprite2D>();

	private string dir_path = "res://Scenes/Terrain_tiles/";
	//Grids and cells
	private bool mouse_down = false;

	private Grid_class<Tile_node> grid;
	[Export] public int width = 72;
	[Export] public int height = 41;
	private const int cellsize = 16;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	

		grid = new Grid_class<Tile_node>(width,height,cellsize,new Godot.Vector2(0,0), (Grid_class<Tile_node> g, int x, int y) => new Tile_node(g,x,y));

		foreach (Vector2I cell in GetUsedCells()){
			Tile_node Tile = grid.GetGridObject(cell.X,cell.Y);
			Tile.health = (float)GetCellTileData(cell).GetCustomData("health");
			Tile.breakable = (bool)GetCellTileData(cell).GetCustomData("breakable");	
		}

		//Ui and building
		Vector2I position = new Vector2I(cellsize,cellsize);
		bool selected = false; 
		foreach (Node node in GetChildren())
		{
			if (node is Sprite2D && node is Node2D)
			{
				Node2D nd = node as Node2D;
				nd.Position = position;
				position += new Vector2I(0,cellsize*2+4);
				TerrainTiles.Add(node as Sprite2D);

				if (!selected){nd.Modulate = Colors.White;selected = true;}
				else {nd.Modulate = Colors.DimGray;}
			}
		}
		if (terrain_texture == null){terrain_texture = GetNode<TextureRect>("TextureRect"); }
		terrain_texture.Size = new Godot.Vector2(32,36 * TerrainTiles.Count);
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton)
		{
			mouse_down = !mouse_down;
			if (Place_tiles && mouse_down)
			{
				bool tile_selected = false;
				foreach (Sprite2D node in TerrainTiles){
					Terrain_tile Script = node as Terrain_tile;
					if (Script.Check_AABB(eventMouseButton.Position) == true){
						//New tile
						node.Modulate = Colors.White;
						tile_selected = true;
						current_atlas = Script.AtlasCoordinates/cellsize;
						current_SourceId = Script.SourceId;
						
						foreach (Sprite2D other_node in TerrainTiles)
						{
							if (other_node != node)
							{
								other_node.Modulate = Colors.DimGray;
							}
						}
					}
				}
				if (!tile_selected)
				{
					Godot.Vector2 MousePos = eventMouseButton.Position;
					Create_tile(MousePos,current_SourceId);
				}			
			}
		}
	}

	private void Create_tile(Godot.Vector2 GlobalPosition, int sourceId){
		Godot.Vector2 LocalPos = ToLocal(GlobalPosition);
		Vector2I TilePos = LocalToMap(LocalPos);

		SetCell(TilePos,sourceId,current_atlas,0);

		if (sourceId != -1)
		{
			Tile_node Tile = grid.GetGridObject(TilePos.X,TilePos.Y);
			if (Tile != null)
			{
				Tile.health = (float)GetCellTileData(TilePos).GetCustomData("health");
				Tile.breakable = (bool)GetCellTileData(TilePos).GetCustomData("breakable");		
			}
		}
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Place_tiles)
		{	
		}
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

	private void read_dir(string name){
		using var dir = DirAccess.Open(name);
	
		if (dir != null)
		{
			GD.Print(dir.GetFiles());
		}

	}
}
