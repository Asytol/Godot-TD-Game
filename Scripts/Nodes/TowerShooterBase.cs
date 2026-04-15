using Godot;
using System.Collections.Generic;
using System;


public partial class TowerShooterBase : Tower_base
{
	[ExportGroup("ShooterBase")] 
    [Export] public float base_damage = 80;
	public float damage = 80;
	//
	[Export] public float Cooldown = 2;
	public bool shooting = false;

	[Export] public float base_spread = 0;
	public float spread = 0;

	[Export] public PackedScene projectile;

    public override void _Ready()
    {
        base._Ready();
        range = base_range; damage = base_damage; spread = base_spread;
		if (projectile == null) {projectile = GD.Load<PackedScene>("res://Scenes/projectile.tscn"); }
    }

    	public override void _Process(double delta)
	{
        base._Process(delta);
		if (entities_in_area.Count > 0 && shooting == false)
		{
			Node2D closest_entity = entities_in_area[0];
			float closest_distance = this.Position.DistanceTo(closest_entity.Position);
			// (:
			for (int i = 1; i < entities_in_area.Count; i++){
				float distance = this.Position.DistanceTo(entities_in_area[i].Position);
				if (distance < closest_distance){
					closest_distance = distance; 
					closest_entity = entities_in_area[i];
				}
			}

			Godot.Vector2 Direction = GlobalPosition.DirectionTo(closest_entity.GlobalPosition);
			GD.Print("" + Direction);
			Summon_projectile(Direction,5,spread);
			shooting = true;
		}
	}

    public void Add_range(float addon){
		this.range += addon;
	}
	public void Multiply_range(float multiplier){
		this.range *= multiplier;
	}
	public void Add_damage(float addon){
		this.damage += addon;
	}
	public void Multiply_damage(float multiplier){
		this.damage *= multiplier;
	}

    private async void Summon_projectile(Godot.Vector2 direction,float speed=30,float extra_spread=0,List<int> ex_layers = null){
		Node instance = projectile.Instantiate();
		AddChild(instance);
		Projectile script = instance as Projectile;
		script.instantiate(direction, speed, extra_spread,ExLayers:ex_layers);

		await ToSignal(GetTree().CreateTimer(Cooldown), SceneTreeTimer.SignalName.Timeout);
		shooting = false;
	}
}
