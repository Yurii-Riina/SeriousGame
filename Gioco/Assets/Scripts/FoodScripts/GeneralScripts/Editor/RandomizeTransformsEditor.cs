using UnityEngine;
using UnityEditor;

public class RandomizeTransformsEditor : EditorWindow
{
    private Vector3 positionRange = new Vector3(0.02f, 0.0f, 0.02f);
    private Vector3 rotationRange = new Vector3(0f, 360f, 0f);
    private Vector3 boundsHalfSize = new Vector3(0.1f, 0.1f, 0.1f);

    [MenuItem("Tools/Randomize Transforms with Bounds")]
    public static void ShowWindow()
    {
        GetWindow<RandomizeTransformsEditor>("Randomize Transforms");
    }

    private void OnGUI()
    {
        GUILayout.Label("Randomize Children Transforms", EditorStyles.boldLabel);

        positionRange = EditorGUILayout.Vector3Field("Position Range (+/-)", positionRange);
        rotationRange = EditorGUILayout.Vector3Field("Rotation Range", rotationRange);
        boundsHalfSize = EditorGUILayout.Vector3Field("Bounding Box Half-Size", boundsHalfSize);

        if (GUILayout.Button("Randomize Selected Object Children"))
        {
            RandomizeSelected();
        }
    }

    private void RandomizeSelected()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Nessun oggetto selezionato.");
            return;
        }

        Transform parent = Selection.activeTransform;
        Vector3 center = Vector3.zero;

        Undo.RegisterFullObjectHierarchyUndo(parent, "Randomize Transforms");

        foreach (Transform child in parent)
        {
            // Posizione offset
            Vector3 posOffset = new Vector3(
                Random.Range(-positionRange.x, positionRange.x),
                Random.Range(-positionRange.y, positionRange.y),
                Random.Range(-positionRange.z, positionRange.z)
            );

            Vector3 newLocalPos = child.localPosition + posOffset;

            // Clamp ai bounds
            newLocalPos.x = Mathf.Clamp(newLocalPos.x, -boundsHalfSize.x, boundsHalfSize.x);
            newLocalPos.y = Mathf.Clamp(newLocalPos.y, -boundsHalfSize.y, boundsHalfSize.y);
            newLocalPos.z = Mathf.Clamp(newLocalPos.z, -boundsHalfSize.z, boundsHalfSize.z);

            // Rotazione offset
            Vector3 rotOffset = new Vector3(
                Random.Range(-rotationRange.x / 2f, rotationRange.x / 2f),
                Random.Range(-rotationRange.y / 2f, rotationRange.y / 2f),
                Random.Range(-rotationRange.z / 2f, rotationRange.z / 2f)
            );

            child.localPosition = newLocalPos;
            child.localEulerAngles += rotOffset;
        }

        Debug.Log($"Randomizzati {parent.childCount} oggetti figli con bounding box.");
    }
}
