using Godot;
using System;

public partial class Crosshair : CenterContainer
{
    private const float CROSSHAIR_DOT_RADIUS = 1f;
    private Color CROSSHAIR_DOT_COLOR = new Color(0f, 0f, 1f, 1f);

    public override void _Draw()
    {
        DrawCircle(new Vector2(0, 0), CROSSHAIR_DOT_RADIUS, CROSSHAIR_DOT_COLOR);
    }
}
