using Godot;
using System.IO;
using System;

public partial class PathNode
{
    public int x;
    public int y;
    private Grid_class<PathNode> grid;

    public int g_cost;
    public int h_cost;
    public int f_cost;
    
    public PathNode previousCell;

    public PathNode(Grid_class<PathNode> grid,int x, int y){
        this.x = x;
        this.y = y;
        this.grid = grid;
    }

    public void CalculateFCost(){
        f_cost = g_cost + h_cost;
    }
}
