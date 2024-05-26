using Godot;
using System;

public partial class ColorPicker3D : Node3D
{
    private Label3D _colorPickerLabel;
    public Color PickedColor { get; set; } = Colors.WebGreen;

    private Sprite3D _colorPickerSprite;
    private CsgBox3D _colorPickerBox;
    private Viewport _viewport;
    private ColorPicker _colorPicker;
    private MeshInstance3D _pickedColorIndicator;

    public override void _Ready()
    {
        _colorPickerLabel = GetNode<Label3D>("ColorPickerLabel");

        _colorPickerBox = GetNode<CsgBox3D>("ColorPickerBox");
        _colorPickerSprite = _colorPickerBox.GetNode<Sprite3D>("ColorPickerSprite");
        _viewport = _colorPickerBox.GetNode<Viewport>("SubViewport");
        _colorPicker = _colorPickerBox.GetNode<ColorPicker>("SubViewport/ColorPicker");

        _pickedColorIndicator = GetNode<MeshInstance3D>("PickedColorIndicator");
    }

    public override void _Process(double delta)
    {
    }

    public void PickColorAtPosition(Vector3 position)
    {
        Image image = _viewport.GetTexture().GetImage();

        Vector2 imageSize = new Vector2(image.GetWidth(), image.GetHeight());
        Vector2 hitPointImage = (new Vector2(position.X, position.Y) + Vector2.One) * 0.5f * imageSize;
        Color color = image.GetPixel((int)hitPointImage.X, (int)hitPointImage.Y);
        PickedColor = color;

        StandardMaterial3D material = new StandardMaterial3D
        {
            AlbedoColor = PickedColor
        };
        _pickedColorIndicator.SetSurfaceOverrideMaterial(0, material);

        //Color color = image.GetPixel((int)position.X, (int)position.Y);
        GD.Print("viewport collide with pos " + position + " after cast to int: " + (int)position.X + ", " + (int)position.Y);
        GD.Print("after hitPointImage coords: " + hitPointImage);
        GD.Print("picked color: " + color);
    }
}
