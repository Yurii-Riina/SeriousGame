using UnityEngine;
using UnityEngine.UI;

public class XManager : MonoBehaviour
{
    public Image[] xImages;
    public Sprite xWhite;
    public Sprite xRed;
    private int currentIndex = 0;

    public GameObject defeatScreen;      
    public GameObject xUIContainer;  

    public void TurnNextXRed()
    {
        if (currentIndex < xImages.Length)
        {
            xImages[currentIndex].sprite = xRed;
            currentIndex++;
        }

        if (currentIndex >= xImages.Length)
        {
            ShowDefeatScreen();
        }
    }

    public void ResetXs()
    {
        foreach (var img in xImages)
        {
            img.sprite = xWhite;
        }
        currentIndex = 0;
    }

    private void ShowDefeatScreen()
    {
        Debug.Log("💀 Hai perso!");

        if (xUIContainer != null)
            xUIContainer.SetActive(false);

        defeatScreen.SetActive(true);   

        Cursor.lockState = CursorLockMode.None;  
        Cursor.visible = true;                 

        Time.timeScale = 0f; 
    }
}
