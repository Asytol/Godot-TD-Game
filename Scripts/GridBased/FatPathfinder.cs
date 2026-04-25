using System;
using System.Collections.Generic;
using Godot;

public class FatPathFinder
{
    private readonly Godot.TileMapLayer Tilemap;
    private readonly Grid_class<PathNode> grid;

    private List<PathNode> cells_to_search;
    private List<PathNode> searched_cells;

    private const int cellsize = 16;
    private const int HoleNeighborCost = 3;

    private bool AreaDamager = false;
    public FatPathFinder(int width, int height, Godot.TileMapLayer tileMap){
        this.Tilemap = tileMap;
        grid = new Grid_class<PathNode>(width, height, cellsize, Godot.Vector2.Zero, (Grid_class<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY,bool AreaDamage)
    {
        this.AreaDamager = AreaDamage;

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

                Vector2I TileCoordinates = GetTileMapCoordinates(Tilemap,node.x,node.y);
                if (!CheckBreakable(Tilemap, TileCoordinates))
                {
                    if (Tilemap.GetCellSourceId(TileCoordinates) == 1){
                        foreach (PathNode cell in GetNeighbourList(node)){
                            cell.cost += HoleNeighborCost;
                        }
                    }
                    node.inactive = true;
                    continue;
                }
                node.inactive = false;
                node.cost = CheckTileCost(Tilemap, TileCoordinates,node,AreaDamager);
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
                if (searched_cells.Contains(neighbour) || neighbour.inactive){ continue; }
                int temp_gcost = current_node.g_cost + Calculate_distance(current_node.x,current_node.y, neighbour.x,neighbour.y);

                if (temp_gcost < neighbour.g_cost)
                {
                    neighbour.previousCell = current_node;
                    neighbour.g_cost = temp_gcost;
                    neighbour.h_cost = Calculate_distance(current_node.x,current_node.y, neighbour.x,neighbour.y);
                    neighbour.CalculateFCost();
                    if (!cells_to_search.Contains(neighbour)){
                        cells_to_search.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    private static List<PathNode> Get_end_path(PathNode endNode)
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

    private static PathNode GetLowestFCostNode(List<PathNode> cell_list){
        PathNode best_cell = cell_list[0];

        for (int i = 1; i < cell_list.Count; i++){
            if (cell_list[i].f_cost < best_cell.f_cost){
                best_cell = cell_list[i];
            }
        }

        return best_cell;
    }

    private static int Calculate_distance(int x1, int y1, int x2,int y2)
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
    private static int CheckTileCost(Godot.TileMapLayer tilemap, Vector2I coordinates, PathNode node, bool AreaDamager)
    {
        float addative = 0;
        int id = 0;
        for (int x = -1; x < 2; x++){
            for (int y = -1; y < 2; y++)
            {
                Godot.Vector2I cords = new Godot.Vector2I(coordinates.X + x, coordinates.Y + y);
                id = tilemap.GetCellSourceId(cords);
                if (id != -1){
                    node.is_obstruction = true;
                    if (!AreaDamager)
                    {
                        Variant cost = tilemap.GetCellTileData(cords).GetCustomData("cost");
                        if (cost.VariantType == Variant.Type.Float){
                            addative += (float)cost;
                        }
                    }
                }
            }
        }
        id = tilemap.GetCellSourceId(coordinates);
        node.tilemap_position = coordinates;
        if (AreaDamager)
        {
            if (id != -1){
                Variant cost = tilemap.GetCellTileData(coordinates).GetCustomData("cost");
                if (cost.VariantType == Variant.Type.Float){
                    return Mathf.RoundToInt((float)cost*1.25);
                }
            }
        }

        return Mathf.RoundToInt((float)addative);
    }
    private static bool CheckBreakable(Godot.TileMapLayer tilemap, Vector2I coordinates)
    {
        bool breakable = true;
        for (int x = -1; x < 2; x++){
            for (int y = -1; y < 2; y++)
            {
                Godot.Vector2I cords = new Godot.Vector2I(coordinates.X + x, coordinates.Y + y);
                int id = tilemap.GetCellSourceId(cords);
                if (id != -1){
                    breakable = (bool)tilemap.GetCellTileData(cords).GetCustomData("breakable");
                    if (!breakable){break;}
                }
            }
        }
        return breakable;
    }

    private List<PathNode> GetNeighbourList(PathNode current_node){
        List<PathNode> NeighbourList = new List<PathNode>();
        int x = current_node.x;
        int y = current_node.y;

        if (x >= 1){
            //Left
            NeighbourList.Add(GetNode(x - 1, y));
        }
        if (x + 1 < grid.Get_width()) {
            //Right
            NeighbourList.Add(GetNode(x + 1, y));
        }
        //down
        if (y >= 1) { NeighbourList.Add(GetNode(x, y - 1)); }
        //up
        if (y + 1 < grid.Get_height()) { NeighbourList.Add(GetNode(x, y + 1)); }

        return NeighbourList;
    }
}