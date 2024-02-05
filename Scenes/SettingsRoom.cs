using Godot;
using System;

public partial class SettingsRoom : Node3D
{
    private Button3D _changeColorButton;
    private Button3D _changeHatButton;
    private Player _player;

    private ColorPicker3D _colorPicker;
    private Sprite3D _colorPickerSprite;

    public override void _Ready()
    {
        _changeColorButton = GetNode<Node3D>("ChangeColorButton") as Button3D;
        _changeColorButton.SetButtonText("Change Color");
        _changeColorButton.ButtonInteract += () => HandleChangeColorButtonInteract();
        _colorPicker = _changeColorButton.GetNode<Node3D>("ColorPicker") as ColorPicker3D;
        _colorPicker.Visible = false;

        _changeHatButton = GetNode<Node3D>("ChangeHatButton") as Button3D;
        _changeHatButton.SetButtonText("Change Hat");
        _changeHatButton.ButtonInteract += () => HandleChangeHatButtonInteract();

        _player = GameController.Instance.CurrentPlayer;
    }

    public override void _Process(double delta)
    {
    }


    #region BUTTON HANDLERS

    private void HandleChangeColorButtonInteract()
    {
        _colorPicker.Visible = !_colorPicker.Visible;
        _player.SetPlayerAlbedoColor(_colorPicker.PickedColor);
    }

    private void HandleChangeHatButtonInteract()
    {
    }

    #endregion
}
