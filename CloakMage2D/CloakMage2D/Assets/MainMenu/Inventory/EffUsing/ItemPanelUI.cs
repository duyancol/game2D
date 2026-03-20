using UnityEngine;
using UnityEngine.UI;

public class ItemPanelUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject panelItem;

    [Header("Buttons")]
    public Button btnUse;

    [Header("Visual")]
    public Image imgItem;
    public Text txtItemName;
    [Header("Description")]
    public Text txtDescription;

    InventorySystem _inventory;
    int _currentIndex = -1;

    void Start()
    {
        if (panelItem != null)
            panelItem.SetActive(false);

        _inventory = InventorySystem.I;

        if (btnUse != null)
        {
            btnUse.onClick.RemoveAllListeners();
            btnUse.onClick.AddListener(OnClickUse);
        }
    }

    public void SetItem(InvSlot slot, int realIndex)

    {
        _currentIndex = realIndex;

        if (imgItem != null)
        {
            imgItem.sprite = slot.item.icon;
            imgItem.enabled = (slot.item.icon != null);
        }

        if (txtItemName != null)
            txtItemName.text = slot.item.itemName;

        panelItem.SetActive(true);
    }

    void OnClickUse()
    {
        Debug.Log("BTN USE CLICKED");

        Debug.Log("_currentIndex = " + _currentIndex);
        Debug.Log("_owner = " + _owner);

        if (_currentIndex < 0)
        {
            Debug.Log("INDEX INVALID");
            return;
        }

        if (_owner == null)
        {
            Debug.Log("OWNER NULL");
            return;
        }

        Debug.Log("CALLING UseItemFromPanel");

        _owner.UseItemFromPanel(_currentIndex);
        ClosePanel();

    }



    public void Show(ItemSO item, int realIndex)
    {
        _currentIndex = realIndex;   // ← SỬA DÒNG NÀY

        panelItem.SetActive(true);
        imgItem.sprite = item.icon;
        txtItemName.text = item.itemName;
        if (txtDescription != null)
            txtDescription.text = item.description;
        // ⭐ xử lý nút Use
        if (btnUse != null)
        {
            if (item.itemType == ItemType.Consumable)
            {
                btnUse.gameObject.SetActive(true);
            }
            else
            {
                btnUse.gameObject.SetActive(false);
            }
        }
    }


    InventoryUI _owner;

    public void Init(InventoryUI ui)
    {
        _owner = ui;

        if (panelItem != null)
            panelItem.SetActive(false);

        if (btnUse != null)
        {
            btnUse.onClick.RemoveAllListeners();
            btnUse.onClick.AddListener(OnClickUse);
        }
    }
    public void ClosePanel()
    {
        panelItem.SetActive(false);
        _currentIndex = -1;
    }

}
