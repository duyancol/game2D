using UnityEngine;
using TMPro;

public class DivineMarkUI : MonoBehaviour
{
    public TextMeshProUGUI stackText;

    DivineMarkRuntime runtime;

    void Update()
    {
        // Nếu chưa có runtime thì tìm
        if (runtime == null)
        {
            runtime = FindObjectOfType<DivineMarkRuntime>();

            if (runtime != null)
            {
                runtime.OnStackChanged += UpdateUI;
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }
    }

    void UpdateUI(int current, int max)
    {
        stackText.text = $"{current}/{max}";
    }
}