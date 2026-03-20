//using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;

//public class PlayerLoader : MonoBehaviour
//{
//    private string baseUrl = "https://userservice-production-fd72.up.railway.app/api/player/";

//    void Start()
//    {
//        StartCoroutine(LoadPlayer());
//    }

//    IEnumerator LoadPlayer()
//    {
//        string url = baseUrl + PlayerSession.UserId;

//        UnityWebRequest request = UnityWebRequest.Get(url);

       

//        yield return request.SendWebRequest();

//        if (request.result == UnityWebRequest.Result.Success)
//        {
//            string json = request.downloadHandler.text;
//            Debug.Log("Player JSON: " + json);

//            PlayerData player = JsonUtility.FromJson<PlayerData>(json);

//            GameManager.Instance.SetPlayerData(player);
//        }
//        else
//        {
//            Debug.LogError("API Error: " + request.error);
//        }
//    }
//}
