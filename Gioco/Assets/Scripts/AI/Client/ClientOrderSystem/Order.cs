using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Represents an order made by a client.
/// </summary>
[System.Serializable]
public class Order
{
    public List<Ingredient> Ingredients { get; private set; } = new List<Ingredient>();
    public BurgerType? SelectedBurger { get; private set; } = null;

    /// <summary>
    /// Generates a random order.
    /// </summary>
    public static Order GenerateRandomOrder()
    {
        Order newOrder = new Order();

        // Randomize burger
        if (Random.value < 0.8f)
        {
            var burger = (BurgerType)Random.Range(0, System.Enum.GetValues(typeof(BurgerType)).Length);
            newOrder.SelectedBurger = burger;
            var burgerItems = GetIngredientsForBurger(burger);
            burgerItems.Insert(0, Ingredient.BottomBun);
            burgerItems.Add(Ingredient.TopBun);
            newOrder.Ingredients.AddRange(burgerItems);
        }

        // Add drink
        if (Random.value < 0.9f)
            newOrder.Ingredients.Add((Ingredient)Random.Range((int)Ingredient.Water, (int)Ingredient.Fanta + 1));

        // Add side
        if (Random.value < 0.5f)
        {
            int roll = Random.Range(0, 2);
            if (roll == 0)
                newOrder.Ingredients.Add(Ingredient.Fries);
            else
                newOrder.Ingredients.Add(Ingredient.Nuggets);
        }

        // Add dessert
        if (Random.value < 0.3f)
        {
            newOrder.Ingredients.Add(Ingredient.Donut);
        }
        Debug.Log("Client expects: " + string.Join(", ", newOrder.Ingredients.Select(i => i.ToString())));
        return newOrder;
    }

    /// <summary>
    /// Returns the list of ingredients for a given burger type.
    /// </summary>
    private static List<Ingredient> GetIngredientsForBurger(BurgerType type)
    {
        switch (type)
        {
            case BurgerType.American:
                return new List<Ingredient> { Ingredient.Hamburger, Ingredient.Bacon, Ingredient.Cheese };
            case BurgerType.SpecialBurger:
                return new List<Ingredient> {
                    Ingredient.Hamburger, Ingredient.Bacon, Ingredient.Cheese,
                    Ingredient.Onion, Ingredient.Lettuce, Ingredient.Pickles, Ingredient.Tomato };
            case BurgerType.Veggie:
                return new List<Ingredient> {
                    Ingredient.Onion, Ingredient.Lettuce, Ingredient.Pickles, Ingredient.Tomato };
            case BurgerType.BabyBurger:
                return new List<Ingredient> { Ingredient.Hamburger, Ingredient.Bacon };
            case BurgerType.Nameless:
                return new List<Ingredient> {
                    Ingredient.Hamburger, Ingredient.Onion, Ingredient.Pickles, Ingredient.Cheese };
            default:
                return new List<Ingredient>();
        }
    }

    /// <summary>
    /// Checks if the delivered ingredients match the order.
    /// Extra ingredients not requested are tolerated.
    /// </summary>
    public bool Matches(List<Ingredient> delivered)
    {
        if (Ingredients == null || Ingredients.Count == 0)
            return delivered == null || delivered.Count == 0;

        // Se è un ordine senza burger
        bool hasBurger = Ingredients.Contains(Ingredient.BottomBun) && Ingredients.Contains(Ingredient.TopBun);

        if (hasBurger)
        {
            // Verifica che ci sia il burger intero
            int start = delivered.IndexOf(Ingredient.BottomBun);
            int end = delivered.LastIndexOf(Ingredient.TopBun);

            if (start == -1 || end == -1 || end <= start)
                return false;

            var burgerContents = delivered.GetRange(start, end - start + 1);
            var expectedBurger = Ingredients.Where(i =>
                i == Ingredient.BottomBun || i == Ingredient.TopBun ||
                i == Ingredient.Hamburger || i == Ingredient.Bacon ||
                i == Ingredient.Cheese || i == Ingredient.Onion ||
                i == Ingredient.Lettuce || i == Ingredient.Pickles ||
                i == Ingredient.Tomato
            ).ToList();

            // Confronta contenuto del burger in modo rigoroso (quantità e tipo)
            if (!SameContents(expectedBurger, burgerContents))
                return false;

            // Confronta il resto degli ingredienti fuori dal burger
            var expectedOutside = Ingredients.Except(expectedBurger).ToList();
            var deliveredOutside = new List<Ingredient>(delivered);
            deliveredOutside.RemoveRange(start, end - start + 1);

            if (!SameContents(expectedOutside, deliveredOutside))
                return false;

            return true;
        }
        else
        {
            // Nessun burger: confronta tutto come "altri ingredienti"
            return SameContents(Ingredients, delivered);
        }
    }

    /// <summary>
    /// Verifica che due liste abbiano esattamente gli stessi ingredienti e quantità.
    /// </summary>
    private bool SameContents(List<Ingredient> expected, List<Ingredient> actual)
    {
        var expectedCounts = expected.GroupBy(i => i).ToDictionary(g => g.Key, g => g.Count());
        var actualCounts = actual.GroupBy(i => i).ToDictionary(g => g.Key, g => g.Count());

        if (expectedCounts.Count != actualCounts.Count)
            return false;

        foreach (var kvp in expectedCounts)
        {
            if (!actualCounts.TryGetValue(kvp.Key, out int count) || count != kvp.Value)
                return false;
        }

        return true;
    }
    
    public string GetReadableDescription()
    {
        if (Ingredients == null || Ingredients.Count == 0) return "(Vuoto)";
        return string.Join(", ", Ingredients);
    }
}
