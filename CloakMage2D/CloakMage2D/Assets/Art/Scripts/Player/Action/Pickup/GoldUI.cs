//using UnityEngine;
//using UnityEngine.UI;

//public class GoldUI : MonoBehaviour
//{
//    public Text goldText;

//    void Start()
//    {
//        goldText.text = GameManager.Instance.gold.ToString();
//        GameManager.Instance.OnGoldChanged += UpdateGold;
//    }

//    void UpdateGold(int value)
//    {
//        goldText.text = value.ToString();
//    }

//    void OnDestroy()
//    {
//        if (GameManager.Instance != null)
//            GameManager.Instance.OnGoldChanged -= UpdateGold;
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class GoldUI : MonoBehaviour
{
    public Text goldText;

    void Start()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnGoldChanged += UpdateGold;

        // ⭐ cập nhật ngay lập tức
        UpdateGold(GameManager.Instance.gold);
    }

    void UpdateGold(int value)
    {
        goldText.text = value.ToString();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= UpdateGold;
    }
}