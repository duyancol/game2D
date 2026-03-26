
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text powerText;

    //void Start()
    //{
    //    GameManager.Instance.OnPlayerLoaded += ShowPlayerInfo;
    //    ShowPlayerInfo();
    //}
    void Start()
    {
        if (GameManager.Instance == null)
            return;
        
        if (GameManager.Instance.playerData != null)
        {
           
            ShowPlayerInfo();   // nếu data đã có → hiện ngay
        }
        else
        {
            
            GameManager.Instance.OnPlayerLoaded += ShowPlayerInfo;
        }
    }
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLoaded -= ShowPlayerInfo;
        }
    }

    void ShowPlayerInfo()
    {
        if (GameManager.Instance == null)
            return;

        var player = GameManager.Instance.playerData;

        if (player == null)
            return;

        if (nameText == null || levelText == null || powerText == null)
            return;

        nameText.text = player.name;
        levelText.text = player.level.ToString();
        powerText.text = player.power.ToString();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Map_Grassland 1", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}