using System;
using Godot;

public partial class Build_tile : TextureButton
{ 
	[Export] public int cost=80;
    [Export] public PackedScene building;

    private float damage;
    private float AttackSpeed;
    private string name;

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
    }

	private void OnHover()
	{
		MarginContainer StatsPanel = GetNode<MarginContainer>("%StatsPanel");
        StatsPanel.Visible = true;
        TileMapLayer.HoveringOnSumShit = true;
        StatsPanel.GlobalPosition = new Vector2(this.GlobalPosition.X +16,this.GlobalPosition.Y +16);

		Label label = StatsPanel.GetChild<Label>(1);
        label.Text = $"cost: {cost}" + "\n" +
        $"building: {name}" + "\n" +
        $"damage: {damage}" + "\n" +
        $"AttackSpeed: {AttackSpeed}"
        ;
    }
	private void NoHover()
	{
        GetNode<MarginContainer>("%StatsPanel").Visible = false;
        TileMapLayer.HoveringOnSumShit = false;
    }
	private void SendBuildInfo_local()
	{
		EmitSignal(SignalName.SendBuildInfo,this);
	}
}
