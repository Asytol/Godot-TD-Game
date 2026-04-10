using Godot;
using System;

public class Tile_node
{
    public float health;
    public bool breakable = false;
    public int source_id;

    private int x;
    private int y;
    public Tile_node(Grid_class<Tile_node> grid,int x, int y){
        this.x = x;
        this.y = y;
    }   
}
