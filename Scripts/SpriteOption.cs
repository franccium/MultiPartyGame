using Godot;
using System;

public partial class SpriteOption : StaticBody3D
{
    public string SpritePath { get; set; } = "res://icon.svg";
    private Sprite3D _sprite;
    
    //? wanted to make this a sprite that would repesent a for instance hat model, and the player would choose based on it, but not a good idea, just the blends themselves for now maybe

    public override void _Ready()
    {
        _sprite = GetNode<Sprite3D>("Sprite3D");
        _sprite.Texture = new ImageTexture();
    }

    public override void _Process(double delta)
    {
    }
}
