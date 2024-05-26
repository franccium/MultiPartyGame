using Godot;

public interface IInteractable
{
    void OnHover();
    void OnInteract();
}

public interface IPushable
{
    void OnPush(Vector3 pushDirection);
}

public interface IHoldable
{
    void OnHold(Vector3 holdPosition);
}