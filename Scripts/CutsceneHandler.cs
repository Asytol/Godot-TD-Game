using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using static System.Net.Mime.MediaTypeNames;

public partial class CutsceneHandler : TextureRect
{
    [Export] DialogueHandle[] Dialogues;
    [Export] TextureRect ArrowTexture;
    private RichTextLabel TextLabel;
    private string CurrentText;
    private int CurrentLetter = 0;

    private bool escaping;
    private float CutsceneSkipTimeSafetyLock;

    [Signal]
    public delegate void CutsceneFinishedEventHandler();

    public override void _Ready()
    {
        TextLabel = GetChild<NinePatchRect>(0).GetChild<MarginContainer>(0).GetChild<RichTextLabel>(0);
        Visible = true;
        escaping = false;
        ArrowTexture.Visible = false;
        RenderText();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Next") && CutsceneSkipTimeSafetyLock <= 0)
        {
            escaping = true;
        }
        else
        {
            CutsceneSkipTimeSafetyLock -= (float)delta;
        }
    }

    private async void RenderText()
    {
        foreach (DialogueHandle handle in Dialogues)
        {
            ArrowTexture.Visible = false;
            CutsceneSkipTimeSafetyLock = 0.4f;
            if (handle.Background != null) { this.Texture = handle.Background; }
            if (handle.NewBubble)
            {
                await ToSignal(GetTree().CreateTimer(handle.TimeBeforeNextBubble), SceneTreeTimer.SignalName.Timeout);
                TextLabel.Text = "";
                CurrentLetter = 0;
            }
            TextLabel.AddText(" ");
            TextLabel.PushColor(handle.color);
            //if (handle.aggressive) { TextLabel.Text += "[shake rate=20.0 level=5 connected=1]{"; }
            //TextLabel.ParseBbcode("[shake rate=20.0 level=5 connected=1]");
            TextLabel.AddText(handle.text);
            //if (handle.aggressive){TextLabel.Text += "}[/shake]";}
            TextLabel.PopAll();
            TextLabel.VisibleCharacters = CurrentLetter;

            float AwaitTime = 1 / handle.speed;
            while (CurrentLetter < TextLabel.GetTotalCharacterCount())
            {
                CurrentLetter++;
                TextLabel.VisibleCharacters = CurrentLetter;
                if (escaping)
                {
                    CurrentLetter = TextLabel.GetTotalCharacterCount();
                    TextLabel.VisibleCharacters = CurrentLetter;
                    escaping = false;
                    break;
                }
                await ToSignal(GetTree().CreateTimer(AwaitTime), SceneTreeTimer.SignalName.Timeout);
            }

            if (handle.WaitForEnter)
            {
                ArrowTexture.Visible = true;
                while (!Input.IsActionJustPressed("Next"))
                {
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }
            if (handle.TimeBoforeNewLine != 0)
            {
                await ToSignal(GetTree().CreateTimer(handle.TimeBoforeNewLine), SceneTreeTimer.SignalName.Timeout);
            }
        }
        EmitSignal(SignalName.CutsceneFinished);
        Visible = false;
        QueueFree();
    }
}