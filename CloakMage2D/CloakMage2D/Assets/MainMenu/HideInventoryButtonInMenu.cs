using UnityEngine;
using UnityEngine.SceneManagement;

public class HideInventoryButtonInMenu : MonoBehaviour
{
    public GameObject buttonTui;

    void Start()
    {
        CheckScene(); // Check ngay khi game start
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene();
    }

    void CheckScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        //Debug.Log("Current Scene: " + currentScene);

       // buttonTui.SetActive(currentScene != "MainMenu");
        //buttonTui.SetActive(currentScene != "Map_Grassland");
    }
}
