using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class OrderUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderText;

    // Imposta il contenuto testuale dell’ordine.
    public void SetOrder(Order order)
    {
        if (orderText == null)
        {
            Debug.LogWarning("orderText non assegnato!");
            return;
        }

        // Genera il testo da mostrare
        string ingredientsText = string.Join(", ", order.Ingredients.Select(i => i.ToString()));
        orderText.text = ingredientsText;
    }
}
