using System;
using System.Diagnostics;
using Godot;
using static System.Net.Mime.MediaTypeNames;

public partial class CutsceneHandler : TextureRect
{
    [Export] DialogueHandle[] Dialogues;
    private RichTextLabel TextLabel;
    private string CurrentText;
    private int CurrentLetter = 0;

    [Signal]
    public delegate void CutsceneFinishedEventHandler();

    public override void _Ready()
    {
        TextLabel = GetChild<NinePatchRect>(0).GetChild<MarginContainer>(0).GetChild<RichTextLabel>(0);
        Visible = true;
        RenderText();
    }

    private async void RenderText()
    {
        foreach (DialogueHandle handle in Dialogues)
        {
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
                await ToSignal(GetTree().CreateTimer(AwaitTime), SceneTreeTimer.SignalName.Timeout);
            }

            if (handle.WaitForEnter)
            {
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