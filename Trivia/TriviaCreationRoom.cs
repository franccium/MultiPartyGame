using Godot;

public partial class TriviaCreationRoom : Node3D
{
    private Control _triviaCreationControl;
    private Player _player;
    private Control _playerGui;
    private Label3D _playerInputLabel;

    private Button3D _submitButton;
    private bool _isPlayerInputOn;
    private Label3D _activeInputLabel;

    private Button3D[] _answerButtons;
    private Button3D[] _correctAnswerButtons;
    private string[] _answers;
    private Label3D[] _answerLabels;
    public const int ANSWER_COUNT = 4;

    private Button3D _saveTriviaButton;

    public override void _Ready()
    {
        _triviaCreationControl = ResourceLoader.Load<PackedScene>("res://Trivia/trivia_question_creation_control.tscn").Instantiate() as Control;
        _player = GetTree().GetNodesInGroup("Players")[0] as Player;
        _playerGui = _player.GetNode<Control>("GUI");
        _playerGui.AddChild(_triviaCreationControl);

        InitialiseQuestion();
        InitialiseAnswers();

        _saveTriviaButton = GetNode<Button3D>("SaveButton");
        _saveTriviaButton.ButtonInteract += () => SaveTriviaQuestion();
        _saveTriviaButton.SetButtonText("Save Trivia");
        _saveTriviaButton.SetButtonColor(Colors.Purple);
        _saveTriviaButton.SetTextColor(Colors.Yellow);
    }

    private void InitialiseQuestion()
    {
        _submitButton = GetNode<Node3D>("SubmitButton") as Button3D;
        _submitButton.SetButtonText("Start Typing");
        _submitButton.SetButtonColor(Colors.Green);
        _submitButton.ButtonInteract += () => HandleSumbitButtonInteract();

        SetPlayerInputProcessing(false);
        _playerInputLabel = GetNode<Label3D>("PlayerInputLabel");
    }

    private void InitialiseAnswers()
    {
        _answerButtons = new Button3D[ANSWER_COUNT];
        _correctAnswerButtons = new Button3D[ANSWER_COUNT];
        _answerLabels = new Label3D[ANSWER_COUNT];
        for (int i = 0; i < ANSWER_COUNT; i++)
        {
            int buttonIndex = i;
            _answerButtons[i] = GetNode<Button3D>($"Answer{i + 1}/AnswerButton{i + 1}");
            _answerButtons[i].ButtonInteract += () => HandleAnswerButtonInteract(buttonIndex);

            _answerButtons[i].SetButtonText("Start Typing");
            _answerButtons[i].SetButtonColor(Colors.Green);

            _correctAnswerButtons[i] = GetNode<Button3D>($"Answer{i + 1}/GoodAnswerButton{i + 1}");
            _correctAnswerButtons[i].ButtonInteract += () => HandleCorrectAnswerButtonInteract(buttonIndex);
            _correctAnswerButtons[i].SetButtonText("N");
            _correctAnswerButtons[i].SetButtonSize(new Vector3(0.5f, 0.5f, 0.5f));

            _answerLabels[i] = GetNode<Label3D>($"Answer{i + 1}/AnswerLabel{i + 1}");
            _answerLabels[i].Text = "Answer " + (i + 1);
        }
    }

    public override void _Process(double delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Backspace)
            {
                if (_activeInputLabel.Text.Length > 0)
                {
                    _activeInputLabel.Text.Remove(_activeInputLabel.Text.Length - 1);
                }
            }
            else if (eventKey.Keycode == Key.Space)
            {
                if (_isPlayerInputOn)
                {
                    _activeInputLabel.Text += " ";
                }
            }
            else
            {
                char character = (char)eventKey.Unicode;
                if (character != 0)
                {
                    _activeInputLabel.Text += character;
                }
            }
        }
    }

    private void SetPlayerInputProcessing(bool to_process)
    {
        SetProcessInput(to_process);
        SetBlockSignals(!to_process);
        GameController.Instance.CurrentPlayer.SetCanMove(!to_process);
        _isPlayerInputOn = to_process;
    }

    private void HandleSumbitButtonInteract()
    {
        if (!_isPlayerInputOn)
        {
            _activeInputLabel = _playerInputLabel;
            SetPlayerInputProcessing(true);
            _submitButton.SetButtonText("Submit");
            _submitButton.SetButtonColor(Colors.Red);
        }
        else
        {
            SetPlayerInputProcessing(false);
            _submitButton.SetButtonText("Start Typing");
            _submitButton.SetButtonColor(Colors.Green);
        }
    }

    private void HandleAnswerButtonInteract(int i)
    {
        if (_answerLabels[i].Text == "Answer " + (i + 1))
            _answerLabels[i].Text = "";
        
        if (!_isPlayerInputOn)
        {
            _activeInputLabel = _answerLabels[i];
            SetPlayerInputProcessing(true);
            _submitButton.SetButtonText("Submit");
            _submitButton.SetButtonColor(Colors.Red);
        }
        else
        {
            SetPlayerInputProcessing(false);
            _submitButton.SetButtonText("Start Typing");
            _submitButton.SetButtonColor(Colors.Green);
        }
    }

    private void HandleCorrectAnswerButtonInteract(int i)
    {
        //todo do this better this is cringe
        if (_correctAnswerButtons[i].ButtonText == "Y")
        {
            _correctAnswerButtons[i].SetButtonText("N");
            _correctAnswerButtons[i].SetButtonColor(Colors.Red);
        }
        else
        {
            _correctAnswerButtons[i].SetButtonText("Y");
            _correctAnswerButtons[i].SetButtonColor(Colors.Green);
        }
    }

    private void SaveTriviaQuestion()
    {
        _answers = new string[ANSWER_COUNT];
        int correctAnswerIndex = 0;
        for (int i = 0; i < ANSWER_COUNT; i++)
        {
            _answers[i] = _answerLabels[i].Text;
            if (_correctAnswerButtons[i].ButtonText == "Y")
            {
                correctAnswerIndex = i;
            }
        }
        /*
        TriviaManager.Instance.SaveQuestion(new DataStructures.TriviaQuiz
        {
            //Author = _player.PlayerName,
            //Question = _playerInputText,
            Author = "anon",
            Question = "test question?",
            Answers = _answers,
            CorrectAnswerIndex = 2,
        });
        */
        TriviaManager.Instance.ReceiveTrivia
        (
            GameController.Instance.CurrentPlayer.Name,
            _playerInputLabel.Text,
            _answers,
            correctAnswerIndex
        );
    }
}
