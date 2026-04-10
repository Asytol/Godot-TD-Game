using Godot;
using System.Numerics;
using System;
using System.ComponentModel;

public partial class TileMapLayer : Godot.TileMapLayer
{
	[Export] public bool Place_tiles = false;
	[Export] public Vector2I current_atlas = new Vector2I(0,0);


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
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseButton eventMouseButton)
		{
			mouse_down = !mouse_down;
			if (Place_tiles && mouse_down)
			{
				Godot.Vector2 MousePos = eventMouseButton.Position;
				Create_tile(MousePos,0);
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
			Tile.health = (float)GetCellTileData(TilePos).GetCustomData("health");
			Tile.breakable = (bool)GetCellTileData(TilePos).GetCustomData("breakable");	
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
}
