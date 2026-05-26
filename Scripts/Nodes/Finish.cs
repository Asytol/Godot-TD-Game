using System;
using Godot;

public partial class Finish : Area2D
{
    [Export] public int health = 10;
    [Export] public float speed = 5;

    private Label HealthNum;
    private CenterContainer DefeatMenu;
    private CenterContainer WinMenu;

    [Export] private GpuParticles2D OnHitParticles;
    [Export] private GpuParticles2D SoulSuckingParticles;


    private RandomNumberGenerator Rand = new RandomNumberGenerator();
    private float VelocityMin;
    private float VelocityMax;

    private Control UiNodes;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UiNodes = GetTree().Root.GetChild(1).GetNode<Control>("%Ui");

        this.BodyEntered += OnBodyEntered;
        HealthNum = UiNodes.GetNode<Label>("%HealthNum");
        HealthNum.Text = health.ToString(); 

        ParticleProcessMaterial material = SoulSuckingParticles.ProcessMaterial as ParticleProcessMaterial;
        VelocityMin = material.InitialVelocityMin;
        VelocityMax = material.InitialVelocityMax;



        if (DefeatMenu == null)
		{
			DefeatMenu = UiNodes.GetNode<CenterContainer>("%DefeatMenu");
		}
		DefeatMenu.Visible = false;

		foreach (BaseButton button in UiNodes.GetNode<HBoxContainer>("%InnerDefMenu").GetChildren())
		{
			button.Connect("SendString",new Callable(this, nameof(GoToLevel)));
		}
        if (WinMenu == null)
        {
            WinMenu = UiNodes.GetNode<CenterContainer>("%WinMenu");
        }
        foreach (BaseButton button in UiNodes.GetNode<HBoxContainer>("%InnerWinMenu").GetChildren())
        {
            button.Connect("SendString",new Callable(this, nameof(GoToLevel))); 
        }
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
		DefeatMenu.Visible = true;
        GetTree().Root.GetChild(1).PropagateCall("set_process", [false]);
        GD.Print(GetTree().Root.GetChild(1).GetNode<Node2D>("%2DNodes"));
    }

    public void EmitSoul(Vector2 Position)
    {
        SoulSuckingParticles.EmitParticle(new Transform2D(0.0f,Position),Vector2.Zero,Colors.White,Colors.White,1);
    }

    private void GoToLevel(string level)
    {
        GD.Print(level);
        CanvasLayer Transitioner = GetTree().Root.GetChild<CanvasLayer>(0);
        SceneTransitioner script = Transitioner as SceneTransitioner;
        script.GoToScene(level);
    }
}