using System;
using System.IO;
using Godot;

public partial class PathNode
{
    public int x;
    public int y;
    private Grid_class<PathNode> grid;

    public int g_cost = int.MaxValue;
    public int h_cost = int.MaxValue;
    public int f_cost;
    public int cost;
    public bool is_obstruction;
    public Vector2I tilemap_position;
    public PathNode previousCell;
    public bool inactive;

    public PathNode(Grid_class<PathNode> grid,int x, int y){
        this.x = x;
        this.y = y;
        this.grid = grid;
    }

    public void CalculateFCost(){
        f_cost = g_cost + h_cost + cost;
    }
}
