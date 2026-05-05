using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.LoadScene("SelectChar");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
