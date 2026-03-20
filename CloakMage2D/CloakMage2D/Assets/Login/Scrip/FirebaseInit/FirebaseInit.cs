using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("🔥 Firebase Ready!");
            }
            else
            {
                Debug.LogError("Firebase lỗi: " + task.Result);
            }
        });
    }
}
