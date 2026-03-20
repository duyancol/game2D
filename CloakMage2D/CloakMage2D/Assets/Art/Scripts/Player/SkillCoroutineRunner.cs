using UnityEngine;

public class SkillCoroutineRunner : MonoBehaviour
{
    public void Run(System.Collections.IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
