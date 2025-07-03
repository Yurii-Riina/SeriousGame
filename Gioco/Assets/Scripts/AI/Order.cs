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

        // Add side or dessert
        if (Random.value < 0.5f)
        {
            int roll = Random.Range(0, 3);
            if (roll == 0)
                newOrder.Ingredients.Add(Ingredient.Fries);
            else if (roll == 1)
                newOrder.Ingredients.Add(Ingredient.Nuggets);
            else
                newOrder.Ingredients.Add(Ingredient.Donut);
        }

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
            case BurgerType.TheNameless:
                return new List<Ingredient> {
                    Ingredient.Hamburger, Ingredient.Onion, Ingredient.Pickles, Ingredient.Cheese };
            default:
                return new List<Ingredient>();
        }
    }

    /// <summary>
    /// Checks if the delivered ingredients match the order.
    /// </summary>
    public bool Matches(List<Ingredient> delivered)
    {
        var expected = new List<Ingredient>(Ingredients);
        var provided = new List<Ingredient>(delivered);

        // Find burger indices in delivered
        int start = provided.IndexOf(Ingredient.BottomBun);
        int end = provided.LastIndexOf(Ingredient.TopBun);

        if (start == -1 || end == -1 || end <= start)
            return false;

        var burgerContents = provided.GetRange(start, end - start + 1);

        var expectedBurger = expected.FindAll(i =>
            i == Ingredient.BottomBun || i == Ingredient.TopBun ||
            i == Ingredient.Hamburger || i == Ingredient.Bacon ||
            i == Ingredient.Cheese || i == Ingredient.Onion ||
            i == Ingredient.Lettuce || i == Ingredient.Pickles ||
            i == Ingredient.Tomato);

        foreach (var item in burgerContents)
        {
            if (!expectedBurger.Remove(item))
                return false;
        }
        if (expectedBurger.Count > 0)
            return false;

        var otherExpected = expected.Except(burgerContents).ToList();
        var otherProvided = provided.Except(burgerContents).ToList();

        foreach (var item in otherProvided)
        {
            if (!otherExpected.Remove(item))
                return false;
        }
        return otherExpected.Count == 0;
    }
}