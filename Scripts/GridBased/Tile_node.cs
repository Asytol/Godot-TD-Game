using System;
using Godot;

public class Tile_node
{
    public float health;
    public bool breakable = false;
    public bool occupied;
    public int source_id;

    public int IndentedMoney;
    public Area2D BuildingPointer;

    private readonly int x;
    private readonly int y;
    public Tile_node(int x, int y){
        this.x = x;
        this.y = y;
    }
}
