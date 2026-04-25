using System;
using Godot;

[GlobalClass]
public partial class ShrapnellResource : Resource
{
    [Export] public Vector2 LocalPosition;
    [Export(PropertyHint.Range, "0,360,")] public float Rotation;
    [Export] public Vector2 Direction;
    [Export(PropertyHint.Range, "0,1,")] public float InheritDirection;
    [Export] public float RelativeVelocity;
    [Export(PropertyHint.Range, "0,1,")] public float InheritVelocity;
    [Export(PropertyHint.Range,"0,360")] public float ExtraSpread;
    [Export] public PackedScene Projectile;
}