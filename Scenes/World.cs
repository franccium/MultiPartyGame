using Godot;
using System;

public partial class World : Node3D
{
    private Button3D _triviaButton;

    private Ball _ball;

    public override void _Ready()
    {
        _triviaButton = GetNode<Node3D>("TriviaButton") as Button3D;
        _triviaButton.SetButtonText("Play Trivia");
        _triviaButton.ButtonInteract += () => HandleTriviaButtonInteract();

        _ball = GetNode<RigidBody3D>("Ball") as Ball;
        _ball.BallInteract += () => HandleBallInteract();
        _ball.BallPush += (pushDirection) => HandleBallPush(pushDirection);
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("num_four"))
        {
            Rpc("ResetBall");
        }
    }

    private void HandleTriviaButtonInteract()
    {
        GameController.Instance.PlayMode(GameModes.trivia);
    }

    private void HandleBallInteract()
    {
        Rpc("RpcHandleBallInteract");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void RpcHandleBallInteract()
    {
        _ball.GlobalPosition = GameController.Instance.CurrentPlayer.GlobalPosition + new Vector3(50, 50, 50);
        GD.Print("Ball grabbed applied");
    }

    private void HandleBallPush(Vector3 pushDirection)
    {
        GD.Print("called push rpc");
        Rpc("RpcHandleBallPush", pushDirection);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void RpcHandleBallPush(Vector3 pushDirection)
    {
        GD.Print("Ball pushed applied");
        _ball.ApplyCentralForce(pushDirection * 100f);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ResetBall()
    {
        _ball.GlobalPosition = new Vector3(0, 10, 0);
        _ball.LinearVelocity = Vector3.Zero;
        _ball.AngularVelocity = Vector3.Zero;
    }
}
