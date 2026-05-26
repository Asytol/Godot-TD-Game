using Godot;
using System;

public partial class SceneTransitioner : CanvasLayer
{
	[Export] public float SwitchDuration = 1f;
	private int DotAmount;
	public string CurrentScene = "";

	private ColorRect thisRect;

	public static bool FinishedLoading;

	/*public SceneTransitioner()
	{
		RegEx regex = new();
		regex.Compile("res:\\/\\/MainScenes\\/([^\\.]+)");
		RegExMatch result = regex.Search(
			(string)ProjectSettings.GetSetting("application/run/main_scene"));
		CurrentScene = result.GetString(1);
	}*/
	public override void _Ready()
	{
		thisRect = GetNode<ColorRect>("ColorRect");
		thisRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		thisRect.Modulate = new Color(1,1,1,0);
		GetNode<RichTextLabel>("Label").Visible = false;
		GetNode<CenterContainer>("CenterContainer").Visible = false;
	}

	public async void GoToScene(string scene, (GodotObject, string)? awaitable = null)
	{
		thisRect.MouseFilter = Control.MouseFilterEnum.Stop;

		Tween tween = GetTree().CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(thisRect, "modulate", new Color(1,1,1,1), SwitchDuration/2f);
		await ToSignal(tween, Tween.SignalName.Finished);
		thisRect.Modulate = new Color(1,1,1,1);
		
		GetTree().ChangeSceneToFile($"res://MainScenes/{scene}.tscn");
		//ResourceLoader.LoadThreadedRequest($"res://MainScenes/{scene}.tscn");
		GetTree().Paused = false;
		GetNode<RichTextLabel>("Label").Visible = true;
		GetNode<CenterContainer>("CenterContainer").Visible = true;
		/*while (ResourceLoader.LoadThreadedGetStatus($"res://MainScenes/{scene}.tscn") != ResourceLoader.ThreadLoadStatus.Loaded)
		{
			GD.Print(ResourceLoader.LoadThreadedGetStatus($"res://MainScenes/{scene}.tscn"));
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		ResourceLoader.LoadThreadedGet($"res://MainScenes/{scene}.tscn");
		*/
		FinishedLoading = false;	
		GetNode<RichTextLabel>("Label").Visible = false;
		GetNode<CenterContainer>("CenterContainer").Visible = false;
		CurrentScene = scene;

		tween = GetTree().CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(thisRect,"modulate",
			new Color(1,1,1,0),SwitchDuration/2f);
		thisRect.MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	public void Continue()
	{
		
	}
}
