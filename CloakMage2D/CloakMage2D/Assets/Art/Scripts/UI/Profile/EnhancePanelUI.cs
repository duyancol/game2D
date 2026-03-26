using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnhancePanelUI : MonoBehaviour
{
    [Header("UI")]
    public Text txtName;
    public Text txtLevel;
    public Text txtRate;
    public Text txtStoneNeed;
    public Text txtResult;

    [Header("Material")]
    public ItemSO stoneItem;

    [Header("Buttons")]
    public Button btnEnhance;
    public Button btnClose;

    [Header("Refs")]
    public WeaponController weaponController;
    public WeaponPanelUI weaponPanelUI;

    [Header("Icons")]
    public Image imgWeapon;
    public Image imgStone;

    [Header("FX")]
    public Image successBorder;
    public Image failFlash;
    public RectTransform panelRoot;

    [Header("API")]
   // string baseUrl = "https://userservice-production-fd72.up.railway.app";
    long userId = PlayerSession.UserId;

    WeaponProfile _weapon;
    int playerStone = 0;
    Vector2 resultOriginalPos;
    void Awake()
    {
        if (successBorder != null) successBorder.gameObject.SetActive(false);
        if (txtResult != null)
        {
            txtResult.gameObject.SetActive(false);
            resultOriginalPos = txtResult.rectTransform.anchoredPosition; // 🔥 lưu vị trí gốc
        }
    }

    public void SetData(WeaponProfile wp)
    {
        _weapon = wp;
        playerStone = InventorySystem.I.GetAmount(stoneItem);

        Refresh();

        btnEnhance.onClick.RemoveAllListeners();
        btnEnhance.onClick.AddListener(OnClickEnhance);

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void Refresh()
    {
        if (_weapon == null) return;

        int lv = _weapon.enhanceLevel;
        int rate = GetRate(lv);
        int need = GetStoneNeed(lv);

        txtName.text = _weapon.weaponName + " +" + lv;
        txtLevel.text = "" + (lv + 1);
        txtRate.text = "Rate: " + rate + "%";
        txtStoneNeed.text = $"{playerStone}/{need}";

        if (imgWeapon != null)
        {
            Sprite icon = _weapon.uiIcon != null ? _weapon.uiIcon : _weapon.weaponSprite;
            imgWeapon.sprite = icon;
            imgWeapon.enabled = (icon != null);
        }

        if (imgStone != null && stoneItem != null)
        {
            imgStone.sprite = stoneItem.icon;
            imgStone.enabled = true;
        }

        btnEnhance.interactable = playerStone >= need;
    }

    //int GetRate(int lv)
    //{
    //    if (lv < 3) return 100;
    //    if (lv < 5) return 70;
    //    if (lv < 7) return 50;
    //    return 30;
    //}
    Dictionary<int, int> SUCCESS_RATE = new Dictionary<int, int>()
{
    {0, 100},
    {1, 90},
    {2, 75},
    {3, 60},
    {4, 45},
    {5, 30},
    {6, 20},
    {7, 12},
    {8, 8},
    {9, 5},
    {10, 3},
    {11, 2}
};

    int GetRate(int lv)
    {
        if (SUCCESS_RATE.ContainsKey(lv))
            return SUCCESS_RATE[lv];

        return 1; // default giống BE
    }
    int GetStoneNeed(int lv)
    {
        return 1 + lv;
    }

    void OnClickEnhance()
    {
        StartCoroutine(CallEnhanceAPI());
    }

    IEnumerator CallEnhanceAPI()
    {
        string url = $"{ApiConfigLoader.Config.baseUrl}/api/weapons/{_weapon.weaponId}/enhance?userId={userId}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.downloadHandler = new DownloadHandlerBuffer();

        btnEnhance.interactable = false;

        yield return req.SendWebRequest();

        btnEnhance.interactable = true;

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Enhance FAIL: " + req.error);
            yield break;
        }

        EnhanceResult rs = JsonUtility.FromJson<EnhanceResult>(req.downloadHandler.text);

        _weapon.enhanceLevel = rs.newLevel;

        if (weaponController != null)
            weaponController.EquipByProfile(_weapon);

        if (weaponPanelUI != null)
            weaponPanelUI.Refresh();

        playerStone = rs.remainStone;
        InventorySystem.I.SetAmount(stoneItem, rs.remainStone);

        if (rs.success)
            StartCoroutine(EffectSuccess());
        else
            StartCoroutine(EffectFail());

        Refresh();
    }

    // ================= SUCCESS =================
    IEnumerator EffectSuccess()
    {
        if (txtResult == null || panelRoot == null) yield break;

        txtResult.gameObject.SetActive(true);
        txtResult.text = "Thành Công!";

        RectTransform rt = txtResult.rectTransform;

        Vector2 startPos = resultOriginalPos;
        rt.anchoredPosition = startPos;
        rt.localScale = Vector3.zero;

        Color startColor = new Color(1f, 0.9f, 0.2f, 0); // vàng sáng + alpha 0
        Color endColor = new Color(1f, 0.95f, 0.4f, 1f);

        txtResult.color = startColor;

        Vector2 panelStart = panelRoot.anchoredPosition;

        if (successBorder != null)
            successBorder.gameObject.SetActive(true);

        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            float t = time / duration;

            // 🔥 EASE OUT (mượt)
            float ease = 1 - Mathf.Pow(1 - t, 3);

            // scale mượt
            float scale = Mathf.Lerp(0, 1.2f, ease);
            if (t > 0.6f)
                scale = Mathf.Lerp(1.2f, 1f, (t - 0.6f) / 0.4f);

            rt.localScale = Vector3.one * scale;

            // bay nhẹ (không bay mất)
            rt.anchoredPosition = startPos + Vector2.up * (ease * 60f);

            // fade + glow nhẹ
            txtResult.color = Color.Lerp(startColor, endColor, ease);

            // rung nhẹ panel thôi
            float shake = (1 - ease) * 6f;
            panelRoot.anchoredPosition = panelStart + new Vector2(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake)
            );

            time += Time.deltaTime;
            yield return null;
        }

        panelRoot.anchoredPosition = panelStart;

        yield return new WaitForSeconds(0.25f);

        // fade out mượt
        float fade = 0;
        while (fade < 0.2f)
        {
            txtResult.color = new Color(1f, 0.95f, 0.4f, 1 - (fade / 0.2f));
            fade += Time.deltaTime;
            yield return null;
        }

        txtResult.gameObject.SetActive(false);

        if (successBorder != null)
            successBorder.gameObject.SetActive(false);
    }

    // ================= FAIL =================
    IEnumerator EffectFail()
    {
        if (txtResult == null) yield break;

        txtResult.gameObject.SetActive(true);
        txtResult.text = "Thất bại!....";

        RectTransform rt = txtResult.rectTransform;

        Vector2 startPos = resultOriginalPos;
        rt.anchoredPosition = startPos;

        rt.localScale = Vector3.zero;

        Color startColor = new Color(0.7f, 0.7f, 0.7f, 0f); // xám nhạt (alpha 0)
        Color endColor = new Color(0.9f, 0.9f, 0.9f, 1f); // xám sáng (full)

        txtResult.color = startColor;

        float time = 0;
        float duration = 0.4f;

        while (time < duration)
        {
            float t = time / duration;

            float ease = 1 - Mathf.Pow(1 - t, 3);

            // scale nhẹ mượt
            float scale = Mathf.Lerp(0, 1.1f, ease);
            rt.localScale = Vector3.one * scale;

            // KHÔNG rung chữ nữa
            rt.anchoredPosition = startPos;

            // fade màu
            txtResult.color = Color.Lerp(startColor, endColor, ease);

            // flash đỏ nhẹ
            if (failFlash != null)
            {
                float alpha = Mathf.Sin(t * Mathf.PI) * 0.5f;
                Color c = failFlash.color;
                failFlash.color = new Color(1, 0, 0, alpha);
            }

            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // fade out
        float fade = 0;
        while (fade < 0.2f)
        {
            txtResult.color = new Color(1f, 0.5f, 0.5f, 1 - (fade / 0.2f));
            fade += Time.deltaTime;
            yield return null;
        }

        if (failFlash != null)
            failFlash.color = new Color(1, 0, 0, 0);

        txtResult.gameObject.SetActive(false);
    }
}