using Godot;
using MultiPartyGame;
using System;
using System.Collections.Generic;

public partial class TriviaQuizRoom : Node3D
{
    private List<DataStructures.TriviaQuiz> _triviaQuizzes = new List<DataStructures.TriviaQuiz>();
    private Label3D _authorLabel;
    private Label3D _questionLabel;
    private int _currentTriviaIndex = 0;

    private Button3D[] _correctAnswerButtons;
    private string[] _answers;
    private Label3D[] _answerLabels;

    private int _chosenAnswerIndex = -1;

    private Button3D _nextTriviaButton;

    private Label3D _playerListLabel;
    private Label3D[] _playerLabels;
    private Label3D[] _playerScoreLabels;

    public override void _Ready()
    {
        GetTrivia();
        _authorLabel = GetNode<Label3D>("AuthorLabel");
        _questionLabel = GetNode<Label3D>("QuestionLabel");

        _nextTriviaButton = GetNode<Button3D>("NextTriviaButton");
        _nextTriviaButton.ButtonInteract += () => GoToNextTrivia();
        _nextTriviaButton.SetButtonText("Next Trivia");
        _nextTriviaButton.SetButtonColor(Colors.Purple);
        _nextTriviaButton.SetTextColor(Colors.Yellow);

        InitialiseAnswers();
        DisplayTrivia();

        _playerListLabel = GetNode<Label3D>("PlayerListLabel");
        _playerLabels = new Label3D[GameController.Players.Count];
        _playerScoreLabels = new Label3D[GameController.Players.Count];
        foreach (PlayerInformation player in GameController.Players)
        {
            GD.Print("Player added to list: " + player.Name);
            Label3D playerLabel = _playerListLabel.Duplicate() as Label3D;
            playerLabel.Text = player.Name;
            Label3D playerScoreLabel = _playerListLabel.Duplicate() as Label3D;
            playerScoreLabel.Text = player.Score.ToString();

            _playerListLabel.AddChild(playerLabel);
            playerLabel.AddChild(playerScoreLabel);
            _playerLabels[GameController.Players.IndexOf(player)] = playerLabel;
            _playerScoreLabels[GameController.Players.IndexOf(player)] = playerScoreLabel;

            playerLabel.Position = new Vector3(-2, 2, 0);
            playerScoreLabel.Position = new Vector3(10, 0, 0);

            GD.Print("player label position: " + playerLabel.GlobalPosition);
            GD.Print("score label position: " + playerScoreLabel.GlobalPosition);
            GD.Print("player list label position: " + _playerListLabel.GlobalPosition);
        }
    }

    public override void _Process(double delta)
    {
    }

    private void GetTrivia()
    {
        if (TriviaManager.Instance.TriviaQuizzes.Count == 0)
        {
            GD.Print("No trivia quizzes available");
            return;
        }
        foreach (DataStructures.TriviaQuiz trivia in TriviaManager.Instance.TriviaQuizzes)
        {
            _triviaQuizzes.Add(trivia);
        }
    }

    private void DisplayTrivia()
    {
        if (_triviaQuizzes.Count == 0)
        {
            GD.Print("No trivia quizzes available");
            return;
        }
        DataStructures.TriviaQuiz trivia = _triviaQuizzes[_currentTriviaIndex];
        _authorLabel.Text = "Author: " + trivia.Author;
        _questionLabel.Text = trivia.Question;
        for (int i = 0; i < trivia.Answers.Length; i++)
        {
            _answerLabels[i].Text = trivia.Answers[i];
        }
    }

    private void GoToNextTrivia()
    {
        AssessAnswers();

        _currentTriviaIndex++;
        if (_currentTriviaIndex >= _triviaQuizzes.Count)
        {
            _currentTriviaIndex = 0;
        }
        DisplayTrivia();
    }

    private void InitialiseAnswers()
    {
        int answerCount = TriviaCreationRoom.ANSWER_COUNT;
        _correctAnswerButtons = new Button3D[answerCount];
        _answerLabels = new Label3D[answerCount];
        for (int i = 0; i < answerCount; i++)
        {
            int buttonIndex = i;

            _correctAnswerButtons[i] = GetNode<Button3D>($"Answer{i + 1}/GoodAnswerButton{i + 1}");
            _correctAnswerButtons[i].ButtonInteract += () => HandleCorrectAnswerButtonInteract(buttonIndex);
            _correctAnswerButtons[i].SetButtonText("N");
            _correctAnswerButtons[i].SetButtonSize(new Vector3(0.5f, 0.5f, 0.5f));

            _answerLabels[i] = GetNode<Label3D>($"Answer{i + 1}/AnswerLabel{i + 1}");
            _answerLabels[i].Text = "Answer " + (i + 1);
        }
    }

    private void HandleCorrectAnswerButtonInteract(int i)
    {
        // dont need to check authority cause only authority can interact with IInteractable
        _chosenAnswerIndex = i;
    }

    private void AssessAnswers()
    {
        if (_chosenAnswerIndex == _triviaQuizzes[_currentTriviaIndex].CorrectAnswerIndex)
        {
            GameController.Instance.CurrentPlayer.Score++;
            Rpc("UpdateScore", GameController.Instance.CurrentPlayer.Score);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateScore(int score)
    {
        _playerScoreLabels[0].Text = score.ToString();
    }
}
