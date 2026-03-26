using UnityEngine;

public class CharacterTestSwitcher : MonoBehaviour
{
    public PlayerCharacterController player;

    public CharacterProfile char1;
    public CharacterProfile char2;
    void Start()
    {
        player.ApplyCharacter(char1); // spawn ngay khi vào game
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.ApplyCharacter(char1);
            Debug.Log("Switched to CHAR 1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.ApplyCharacter(char2);
            Debug.Log("Switched to CHAR 2");
        }
    }
}