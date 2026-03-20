////using UnityEngine;
////using UnityEngine.SceneManagement;

////public class MainMenuUI : MonoBehaviour
////{
////    public void PlayGame()
////    {
////        // SceneManager.LoadScene("Map_Grassland");
////        SceneManager.LoadScene("Map_Grassland", LoadSceneMode.Additive);

////        // Unload MainMenu
////        SceneManager.UnloadSceneAsync("MainMenu");
////    }

////    public void QuitGame()
////    {
////        Application.Quit();
////    }
////}
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class MainMenuUI : MonoBehaviour
//{
//    public Text nameText;
//    public Text levelText;
//    public Text powerText;
//    //public Text goldText;

//    //void Start()
//    //{
//    //    if (GameManager.Instance == null)
//    //        return;
//    //    Debug.Log("GameManager == null");
//    //    if (GameManager.Instance.playerData != null)
//    //    {
//    //        Debug.Log("GameManager != null");
//    //        ShowPlayerInfo();   // nếu data đã có → hiện ngay
//    //    }
//    //    else
//    //    {
//    //        Debug.Log("GameManager === null");
//    //        GameManager.Instance.OnPlayerLoaded += ShowPlayerInfo;
//    //    }
//    //}

//    void Start()
//    {
//        if (GameManager.Instance == null)
//        {
//            Debug.LogError("GameManager NULL");
//            return;
//        }

//        if (GameManager.Instance.playerData != null)
//        {
//            Debug.Log("Player data ready");
//            ShowPlayerInfo();
//        }
//        else
//        {
//            Debug.Log("Waiting player data...");
//            GameManager.Instance.OnPlayerLoaded += ShowPlayerInfo;
//        }
//    }



//    //void ShowPlayerInfo()
//    //{
//    //     Debug.Log("nameText: " + nameText);
//    //    Debug.Log("levelText: " + levelText);
//    //    Debug.Log("powerText: " + powerText);

//    //    if (GameManager.Instance == null)
//    //    {
//    //        Debug.LogError("GameManager null");
//    //        return;
//    //    }

//    //    if (GameManager.Instance.playerData == null)
//    //    {
//    //        Debug.LogError("PlayerData null");
//    //        return;
//    //    }

//    //    var player = GameManager.Instance.playerData;

//    //    nameText.text = player.name;
//    //    levelText.text =""+ player.level;
//    //    powerText.text = ""+player.power;
//    //    Debug.Log("UI UPDATED SUCCESS");

//    //    //goldText.text = "Gold: " + GameManager.Instance.gold;
//    //}
//    void ShowPlayerInfo()
//    {
//        if (GameManager.Instance == null)
//        {
//            Debug.LogError("GameManager null");
//            return;
//        }

//        if (GameManager.Instance.playerData == null)
//        {
//            Debug.LogError("PlayerData null");
//            return;
//        }

//        if (nameText == null || levelText == null || powerText == null)
//        {
//            Debug.LogError("UI Text not assigned!");
//            return;
//        }

//        var player = GameManager.Instance.playerData;

//        nameText.text = player.name;
//        levelText.text = player.level.ToString();
//        powerText.text = player.power.ToString();

//        Debug.Log("UI UPDATED SUCCESS");
//    }
//    public void PlayGame()
//    {
//        SceneManager.LoadScene("Map_Grassland", LoadSceneMode.Additive);
//        SceneManager.UnloadSceneAsync("MainMenu");
//    }

//    public void QuitGame()
//    {
//        Application.Quit();
//    }


//}
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
        SceneManager.LoadScene("Map_Grassland", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}