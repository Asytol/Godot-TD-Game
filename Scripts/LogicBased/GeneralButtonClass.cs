using System;
using Godot;

public partial class GeneralButtonClass : BaseButton
{
    [Signal]
    public delegate void SendStringEventHandler(string message);
    [Signal]
    public delegate void SendIntEventHandler(int number);
    [Signal]
    public delegate void SendBoolEventHandler(bool boolean);

    [Export] private bool SendMessage;
    [Export] private string message;
    [Export] private bool SendNumber;
    [Export] private int number;
    [Export] private bool SendBoolean;
    [Export] private bool boolean;


    public override void _Ready()
    {
        if (SendMessage)
        {
            ButtonUp += SendStringLocal;
        }
        if (SendNumber)
        {
            ButtonUp += SendNumberLocal;
        }
        if (SendBoolean)
        {
            ButtonUp += SendBoolLocal;
        }
    }

    private void SendStringLocal()
    {
        EmitSignal(SignalName.SendString, message);
    }
    private void SendNumberLocal()
    {
        EmitSignal(SignalName.SendString, number);
    }
    private void SendBoolLocal()
    {
        EmitSignal(SignalName.SendString, boolean);
    }
}



