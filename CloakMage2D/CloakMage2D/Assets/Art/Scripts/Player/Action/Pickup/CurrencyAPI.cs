using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CurrencyResponse
{
    public int gold;
    public int gem;
}

public class CurrencyAPI : MonoBehaviour
{
   
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void LoadCurrency(int playerId)
    {
        Debug.Log("LoadCurrency called with id: " + playerId);
        StartCoroutine(GetCurrency(playerId));
    }

    //IEnumerator GetCurrency(int playerId)
    //{
    //    UnityWebRequest request = UnityWebRequest.Get(apiUrl + playerId);

    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        CurrencyResponse data =
    //            JsonUtility.FromJson<CurrencyResponse>(request.downloadHandler.text);

    //        Debug.Log("Gold from server: " + data.gold);

    //        GameManager.Instance.SetGold(data.gold);
    //    }
    //    else
    //    {
    //        Debug.LogError("API Error: " + request.error);
    //    }
    //}
    IEnumerator GetCurrency(int playerId)
    {
       
        string url = ApiConfigLoader.Config.baseUrl + "/api/currency/" + playerId;

        Debug.Log("Request URL: " + url);

        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        Debug.Log("Result: " + request.result);
        Debug.Log("Response: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            CurrencyResponse data =
                JsonUtility.FromJson<CurrencyResponse>(request.downloadHandler.text);

            Debug.Log("Gold from server: " + data.gold);

            GameManager.Instance.SetGold(data.gold);
        }
        else
        {
            Debug.LogError("API Error: " + request.error);
        }
    }
    public void AddGold(int playerId, int amount)
    {
        StartCoroutine(AddGoldCoroutine(playerId, amount));
    }

    IEnumerator AddGoldCoroutine(int playerId, int amount)
    {
        string url = "https://userservice-production-fd72.up.railway.app/api/currency/addGold?playerId="
                     + playerId + "&amount=" + amount;

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Gold added on server");

            GameManager.Instance.AddGold(amount);
        }
        else
        {
            Debug.LogError("Add gold failed: " + request.error);
        }
    }
}