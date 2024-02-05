using Godot;
using System;

public partial class World : Node3D
{
    private Button3D _triviaButton;

	public override void _Ready()
	{
        _triviaButton = GetNode<Node3D>("TriviaButton") as Button3D;
        _triviaButton.SetButtonText("Play Trivia");
        _triviaButton.ButtonInteract += () => HandleTriviaButtonInteract();
    }

	public override void _Process(double delta)
	{
	}

    private void HandleTriviaButtonInteract()
    {
        GameController.Instance.PlayMode(GameModes.trivia);
    }
}
