using UnityEngine;
using System.Collections;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Various Elements")]
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Transform hand;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    private Transform tray;

    [Header("Scripts")]
    [SerializeField] private PickUpAndPlace pickUpAndPlace;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private GrillCookingManager[] grillManagers; //usiamo un array così funziona per qualsiasi griglia
    [SerializeField] private ToasterManager toasterManager;
    [SerializeField] private FryerBasketMover[] fryerBasketMovers;
    [SerializeField] private FryerPackagingSpawner fryerPackagingSpawner;

    private Coroutine tutorialCoroutine;

    void Awake()
    {
        if (tutorialText == null)
        {
            Debug.LogError("Tutorial Text is not assigned in the inspector.");
        }

        if (hand == null)
        {
            Debug.LogError("Hand Transform is not assigned in the inspector.");
        }

        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is not assigned in the inspector.");
        }

        if (pickUpAndPlaceScript == null)
        {
            Debug.LogError("PickUpAndPlace script is not assigned in the inspector.");
        }

        if (placeOnTrayScript == null)
        {
            Debug.LogError("PlaceOnTray script is not assigned in the inspector.");
        }

        if (grillManagers == null)
        {
            Debug.LogError("GrillCookingManager script is not assigned in the inspector.");
        }

        if (toasterManager == null)
        {
            Debug.LogError("ToasterManager script is not assigned in the inspector.");
        }

        if (fryerBasketMovers == null)
        {
            Debug.LogError("FryerBasketMover script is not assigned in the inspector.");
        }

        if (fryerPackagingSpawner == null)
        {
            Debug.LogError("FryerPackagingSpawner script is not assigned in the inspector.");
        }

    }
    void Start()
    {
        tutorialCoroutine = StartCoroutine(RunTutorial());
    }

    void Update()
    {
        // Controlla se l'utente preme ESC per saltare il tutorial
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipTutorial();
        }
    }

    private void SkipTutorial()
    {
        // Se il tutorial è in corso, fermalo
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator RunTutorial()
    {
        //inizio gioco
        tutorialText.text = "Welcome to the game! Use WASD to move around and move the mouse to control the camera.";
        yield return new WaitForSeconds(4f);

        //skip
        tutorialText.text = "You can skip the tutorial by pressing ESC";
        yield return new WaitForSeconds(4f);

        //prendere gli oggetti
        tutorialText.text = "Pick the object that you want from the package by pressing 'E'. (E for top bread or double E for bottom bread)";
        yield return new WaitUntil(() => hand.childCount > 0);

        //posizionare gli oggetti
        tutorialText.text = "Place the tray on the table by left-clicking on it, or drop it by right-clicking anywhere.";
        yield return new WaitUntil(() => hand.childCount == 0);

        //inizio preparazione del vassoio
        tutorialText.text = "Place drink, donut, and burger carton on the tray by pressing 'F' on it while holding them.";
        yield return new WaitUntil(() => placeOnTrayScript.currentTray != null);
        Transform tray = placeOnTrayScript.currentTray;
        yield return new WaitUntil(() => tray.childCount >= 7);

        //cottura pane
        tutorialText.text = "Press 'C' to place the bread in the toaster.";
        yield return new WaitUntil(() => toasterManager.IsCooking.Any(c => c)); //controlla se almeno un elemento dell'array isCooking è true

        //attesa cottura pane
        tutorialText.text = "Wait for the bread to cook.";
        yield return new WaitForSeconds(toasterManager.cookingTime);

        //prendere il pane cotto
        tutorialText.text = "Pick the cooked bread by left-clicking on it and place it in the carton burger.";
        yield return new WaitUntil(() => hand.childCount > 0 && (hand.GetChild(0).name == "TopCookedBread" || hand.GetChild(0).name == "BottomCookedBread"));

        //cottura carne
        tutorialText.text = "Move the grill up and down by pressing 'R' and place the meat on it by pressing 'C' to start cooking it.";
        yield return new WaitUntil(() => grillManagers.Any(manager => manager.IsCooking.Any(c => c))); //questa funzione del linq controlla se almeno un elemento dell'array isCooking è true

        //attesa cottura
        tutorialText.text = "Wait for the meat to cook.";
        yield return new WaitForSeconds(grillManagers[0].CookingTime);

        //metti carne cotta nella vaschetta
        tutorialText.text = "Place the cooked meat in the container by pressing 'Q' on the grill and press 'F' on the container to pick it up.";
        yield return new WaitUntil(() => hand.childCount > 0 && hand.GetChild(0).name == "CookedMeat");

        //altre informazioni
        tutorialText.text = "You can cook multiple pieces of meat at once";
        yield return new WaitForSeconds(4f);

        //altre informazioni
        tutorialText.text = "You can cook bacon by using the same method and keyboard keys.";
        yield return new WaitForSeconds(4f);

        //frittura patatine
        tutorialText.text = "Now let's fry! Move the fryer basket up and down by pressing 'R' and place the fries in it by pressing 'C'";
        yield return new WaitUntil(() => fryerBasketMovers.Any(f => f.IsCooking));

        //attesa cottura patatine
        tutorialText.text = "Wait for the fries to cook.";
        yield return new WaitForSeconds(fryerBasketMovers[0].cookingTime);

        //prendere le patatine
        tutorialText.text = "Put the fries in their spot by pressing 'Q' on them and then press 'E' to pack them.";
        yield return new WaitUntil(() => fryerPackagingSpawner.AllSpawnSlots.Any(slot => slot.childCount > 0 && slot.GetChild(0).name == "CookedFriesPack"));

        //piazzamento sul vassoio
        tutorialText.text = "Now you can place the fries pack on the tray by pressing 'F' on it.";
        yield return new WaitUntil(() => placeOnTrayScript.currentTray.childCount >= 8);

        //composizione del panino
        tutorialText.text = "It's time to prepare our first burger! Start adding ingredients in the burger carton by left-clicking on it.";
        Transform currentTray = placeOnTrayScript.currentTray;
        Transform burgerCarton = currentTray.Find("CartonHamburger");
        yield return new WaitUntil(() => burgerCarton != null && burgerCarton.childCount >= 3); //3 perchè il cartonHamburger ha già 2 figli quindi basta un ingrediente per far passare alla prossima parte del tutorial

        //prendi il vassoio
        tutorialText.text = "Now you can pick the tray up by left-clicking on it.";
        yield return new WaitUntil(() => hand.childCount > 0 && hand.GetChild(0).name == "Tray");

        //fine tutorial
        tutorialText.text = "Congratulations! You have completed the tutorial. Now you can start playing the game!";
        yield return new WaitForSeconds(7f);

        SceneManager.LoadScene("GameScene");
    }
}
