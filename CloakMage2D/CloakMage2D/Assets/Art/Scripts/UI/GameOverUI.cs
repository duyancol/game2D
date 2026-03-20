using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject gameOverPanel;

    [Header("Options")]
    public bool pauseGameWhenShow = true;

    void Awake()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void OnEnable()
    {
        // phòng trường hợp object này bật lại khi load scene
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void Show()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);

        if (pauseGameWhenShow)
            Time.timeScale = 0f; // PAUSE -> UI click vẫn ăn
    }

    public void Hide()
    {
        if (pauseGameWhenShow)
            Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Map_Grassland");
    }


    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
