using UnityEngine;

public class AbsoluteZeroFlag : MonoBehaviour
{
    public AbsoluteZeroPassive passive;
    public bool isReady;

    void Update()
    {
        if (passive != null)
            passive.Tick();
    }
}