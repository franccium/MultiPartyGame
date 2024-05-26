using Godot;
using System;

public partial class Ball : RigidBody3D, IInteractable, IPushable
{
    [Signal]
    public delegate void BallInteractEventHandler();
    [Signal]
    public delegate void BallPushEventHandler(Vector3 pushDirection);

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void OnHover()
    {
        //EmitSignal("BallInteract");
    }

    public void OnInteract()
    {
        GD.Print("Ball interacted");
        EmitSignal("BallInteract");
    }

    public void OnPush(Vector3 pushDirection)
    {
        GD.Print("Ball pushed");
        EmitSignal("BallPush", pushDirection);
    }
}

