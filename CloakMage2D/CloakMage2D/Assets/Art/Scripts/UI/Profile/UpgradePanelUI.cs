//////using System.Collections;
//////using UnityEngine;
//////using UnityEngine.UI;
//////using UnityEngine.Networking;

//////public class UpgradePanelUI : MonoBehaviour
//////{
//////    [Header("UI")]
//////    public Text txtName;
//////    public Text txtLevel;
//////    public Text txtExp;
//////    public Text txtResult;

//////    [Header("Buttons")]
//////    public Button btnLevelUp;
//////    public Button btnAscend;
//////    public Button btnClose;

//////    [Header("Refs")]
//////    public WeaponController weaponController;
//////    public WeaponPanelUI weaponPanelUI;

//////    [Header("API")]
//////    string baseUrl = "https://userservice-production-fd72.up.railway.app";
//////    long userId = PlayerSession.UserId;

//////    WeaponProfile _weapon;
//////    int GetRequiredExp(int level)
//////    {
//////        return 100 + level * 20;
//////    }
//////    int CalculateStoneNeed()
//////    {
//////        if (_weapon == null) return 0;

//////        int currentLevel = _weapon.level;

//////        // 🚨 nếu đã max level thì không cho up
//////        if (currentLevel >= 60)
//////            return 0;

//////        int needExp = GetRequiredExp(currentLevel);
//////        int currentExp = _weapon.exp;

//////        int remainExp = needExp - currentExp;

//////        if (remainExp <= 0) return 0;

//////        int stone = Mathf.CeilToInt(remainExp / 100f);

//////        return stone;
//////    }
//////    public void SetData(WeaponProfile wp)
//////    {
//////        _weapon = wp;

//////        Refresh();

//////        btnLevelUp.onClick.RemoveAllListeners();
//////        btnLevelUp.onClick.AddListener(OnClickLevelUpSmart); // 👈 đổi sang smart

//////        btnAscend.onClick.RemoveAllListeners();
//////        btnAscend.onClick.AddListener(OnClickAscend);

//////        btnClose.onClick.RemoveAllListeners();
//////        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
//////    }
//////    void OnClickLevelUpSmart()
//////    {
//////        if (_weapon == null) return;

//////        // 🚨 nếu đã max
//////        if (_weapon.level >= 60)
//////        {
//////            ShowResult("Đạt cấp tối đa, hãy tiến bậc!");
//////            return;
//////        }

//////        int needStone = CalculateStoneNeed();

//////        if (needStone <= 0)
//////        {
//////            ShowResult("Đã đủ exp!");
//////            return;
//////        }

//////        // 🚨 CHẶN không cho vượt 60
//////        OnClickLevelUp(needStone);
//////    }
//////    void Refresh()
//////    {
//////        if (_weapon == null) return;

//////        txtName.text = _weapon.weaponName;
//////        txtLevel.text = "Lv: " + _weapon.level;
//////        txtExp.text = "EXP: " + _weapon.exp;

//////        int needStone = CalculateStoneNeed();
//////        txtResult.text = $"Cần {needStone} đá để lên cấp";
//////    }

//////    // ================= LEVEL UP =================
//////    void OnClickLevelUp(int expStoneUse)
//////    {
//////        StartCoroutine(CallLevelUpAPI(expStoneUse));
//////    }

//////    IEnumerator CallLevelUpAPI(int expStoneUse)
//////    {
//////        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/level-up?userId={userId}&expStoneUse={expStoneUse}";

//////        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");

//////        yield return req.SendWebRequest();

//////        if (req.result != UnityWebRequest.Result.Success)
//////        {
//////            ShowResult("Fail: " + req.downloadHandler.text);
//////            Debug.Log("Fail: " + req.downloadHandler.text);
//////            yield break;
//////        }

//////        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

//////        _weapon.level = rs.level;
//////        _weapon.exp = rs.exp;

//////        AfterUpdate("Level Up!");
//////    }

//////    // ================= ASCEND =================
//////    void OnClickAscend()
//////    {
//////        StartCoroutine(CallAscendAPI());
//////    }

//////    IEnumerator CallAscendAPI()
//////    {
//////        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend?userId={userId}";

//////        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");

//////        yield return req.SendWebRequest();

//////        if (req.result != UnityWebRequest.Result.Success)
//////        {
//////            ShowResult("Thiếu nguyên liệu!");
//////            yield break;
//////        }

//////        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

//////        _weapon.ascend = rs.ascend;

//////        AfterUpdate("Ascend Success!");
//////    }

//////    // ================= COMMON =================
//////    void AfterUpdate(string msg)
//////    {
//////        ShowResult(msg);

//////        if (weaponController != null)
//////            weaponController.EquipByProfile(_weapon);

//////        if (weaponPanelUI != null)
//////            weaponPanelUI.Refresh();

//////        Refresh();
//////    }

//////    void ShowResult(string msg)
//////    {
//////        if (txtResult == null) return;

//////        txtResult.text = msg;
//////        txtResult.gameObject.SetActive(true);

//////         StopAllCoroutines();
//////        StartCoroutine(HideResult());
//////        //StartCoroutine(HideResult());
//////    }

//////    IEnumerator HideResult()
//////    {
//////        yield return new WaitForSeconds(1.5f);
//////        txtResult.gameObject.SetActive(false);
//////    }
//////}
////using System.Collections;
////using UnityEngine;
////using UnityEngine.UI;
////using UnityEngine.Networking;

////public class UpgradePanelUI : MonoBehaviour
////{
////    [Header("UI")]
////    public Text txtName;
////    public Text txtLevel;
////    public Text txtExp;
////    public Text txtResult;

////    [Header("Cost UI")]
////    public Text txtStone;
////    public Text txtAscendItem;

////    [Header("Icons")]
////    public Image imgWeapon;
////    public Image imgStone;
////    public Image imgAscendItem;

////    [Header("Materials")]
////    public ItemSO stoneItem;
////    public ItemSO ascendItem;

////    [Header("Buttons")]
////    public Button btnLevelUp;
////    public Button btnAscend;
////    public Button btnClose;

////    [Header("Refs")]
////    public WeaponController weaponController;
////    public WeaponPanelUI weaponPanelUI;

////    [Header("API")]
////    string baseUrl = "https://userservice-production-fd72.up.railway.app";
////    long userId = PlayerSession.UserId;

////    WeaponProfile _weapon;

////    int playerStone;
////    int playerItem;

////    // ================= INIT =================
////    public void SetData(WeaponProfile wp)
////    {
////        _weapon = wp;

////        LoadInventory();
////        Refresh();

////        btnLevelUp.onClick.RemoveAllListeners();
////        btnLevelUp.onClick.AddListener(OnClickLevelUpSmart);

////        btnAscend.onClick.RemoveAllListeners();
////        btnAscend.onClick.AddListener(OnClickAscend);

////        btnClose.onClick.RemoveAllListeners();
////        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
////    }

////    void LoadInventory()
////    {
////        playerStone = stoneItem != null ? InventorySystem.I.GetAmount(stoneItem) : 0;
////        playerItem = ascendItem != null ? InventorySystem.I.GetAmount(ascendItem) : 0;
////    }

////    // ================= CALCULATE =================
////    int GetRequiredExp(int level)
////    {
////        return 100 + level * 20;
////    }

////    int CalculateStoneNeed()
////    {
////        if (_weapon == null) return 0;

////        if (_weapon.level >= 60) return 0;

////        int needExp = GetRequiredExp(_weapon.level);
////        int remainExp = needExp - _weapon.exp;

////        if (remainExp <= 0) return 0;

////        return Mathf.CeilToInt(remainExp / 100f);
////    }

////    int CalculateAscendItemNeed()
////    {
////        if (_weapon == null) return 0;

////        return 5 + _weapon.ascend * 3;
////    }

////    // ================= UI =================
////    void Refresh()
////    {
////        if (_weapon == null) return;

////        LoadInventory();

////        int needStone = CalculateStoneNeed();
////        int needItem = CalculateAscendItemNeed();

////        txtName.text = _weapon.weaponName;
////        txtLevel.text = "Lv: " + _weapon.level;
////        txtExp.text = "EXP: " + _weapon.exp;

////        // ===== TEXT COST =====
////        if (txtStone != null)
////            txtStone.text = $"{playerStone}/{needStone}";

////        if (txtAscendItem != null)
////            txtAscendItem.text = $"{playerItem}/{needItem}";

////        // ===== COLOR =====
////        if (txtStone != null)
////            txtStone.color = playerStone >= needStone ? Color.white : Color.red;

////        if (txtAscendItem != null)
////            txtAscendItem.color = playerItem >= needItem ? Color.white : Color.red;

////        // ===== ICON WEAPON =====
////        if (imgWeapon != null)
////        {
////            Sprite icon = _weapon.uiIcon != null ? _weapon.uiIcon : _weapon.weaponSprite;
////            imgWeapon.sprite = icon;
////            imgWeapon.enabled = icon != null;
////        }

////        // ===== ICON STONE =====
////        if (imgStone != null && stoneItem != null)
////        {
////            imgStone.sprite = stoneItem.icon;
////            imgStone.enabled = true;
////        }

////        // ===== ICON ASCEND =====
////        if (imgAscendItem != null && ascendItem != null)
////        {
////            imgAscendItem.sprite = ascendItem.icon;
////            imgAscendItem.enabled = true;
////        }

////        // ===== BUTTON =====
////        btnLevelUp.interactable = playerStone >= needStone && needStone > 0;
////        btnAscend.interactable = playerItem >= needItem;

////        txtResult.text = $"LvUp: {needStone} đá | Ascend: {needItem} NL";
////    }

////    // ================= LEVEL UP =================
////    void OnClickLevelUpSmart()
////    {
////        if (_weapon == null) return;

////        if (_weapon.level >= 60)
////        {
////            ShowResult("Max level! Hãy tiến bậc");
////            return;
////        }

////        int needStone = CalculateStoneNeed();

////        if (needStone <= 0)
////        {
////            ShowResult("Đã đủ EXP!");
////            return;
////        }

////        OnClickLevelUp(needStone);
////    }

////    void OnClickLevelUp(int stoneUse)
////    {
////        StartCoroutine(CallLevelUpAPI(stoneUse));
////    }

////    IEnumerator CallLevelUpAPI(int stoneUse)
////    {
////        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/level-up?userId={userId}&expStoneUse={stoneUse}";

////        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");

////        yield return req.SendWebRequest();

////        if (req.result != UnityWebRequest.Result.Success)
////        {
////            ShowResult("Fail: " + req.downloadHandler.text);
////            yield break;
////        }

////        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

////        _weapon.level = rs.level;
////        _weapon.exp = rs.exp;

////        // 🔥 trừ đá local
////        if (stoneItem != null)
////        {
////            int current = InventorySystem.I.GetAmount(stoneItem);
////            InventorySystem.I.SetAmount(stoneItem, current - stoneUse);
////        }

////        AfterUpdate("Level Up!");
////    }

////    // ================= ASCEND =================
////    void OnClickAscend()
////    {
////        StartCoroutine(CallAscendAPI());
////    }

////    IEnumerator CallAscendAPI()
////    {
////        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend?userId={userId}";

////        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");

////        yield return req.SendWebRequest();

////        if (req.result != UnityWebRequest.Result.Success)
////        {
////            ShowResult("Thiếu nguyên liệu!");
////            yield break;
////        }

////        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

////        _weapon.ascend = rs.ascend;

////        // 🔥 trừ item local
////        if (ascendItem != null)
////        {
////            int need = CalculateAscendItemNeed();
////            int current = InventorySystem.I.GetAmount(ascendItem);
////            InventorySystem.I.SetAmount(ascendItem, current - need);
////        }

////        AfterUpdate("Ascend Success!");
////    }

////    // ================= COMMON =================
////    void AfterUpdate(string msg)
////    {
////        ShowResult(msg);

////        if (weaponController != null)
////            weaponController.EquipByProfile(_weapon);

////        if (weaponPanelUI != null)
////            weaponPanelUI.Refresh();

////        Refresh();
////    }

////    void ShowResult(string msg)
////    {
////        if (txtResult == null) return;

////        txtResult.text = msg;
////        txtResult.gameObject.SetActive(true);

////        StopAllCoroutines();
////        StartCoroutine(HideResult());
////    }

////    IEnumerator HideResult()
////    {
////        yield return new WaitForSeconds(1.5f);
////        txtResult.gameObject.SetActive(false);
////    }
////}
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Networking;

//public class UpgradePanelUI : MonoBehaviour
//{
//    [Header("UI")]
//    public Text txtName;
//    public Text txtLevel;
//    public Text txtExp;
//    public Text txtResult;

//    // ===== LEVEL UP =====
//    [Header("Level Up")]
//    public Text txtStone;
//    public Text txtGoldLevel;
//    public Image imgStone;

//    // ===== ASCEND =====
//    [Header("Ascend")]
//    public Image imgMat1;
//    public Image imgMat2;
//    public Text txtMat1;
//    public Text txtMat2;
//    public Text txtGoldAscend;

//    [Header("Weapon")]
//    public Image imgWeapon;

//    [Header("Buttons")]
//    public Button btnLevelUp;
//    public Button btnAscend;
//    public Button btnClose;

//    [Header("Refs")]
//    public WeaponController weaponController;
//    public WeaponPanelUI weaponPanelUI;

//    [Header("Config")]
//    public ItemSO expStone; // đá exp (fix)
//    public int goldPerLevel = 100; // vàng level up fix

//    string baseUrl = "https://userservice-production-fd72.up.railway.app";
//    long userId = PlayerSession.UserId;

//    WeaponProfile _weapon;

//    // ===== ASCEND DATA =====
//    AscendCostResponse ascendCost;

//    // ================= INIT =================
//    public void SetData(WeaponProfile wp)
//    {
//        _weapon = wp;

//        Refresh();
//        StartCoroutine(LoadAscendCost());

//        btnLevelUp.onClick.RemoveAllListeners();
//        btnLevelUp.onClick.AddListener(OnClickLevelUpSmart);

//        btnAscend.onClick.RemoveAllListeners();
//        btnAscend.onClick.AddListener(OnClickAscend);

//        btnClose.onClick.RemoveAllListeners();
//        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
//    }

//    // ================= LEVEL LOGIC =================
//    int GetRequiredExp(int level)
//    {
//        return 100 + level * 20;
//    }

//    int CalculateStoneNeed()
//    {
//        if (_weapon == null) return 0;
//        if (_weapon.level >= 60) return 0;

//        int needExp = GetRequiredExp(_weapon.level);
//        int remain = needExp - _weapon.exp;

//        if (remain <= 0) return 0;

//        return Mathf.CeilToInt(remain / 100f);
//    }

//    // ================= LOAD ASCEND API =================
//    IEnumerator LoadAscendCost()
//    {
//        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend-cost?userId={userId}";

//        UnityWebRequest req = UnityWebRequest.Get(url);
//        yield return req.SendWebRequest();

//        if (req.result != UnityWebRequest.Result.Success)
//        {
//            Debug.LogError(req.downloadHandler.text);
//            yield break;
//        }

//        ascendCost = JsonUtility.FromJson<AscendCostResponse>(req.downloadHandler.text);

//        RenderAscend();
//    }

//    // ================= UI =================
//    void Refresh()
//    {
//        if (_weapon == null) return;

//        txtName.text = _weapon.weaponName;
//        txtLevel.text = "Lv: " + _weapon.level;
//        txtExp.text = "EXP: " + _weapon.exp;

//        // ===== WEAPON ICON =====
//        if (imgWeapon != null)
//        {
//            imgWeapon.sprite = _weapon.uiIcon;
//            imgWeapon.enabled = true;
//        }

//        // ===== LEVEL UP =====
//        int stoneNeed = CalculateStoneNeed();

//        txtStone.text = stoneNeed.ToString();
//        txtGoldLevel.text = (stoneNeed * goldPerLevel).ToString();

//        if (imgStone != null && expStone != null)
//        {
//            imgStone.sprite = expStone.icon;
//        }
//    }

//    void RenderAscend()
//    {
//        if (ascendCost == null) return;

//        txtGoldAscend.text = ascendCost.gold.ToString();

//        // ===== MATERIAL 1 =====
//        if (ascendCost.materials.Count > 0)
//        {
//            var m1 = ascendCost.materials[0];

//            ItemSO item = InventorySystem.I.GetItemById(m1.itemId);

//            if (item != null)
//            {
//                imgMat1.sprite = item.icon;
//                imgMat1.enabled = true;

//                int have = InventorySystem.I.GetAmount(item);
//                txtMat1.text = $"{have}/{m1.amount}";
//            }
//            else
//            {
//                Debug.LogWarning("Không có item trong inventory: " + m1.itemId);

//                imgMat1.enabled = false;
//                txtMat1.text = $"0/{m1.amount}";
//            }
//        }

//        // ===== MATERIAL 2 =====
//        if (ascendCost.materials.Count > 1)
//        {
//            var m2 = ascendCost.materials[1];

//            ItemSO item = InventorySystem.I.GetItemById(m2.itemId);

//            if (item != null)
//            {
//                imgMat2.sprite = item.icon;
//                imgMat2.enabled = true;

//                int have = InventorySystem.I.GetAmount(item);
//                txtMat2.text = $"{have}/{m2.amount}";
//            }
//            else
//            {
//                Debug.LogWarning("Không có item trong inventory: " + m2.itemId);

//                imgMat2.enabled = false;
//                txtMat2.text = $"0/{m2.amount}";
//            }
//        }
//    }
//    // ================= LEVEL UP =================
//    void OnClickLevelUpSmart()
//    {
//        if (_weapon.level >= 60)
//        {
//            ShowResult("Max level → tiến bậc");
//            return;
//        }

//        int need = CalculateStoneNeed();
//        if (need <= 0)
//        {
//            ShowResult("Đủ EXP rồi");
//            return;
//        }

//        StartCoroutine(CallLevelUpAPI(need));
//    }

//    IEnumerator CallLevelUpAPI(int stoneUse)
//    {
//        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/level-up?userId={userId}&expStoneUse={stoneUse}";

//        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
//        yield return req.SendWebRequest();

//        if (req.result != UnityWebRequest.Result.Success)
//        {
//            ShowResult("Fail");
//            yield break;
//        }

//        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

//        _weapon.level = rs.level;
//        _weapon.exp = rs.exp;

//        AfterUpdate("Level Up!");
//    }

//    // ================= ASCEND =================
//    void OnClickAscend()
//    {
//        StartCoroutine(CallAscendAPI());
//    }

//    IEnumerator CallAscendAPI()
//    {
//        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend?userId={userId}";

//        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
//        yield return req.SendWebRequest();

//        if (req.result != UnityWebRequest.Result.Success)
//        {
//            ShowResult("Thiếu nguyên liệu");
//            yield break;
//        }

//        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

//        _weapon.ascend = rs.ascend;

//        AfterUpdate("Ascend Success!");
//    }

//    // ================= COMMON =================
//    void AfterUpdate(string msg)
//    {
//        ShowResult(msg);

//        weaponController?.EquipByProfile(_weapon);
//        weaponPanelUI?.Refresh();

//        Refresh();
//        StartCoroutine(LoadAscendCost());
//    }

//    void ShowResult(string msg)
//    {
//        txtResult.text = msg;
//        txtResult.gameObject.SetActive(true);

//        StopAllCoroutines();
//        StartCoroutine(HideResult());
//    }

//    IEnumerator HideResult()
//    {
//        yield return new WaitForSeconds(1.5f);
//        txtResult.gameObject.SetActive(false);
//    }
//}

//// ================= MODEL =================
//[System.Serializable]
//public class AscendCostResponse
//{
//    public int gold;
//    public List<AscendMaterial> materials;
//}

//[System.Serializable]
//public class AscendMaterial
//{
//    public string itemId;
//    public int amount;
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Effect")]
    public float animTime = 0.5f;
    public Vector3 scaleMax = new Vector3(1.5f, 1.5f, 1);
    [Header("UI")]
    public Text txtName;
    public Text txtLevel;
    public Text txtExp;
    public Text txtResult;
    [Header("Shake")]
    public RectTransform panelRoot;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 10f;
    [Header("Panels")]
    public GameObject levelUpPanel;
    public GameObject ascendPanel;

    // ===== LEVEL UP =====
    [Header("Level Up")]
    public Text txtStone;
    public Text txtGoldLevel;
    public Image imgStone;

    // ===== ASCEND =====
    [Header("Ascend")]
    public Image imgMat1;
    public Image imgMat2;
    public Text txtMat1;
    public Text txtMat2;
    public Text txtGoldAscend;

    [Header("Weapon")]
    public Image imgWeapon;

    [Header("Buttons")]
    public Button btnLevelUp;
    public Button btnAscend;
    public Button btnClose;

    [Header("Refs")]
    public WeaponController weaponController;
    public WeaponPanelUI weaponPanelUI;

    [Header("Config")]
    public ItemSO expStone;
    public int goldPerLevel = 100;
    public int maxLevel = 100;

    string baseUrl = "https://userservice-production-fd72.up.railway.app";
    long userId = PlayerSession.UserId;

    WeaponProfile _weapon;
    AscendCostResponse ascendCost;
    Coroutine hideCoroutine;
    // ================= INIT =================
    public void SetData(WeaponProfile wp)
    {
        _weapon = wp;

        Refresh();
        StartCoroutine(LoadAscendCost());

        btnLevelUp.onClick.RemoveAllListeners();
        btnLevelUp.onClick.AddListener(OnClickLevelUpSmart);

        btnAscend.onClick.RemoveAllListeners();
        btnAscend.onClick.AddListener(OnClickAscend);

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
    }

    // ================= LOGIC =================
    //bool IsMaxLevel()
    //{
    //    if (_weapon == null) return false;

    //    // max thật sự
    //    if (_weapon.level >= maxLevel) return false;

    //    return _weapon.level % 20 == 0;
    //}
    bool IsMaxLevel()
    {
        if (_weapon == null) return false;

        // ✅ max thật sự
        if (_weapon.level >= maxLevel) return true;

        // ✅ mốc cần ascend
        return _weapon.level % 20 == 0;
    }
    int GetRequiredExp(int level)
    {
        return 100 + level * 20;
    }
    IEnumerator ShakePanel(float duration, float strength)
    {
        if (panelRoot == null) yield break;

        Vector3 originalPos = panelRoot.anchoredPosition;

        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            // 🔥 giảm dần theo thời gian (smooth)
            float damper = 0.5f - (t / duration);

            float x = Mathf.Sin(Time.time * 20f) * strength * damper;
            float y = Mathf.Cos(Time.time * 15f) * strength * damper;

            panelRoot.anchoredPosition = originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        panelRoot.anchoredPosition = originalPos;
    }
    int CalculateStoneNeed()
    {
        if (_weapon == null || IsMaxLevel()) return 0;

        int needExp = GetRequiredExp(_weapon.level);
        int remain = needExp - _weapon.exp;

        if (remain <= 0) return 0;

        return Mathf.CeilToInt(remain / 10f);
    }

    // ================= LOAD ASCEND =================
    //IEnumerator LoadAscendCost()
    //{
    //    if (_weapon == null) yield break;

    //    string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend-cost?userId={userId}";

    //    UnityWebRequest req = UnityWebRequest.Get(url);
    //    yield return req.SendWebRequest();

    //    if (req.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError(req.downloadHandler.text);
    //        yield break;
    //    }

    //    ascendCost = JsonUtility.FromJson<AscendCostResponse>(req.downloadHandler.text);

    //    RenderAscend();
    //}
    IEnumerator LoadAscendCost()
    {
        if (_weapon == null) yield break;

        // ✅ Nếu đã max level thật sự thì skip luôn
        if (_weapon.level >= maxLevel)
        {
            HandleMaxLevel();
            yield break;
        }

        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend-cost?userId={userId}";

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            // 🔥 CASE: HẾT BẬC → API trả lỗi
            Debug.LogWarning("No more ascend: " + req.downloadHandler.text);

            HandleMaxLevel();
            yield break;
        }

        ascendCost = JsonUtility.FromJson<AscendCostResponse>(req.downloadHandler.text);

        RenderAscend();
    }
    void HandleMaxLevel()
    {
        txtResult.text = "Đã đạt cấp tối đa";
        txtResult.gameObject.SetActive(true);

        // ❌ tắt hết nâng cấp
        levelUpPanel.SetActive(false);
        ascendPanel.SetActive(false);

        btnLevelUp.gameObject.SetActive(false);
        btnAscend.gameObject.SetActive(false);
    }
    // ================= UI =================
    void Refresh()
    {
        if (_weapon == null) return;

        txtName.text = _weapon.weaponName;
        txtLevel.text = "Lv: " + _weapon.level;
        txtExp.text = "EXP: " + _weapon.exp;

        // ===== ICON =====
        if (imgWeapon != null)
        {
            imgWeapon.sprite = _weapon.uiIcon;
            imgWeapon.enabled = true;
        }

        bool isMax = IsMaxLevel();

        // ===== TOGGLE PANEL =====
        levelUpPanel.SetActive(!isMax);
        ascendPanel.SetActive(isMax);

        btnLevelUp.gameObject.SetActive(!isMax);
        btnAscend.gameObject.SetActive(isMax);

        // ===== LEVEL UI =====
        if (!isMax)
        {
            int stoneNeed = CalculateStoneNeed();

            txtStone.text = stoneNeed.ToString();
            txtGoldLevel.text = (stoneNeed * goldPerLevel).ToString();

            if (imgStone != null && expStone != null)
            {
                imgStone.sprite = expStone.icon;
            }
        }
    }

    void RenderAscend()
    {
        if (ascendCost == null) return;

        txtGoldAscend.text = ascendCost.gold.ToString();

        RenderMaterial(imgMat1, txtMat1, ascendCost.materials, 0);
        RenderMaterial(imgMat2, txtMat2, ascendCost.materials, 1);
    }

    void RenderMaterial(Image img, Text txt, List<AscendMaterial> mats, int index)
    {
        if (mats.Count <= index) return;

        var m = mats[index];

        // ItemSO item = InventorySystem.I.GetItemById(m.itemId);
        ItemSO item = InventoryLoader.I.GetItem(m.itemId);
        int have = 0;

        if (item != null)
        {
            img.sprite = item.icon;
            img.enabled = true;

            have = InventorySystem.I.GetAmount(item);
        }
        else
        {
            img.enabled = false;
        }

        txt.text = $"{have}/{m.amount}";

        // 🔴 thiếu thì đỏ
        txt.color = (have < m.amount) ? Color.red : Color.white;
    }

    // ================= LEVEL UP =================
    void OnClickLevelUpSmart()
    {
        if (IsMaxLevel())
        {
            Refresh();
            ShowResult("Đã max → tiến bậc");
            return;
        }

        int need = CalculateStoneNeed();
        if (need <= 0)
        {
            ShowResult("Đủ EXP rồi");
            return;
        }

        StartCoroutine(CallLevelUpAPI(need));
    }

    IEnumerator CallLevelUpAPI(int stoneUse)
    {
        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/level-up?userId={userId}&expStoneUse={stoneUse}";

        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            ShowResult("Nhấn nhanh quá vui lòng thử lại sau!");
            yield break;
        }

        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

        _weapon.level = rs.level;
        _weapon.exp = rs.exp;
        InventorySystem.I.Remove(expStone, stoneUse);
        AfterUpdate($"Đạt Lv.{_weapon.level}!");
        StartCoroutine(ShakePanel(0.25f, 8f));
    }

    // ================= ASCEND =================
    void OnClickAscend()
    {
        StartCoroutine(CallAscendAPI());
    }

    IEnumerator CallAscendAPI()
    {
        string url = $"{baseUrl}/api/weapons/{_weapon.weaponId}/ascend?userId={userId}";

        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            ShowResult("Thiếu nguyên liệu");
            yield break;
        }

        WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

        _weapon.level = rs.level;
        _weapon.exp = rs.exp;
        _weapon.ascend = rs.ascend;


        // 👇 THÊM ĐOẠN NÀY
        if (_weapon.level % 20 == 0)
        {
            _weapon.level = rs.level+1;
            _weapon.exp = rs.exp;
            _weapon.ascend = rs.ascend;
        }
        foreach (var m in ascendCost.materials)
        {
            ItemSO item = InventorySystem.I.GetItemById(m.itemId);
            if (item != null)
            {
                InventorySystem.I.Remove(item, m.amount);
            }
        }

        GameManager.Instance.AddGold(-ascendCost.gold);
        AfterUpdate($"Tiến bậc thành công!");
        _weapon.level = rs.level + 1;
        StartCoroutine(ShakePanel(0.4f, 15f));
    }

    // ================= COMMON =================
    void AfterUpdate(string msg)
    {
        ShowResult(msg);

        weaponController?.EquipByProfile(_weapon);
        weaponPanelUI?.Refresh();

        Refresh();
        StartCoroutine(LoadAscendCost());
    }

    //void ShowResult(string msg)
    //{
    //    txtResult.text = msg;
    //    txtResult.gameObject.SetActive(true);

    //    StopAllCoroutines();
    //    StartCoroutine(HideResult());
    //}
    void ShowResult(string msg)
    {
        txtResult.text = msg;
        txtResult.gameObject.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(PlayResultAnim());
    }
    IEnumerator PlayResultAnim()
    {
        float t = 0;

        txtResult.transform.localScale = Vector3.zero;
        Color c = txtResult.color;
        c.a = 0;
        txtResult.color = c;

        // 🔥 SCALE UP + FADE IN
        while (t < animTime)
        {
            t += Time.deltaTime;
            float progress = t / animTime;

            txtResult.transform.localScale = Vector3.Lerp(Vector3.zero, scaleMax, progress);

            c.a = Mathf.Lerp(0, 1, progress);
            txtResult.color = c;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // 🔻 FADE OUT
        t = 0;
        while (t < animTime)
        {
            t += Time.deltaTime;
            float progress = t / animTime;

            txtResult.transform.localScale = Vector3.Lerp(scaleMax, Vector3.one, progress);

            c.a = Mathf.Lerp(1, 0, progress);
            txtResult.color = c;

            yield return null;
        }

        txtResult.gameObject.SetActive(false);
    }
    IEnumerator HideResult()
    {
        yield return new WaitForSeconds(1.5f);
        txtResult.gameObject.SetActive(false);
    }
}

// ================= MODEL =================
[System.Serializable]
public class AscendCostResponse
{
    public int gold;
    public List<AscendMaterial> materials;
}

[System.Serializable]
public class AscendMaterial
{
    public string itemId;
    public int amount;
}