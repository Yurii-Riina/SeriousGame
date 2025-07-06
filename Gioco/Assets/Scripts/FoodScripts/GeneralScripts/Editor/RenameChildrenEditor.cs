using UnityEngine;
using UnityEditor;

public class RenameChildrenEditor : EditorWindow
{
    private string baseName = "FriesCooked";

    [MenuItem("Tools/Rename Children")]
    public static void ShowWindow()
    {
        GetWindow<RenameChildrenEditor>("Rename Children");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Children of Selected Object", EditorStyles.boldLabel);

        baseName = EditorGUILayout.TextField("Base Name", baseName);

        if (GUILayout.Button("Rename Children"))
        {
            RenameSelected();
        }
    }

    private void RenameSelected()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Nessun oggetto selezionato.");
            return;
        }

        Transform parent = Selection.activeTransform;
        int counter = 1;

        Undo.RegisterFullObjectHierarchyUndo(parent, "Rename Children");

        foreach (Transform child in parent)
        {
            child.name = $"{baseName}_{counter}";
            counter++;
        }

        Debug.Log($"Rinominati {parent.childCount} figli.");
    }
}
