using Godot;

public partial class PlayerManager : Node
{
    private PackedScene _playerScene;

    public override void _Ready()
    {
        CallDeferred(nameof(SpawnPlayers));
    }

    private void SpawnPlayers()
    {
        _playerScene = ResourceLoader.Load<PackedScene>("res://Player/player.tscn");
        int playerSpawnpointIndex = 0;
        Node3D spawnpointContainer = GetParent().GetNode<Node3D>("PlayerSpawnpoints");
        foreach (PlayerInformation player in GameController.Players)
        {
            Player playerInstance = _playerScene.Instantiate<Player>();
            playerInstance.Name = player.Id.ToString();
            playerInstance.PlayerName = player.Name;
            playerInstance.RoomSpawnpointIndex = playerSpawnpointIndex;
            GetParent().AddChild(playerInstance);
            GD.Print("spawn point index: " + playerSpawnpointIndex);
            Marker3D spawnpoint = spawnpointContainer.GetNode<Marker3D>("Spawn" + playerSpawnpointIndex);
            playerInstance.TeleportToSpawnpoint(spawnpoint);
            playerSpawnpointIndex++;

            GameController.Instance.CurrentPlayerCount += 1;
        }
    }
}
