using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private enum States
    {
        Stand,
        Walk,
        Sprint,
        Jump,
        InAir,
    }
    private States _currentState = States.Stand;

    private Node3D _arms;
    private MeshInstance3D _playerBody;

    public const float BASE_SPEED = 15.0f;
    private const float SPRINT_SPEED = 30f;
    public const float BASE_JUMP_VELOCITY = 8f;
    public float CurrSpeed { get; set; } = 15f;
    private bool _canMove = true;

    private Camera3D _camera;
    public const float CAMERA_SENSITIVITY = 2.5f;

    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private short _maxJumps = 5;
    private short _remainingJumps;

    private MultiplayerSynchronizer _multiplayerSynchronizer;
    private bool _hasAuthority;
    public int PlayerId { get; set; }
    private Vector3 _syncedPosition;
    private Vector3 _syncedRotation;
    public string PlayerName { get; set; }
    private Label3D _playerNameLabel;
    public int RoomSpawnpointIndex { get; set; } = 1;
    public int Score { get; set; } = 0;

    private RayCast3D _crosshairRayCast;
    private CenterContainer _crosshair;

    public override void _Ready()
    {
        _multiplayerSynchronizer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        _multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name)); // players node name is currently being set to his server ID in PlayerManager.cs
        PlayerId = int.Parse(Name);

        GD.Print("Player " + Name + " ready");
        _arms = GetNode<Node3D>("Head/Arms");
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _camera = GetNode<Camera3D>("Head/Camera3D");
        _playerNameLabel = GetNode<Label3D>("PlayerNameLabel");
        _playerNameLabel.Text = PlayerName;

        _crosshairRayCast = GetNode<RayCast3D>("Head/Camera3D/CrosshairRaycast");
        _crosshairRayCast.AddException(this);
        _crosshairRayCast.TargetPosition = new Vector3(0, -1000f, 0);
        _crosshair = GetNode<CenterContainer>("GUI/Crosshair");

        _playerBody = GetNode<MeshInstance3D>("Body");

        _hasAuthority = _multiplayerSynchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
        if (_hasAuthority)
            GameController.Instance.CurrentPlayer = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_hasAuthority)
        {
            GlobalPosition = GlobalPosition.Lerp(_syncedPosition, 0.5f);
            Rotation = Rotation.Lerp(_syncedRotation, 0.5f);
            return;
        }

        _camera.Current = true;
        if (_canMove)
        {
            Vector3 direction = GetInput();
            HandleMovement(direction, delta);
        }

        UpdateRaycastCollision();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        /// Rotates the player model and the camera based on mouse movement
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion motion = @event as InputEventMouseMotion;
            Rotation = new Vector3(Rotation.X, Rotation.Y - motion.Relative.X * 0.001f * CAMERA_SENSITIVITY, Rotation.Z);
            // clamp to make it impossible to turn upside down
            _camera.Rotation = new Vector3(Mathf.Clamp(_camera.Rotation.X - motion.Relative.Y * 0.001f * CAMERA_SENSITIVITY, -2, 2), _camera.Rotation.Y, _camera.Rotation.Z);
        }
    }

    private void ToggleMouseMode()
    {
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    /// <summary>
    /// gets input from the player and changes his state accordingly
    /// </summary>
    private Vector3 GetInput()
    {
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backwards");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            if (Input.IsActionPressed("sprint"))
                _currentState = States.Sprint;
            else
                _currentState = States.Walk;
        }
        else
            _currentState = States.Stand;

        if (Input.IsActionJustPressed("jump") && _remainingJumps > 0)
        {
            _currentState = States.Jump;
            _remainingJumps--;
        }
        if (IsOnFloor())
            _remainingJumps = _maxJumps;
        else
        {
            _currentState = States.InAir;
            if (Input.IsActionJustPressed("jump") && _remainingJumps > 0)
            {
                _currentState = States.Jump;
                _remainingJumps--;
            }
        }

        if (Input.IsActionJustPressed("escape"))
        {
            GameController.Instance.GoToSettings();
        }

        return direction;
    }

    #region MOVEMENT
    private void HandleMovement(Vector3 direction, double delta)
    {
        Vector3 velocity = Velocity;
        CurrSpeed = BASE_SPEED;

        if (_currentState == States.Sprint)
            CurrSpeed = SPRINT_SPEED;

        if (_currentState == States.Jump)
            velocity.Y = BASE_JUMP_VELOCITY;

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * CurrSpeed;
            velocity.Z = direction.Z * CurrSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrSpeed);
        }

        if (_currentState == States.InAir)
            velocity.Y -= gravity * (float)delta;

        Velocity = velocity;
        MoveAndSlide();
        _syncedPosition = GlobalPosition;
        _syncedRotation = Rotation;
    }

    public void Teleport(Vector3 position) => GlobalPosition = position;
    public void TeleportToSpawnpoint(Marker3D spawnpoint) => GlobalPosition = spawnpoint.GlobalPosition;

    #endregion

    private void UpdateRaycastCollision()
    {
        if (!_crosshairRayCast.IsColliding()) return;

        GodotObject collider = _crosshairRayCast.GetCollider();
        if (collider is IInteractable interactable)
        {
            _crosshair.Modulate = new Color(0, 1, 0);
            interactable.OnHover();
            if (Input.IsActionJustPressed("lmb"))
                interactable.OnInteract();
        }
        else
            _crosshair.Modulate = new Color(1, 1, 1);

        if (collider is Player other_player)
        {
            if (Input.IsActionJustPressed("lmb"))
            {
                Vector3 pushDirection = (other_player.GlobalPosition - GlobalPosition).Normalized();
                Vector3 newPlayerPosition = GlobalPosition - pushDirection * 3f;
                //other_player.ForceSynchronisePosition(newPlayerPosition);
                RpcUpdatePlayerPosition(other_player.PlayerId, newPlayerPosition);
                //RpcId(MultiplayerMenu.HOST_ID, "RpcUpdatePlayerPosition", other_player.PlayerId, newPlayerPosition);
            }
        }
    }

    public void SetPlayerAlbedoColor(Color color)
    {
        StandardMaterial3D material = new StandardMaterial3D
        {
            AlbedoColor = color
        };
        _playerBody.SetSurfaceOverrideMaterial(0, material);
    }

    public void SetCanMove(bool can_move) => _canMove = can_move;


    #region MULTIPLAYER

    public void RpcUpdatePlayerPosition(int player_id, Vector3 position)
    {
        Rpc("UpdatePlayerPosition", player_id, position);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void UpdatePlayerPosition(int player_id, Vector3 position)
    {
        if (player_id == PlayerId)
            return;

        _syncedPosition = position;
        GlobalPosition = position;
    }

    #endregion


    #region GUI

    public void SetMouseCursorVisible(bool visible) => Input.MouseMode = visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

    #endregion
}
