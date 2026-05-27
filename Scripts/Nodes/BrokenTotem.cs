using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

public partial class BrokenTotem : Area2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] private TextureButton SelectionButton;
	[Export] private TextureButton AimingButton;


	private bool mouse_down;
	private bool RefreshBox;

	private Vector2[] SelectionCoordinates = new Vector2[2];
	[Export] private Area2D SelectionBox;
	private RectangleShape2D SelectionBoxHitbox;
	private CollisionShape2D SelectionShape;
	private NinePatchRect SelectionBoxRect;


	private List<Node2D> BodiesInside = new List<Node2D>();
	private List<CharacterBody2D> SelectedLizards = new List<CharacterBody2D>();


	public bool SelectionActive;
	public bool AimingActive;

	public static List<Vector2I> DrawList = new List<Vector2I>();


	[Export] private int TreeReperationAmount;
	private int CurrentTreeAmount = 0;
	[Export] private Label TreeSelectionLabel;

	public override void _Ready()
	{
		mouse_down = false;

		SelectionShape = SelectionBox.GetChild<CollisionShape2D>(0);
		SelectionBoxHitbox = SelectionBox.GetChild<CollisionShape2D>(0).Shape as RectangleShape2D;
		SelectionBoxRect = SelectionBox.GetChild<NinePatchRect>(1);

		DisableSelector();

		SelectionBox.BodyEntered += SelectionBodyEntered;
		SelectionBox.BodyExited += SelectionBodyExited;

		SelectionButton.ButtonUp += ActivateSelection;
		AimingButton.ButtonUp += ActivateAiming;

		TreeSelectionLabel.Text = "0/"+TreeReperationAmount;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Exit"))
		{
			DisableSelector();
			SelectionActive = false;
			RefreshBox = false;
			SelectionBox.GetChild<Sprite2D>(2).Visible = false;
			BodiesInside.Clear();
			foreach (CharacterBody2D Lizard in SelectedLizards)
			{
				ShaderMaterial material = Lizard.GetChild<AnimatedSprite2D>(0).Material as ShaderMaterial;
				material.SetShaderParameter("EnableShader",false);
				Lizard.GetChild<Sprite2D>(1).Visible = false;
			}
			SelectedLizards.Clear();
		}
	}
    public override void _Draw()
    {
		foreach (Vector2 Point in DrawList)
		{
			DrawCircle(Point*16,3,Colors.Red);
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton)
		{
			mouse_down = !mouse_down;
			if (mouse_down)
			{
				RefreshBox = !RefreshBox;
				SelectionBox.GlobalPosition = eventMouseButton.Position;
				if (SelectionActive)
				{
					if (RefreshBox)
					{
						EnableSelector();

						SelectionCoordinates[0] = eventMouseButton.Position;
						SelectionCoordinates[1] = eventMouseButton.Position;

						UpdateSelectorPos();
					}
					else
					{
						DisableSelector();

						foreach (Node2D body in BodiesInside)
						{
							if (body is FriendlyLizardScript)
							{
								SelectedLizards.Add(body as CharacterBody2D);
								ShaderMaterial material = body.GetChild<AnimatedSprite2D>(0).Material as ShaderMaterial;
								material.SetShaderParameter("EnableShader",true);

								body.GetChild<Sprite2D>(1).Visible = true;
							}
						}
					}
				}
				

				if (AimingActive)
				{
					SelectionBox.GlobalPosition = eventMouseButton.Position;
					foreach (CharacterBody2D Lizard in SelectedLizards)
					{
						FriendlyLizardScript Script = Lizard as FriendlyLizardScript;
						Script.MoveTowardsPos(eventMouseButton.Position);
					}
					DrawList.Add(new Vector2I (Mathf.FloorToInt(this.GlobalPosition.X/16),Mathf.FloorToInt(this.GlobalPosition.Y/16)));
					QueueRedraw();	
				}
			}
		}
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			if (SelectionActive)
			{	
				SelectionCoordinates[1] = eventMouseMotion.Position;
				UpdateSelectorPos();
			}

			if (AimingActive)
			{
				SelectionBox.GlobalPosition = eventMouseMotion.Position;	
			}
		}
    }

	private void SelectionBodyEntered(Node2D Body)
	{
		BodiesInside.Add(Body);
		GD.Print(Body+" Entered");
		if (Body is FriendlyLizardScript)
		{
			ShaderMaterial material = Body.GetChild<AnimatedSprite2D>(0).Material as ShaderMaterial;
			material.SetShaderParameter("EnableShader",true);

			Body.GetChild<Sprite2D>(1).Visible = true;
		}
	} 
	private void SelectionBodyExited(Node2D Body)
	{
		BodiesInside.Remove(Body);
		if (Body is FriendlyLizardScript && RefreshBox)
		{
			ShaderMaterial material = Body.GetChild<AnimatedSprite2D>(0).Material as ShaderMaterial;
			material.SetShaderParameter("EnableShader",false);
			Body.GetChild<Sprite2D>(1).Visible = false;

			SelectedLizards.Remove(Body as CharacterBody2D);
		}
	}

	private void DisableSelector()
	{
		SelectionBoxRect.Visible = false;
		SelectionShape.Disabled = true;
	}
	private void EnableSelector()
	{
		SelectionBoxRect.Visible = true;
		SelectionShape.Disabled = false;
	}
	private void UpdateSelectorPos()
	{
		Vector2 LocalCoordinates = SelectionCoordinates[1] - SelectionCoordinates[0];
		if (LocalCoordinates.X >= 0 && LocalCoordinates.Y >= 0)
		{
			SelectionBoxHitbox.Size = LocalCoordinates;
			SelectionShape.Position = LocalCoordinates/2;
			SelectionBoxRect.Size = LocalCoordinates;		
		}
	}

	private void ActivateSelection()
	{
		RefreshBox = false;
		SelectionActive = true;
		AimingActive = false;
		SelectionBox.GetChild<Sprite2D>(2).Visible = false;
	}
	private void ActivateAiming()
	{
		AimingActive = true;
		SelectionActive = false;
		DisableSelector();
		SelectionBox.GetChild<Sprite2D>(2).Visible = true;
	}
	public void DepositTree()
	{
		CurrentTreeAmount++;
		TreeSelectionLabel.Text = CurrentTreeAmount+"/"+TreeReperationAmount;
		if (CurrentTreeAmount >= TreeReperationAmount)
		{
			GetTree().Root.GetChild(1).GetNode<CenterContainer>("%WinMenu").Visible = true;
		}
	}
}
