using System;
using Godot;

public partial class Build_tile : TextureButton
{ 
	[Export] public int cost=80;
	[Export] public PackedScene building;

	[Signal]
	public delegate void SendBuildInfoEventHandler(TextureButton button);
	public override void _Ready()
	{
		this.MouseEntered += OnHover;
		this.MouseExited += NoHover;

		this.ButtonUp += SendBuildInfo_local;
	}

	private void OnHover()
	{
		MarginContainer StatsPanel = GetNode<MarginContainer>("%StatsPanel");
		StatsPanel.Visible = true;
		StatsPanel.GlobalPosition = new Vector2(this.GlobalPosition.X +16,this.GlobalPosition.Y +16);

		Label label = StatsPanel.GetChild<Label>(1);
		label.Text = $"cost: {cost}"+"\n"+ $"building: {building.ResourceName}";
	}
	private void NoHover()
	{
		GetNode<MarginContainer>("%StatsPanel").Visible = false;
	}
	private void SendBuildInfo_local()
	{
		EmitSignal(SignalName.SendBuildInfo,this);
	}
}
