using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GoToStartGame()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    public void GoToRules()
    {
        SceneManager.LoadScene("RulesScene");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }
}