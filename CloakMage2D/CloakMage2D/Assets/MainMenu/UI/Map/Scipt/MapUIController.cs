using UnityEngine;

public class MapUIController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject mapPanel;   // Kéo Panel Map vào đây
    public GameObject player;     // Kéo Player vào đây

    bool isOpen = false;

    public void ToggleMap()
    {
        isOpen = !isOpen;

        mapPanel.SetActive(isOpen);

        if (player != null)
            player.SetActive(!isOpen);   // Mở map -> tắt player
    }
}
