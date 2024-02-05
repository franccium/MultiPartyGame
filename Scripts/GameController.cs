using Godot;
using System;
using System.Collections.Generic;

public enum GameModes
{
    trivia,
}

public partial class GameController : Node
{
    public static GameController Instance { get; private set; }

    public enum GameViewScenes
    {
        multiplayer_menu,
        world
    }
    private PackedScene[] _gameViewScenes;

    public static List<PlayerInformation> Players = new List<PlayerInformation>();
    public const int MAX_PLAYER_COUNT = 16;
    public int CurrentPlayerCount { get; set; } = 0;
    public Player CurrentPlayer { get; set; }

    public override void _Ready()
    {
        Instance = this;

        int gameViewScenesCount = Enum.GetNames(typeof(GameViewScenes)).Length;
        _gameViewScenes = new PackedScene[gameViewScenesCount];

        for (int i = 0; i < gameViewScenesCount; i++)
        {
            _gameViewScenes[i] = ResourceLoader.Load<PackedScene>($"res://Scenes/{Enum.GetName(typeof(GameViewScenes), i)}.tscn");
        }

        AddChild(_gameViewScenes[(int)GameViewScenes.multiplayer_menu].Instantiate());
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("num_one"))
        {
            SceneManager.Instance.TeleportPlayerToRoom(SceneManager.PlayerRoomScenes.trivia_creation_room);
        }
        if (Input.IsActionJustPressed("num_two"))
        {
            SceneManager.Instance.TeleportAllPlayersToRoom(SceneManager.PlayerRoomScenes.trivia_quiz_room);
        }
    }

    public void ChangeScene(GameViewScenes view_scene)
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        AddChild(_gameViewScenes[(int)view_scene].Instantiate());
    }

    public void PlayMode(GameModes mode)
    {
        switch (mode)
        {
            case GameModes.trivia:
                SceneManager.Instance.TeleportPlayerToRoom(SceneManager.PlayerRoomScenes.trivia_quiz_room);
                break;
        }
    }

    public void GoToSettings()
    {
        SceneManager.Instance.TeleportPlayerToRoom(SceneManager.PlayerRoomScenes.settings_room);
    }
}
