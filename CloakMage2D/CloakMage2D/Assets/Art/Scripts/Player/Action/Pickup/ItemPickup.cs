
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    public ItemSO item;
    public int amount = 1;

    [Header("UI")]
    public GameObject pickupUI;

    [Header("Settings")]
    public KeyCode pickupKey = KeyCode.H;
    public float pickupDelay = 0.3f; // delay sau khi rơi

    [Header("API")]
    public string apiUrl = "https://userservice-production-fd72.up.railway.app/api/inventory/add";

    bool playerInRange = false;
    bool canPickup = false;

    void Start()
    {
        if (pickupUI != null)
            pickupUI.SetActive(false);

        StartCoroutine(EnablePickup());
    }

    IEnumerator EnablePickup()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickup = true;
    }

    void Update()
    {
        if (!playerInRange || !canPickup) return;

        if (Input.GetKeyDown(pickupKey))
        {
            PickupItem();
        }
    }

    void PickupItem()
    {
        // Add local inventory
        InventorySystem.I.Add(item, amount);

        // Call API
        StartCoroutine(AddItemToServer());

        Destroy(gameObject);
    }

    IEnumerator AddItemToServer()
    {
        AddItemRequest reqData = new AddItemRequest
        {
            userId = PlayerSession.UserId,
            itemId = item.itemId,
            amount = amount
        };

        string json = JsonUtility.ToJson(reqData);

        UnityWebRequest req = new UnityWebRequest(ApiConfigLoader.Config.baseUrl+ "/api/inventory/add", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("AddItem API Error: " + req.error);
        }
        else
        {
            Debug.Log("Item added: " + item.itemId);
        }
    }

    [System.Serializable]
    public class AddItemRequest
    {
        public long userId;
        public string itemId;
        public int amount;
    }

    void UpdateUIText()
    {
        if (pickupUI == null) return;

        var text = pickupUI.GetComponent<Text>();
        if (text != null)
        {
            text.text = "Nhấn H để nhặt";
        }
    }

    // 🔥 QUAN TRỌNG: dùng Stay để không bị miss
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (pickupUI != null && !pickupUI.activeSelf)
        {
            pickupUI.SetActive(true);
            UpdateUIText();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (pickupUI != null)
            pickupUI.SetActive(false);
    }
}