using System.Threading.Tasks.Dataflow;
using Godot;
using System;

public partial class Button3D : Node3D, IInteractable
{
    private MeshInstance3D _buttonMesh;
    private CollisionShape3D _buttonCollisionShape;
    private Label3D _buttonLabel;

    public Vector3 ButtonSize { get; set; } = new Vector3(6f, 2f, 1f);
    public Color ButtonColor { get; set; } = Colors.Red;
    public Color TextColor { get; set; } = Colors.White;
    public string ButtonText { get; set; } = "Button";

    [Signal]
    public delegate void ButtonInteractEventHandler();

    public override void _Ready()
    {
        _buttonMesh = GetNode<MeshInstance3D>("ButtonBox");
        _buttonCollisionShape = GetNode<CollisionShape3D>("ButtonCollisionBox");
        _buttonMesh.Mesh = new BoxMesh { Size = ButtonSize };
        _buttonCollisionShape.Shape = new BoxShape3D { Size = ButtonSize };
        _buttonLabel = GetNode<Label3D>("ButtonLabel");
        _buttonLabel.Text = ButtonText;
        SetButtonColor(ButtonColor);
        SetTextColor(TextColor);
    }

    public override void _Process(double delta)
    {
    }

    public void OnHover()
    {
    }

    public void OnInteract()
    {
        EmitSignal("ButtonInteract");
    }

    public void SetButtonSize(Vector3 size)
    {
        ButtonSize = size;
        _buttonMesh.Mesh = new BoxMesh { Size = size };
        _buttonCollisionShape.Shape = new BoxShape3D { Size = size };
    }

    public void SetButtonText(string text)
    {
        ButtonText = text;
        _buttonLabel.Text = text;
    }

    public void SetButtonColor(Color color)
    {
        ButtonColor = color;
        StandardMaterial3D material = new StandardMaterial3D
        {
            AlbedoColor = color
        };
        _buttonMesh.SetSurfaceOverrideMaterial(0, material);
    }

    public void SetTextColor(Color color)
    {
        TextColor = color;
        _buttonLabel.Modulate = color;
    }
}
