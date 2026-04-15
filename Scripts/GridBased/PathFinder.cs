using Godot;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System;

public class PathFinder
{
    [Export] public  Godot.TileMapLayer Tilemap;
    private Grid_class<PathNode> grid;

    private List<PathNode> cells_to_search;
    private List<PathNode> searched_cells;

    public PathFinder(int width, int height, Godot.TileMapLayer tileMap){
        this.Tilemap = tileMap;
        grid = new Grid_class<PathNode>(width,height,16, Godot.Vector2.Zero, (Grid_class<PathNode> g, int x, int y) => new PathNode(g,x,y));
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startnode = grid.GetGridObject(startX, startY);
        PathNode endnode = grid.GetGridObject(endX, endY);

        cells_to_search = new List<PathNode> {startnode};
        searched_cells = new List<PathNode> ();

        for (int x = 0; x < grid.Get_width(); x++){
            for (int y = 0; y < grid.Get_height(); y++){
                PathNode node = grid.GetGridObject(x,y);
                node.g_cost = int.MaxValue;
                node.CalculateFCost();
                node.previousCell = null;
            }
        }

        while (cells_to_search.Count > 0)
        {
            PathNode current_node = GetLowestFCostNode(cells_to_search);
            if (current_node == endnode){
                return Get_end_path(endnode);
            }

            cells_to_search.Remove(current_node);
            searched_cells.Add(current_node);

            foreach (PathNode neighbour in GetNeighbourList(current_node))
            {
                if (searched_cells.Contains(neighbour)){ continue; }

                int temp_gcost = current_node.g_cost + Calculate_distance(current_node.x,current_node.y, neighbour.x,neighbour.y);
                Vector2I TileCoordinates = GetTileMapCoordinates(Tilemap,neighbour.x,neighbour.y);

                if (temp_gcost < neighbour.g_cost && CheckBreakable(Tilemap,TileCoordinates) == true)
                {
                    neighbour.previousCell = current_node;
                    neighbour.g_cost = temp_gcost;
                    neighbour.h_cost = Calculate_distance(current_node.x,current_node.y, neighbour.x,neighbour.y);
                    neighbour.CalculateFCost();

                    neighbour.f_cost += CheckTileCost(Tilemap, TileCoordinates,neighbour);
                    if (!cells_to_search.Contains(neighbour)){
                        cells_to_search.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    private List<PathNode> Get_end_path(PathNode endNode)
    {
        List<PathNode> Cell_list = new List<PathNode> { endNode }; 
        PathNode current_cell = endNode;
        

        while (current_cell.previousCell != null){
            current_cell = current_cell.previousCell;
            Cell_list.Add(current_cell);
        }
        Cell_list.Reverse();
        return Cell_list;
    }

    private PathNode GetLowestFCostNode(List<PathNode> cell_list){
        PathNode best_cell = cell_list[0];

        for (int i = 1; i < cell_list.Count; i++){
            if (cell_list[i].f_cost < best_cell.f_cost){
                best_cell = cell_list[i];
            }
        }

        return best_cell;
    }

    private int Calculate_distance(int x1, int y1, int x2,int y2)
    {
        int XDistance = Math.Abs(x1 - x2);
        int YDistance = Math.Abs(y1 - y2);
        int remaining = Math.Abs(XDistance - YDistance);
        return remaining;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x,y);    
    }
    public Grid_class<PathNode> GetGrid(){
        return grid;
    }
    private Vector2I GetTileMapCoordinates(Godot.TileMapLayer tilemap,int x, int y)
    {
        Godot.Vector2 coordinates = grid.GetWorldPosition(x,y);
        coordinates = tilemap.ToLocal(coordinates);
        Vector2I tilemap_coordinates = tilemap.LocalToMap(coordinates);
        return tilemap_coordinates;
    }
    private int CheckTileCost(Godot.TileMapLayer tilemap, Vector2I coordinates,PathNode node){
        int id = tilemap.GetCellSourceId(coordinates);

        if (id != -1){
            Variant cost = tilemap.GetCellTileData(coordinates).GetCustomData("cost");
            node.is_obstruction = true;
            node.tilemap_position = coordinates;
            if (cost.VariantType == Variant.Type.Float){
                return Mathf.RoundToInt((float)cost);
            }
        }

        return 0;
    }
    private bool CheckBreakable(Godot.TileMapLayer tilemap, Vector2I coordinates){
        int id = tilemap.GetCellSourceId(coordinates);
        if (id != -1)
        {
            Variant breakable = tilemap.GetCellTileData(coordinates).GetCustomData("breakable");
            return breakable.VariantType == Variant.Type.Bool && (bool)breakable;
        }
        return true;
    }

    private List<PathNode> GetNeighbourList(PathNode current_node){
        List<PathNode> NeighbourList = new List<PathNode>();
        var x = current_node.x;
        var y = current_node.y;

        if (x - 1 >= 0){
            //Left
            NeighbourList.Add(GetNode(x - 1, y));
        }
        if (x + 1 < grid.Get_width()) {
            //Right
            NeighbourList.Add(GetNode(x + 1, y));
        }
        //down
        if (y - 1 >= 0) { NeighbourList.Add(GetNode(x, y - 1)); }
        //up
        if (y + 1 < grid.Get_height()) { NeighbourList.Add(GetNode(x, y + 1)); }

        return NeighbourList;
    }
}
