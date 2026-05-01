using System;
using Godot;

public partial class Finish : Area2D
{
    [Export] public int health = 10;
    [Export] public float speed = 5;

    private Label HealthNum;

    [Export] private GpuParticles2D OnHitParticles;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        HealthNum = GetNode<Label>("%HealthNum");
        HealthNum.Text = health.ToString(); 
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }

    private void OnBodyEntered(Node2D body)
    {
    	GD.Print("enemy reached flag");
        health--;
        if (OnHitParticles != null) { OnHitParticles.Restart(); }
        if (body is Enemy_base script)
		{
			script.Damage(int.MaxValue,0,"finishflag");
		}
        HealthNum.Text = health.ToString();
        if (health == 0)
        {
            OnDefeat();
        }
    }
    private void OnDefeat()
    {
		CanvasLayer Transitioner = GetTree().Root.GetChild<CanvasLayer>(0);
        SceneTransitioner script = Transitioner as SceneTransitioner;
        script.GoToScene(GetTree().Root.GetChild<Node>(1).Name.ToString());
    }
}