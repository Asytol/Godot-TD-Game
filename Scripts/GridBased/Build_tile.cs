using System;
using Godot;

public partial class Build_tile : TextureButton
{ 
	[Export] public int cost=80;
    [Export] public PackedScene building;

    private float damage;
    private float AttackSpeed;
    private string name;

    private MarginContainer StatsPanel;

    [Signal]
	public delegate void SendBuildInfoEventHandler(TextureButton button);
	public override void _Ready()
	{
		this.MouseEntered += OnHover;
		this.MouseExited += NoHover;

        this.ButtonUp += SendBuildInfo_local;

        Node2D temp = building.Instantiate<Node2D>();
        if (temp is TowerShooterBase)
        {
			TowerShooterBase BuildingScript = temp as TowerShooterBase;
            damage = BuildingScript.base_damage;
            AttackSpeed = BuildingScript.Cooldown;
            name = BuildingScript.Name.ToString();
        }

        StatsPanel = GetTree().Root.GetChild(1).GetNode<TileMapLayer>("%TileMap").GetNode<MarginContainer>("%StatsPanel");
    }

	private void OnHover()
    {
    	StatsPanel.GlobalPosition = new Vector2(this.GlobalPosition.X - StatsPanel.Size.X-16,this.GlobalPosition.Y + 16);
        StatsPanel.Visible = true;
        TileMapLayer.HoveringOnSumShit = true;
        

		Label label = StatsPanel.GetChild<Label>(1);
        label.Text = $"cost: {cost}" + "\n" +
        $"building: {name}" + "\n" +
        $"damage: {damage}" + "\n" +
        $"AttackSpeed: {AttackSpeed}"
        ;
    }
	private void NoHover()
	{
        StatsPanel.Visible = false;
        TileMapLayer.HoveringOnSumShit = false;
    }
	private void SendBuildInfo_local()
	{
		EmitSignal(SignalName.SendBuildInfo,this);
	}
}
