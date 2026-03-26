using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

[System.Serializable]
public class InventoryData
{
    public long id;
    public long userId;
    public string itemId;
    public int amount;
    public long equipmentInstanceId; // ⭐ THÊM DÒNG NÀY
}

[System.Serializable]
public class InventoryWrapper
{
    public InventoryData[] data;
}

public class InventoryLoader : MonoBehaviour
{
    public ItemSO[] allItems; // kéo toàn bộ item SO vào đây
    public static InventoryLoader I;
    string url = "";
    void Awake()
    {
        I = this;
    }
    public ItemSO GetItem(string id)
    {
        return allItems.FirstOrDefault(x => x.itemId == id);
    }
    void Start()
    {
        url = ApiConfigLoader.Config.baseUrl+ "/api/inventory?userId=" + PlayerSession.UserId;
        StartCoroutine(LoadInventory());
    }
    public EquipmentResponse equipmentData;
    IEnumerator LoadInventory()
    {
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
           
            yield break;
        }

        string json = "{ \"data\": " + req.downloadHandler.text + "}";

        InventoryWrapper wrapper =
            JsonUtility.FromJson<InventoryWrapper>(json);

        InventorySystem.I.ClearInventory();

        foreach (var serverItem in wrapper.data)
        {
          
            ItemSO item = allItems
                .FirstOrDefault(x => x.itemId == serverItem.itemId);

            if (item != null)
            {
                //InventorySystem.I.Add(item, serverItem.amount);
                InventorySystem.I.AddFromServer(
    item,
    serverItem.amount,
    serverItem.id,
    serverItem.equipmentInstanceId


);

            }
            else
            {
                Debug.LogWarning("Không tìm thấy ItemSO: " + serverItem.itemId);
            }
        }

       
    }
}
