////using UnityEngine;
////using UnityEngine.SceneManagement;

////public static class SceneBootstrap
////{
////    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
////    static void Init()
////    {
////        SceneManager.sceneLoaded += OnSceneLoaded;
////    }

////    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
////    {
////        if (!SceneManager.GetSceneByName("GlobalUI").isLoaded)
////        {
////            SceneManager.LoadScene("GlobalUI", LoadSceneMode.Additive);
////        }
////    }
////}
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Không load GlobalUI ở LoginScene
        if (scene.name == "LoginScene")
            return;

        if (!SceneManager.GetSceneByName("GlobalUI").isLoaded)
        {
            SceneManager.LoadScene("GlobalUI", LoadSceneMode.Additive);
        }
    }
}
