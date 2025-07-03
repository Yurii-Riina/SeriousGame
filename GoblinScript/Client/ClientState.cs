/// <summary>
/// Stati possibili del client.
/// </summary>
public enum ClientState
{
    MovingToOrder,
    WaitingInQueue,
    Ordering,
    Angry,
    Leaving
}