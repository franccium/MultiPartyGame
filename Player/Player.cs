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
        InAnimation,
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
    private Vector3 _syncedPosition;
    private Vector3 _syncedRotation;

    public int PlayerId { get; set; } // id with which the player connected to the server
    public string PlayerName { get; set; } // player's personal nickname
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

        if (Input.IsActionJustPressed("t"))
        {
            _currentState = States.InAnimation;
            PlayAnimation(_testBindedAnimation);
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

        bool isLmbJustPressed = Input.IsActionJustPressed("lmb");
        bool isEJustPressed = Input.IsActionJustPressed("e");
        bool isEHeld = Input.IsActionPressed("e");

        if (collider is IInteractable interactable)
        {
            _crosshair.Modulate = new Color(0, 1, 0);
            interactable.OnHover();
            if (isLmbJustPressed)
                interactable.OnInteract();
        }
        else
            _crosshair.Modulate = new Color(1, 1, 1);

        if (collider is IPushable pushable && isEJustPressed)
        {
            Node3D colliderNode = collider as Node3D;
            Vector3 pushDirection = (colliderNode.GlobalPosition - GlobalPosition).Normalized();
            pushable.OnPush(pushDirection);
        }

        if (collider is IHoldable holdable && isEHeld)
        {
            holdable.OnHold(GlobalPosition);
        }


        if (collider is Player other_player && isLmbJustPressed)
        {
            Vector3 pushDirection = (other_player.GlobalPosition - GlobalPosition).Normalized();
            Vector3 newPlayerPosition = GlobalPosition - pushDirection * 3f;
            //other_player.ForceSynchronisePosition(newPlayerPosition);
            RpcUpdatePlayerPosition(other_player.PlayerId, newPlayerPosition);
            //RpcId(MultiplayerMenu.HOST_ID, "RpcUpdatePlayerPosition", other_player.PlayerId, newPlayerPosition);
        }

        //Image image = new Image();
        //image.GetPixel(0, 0);
        if (collider is ColorPicker3D colorPicker && isLmbJustPressed)
        {
            Vector3 collisionPoint = _crosshairRayCast.GetCollisionPoint();
            Vector3 collisionPointLocal = colorPicker.ToLocal(collisionPoint) / 2f;
            colorPicker.PickColorAtPosition(collisionPointLocal);
            /*
            Vector3 collidePosition = _crosshairRayCast.GetCollisionPoint();
            GD.Print("viewport collide");
            Image image = viewport.GetTexture().GetImage();
            Color color = image.GetPixel((int)collidePosition.X, (int)collidePosition.Y);
            GD.Print("picked color: " + color);
            */
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

    public void GetLookDirection(out Vector3 lookDirection)
    {
        lookDirection = _camera.GlobalTransform.Basis.Z;
    }


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


    #region ANIMATION

    private AnimationPlayer _animationPlayer;
    private string _currentAnimation;
    private string _testBindedAnimation = "eye_roll";

    public void PlayAnimation(string animation_name)
    {
        if (_animationPlayer == null)
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        _animationPlayer.Play(animation_name);
    }

    #endregion


    #region GARMENTS

    public enum PlayerHats { None, Knight, Dumb }

    public void SetPlayerHat(PlayerHats hat)
    {
        Rpc("RpcSetPlayerHat", (int)hat);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RpcSetPlayerHat(int hat)
    {
        PlayerHats playerHat = (PlayerHats)hat;
        MeshInstance3D prevHat = GetNodeOrNull<MeshInstance3D>("Head/HatMarker/Hat");
        if (playerHat == PlayerHats.None || prevHat != null)
        {
            prevHat.QueueFree();
            return;
        }

        PackedScene hatScene = ResourceLoader.Load<PackedScene>("res://Models/Hats/" + playerHat.ToString() + ".glb");
        MeshInstance3D hatMesh = hatScene.Instantiate() as MeshInstance3D;
        GetNode<Node3D>("Head/HatMarker").AddChild(hatMesh);
        hatMesh.Position = new Vector3(0, 0, 0);

        GD.Print("Hat set to " + hat.ToString());
    }

    #endregion


    #region GUI

    public void SetMouseCursorVisible(bool visible) => Input.MouseMode = visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

    #endregion
}
