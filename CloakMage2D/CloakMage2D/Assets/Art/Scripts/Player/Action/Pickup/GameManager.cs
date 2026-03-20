
using UnityEngine;
using System;

[System.Serializable]
public class PlayerData
{
    public int power;
    public string name;
    public int level;
    public int id;
    public int exp;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public System.Action OnPlayerLoaded;

    public PlayerData playerData;

    public int gold = 0;
    public event Action<int> OnGoldChanged;
  
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerData(PlayerData data)
    {
        playerData = data;
        

        Debug.Log("Loaded Player:");
        Debug.Log("Name: " + data.name);
        Debug.Log("Level: " + data.level);
        Debug.Log("Power: " + data.power);
        OnPlayerLoaded?.Invoke();
        // GỌI API LẤY GOLD
        FindObjectOfType<CurrencyAPI>()
            .LoadCurrency(playerData.id);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }
    public void SetGold(int value)
    {
        gold = value;
        OnGoldChanged?.Invoke(gold);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(10);
            Debug.Log("Gold: " + gold);
        }
    }
    

    

}
