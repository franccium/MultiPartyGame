using System;
using System.Linq;
using Godot;

public partial class MultiplayerMenu : Node3D
{
    private int _serverPort = 727;
    private string _serverIP = "127.0.0.1";
    private ENetMultiplayerPeer _peer;
    public const int HOST_ID = 1;
    private const int MAX_CLIENTS = 16;

    private Button3D _hostButton;
    private Button3D _joinButton;
    private Button3D _startButton;

    private Label3D _nickLabel;
    private Label3D _playerNicknameLabel;
    private Camera3D _camera;
    private string _playerNickname = "Player" + new Random().Next(1, 20000).ToString();

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;

        _nickLabel = GetNode<Label3D>("NickLabel");
        _playerNicknameLabel = GetNode<Label3D>("PlayerNickLabel");
        _nickLabel.Text = "Your name: ";
        _playerNicknameLabel.Text = _playerNickname;

        _hostButton = GetNode<Button3D>("HostButton");
        _joinButton = GetNode<Button3D>("JoinButton");
        _startButton = GetNode<Button3D>("StartButton");
        _hostButton.SetButtonText("Host Game");
        _joinButton.SetButtonText("Join Game");
        _startButton.SetButtonText("Start Game");
        _hostButton.ButtonInteract += () => OnHostButtonPressed();
        _joinButton.ButtonInteract += () => OnJoinButtonPressed();
        _startButton.ButtonInteract += () => OnStartButtonPressed();
    }


    private void OnPeerConnected(long id)
    {
        GD.Print("Peer connected: " + id.ToString());
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print("Peer disconnected: " + id.ToString());
        GameController.Players.Remove(GameController.Players.Where(x => x.Id == id).First<PlayerInformation>());
        var players = GetTree().GetNodesInGroup("Players");
        foreach (Player player in players)
        {
            if (player.Name == id.ToString())
            {
                player.QueueFree();
            }
        }
    }

    private void OnConnectedToServer()
    {
        GD.Print("Succesfully connected to server!");
        RpcId(HOST_ID, "SendPlayerInformation", _playerNickname, Multiplayer.GetUniqueId());
    }

    private void OnConnectionFailed()
    {
        GD.Print("ERROR: Connection failed");
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("host"))
        {
            OnHostButtonPressed();
        }
        else if (Input.IsActionJustPressed("join"))
        {
            OnJoinButtonPressed();
        }
        else if (Input.IsActionJustPressed("start"))
        {
            OnStartButtonPressed();
        }
    }

    private void OnHostButtonPressed()
    {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(_serverPort, MAX_CLIENTS);
        if (error != Error.Ok)
        {
            GD.Print("ERROR: OnHostButtonPressed: " + error.ToString());
            return;
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.Fastlz);
        Multiplayer.MultiplayerPeer = _peer;
        GD.Print("Server hosted! Waiting for players...");
        SendPlayerInformation(_playerNickname, HOST_ID);
    }

    private void OnJoinButtonPressed()
    {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(_serverIP, _serverPort);
        if (error != Error.Ok)
        {
            GD.Print("ERROR: OnJoinButtonPressed: " + error.ToString());
            return;
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.Fastlz);
        Multiplayer.MultiplayerPeer = _peer;
        GD.Print("Connected to server!");
    }

    private void OnStartButtonPressed()
    {
        Rpc("StartGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame()
    {
        GameController.Instance.ChangeScene(GameController.GameViewScenes.world);
        foreach (PlayerInformation player in GameController.Players)
        {
            GD.Print(player.Name + " is in the game!");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string nickname, int id)
    {
        PlayerInformation playerInformation = new PlayerInformation
        {
            Name = nickname,
            Id = id,
            Score = 0
        };
        if (!GameController.Players.Contains(playerInformation))
        {
            GameController.Players.Add(playerInformation);
        }

        if (Multiplayer.IsServer())
        {
            foreach (PlayerInformation player in GameController.Players)
            {
                Rpc("SendPlayerInformation", player.Name, player.Id);
            }
        }
    }
}
