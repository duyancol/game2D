
using UnityEngine;
using Google;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using Firebase.Extensions;

public class GoogleLogin : MonoBehaviour
{
   // string baseUrl = ApiConfigLoader.Config.baseUrl;
    private GoogleSignInConfiguration configuration;

   // private string API_BASE_URL = "https://userservice-production-fd72.up.railway.app";

    void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = "173231895009-8hq2821ha5aviccme6lm3a87omveeogd.apps.googleusercontent.com",
            RequestEmail = true,
            RequestIdToken = true
        };
    }

    public void SignIn()
    {
        Debug.Log("=== SignIn Clicked ===");

#if UNITY_EDITOR
        Debug.Log("Editor Mode → Skip Google Login");

        PlayerSession.UserId = 4;
        PlayerSession.JwtToken = "EDITOR_MODE_TOKEN";

        StartCoroutine(FullLoginFlow());
#else
        GoogleSignIn.Configuration = configuration;

        GoogleSignIn.DefaultInstance.SignIn()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("❌ Google Sign-In Canceled");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("❌ Google Sign-In Error: " + task.Exception);
                    return;
                }

                GoogleSignInUser user = task.Result;

                Debug.Log("✅ Google Login Success");
                StartCoroutine(PostGoogleToken(user.IdToken));
            });
#endif
    }

    IEnumerator PostGoogleToken(string idToken)
    {
        Debug.Log("=== START AUTH API ===");

        string url = ApiConfigLoader.Config.baseUrl + "/api/v1/auth/google";

        string jsonBody = "{\"idToken\":\"" + idToken + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Auth API Failed: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        AuthenticationResponse auth =
            JsonUtility.FromJson<AuthenticationResponse>(json);

        PlayerSession.UserId = auth.id;
        PlayerSession.JwtToken = auth.token;

        Debug.Log("Saved UserId: " + PlayerSession.UserId);

        request.Dispose();

        yield return StartCoroutine(FullLoginFlow());
    }

    IEnumerator FullLoginFlow()
    {
        Debug.Log("=== START FULL LOGIN FLOW ===");

        // 1️⃣ Load GlobalUI trước
        if (!SceneManager.GetSceneByName("GlobalUI").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("GlobalUI", LoadSceneMode.Additive);
            Debug.Log("GlobalUI Loaded");
        }

        // Đợi 1 frame để GameManager Awake xong
        yield return null;

        // 2️⃣ Load Player Data API
        yield return StartCoroutine(LoadPlayerData());

        // 3️⃣ Load MainMenu (replace LoginScene)
        yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);

        Debug.Log("=== END FULL LOGIN FLOW ===");
    }

    IEnumerator LoadPlayerData()
    {
        Debug.Log("=== START LOAD PLAYER API ===");

        string url = ApiConfigLoader.Config.baseUrl + "/api/player/" + PlayerSession.UserId;

        UnityWebRequest request = UnityWebRequest.Get(url);
       

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Load Player API Failed: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        Debug.Log("Player JSON: " + json);

        PlayerData player =
            JsonUtility.FromJson<PlayerData>(json);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerData(player);
            Debug.Log("PlayerData Set Into GameManager");
        }
        else
        {
            Debug.LogError("GameManager Instance NULL");
        }

        request.Dispose();

        Debug.Log("=== END LOAD PLAYER API ===");
    }
}
