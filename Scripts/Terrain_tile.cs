using Godot;
using System.Drawing;
using System.Numerics;
using System;

public partial class Terrain_tile : Sprite2D
{
    [Export] public int cost;
    [Export] public int SourceId;
    [Export] public Vector2I AtlasCoordinates;
    public Godot.Vector2[,] Hitbox_coordinates = new Godot.Vector2[2,2];

    private const int width = 32;
    private const int height = 32;
    private const int cellsize = 16;

    public override void _Ready()
    {
        RegionEnabled = true;
        RegionRect = new Godot.Rect2(AtlasCoordinates.X, AtlasCoordinates.Y, cellsize,cellsize);

        for (int x = 0; x < 2; x++){
            for (int y = 0; y < 2; y++){
                Hitbox_coordinates[x,y] = new Godot.Vector2(this.GlobalPosition.X-16 + width*x, this.GlobalPosition.Y-16 + height*y);
                GD.Print(Hitbox_coordinates[x,y]);
            }
        }
    }
    public bool Check_AABB(Godot.Vector2 position){
        if (position.X < Hitbox_coordinates[1,0].X && position.X > Hitbox_coordinates[0, 0].X){
            if (position.Y > Hitbox_coordinates[0,0].Y && position.Y < Hitbox_coordinates[0, 1].Y){
                GD.Print("changed tile");
                return true;
            }
        }
        return false;
    }

}
