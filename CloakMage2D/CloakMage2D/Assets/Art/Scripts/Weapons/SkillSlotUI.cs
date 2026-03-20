using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image icon;
    public Image cdFill; // Image type = Filled Radial360
    public Text cdText;

    WeaponSkill skill;

    public void SetSkill(WeaponSkill s)
    {
        skill = s;

        if (icon)
        {
            icon.sprite = (skill != null) ? skill.skillIcon : null;
            icon.enabled = icon.sprite != null;
        }
        Refresh();
    }

    void Update() => Refresh();

    void Refresh()
    {
        if (skill == null)
        {
            SetUI(0f, "");
            return;
        }

        float remain = skill.GetRemain();
        float fill = skill.GetCooldownFill01();

        if (remain > 0.05f)
            SetUI(fill, Mathf.CeilToInt(remain).ToString());
        else
            SetUI(0f, "");
    }

    void SetUI(float fill, string txt)
    {
        if (cdFill)
        {
            cdFill.enabled = fill > 0.001f;
            cdFill.fillAmount = fill;
        }

        if (cdText)
        {
            cdText.text = txt;
            cdText.enabled = !string.IsNullOrEmpty(txt);
        }
    }
}
