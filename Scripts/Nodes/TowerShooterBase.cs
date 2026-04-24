using System;
using System.Collections.Generic;
using Godot;
using System.Numerics;


public partial class TowerShooterBase : TowerBase
{
	[ExportGroup("ShooterBase")] 
    [Export] public float base_damage = 10;
    public float damage = 80;
    [Export] public float ProjectileSpeedMultiplier = 1;
    //
    [Export] public float Cooldown = 2;
	public bool shooting = false;

	[Export] public float base_spread = 0;
	public float spread = 0;

	[Export] public PackedScene projectile;
	[Export] private GpuParticles2D Particles2D;

    private AnimatedSprite2D TowerHead;
    private bool AnimationFinished;

    public override void _Ready()
    {
        base._Ready();
        range = base_range; damage = base_damage; spread = base_spread;
        if (projectile == null) { projectile = GD.Load<PackedScene>("res://Scenes/Arrow.tscn"); }

        TowerHead = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        TowerHead.Play("default");
        TowerHead.AnimationFinished += OnAnimationFinished;
    }

    	public override void _Process(double delta)
	{
        base._Process(delta);
		if (EntitiesInArea.Count > 0)
		{
			Node2D ClosestEntity = EntitiesInArea[0];
			float ClosestDistance = this.Position.DistanceTo(ClosestEntity.Position);
			// (:
			for (int i = 1; i < EntitiesInArea.Count; i++){
				float distance = this.Position.DistanceTo(EntitiesInArea[i].Position);
				if (distance < ClosestDistance){
					ClosestDistance = distance;
					ClosestEntity = EntitiesInArea[i];
				}
			}

            Godot.Vector2 Direction = GlobalPosition.DirectionTo(ClosestEntity.GlobalPosition);
            TowerHead.GlobalRotation = Direction.Angle() - 1.57079632679f;
            if (!shooting)
            {
				Summon_projectile(Direction,ProjectileSpeedMultiplier,spread);
				shooting = true;
            }
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

    private async void Summon_projectile(Godot.Vector2 direction, float SpeedMultiplier = 1, float extra_spread = 0, List<int> ex_layers = null)
    {
        TowerHead.Play("LoadingUp");
		while (!AnimationFinished)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (Particles2D != null){
			Particles2D.ProcessMaterial.Set("direction", new Godot.Vector3(direction.X, direction.Y, 0));
			Particles2D.Restart(); 
		}
		
        Node instance = projectile.Instantiate();
		AddChild(instance);
		Projectile script = instance as Projectile;
		script.instantiate(direction, SpeedMultiplier, extra_spread,ExLayers:ex_layers);

		await ToSignal(GetTree().CreateTimer(Cooldown), SceneTreeTimer.SignalName.Timeout);
        shooting = false;
        TowerHead.Play("default");
    }
    private void OnAnimationFinished(){
		AnimationFinished = true;
	}
}
