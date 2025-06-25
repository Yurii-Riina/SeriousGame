using System.Collections.Generic;
using UnityEngine;

public class StackPointRestrictions : MonoBehaviour
{
    [SerializeField] private List<string> allowedTags = new List<string>();

    public bool IsTagAllowed(string tag)
    {
        return allowedTags.Contains(tag);
    }
}
