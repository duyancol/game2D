using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        // Load map additive
        SceneManager.LoadScene("Map_Grassland", LoadSceneMode.Additive);

        // Unload MainMenu
        SceneManager.UnloadSceneAsync("MainMenu");
    }
}
