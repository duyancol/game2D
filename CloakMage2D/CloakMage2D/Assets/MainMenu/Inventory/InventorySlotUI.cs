
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private Text amountText;
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private Button button;

    int index;
    InventoryUI owner;

    void Awake()
    {
        // Safety check
        if (icon == null) Debug.LogError("Icon chưa được gán!");
        if (amountText == null) Debug.LogError("AmountText chưa được gán!");
        if (button == null) Debug.LogError("Button chưa được gán!");

        if (selectedGO != null)
            selectedGO.SetActive(false);
    }

    public void Init(InventoryUI ui, int slotIndex)
    {
        owner = ui;
        index = slotIndex;

        if (button == null)
        {
            Debug.LogError("Button NULL trong Init()");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => owner.OnClickSlot(index));
    }

    public void SetSelected(bool on)
    {
        if (selectedGO != null)
            selectedGO.SetActive(on);
    }

    public void Render(InvSlot slot)
    {
        if (slot == null || slot.item == null || slot.IsEmpty)
        {
            icon.enabled = false;
            amountText.text = "";
            return;
        }

        if (slot.item.icon == null)
        {
            Debug.LogError("Item không có icon: " + slot.item.name);
            icon.enabled = false;
            amountText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;

        amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";
    }
}
