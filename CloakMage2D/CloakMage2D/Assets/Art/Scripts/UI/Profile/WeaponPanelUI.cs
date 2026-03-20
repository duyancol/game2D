
using UnityEngine;
using UnityEngine.Networking;

using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
public class WeaponPanelUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject panelWeapon;
    [Header("Buttons")]
    public Button btnEquip;
    public Button btnEnhance;
    public Button btnUpgrade;
    WeaponController _weaponController;
    public GameObject panelEnhance;
    public GameObject panelUpgrade;
    public UpgradePanelUI upgradeUI;
    public EnhancePanelUI enhanceUI;
    [Header("Weapon Visual")]
    public Image imgWeapon;
    public Text txtWeaponName;

    [Header("Stat Texts")]
    public Text txtATK;
    public Text txtSMPT;
    public Text txtCrit;
    public Text txtCritDmg;
    public Text txtDef;
    public Text txtMDef;
    [Header("API")]
    string baseUrl = "https://userservice-production-fd72.up.railway.app";
    long userId = PlayerSession.UserId;
    WeaponProfile _current;

    void Start()
    {
        if (panelWeapon != null) panelWeapon.SetActive(false);
        Refresh();
    }

    public void Toggle()
    {
        if (panelWeapon == null) return;
        panelWeapon.SetActive(!panelWeapon.activeSelf);
        if (panelWeapon.activeSelf) Refresh();
    }

    public void SetWeapon(WeaponProfile wp)
    {
        _current = wp;

        // ✅ đảm bảo panel active trước
        if (panelWeapon != null && !panelWeapon.activeSelf)
            panelWeapon.SetActive(true);

        StartCoroutine(LoadWeaponLevel());
    }

    //void Refresh()
    //{
    //    // ✅ log an toàn
    //    Debug.Log($"[WeaponPanelUI] weapon={(_current ? _current.weaponName : "NULL")} " +
    //              $"uiIcon={(_current && _current.uiIcon ? _current.uiIcon.name : "NULL")} " +
    //              $"weaponSprite={(_current && _current.weaponSprite ? _current.weaponSprite.name : "NULL")} " +
    //              $"imgRef={(imgWeapon ? imgWeapon.name : "NULL")}");

    //    // ===== NULL: clear UI =====
    //    if (_current == null)
    //    {
    //        if (imgWeapon != null)
    //        {
    //            imgWeapon.sprite = null;
    //            imgWeapon.enabled = false;
    //        }

    //        if (txtWeaponName != null) txtWeaponName.text = "";

    //        SetLine(txtATK, "ATK", 0);
    //        SetLine(txtSMPT, "SMPT", 0);
    //        SetLinePercent(txtCrit, "Crit", 0f);
    //        SetLinePercent(txtCritDmg, "CritD", 0f);
    //        SetLine(txtDef, "Def", 0);
    //        SetLine(txtMDef, "MDef", 0);

    //        return;
    //    }

    //    // ===== HAS WEAPON: set icon + name =====
    //    if (imgWeapon != null)
    //    {
    //        Sprite icon = _current.uiIcon != null ? _current.uiIcon : _current.weaponSprite; // ✅ dùng uiIcon
    //        imgWeapon.sprite = icon;
    //        imgWeapon.enabled = (icon != null);
    //        imgWeapon.preserveAspect = true;
    //    }

    //    if (txtWeaponName != null)
    //        txtWeaponName.text = _current.weaponName + " " + _current.level;

    //    // ===== stats =====
    //    var b = _current.bonus;

    //    SetLine(txtATK, "ATK", b.addAtk);
    //    SetLine(txtSMPT, "SMPT", b.addSmpt);
    //    SetLinePercent(txtCrit, "Crit", b.addCritChance * 100f);
    //    SetLinePercent(txtCritDmg, "CritD", b.addCritDamage * 100f);
    //    SetLine(txtDef, "Def", b.addDef);
    //    SetLine(txtMDef, "MDef", b.addMdef);
    //}
    public void Refresh()
    {
        // ===== NULL =====
        if (_current == null)
        {
            if (imgWeapon != null)
            {
                imgWeapon.sprite = null;
                imgWeapon.enabled = false;
            }

            if (txtWeaponName != null) txtWeaponName.text = "";

            SetLine(txtATK, "ATK", 0);
            SetLine(txtSMPT, "SMPT", 0);
            SetLinePercent(txtCrit, "Crit", 0);
            SetLinePercent(txtCritDmg, "CritD", 0);
            SetLine(txtDef, "Def", 0);
            SetLine(txtMDef, "MDef", 0);
            return;
        }

        // ===== ICON =====
        if (imgWeapon != null)
        {
            Sprite icon = _current.uiIcon != null ? _current.uiIcon : _current.weaponSprite;
            imgWeapon.sprite = icon;
            imgWeapon.enabled = (icon != null);
            imgWeapon.preserveAspect = true;
        }

        // ===== NAME + LEVEL =====
        int lv = _current.enhanceLevel;
        if (txtWeaponName != null)
            txtWeaponName.text = $"{_current.weaponName} +{lv} + (Lv {{_current.level}})";

        // ===== CREATE INSTANCE (FIX CONSTRUCTOR) =====
        WeaponInstance instance = new WeaponInstance(_current, _current.level, _current.enhanceLevel);

        // ===== CALCULATE =====
        //StatBonus baseStat = _current.bonus;
        //StatBonus final = WeaponCalculator.GetFinalStat(instance);
        StatBonus baseStat = _current.bonus;

        // 1. lấy stat theo level
        StatBonus stat = WeaponCalculator.GetBaseStatByLevel(instance);

        // 2. apply enhance
        float percent = 1f + instance.enhanceLevel * 0.1f;

        stat.addAtk = Mathf.RoundToInt(stat.addAtk * percent);
        stat.addMaxHP = Mathf.RoundToInt(stat.addMaxHP * percent);
        stat.addSmpt = Mathf.RoundToInt(stat.addSmpt * percent);

        stat.addCritChance *= percent;
        stat.addCritDamage *= percent;

        stat.addDef = Mathf.RoundToInt(stat.addDef * percent);
        stat.addMdef = Mathf.RoundToInt(stat.addMdef * percent);
        // ===== SHOW STAT + BONUS (FLOAT SAFE) =====
        //SetLineWithBonus(txtATK, "ATK", final.addAtk, baseStat.addAtk);
        //SetLineWithBonus(txtSMPT, "SMPT", final.addSmpt, baseStat.addSmpt);

        //SetLinePercentWithBonus(txtCrit, "Crit",
        //    final.addCritChance, baseStat.addCritChance);

        //SetLinePercentWithBonus(txtCritDmg, "CritD",
        //    final.addCritDamage, baseStat.addCritDamage);

        //SetLineWithBonus(txtDef, "Def", final.addDef, baseStat.addDef);
        //SetLineWithBonus(txtMDef, "MDef", final.addMdef, baseStat.addMdef);
        SetLineWithBonus(txtATK, "ATK", stat.addAtk, baseStat.addAtk);
        SetLineWithBonus(txtSMPT, "SMPT", stat.addSmpt, baseStat.addSmpt);

        SetLinePercentWithBonus(txtCrit, "Crit",
            stat.addCritChance, baseStat.addCritChance);

        SetLinePercentWithBonus(txtCritDmg, "CritD",
            stat.addCritDamage, baseStat.addCritDamage);

        SetLineWithBonus(txtDef, "Def", stat.addDef, baseStat.addDef);
        SetLineWithBonus(txtMDef, "MDef", stat.addMdef, baseStat.addMdef);
    }
    void SetLineWithBonus(Text t, string label, float final, float baseVal)
    {
        if (t == null) return;

        float bonus = final - baseVal;

        if (bonus > 0)
            t.text = $"{label}: {final} (+{bonus})";
        else
            t.text = $"{label}: {final}";
    }

    void SetLinePercentWithBonus(Text t, string label, float final, float baseVal)
    {
        if (t == null) return;

        float bonus = final - baseVal;

        if (bonus > 0.0001f)
            t.text = $"{label}: {(final * 100f):0}% (+{(bonus * 100f):0}%)";
        else
            t.text = $"{label}: {(final * 100f):0}%";
    }
    void SetLine(Text t, string label, float v)
    {
        if (t == null) return;
        t.text = $"{label}: {v:0}";
    }

    void SetLinePercent(Text t, string label, float v)
    {
        if (t == null) return;
        t.text = $"{label}: {v:0}%";
    }
    public void Init(WeaponController controller)
    {
        _weaponController = controller;

        if (btnEquip != null)
        {
            btnEquip.onClick.RemoveAllListeners();
            btnEquip.onClick.AddListener(OnClickEquip);
        }
        if (btnEnhance != null)
        {
            btnEnhance.onClick.RemoveAllListeners();
            btnEnhance.onClick.AddListener(OnClickEnhance);
        }
        if (btnUpgrade != null)
        {
            btnUpgrade.onClick.RemoveAllListeners();
            btnUpgrade.onClick.AddListener(OnClickUpgrade);
        }
    }
    void OnClickUpgrade()
    {
        if (_current == null) return;

        panelUpgrade.SetActive(true);

        upgradeUI.weaponController = _weaponController;
        upgradeUI.weaponPanelUI = this;
        upgradeUI.SetData(_current);
    }
    void OnClickEnhance()
    {
        if (_current == null) return;

        panelEnhance.SetActive(true);
        enhanceUI.weaponController = _weaponController; // 🔥 TRUYỀN QUA
        enhanceUI.weaponPanelUI = this; // 🔥 THÊM DÒNG NÀY
        enhanceUI.SetData(_current); // truyền weapon qua panel mới
    }
    IEnumerator CallEnhanceAPI()
    {
        string weaponId = _current.weaponId;

        string url = $"{baseUrl}/api/weapons/{weaponId}/enhance?userId={userId}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log("CALL ENHANCE: " + url);

        btnEnhance.interactable = false; // 🔒 chống spam

        yield return req.SendWebRequest();

        btnEnhance.interactable = true;

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Enhance FAIL: " + req.error);
            Debug.LogError("Response: " + req.downloadHandler.text);
            yield break;
        }

        Debug.Log("Enhance OK: " + req.downloadHandler.text);

        EnhanceResult rs = JsonUtility.FromJson<EnhanceResult>(req.downloadHandler.text);

        HandleEnhance(rs);
    }
    void HandleEnhance(EnhanceResult rs)
    {
        if (rs.success)
        {
            Debug.Log("🔥 SUCCESS LV " + rs.newLevel);

        }
        else
        {
            Debug.Log("💀 FAIL");
        }

        // 👉 update level local
        // StartCoroutine(AfterEnhanceFlow());
        // update level local
        _current.enhanceLevel = rs.newLevel;

        // 🔥 apply ngay
        if (_weaponController != null)
        {
            _weaponController.EquipByProfile(_current);
        }

        Refresh();
        
    }
    IEnumerator AfterEnhanceFlow()
    {
        // 1. load lại level từ server
        yield return LoadWeaponLevel();

        // 2. nếu đang cầm weapon này → apply lại stat
        if (_weaponController != null && _current != null)
        {
            _weaponController.EquipByProfile(_current);
        }

        // 3. refresh UI
        Refresh();
    }
    void OnClickEquip()
    {
        if (_current == null || _weaponController == null)
            return;

        //if (_weaponController.EquipByProfile(_current))
        //{
        //    panelWeapon.SetActive(false);
        //}
        StartCoroutine(CallEquipAPI());
    }
    IEnumerator CallEquipAPI()
    {
        string weaponId = _current.weaponId;

        string url = $"{baseUrl}/api/weapons/equip?userId={userId}&itemId={weaponId}";

        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(url, ""))
        {
            Debug.Log("CALL API: " + url);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Equip API FAIL: " + req.error);
                yield break;
            }

            Debug.Log("Equip API OK: " + req.downloadHandler.text);

            // 👉 API OK rồi mới equip local
            //if (_weaponController.EquipByProfile(_current))

            //{
            //    StartCoroutine(LoadWeaponLevel()); // ✅ reload level

            //    panelWeapon.SetActive(false);
            //}
            // load level xong rồi mới equip
            StartCoroutine(EquipAfterLoad());
        }
    }
    IEnumerator EquipAfterLoad()
    {
        yield return LoadWeaponLevel(); // có thể ra lv 0

        // ✅ lúc này _current.level đã đúng (0 hoặc từ BE)
        if (_weaponController.EquipByProfile(_current))
        {
            panelWeapon.SetActive(false);
        }
    }
    //IEnumerator LoadWeaponLevel()
    //{
    //    if (_current == null) yield break;

    //    string url = $"{baseUrl}/api/weapons/{_current.weaponId}?userId={userId}";

    //    UnityWebRequest req = UnityWebRequest.Get(url);

    //    Debug.Log("CALL GET WEAPON: " + url);

    //    yield return req.SendWebRequest();

    //    if (req.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("Load weapon FAIL: " + req.error);
    //        yield break;
    //    }

    //    Debug.Log("Load weapon OK: " + req.downloadHandler.text);

    //    WeaponDataResponse rs = JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

    //    // ✅ update level từ server
    //    _current.level = rs.enhanceLevel;

    //    Refresh();
    //}
    IEnumerator LoadWeaponLevel()
    {
        if (_current == null) yield break;

        string url = $"{baseUrl}/api/weapons/{_current.weaponId}?userId={userId}";
        UnityWebRequest req = UnityWebRequest.Get(url);

       

        yield return req.SendWebRequest();

        // ❌ FAIL
        if (req.result != UnityWebRequest.Result.Success)
        {
            // 🔥 CASE QUAN TRỌNG: CHƯA CÓ DATA TRÊN SERVER
            if (req.responseCode == 401 || req.responseCode == 404)
            {
                Debug.Log("⚠️ Weapon chưa có data trên server → set level 0");

                _current.enhanceLevel = 0; // ✅ fallback chuẩn
                Refresh();
                yield break;
            }

            Debug.LogError("Load weapon FAIL: " + req.error);
            yield break;
        }

        // ✅ SUCCESS
       

        WeaponDataResponse rs =
            JsonUtility.FromJson<WeaponDataResponse>(req.downloadHandler.text);

        _current.enhanceLevel = rs.enhanceLevel;
        _current.level = rs.level;
        _current.exp = rs.exp;
        _current.ascend = rs.ascend;
        Refresh();
    }
}
[System.Serializable]
public class EnhanceResult
{
    public bool success;
    public int newLevel;
    public int rate;
    public int remainStone;

}
[System.Serializable]
public class WeaponDataResponse
{
    public string weaponId;
    public int enhanceLevel;
    public int level;
    public int exp;
    public int ascend;
}