using UnityEngine;
using System.Collections;
using UnityEngine.UI;   // nếu dùng Text thường
// using TMPro;        // nếu dùng TextMeshPro

public class AppleTree : MonoBehaviour
{
    [Header("Drop")]
    public ItemSO appleItem;
    public int amount = 1;

    [Header("Sprites")]
    public Sprite treeWithApple;
    public Sprite treeWithoutApple;

    [Header("UI")]
    public GameObject harvestUI; // kéo Text vào đây

    [Header("Settings")]
    public KeyCode harvestKey = KeyCode.H;
    public float respawnTime = 5f;

    bool playerInRange = false;
    bool hasApple = true;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateTreeVisual();

        if (harvestUI != null)
            harvestUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(harvestKey))
        {
            TryHarvest();
        }
    }

    void TryHarvest()
    {
        if (!hasApple)
            return;

        InventorySystem.I.Add(appleItem, amount);

        hasApple = false;
        UpdateTreeVisual();
        UpdateUIText();

        StartCoroutine(RespawnApple());
    }

    IEnumerator RespawnApple()
    {
        yield return new WaitForSeconds(respawnTime);

        hasApple = true;
        UpdateTreeVisual();
        UpdateUIText();
    }

    void UpdateTreeVisual()
    {
        sr.sprite = hasApple ? treeWithApple : treeWithoutApple;
    }

    void UpdateUIText()
    {
        if (harvestUI == null) return;

        var text = harvestUI.GetComponent<Text>();
        if (text != null)
        {
            text.text = hasApple ? "Nhấn H để hái" : "Đã hái";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (harvestUI != null)
        {
            harvestUI.SetActive(true);
            UpdateUIText();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (harvestUI != null)
            harvestUI.SetActive(false);
    }
}
