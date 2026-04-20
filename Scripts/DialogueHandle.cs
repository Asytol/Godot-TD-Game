using System;
using Godot;

[GlobalClass]
public partial class DialogueHandle : Resource
{
    [Export] public string text;
    [Export] public float speed = 20;
    [Export] public Color color = Colors.Black;
    [Export] public bool aggressive = false;
    [Export] public bool NewBubble = false;
    [Export] public bool WaitForEnter = true;
    [Export] public float TimeBeforeNextBubble = 0;
    [Export] public float TimeBoforeNewLine = 0;
    [Export] public Texture2D Background;
}
