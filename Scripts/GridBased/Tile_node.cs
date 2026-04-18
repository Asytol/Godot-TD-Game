using Godot;
using System;

public class Tile_node
{
    public float health;
    public bool breakable = false;
    public int source_id;

    private readonly int x;
    private readonly int y;
    public Tile_node(int x, int y){
        this.x = x;
        this.y = y;
    }
}
