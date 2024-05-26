using Godot;
using System;

public partial class Mirror : Node3D, IHoldable
{
    private SubViewport _mirrorViewport;
    private Sprite3D _mirrorSprite;
    private Camera3D _mirrorCamera;

    public override void _Ready()
    {
        _mirrorViewport = GetNode<SubViewport>("SubViewport");
        _mirrorSprite = GetNode<Sprite3D>("Sprite3D");
        _mirrorCamera = GetNode<Camera3D>("SubViewport/Camera3D");
    }

    public override void _Process(double delta)
    {
    }

    public void OnHold(Vector3 holdPosition)
    {
        GlobalPosition = holdPosition + new Vector3(5, 5, 5); // vector3 for testing
        GD.Print("calling onhold in Mirror");
    }
}
