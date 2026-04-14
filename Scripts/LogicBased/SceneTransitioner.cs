using Godot;
using System;

public partial class SceneTransitioner : CanvasLayer
{
	[Export] public float SwitchDuration = 1f;
	public string CurrentScene = "";

	private ColorRect thisRect;

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
	}

	public async void GoToScene(string scene, (GodotObject, string)? awaitable = null)
	{
		thisRect.MouseFilter = Control.MouseFilterEnum.Stop;

		Tween tween = GetTree().CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(thisRect, "modulate", new Color(1,1,1,1), SwitchDuration/2f);
		await ToSignal(tween, Tween.SignalName.Finished);

		GetTree().ChangeSceneToFile($"res://MainScenes/{scene}.tscn");
		GetTree().Paused = false;
		if (awaitable != null)
		{
			await ToSignal(awaitable.Value.Item1, awaitable.Value.Item2);
		}
		CurrentScene = scene;

		tween = GetTree().CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(thisRect,"modulate",
			new Color(1,1,1,0),SwitchDuration/2f);
		thisRect.MouseFilter = Control.MouseFilterEnum.Ignore;
	}
}
