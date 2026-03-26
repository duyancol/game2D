using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CurrencySync : MonoBehaviour
{
    public static CurrencySync Instance;

    int pendingGold = 0;
    float syncInterval = 5f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(SyncLoop());
    }

    public void AddPendingGold(int amount)
    {
        pendingGold += amount;

        // update UI ngay
        GameManager.Instance.AddGold(amount);
    }

    IEnumerator SyncLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(syncInterval);

            if (pendingGold > 0)
            {
                StartCoroutine(SendGoldToServer(pendingGold));
                pendingGold = 0;
            }
        }
    }

    IEnumerator SendGoldToServer(int amount)
    {
        int playerId = GameManager.Instance.playerData.id;

        string url = ApiConfigLoader.Config.baseUrl+
            "/api/currency/addGold?playerId=" +
            playerId + "&amount=" + amount;

        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Sync failed");
        }
        else
        {
            Debug.Log("Synced gold: " + amount);
        }
    }
}