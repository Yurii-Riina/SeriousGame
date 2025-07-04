using System.Collections.Generic;
using UnityEngine;

public class TrayTestTool : MonoBehaviour
{
    [SerializeField] private Transform hand; // 🔹 Riferimento alla "mano"
    [SerializeField] private PlaceOnTray placeOnTray; // 🔹 Script dove c'è GetObjectsOnTray()

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // T per "Test"
        {
            // Verifica se in mano c'è qualcosa
            if (hand.childCount > 0)
            {
                Transform heldObject = hand.GetChild(0);

                // Verifica se è un tray
                if (heldObject.name.Contains("Tray"))
                {
                    Debug.Log($"Tray in mano: {heldObject.name}");

                    List<GameObject> trayObjects = placeOnTray.GetObjectsOnTray(heldObject);
                    Debug.Log($"Trovati {trayObjects.Count} oggetti sul tray '{heldObject.name}':");

                    foreach (GameObject obj in trayObjects)
                    {
                        Debug.Log($" - {obj.name}");
                    }
                }
                else
                {
                    Debug.Log("L'oggetto in mano non è un tray.");
                }
            }
            else
            {
                Debug.Log("Nessun oggetto nella mano.");
            }
        }
    }
}

