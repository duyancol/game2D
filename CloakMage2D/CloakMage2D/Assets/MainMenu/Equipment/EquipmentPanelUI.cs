

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EquipmentPanelUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI mainStatText;
    public TextMeshProUGUI subStatText;
    public TextMeshProUGUI rarityText;
    public Button closeButton;
    public PlayerStatsMono playerStats;
    private EquipmentResponse currentData;
    private EquipmentProfile currentProfile;
    private Image panelImage; // <-- THÊM
    [Header("Rarity Frame")]
    [SerializeField] private Image panelFrame;
    [SerializeField] private Sprite greenFrame;
    [SerializeField] private Sprite blueFrame;
    [SerializeField] private Sprite purpleFrame;
    [SerializeField] private Sprite orangeFrame;


    [SerializeField]
    //private string detailUrl =
   // "https://userservice-production-fd72.up.railway.app/api/equipment/";

    private bool isLoading = false;
    long currentEquipmentId;
    private void Awake()
    {
        // Lấy Image của EquipmentPanel
        panelImage = GetComponent<Image>();
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }



    //public void Show(long equipmentId, EquipmentProfile profile, Sprite itemIcon = null)
    //{
    //    if (panel == null)
    //        return;

    //    currentProfile = profile; // ⭐ lưu profile của item đang mở

    //    panel.SetActive(true);

    //    if (itemIcon != null)
    //        icon.sprite = itemIcon;

    //    StartCoroutine(GetEquipmentDetail(equipmentId));
    //}

    public void Show(long equipmentId, EquipmentProfile profile, Sprite itemIcon = null)
    {
        if (panel == null)
            return;

        currentEquipmentId = equipmentId;   // ⭐ LƯU ID SERVER
        currentProfile = profile;

        panel.SetActive(true);

        if (itemIcon != null)
            icon.sprite = itemIcon;

        StartCoroutine(GetEquipmentDetail(equipmentId));
    }
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    IEnumerator GetEquipmentDetail(long equipmentId)
    {
        if (isLoading)
            yield break;

        isLoading = true;

        string url = ApiConfigLoader.Config.baseUrl+ "/api/equipment/" + equipmentId;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        isLoading = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API Error: ID :" + equipmentId + " ///// " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;

        EquipmentResponse data =
            JsonUtility.FromJson<EquipmentResponse>(json);

        if (data == null)
        {
            Debug.LogWarning("Parse equipment detail failed");
            yield break;
        }

        Render(data);
    }

    void Render(EquipmentResponse data)
    {
        currentData = data;
        // nameText.text = data.itemId;
        if (currentProfile != null)
            nameText.text = currentProfile.equipmentName;
        else
            nameText.text = data.itemId;
        string rarity = string.IsNullOrEmpty(data.quality)
            ? "green"
            : data.quality;

        rarityText.text = rarity;

        SetPanelColor(rarity); // <-- QUAN TRỌNG
        if (panelFrame != null)
        {
            panelFrame.sprite = GetFrameByRarity(rarity);
        }

        mainStatText.text = data.mainStat + " +" + data.mainValue;

        subStatText.text = "";

        if (!string.IsNullOrEmpty(data.subStatsJson))
        {
            List<SubStat> subStats =
                JsonHelper.FromJson<SubStat>(data.subStatsJson);

            foreach (var sub in subStats)
            {
                subStatText.text += sub.type + " +" + sub.value + "\n";
            }
        }



    }
    //public void OnClickEquip()
    //{
    //    if (currentData == null)
    //        return;

    //    EquipmentBonus bonus = ConvertToBonus(currentData);

    //    //playerStats.EquipArmor(bonus);
    //    playerStats.EquipArmor(bonus, icon.sprite, currentData.quality);
    //    Debug.Log("Quality API: " + currentData.quality);


    //    Debug.Log("Đã trang bị: " + currentData.itemId);
    //    Debug.Log("UI Equip → Slot: " + bonus.slot +
    //          " | Item: " + currentData.itemId);
    //}

    //public void OnClickEquip()
    //{
    //    if (currentData == null)
    //        return;

    //    EquipmentBonus bonus = ConvertToBonus(currentData);

    //    playerStats.EquipArmor(bonus, icon.sprite, currentData.quality);

    //    StartCoroutine(EquipItem(currentEquipmentId, PlayerSession.UserId));

    //    Debug.Log("Đã trang bị: " + currentData.itemId);
    //}
    public void OnClickEquip()
    {
        if (currentData == null || currentProfile == null)
            return;

        CharacterEquipmentUI equipUI = FindObjectOfType<CharacterEquipmentUI>();

        long oldId = -1;

        if (equipUI != null)
            oldId = equipUI.GetEquippedId(currentProfile.slot);
        Debug.Log("Item : oldId" + oldId);
        StartCoroutine(EquipItem(currentEquipmentId, oldId, PlayerSession.UserId));
    }
    void SetPanelColor(string quality)
    {
        if (panelImage == null)
            return;

        switch (quality.ToLower())
        {
            case "green":
                panelImage.color = new Color(0.3f, 0.8f, 0.3f, 0.6f);
                break;

            case "blue":
                panelImage.color = new Color(0.3f, 0.5f, 1f, 0.6f);
                break;

            case "purple":
                panelImage.color = new Color(0.6f, 0.3f, 1f, 0.6f);
                break;

            case "orange":
                panelImage.color = new Color(1f, 0.5f, 0.1f, 0.6f);
                break;

            default:
                panelImage.color = Color.white;
                break;
        }
    }
    //EquipmentBonus ConvertToBonus(EquipmentResponse data)
    //{
    //    EquipmentBonus b = new EquipmentBonus();

    //    b.slot = slot;   // ⭐ QUAN TRỌNG NHẤT

    //    // MAIN STAT (flat)
    //    ApplyFlatStat(ref b, data.mainStat, data.mainValue);

    //    // SUB STAT (%)
    //    if (!string.IsNullOrEmpty(data.subStatsJson))
    //    {
    //        List<SubStat> subStats =
    //            JsonHelper.FromJson<SubStat>(data.subStatsJson);

    //        foreach (var sub in subStats)
    //        {
    //            ApplyPercentStat(ref b, sub.type, sub.value);
    //        }
    //    }

    //    return b;
    //}
    EquipmentBonus ConvertToBonus(EquipmentResponse data)
    {
        EquipmentBonus b = new EquipmentBonus();

        if (currentProfile == null)
        {
            Debug.LogError("Profile null!");
            return b;
        }

        b.slot = currentProfile.slot;   // ⭐ slot lấy từ item

        ApplyFlatStat(ref b, data.mainStat, data.mainValue);

        if (!string.IsNullOrEmpty(data.subStatsJson))
        {
            List<SubStat> subStats =
                JsonHelper.FromJson<SubStat>(data.subStatsJson);

            foreach (var sub in subStats)
            {
                ApplyPercentStat(ref b, sub.type, sub.value);
            }
        }

        return b;
    }


    public void OnClickUnequip()
    {
        if (currentProfile == null)
            return;

        playerStats.RemoveArmor(currentProfile.slot);
    }



    void ApplyFlatStat(ref EquipmentBonus b, string type, float value)
    {
        switch (type.ToLower())
        {
            case "hp":
                b.flatHP += Mathf.RoundToInt(value);
                break;

            case "atk":
            case "attack":
                b.flatAtk += value;
                break;
            case "smpt":
            case "Smpt":
                b.flatSmpt += value;
                break;

            case "def":
                b.flatDef += value;
                break;

            case "mdef":
                b.flatMdef += value;
                break;

            case "crit":
                b.flatCritChance += value;
                break;

            case "crit_damage":
                b.flatCritDamage += value;
                break;
        }
    }
    void ApplyPercentStat(ref EquipmentBonus b, string type, float value)
    {
        float percent = value / 100f;

        switch (type.ToLower())
        {
            case "hp":
                b.percentHP += percent;
                break;

            case "atk":
            case "attack":
                b.percentAtk += percent;
                break;
            case "smpt":
            case "Smpt":
                b.flatSmpt += value;
                break;
            case "def":
                b.percentDef += percent;
                break;

            case "mdef":
                b.percentMdef += percent;
                break;

            case "crit":
                b.percentCritChance += percent;
                break;

            case "crit_damage":
                b.percentCritDamage += percent;
                break;
        }
    }

    Sprite GetFrameByRarity(string quality)
    {
        switch (quality.ToLower())
        {
            case "green": return greenFrame;
            case "blue": return blueFrame;
            case "purple": return purpleFrame;
            case "orange": return orangeFrame;
        }

        return greenFrame;
    }
    //IEnumerator EquipItem(long instanceId, long userId)
    //{
    //    string url = "https://userservice-production-fd72.up.railway.app/api/equipment/equip/"
    //                + instanceId + "?userId=" + userId;

    //    UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //        Debug.Log("Equip success");
    //    else
    //        Debug.LogError(request.error);
    //}
    IEnumerator EquipItem(long newItemId, long oldItemId, long userId)
    {
        // ⭐ 1. UNEQUIP ITEM CŨ
        if (oldItemId != -1)
        {
           

            UnityWebRequest unequipReq = UnityWebRequest.PostWwwForm(ApiConfigLoader.Config.baseUrl +
            "/api/equipment/unequip/" + oldItemId, "");

            yield return unequipReq.SendWebRequest();

            if (unequipReq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Unequip fail: " + unequipReq.error);
                yield break;
            }

            Debug.Log("Unequip success");
        }

        // ⭐ 2. EQUIP ITEM MỚI


        UnityWebRequest equipReq = UnityWebRequest.PostWwwForm(ApiConfigLoader.Config.baseUrl + "/api/equipment/equip/"
        + newItemId + "?userId=" + userId, "");

        yield return equipReq.SendWebRequest();

        if (equipReq.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Equip success");

            CharacterEquipmentUI equipUI = FindObjectOfType<CharacterEquipmentUI>();

            if (equipUI != null)
                equipUI.ReloadEquip();
        }
        else
        {
            Debug.LogError(equipReq.error);
        }
    }
}
