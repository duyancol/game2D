using UnityEngine;

public class AutoHideUI : MonoBehaviour
{
    void Start()
    {
        Invoke("Hide", 1.5f);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}