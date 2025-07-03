using UnityEngine;

public enum GrillableType
{
    Hamburger,
    Bacon,
    Fries,
    Nuggets
}

public class Grillable : MonoBehaviour
{
    public GrillableType type;
}
