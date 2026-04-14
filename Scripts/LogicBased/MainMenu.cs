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
        GetTree().ChangeSceneToFile("res://MainScenes/main_test_scene.tscn");
    }
}
