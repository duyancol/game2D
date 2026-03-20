using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsPanelUI : MonoBehaviour
{
    [Header("Data")]
    public PlayerStatsMono stats;

    [Header("Panel Root")]
    public GameObject panelStats;          // Panel_Stats

    [Header("Title")]
    public Text txtTitle;                  // Txt_Title

    [Header("HP UI")]
    public Slider hpSlider;                // PlayerHealthUI/PlayerHealthBarUI
    public Text hpText;                    // PlayerHealthUI/Text (Legacy)

    [Header("Stat Texts")]
    public Text txtATK;                    // Txt_ATK
    public Text txtSMPT;                   // Txt_SMPT
    public Text txtCrit;                   // Txt_Crit
    public Text txtCritDame;               // Txt_CritDame
    public Text txtDef;                    // Txt_Def
    public Text txtMDef;                   // Txt_MDEF
    public GameObject playerHealthUI; // kéo PlayerHealthUI (HUD) vào đây

    void Start()
    {
        if (panelStats != null)
            panelStats.SetActive(false);

        RefreshAll();
    }

    void Update()
    {
        // Panel mở thì cập nhật HP realtime
        if (panelStats != null && panelStats.activeSelf)
            RefreshHP();
    }

    // Gọi từ Button Btn_Portrait
    public void TogglePanel()
    {
        if (panelStats == null) return;

        bool isOpen = panelStats.activeSelf;
        panelStats.SetActive(!isOpen);

        // ẩn HUD khi mở panel
        if (playerHealthUI != null)
            playerHealthUI.SetActive(isOpen); // mở panel => HUD tắt, đóng panel => HUD bật

        if (!isOpen) RefreshAll();
    }


    void RefreshAll()
    {
        if (stats == null) return;

        if (txtTitle != null)
            txtTitle.text = "";

        RefreshHP();

        if (txtATK != null)
            txtATK.text = $"Atk: {stats.atk:0}";

        if (txtSMPT != null)
            txtSMPT.text = $"Smpt: {stats.smpt:0}";

        if (txtCrit != null)
            txtCrit.text = $"Crit: {(stats.critChance * 100f):0}%";

        if (txtCritDame != null)
            txtCritDame.text = $"Crit.d: {(stats.critDamage * 100f):0}%";

        if (txtDef != null)
            txtDef.text = $"Def: {stats.def:0}";

        if (txtMDef != null)
            txtMDef.text = $"M.def: {stats.mdef:0}";
    }

    void RefreshHP()
    {
        if (stats == null) return;

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = stats.maxHP;
            hpSlider.value = stats.hp;
        }

        if (hpText != null)
            hpText.text = $"{stats.hp} / {stats.maxHP}";
    }
}
