using UnityEngine;
using UnityEngine.SceneManagement;

public class DefeatScreenUI : MonoBehaviour
{
    public GameObject defeatScreen;

    private void Start()
    {
        if (defeatScreen == null)
        {
            Debug.LogError("Defeat screen GameObject is not assigned in the inspector.");
            return;
        }
        defeatScreen.SetActive(false);
    }

    public void ShowDefeatScreen()
    {
        defeatScreen.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene"); 
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
