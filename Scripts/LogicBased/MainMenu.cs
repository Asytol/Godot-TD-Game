using System;
using Godot;
using static System.Net.Mime.MediaTypeNames;

public partial class MainMenu : Control
{
    [Export] Texture2D RebirthTexture;
    [Export] Texture2D RegularTexture;
    private AudioStreamPlayer2D audioStreamPLayer2D;

    private float C_time;
    private bool Changing = false;
    private const bool AllBack = true;

    public override void _Ready()
    {
        audioStreamPLayer2D = GetChild<AudioStreamPlayer2D>(1);

        GetNode<Button>("%Level select").ButtonUp += LevelSelect;
        GetNode<Button>("%Quit").ButtonUp += QuitGame;
        GetNode<Button>("%TestLevel1").ButtonUp += TestLevel;
        GetNode<Button>("%Rebirth").ButtonUp += OnRebirth;
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
    private void OnRebirth()
    {
        C_time = 0;
        GetChild<TextureRect>(0).Texture = RebirthTexture;
        if (!Changing) { Change(); } //don't move position of audioplayer gng
        audioStreamPLayer2D.Play();
    }
    private async void Change()
    {
        Changing = true;
        GD.Print(C_time);
        //C_time += (float)GetProcessDeltaTime();
        await ToSignal(audioStreamPLayer2D,"finished");
        GetChild<TextureRect>(0).Texture = RegularTexture;
        Changing = false;
    }
}


