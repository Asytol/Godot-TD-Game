using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class Grid_class<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private Vector2 originPosition;
    private TGridObject[,] gridArray;

    public Grid_class(int width, int height, float cellSize,Vector2 originPosition, Func<Grid_class<TGridObject>,int,int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                gridArray[x,y] = createGridObject(this,x,y);
            }
        }

        this.cellSize = cellSize;
    }

    public Vector2 GetWorldPosition(float x, float y) {
        return new Vector2(x, y) * cellSize + originPosition; 
    }
    public void GetXY(Vector2 WorldPosition, out int x, out int y)
    {
        x = Mathf.RoundToInt(WorldPosition.X - originPosition.X / cellSize);
        y = Mathf.RoundToInt(WorldPosition.Y - originPosition.Y / cellSize);
    } 

    public void SetValue(int x, int y, TGridObject value){
        if (x >= 0 && y >= 0 && x <= width && y <= height){
            gridArray[x, y] = value;
        }
    }
    public void SetValue(Vector2 WorldPosition, TGridObject value){
        int x, y;
        GetXY(WorldPosition, out x, out y);
        SetValue(x,y, value);
    }

    public TGridObject GetGridObject(int x, int y){
        if (x >= 0 && y >= 0 && x <= width && y <= height)
        {
            return gridArray[x, y];
        }
        else { 
            return default(TGridObject); }
        }

    public int Get_width(){
        return width;
    }
    public int Get_height(){
        return height;
    }

}