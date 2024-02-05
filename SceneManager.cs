using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    public enum PlayerRoomScenes
    {
        settings_room,
        trivia_creation_room,
        trivia_quiz_room,
    }
    //private Dictionary<PackedScene, int> _playerRoomScenes;
    private PackedScene[] _playerRoomScenes;
    private int _playerCount;

    private Node3D _roomSpawnpointsContainer;
    private Vector3 _sharedRoomPosition = new Vector3(1000, 1000, 1000);

    public override void _Ready()
    {
        Instance = this;

        int playerRoomScenesCount = Enum.GetNames(typeof(PlayerRoomScenes)).Length;
        _playerRoomScenes = new PackedScene[playerRoomScenesCount];
        _playerCount = GameController.Instance.CurrentPlayerCount;
        for (int i = 0; i < playerRoomScenesCount; i++)
        {
            _playerRoomScenes[i] = ResourceLoader.Load<PackedScene>($"res://Scenes/{Enum.GetName(typeof(PlayerRoomScenes), i)}.tscn");
        }

        _roomSpawnpointsContainer = GetParent().GetNode<Node3D>("PlayerRoomSpawnpoints");
    }

    public override void _Process(double delta)
    {
    }

    public void TeleportAllPlayersToRoom(PlayerRoomScenes room_scene)
    {
        DeleteRooms();

        // teleport all players to the room
        Rpc("TeleportPlayerToRoom", (int)room_scene, false);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void TeleportPlayerToRoom(PlayerRoomScenes room_scene, bool own_room = true)
    {
        DeleteRooms();

        Node3D playerRoomScene = _playerRoomScenes[(int)room_scene].Instantiate() as Node3D;
        Vector3 roomPosition = _sharedRoomPosition;
        if (own_room)
        {
            Marker3D roomspawnpoint = _roomSpawnpointsContainer.GetNode<Marker3D>("Spawn" + GameController.Instance.CurrentPlayer.RoomSpawnpointIndex);
            roomPosition = roomspawnpoint.GlobalPosition;
            GD.Print("Spawn" + GameController.Instance.CurrentPlayer.RoomSpawnpointIndex);
            GD.Print("for player: " + GameController.Instance.CurrentPlayer.Name);
        }
        AddChild(playerRoomScene);
        playerRoomScene.GlobalPosition = roomPosition;
        GD.Print(playerRoomScene.GlobalPosition);

        Marker3D spawnPoint = playerRoomScene.GetNode<Marker3D>("Spawnpoint");
        Player player = GetTree().GetNodesInGroup("Players").FirstOrDefault(x => x.Name == Multiplayer.GetUniqueId().ToString()) as Player;
        player.TeleportToSpawnpoint(spawnPoint);
        GD.Print("Player " + player.Name + " teleported to " + room_scene.ToString());
    }

    public void DeleteRooms()
    {
        foreach (Node3D child in GetChildren())
        {
            child.QueueFree();
        }
    }
}
