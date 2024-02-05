using Godot;
using System;

public partial class ColorPicker3D : Node3D
{
    private Label3D _colorPickerLabel;
    public Color PickedColor { get; set; } = Colors.WebGreen;

    public override void _Ready()
    {
        _colorPickerLabel = GetNode<Label3D>("ColorPickerLabel");
        _colorPickerLabel.Text = "Pick a color!";


    }

    public override void _Process(double delta)
    {
    }
}
