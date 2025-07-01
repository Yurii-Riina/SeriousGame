using UnityEngine;
using System.Collections;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Transform hand;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PickUpAndPlace pickUpAndPlaceScript;
    private Transform tray;
    [SerializeField] private PlaceOnTray placeOnTrayScript;
    [SerializeField] private GrillCookingManager grillCookingManager;
    private Coroutine tutorialCoroutine;
    
    void Start()
    {
        tutorialCoroutine =StartCoroutine(RunTutorial());
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
        // Carica subito la scena desiderata, ad esempio "MainMenu"
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator RunTutorial()
    {
        //inizio gioco
        tutorialText.text = "Welcome to the game! Use WASD to move around and move the mouse to control the camera. (You can skip the tutorial by pressing ESC)";
        yield return new WaitForSeconds(4f);

        //prendere gli oggetti
        tutorialText.text = "Pick the object that you want from the package by pressing 'E'. (E for top bread or double E for bottom bread)";
        yield return new WaitUntil(() => hand.childCount > 0);

        //posizionare gli oggetti
        tutorialText.text = "Place the tray on the table by left-clicking on it, or drop it by right-clicking anywhere.";
        yield return new WaitUntil(() => hand.childCount == 0);

        //inizio preparazione del vassoio
        tutorialText.text = "Place drinks, donut, and cartons on the tray by pressing 'F' on it while holding them.";
        yield return new WaitUntil(() => placeOnTrayScript.currentTray != null);
        Transform tray = placeOnTrayScript.currentTray;
        yield return new WaitUntil(() => tray.childCount >= 8);

        //cottura carne
        tutorialText.text = "Move the grill up and down by pressing 'R' and place the meat on it by pressing 'C' to start cooking it.";
        yield return new WaitUntil(() => grillCookingManager.IsCooking.Any(c => c)); //questa funzione del linq controlla se almeno un elemento dell'array isCooking è true

        //attesa cotture
        tutorialText.text = "Wait for the meat to cook.";
        yield return new WaitForSeconds(grillCookingManager.CookingTime);

        //metti carne cotta nella vaschetta
        tutorialText.text = "Place the cooked meat in the container by pressing 'Q' on the grill and press 'F' on the container to pick it up.";
        yield return new WaitUntil(() => hand.childCount > 0 && hand.GetChild(0).name == "CookedMeat");

        //altre informazioni
        tutorialText.text = "You can cook multiple pieces of meat at once";
        yield return new WaitForSeconds(4f);

        //altre informazioni
        tutorialText.text = "You can cook bacon and bread by using the same method and keyboard keys.";
        yield return new WaitForSeconds(4f);

        //altre informazioni
        tutorialText.text = "You can cook fries and nuggets in the fryer by using the same method and keyboard keys.";
        yield return new WaitForSeconds(4f);

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

        SceneManager.LoadScene("MainMenu"); //per ora torniamo al menu principale. quando avremo creato il livello di gioco la cambiamo
    }
}
