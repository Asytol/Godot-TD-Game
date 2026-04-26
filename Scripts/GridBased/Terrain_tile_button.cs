using System;
using System.Drawing;
using System.Numerics;
using Godot;

public partial class Terrain_tile_button : TextureButton
{
    [Export] public int cost;
    [Export] public int SourceId;
    [Export] public Vector2I AtlasCoordinates;
    private float health;

    private const int width = 32;
    private const int height = 32;
    private const int cellsize = 16;


    private MarginContainer StatsPanel;

    [Signal]
    public delegate void SendTileInfoEventHandler(TextureButton button);

    public override void _Ready()
    {
        this.MouseEntered += OnHover;
        this.MouseExited += NoHover;

        this.ButtonUp += SendTileInfo_local;

        StatsPanel = GetTree().Root.GetChild(1).GetNode<TileMapLayer>("%TileMap").GetNode<MarginContainer>("%StatsPanel");
    }

    private void OnHover()
    {
        StatsPanel.GlobalPosition = new Godot.Vector2(this.GlobalPosition.X - StatsPanel.Size.X-16,this.GlobalPosition.Y + 16);
        StatsPanel.Visible = true;

		Label label = StatsPanel.GetChild<Label>(1);
        label.Text = $"cost: {cost}" + "\n" +
        $"Tile: {Name}" + "\n" +
        $"Health: idk";
    }
	private void NoHover()
	{
		StatsPanel.Visible = false;
    }

    private void SendTileInfo_local()
    {
        EmitSignal(SignalName.SendTileInfo,this);
    }

}

