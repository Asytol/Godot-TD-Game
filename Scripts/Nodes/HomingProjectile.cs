using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class HomingProjectile : Projectile
{
    private Area2D EnemyDetectionCollider;

    private List<Node2D> EnemiesInArea = new List<Node2D>();

    [Export] private float PivotStrength;
    private float DebugAngle;
    public override void _Ready()
    {
        base._Ready();
        EnemyDetectionCollider = GetNode<Area2D>("EnemyDetectionCollider");
        EnemyDetectionCollider.BodyEntered += OnEnter;
        EnemyDetectionCollider.BodyExited += OnExit;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (EnemiesInArea.Count != 0)
        {
            Node2D ClosestEnemy = EnemiesInArea[0];
            float ClosestDistance = Mathf.Abs(GlobalPosition.DistanceTo(ClosestEnemy.GlobalPosition));
            for (int i = 1; i < EnemiesInArea.Count; i++)
            {
                float dist = Mathf.Abs(GlobalPosition.DistanceTo(EnemiesInArea[i].GlobalPosition));
                if (dist < ClosestDistance)
                {
                    if (EnemiesInArea[i] is Enemy_base Script){
                        if (Script.CheckIFrames(Name)){continue;}}
                    ClosestDistance = dist;
                    ClosestEnemy = EnemiesInArea[i];
                }
            }
            Godot.Vector2 direction = GlobalPosition.DirectionTo(ClosestEnemy.GlobalPosition);
            DebugAngle = GlobalPosition.AngleTo(ClosestEnemy.GlobalPosition);
            Direction += new Godot.Vector2(PivotStrength * direction.X, PivotStrength * direction.Y);
            Direction = Direction.Normalized();
        }
    }
    //public override void _Draw()
    //{
    //    DrawLine(this.GlobalPosition, GlobalPosition + Direction, Colors.Red, 2);
    //}

    private void OnEnter(Node2D body)
    {
        EnemiesInArea.Add(body);
    }
    private void OnExit(Node2D body)
    {
        EnemiesInArea.Remove(body);
    }
}