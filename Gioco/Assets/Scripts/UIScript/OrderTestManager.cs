using UnityEngine;

public class OrderTestManager : MonoBehaviour //serve solo per il test della lavagnetta, poi andrà tolto
{
    public OrderBoardUI boardUI;
    public float interval = 5f;  // Ogni quanti secondi generare un ordine

    private float timer;

    void Start()
    {
        timer = interval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            GenerateOrder();
            timer = interval;
        }
    }

    void GenerateOrder()
    {
        Order newOrder = Order.GenerateRandomOrder();

        if (boardUI != null)
        {
            boardUI.AddOrderToBoard(newOrder);
        }

        Debug.Log("Nuovo ordine generato: " +
            string.Join(", ", newOrder.Ingredients));
    }
}
