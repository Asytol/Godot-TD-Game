using Godot;
using System;

public partial class MainMenu : Control
{


    public override void _Ready()
    {
        GetNode<Button>("%Level select").ButtonUp += LevelSelect;
        GetNode<Button>("%Quit").ButtonUp += QuitGame;
        GetNode<Button>("%TestLevel1").ButtonUp += TestLevel;
    }

    private void LevelSelect()
    {
        
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }
    private void TestLevel()
    {
        GD.Print("bing");
        CanvasLayer Transitioner = GetTree().Root.GetChild<CanvasLayer>(0);
        SceneTransitioner script = Transitioner as SceneTransitioner;
        script.GoToScene("main_test_scene");
    }
}
