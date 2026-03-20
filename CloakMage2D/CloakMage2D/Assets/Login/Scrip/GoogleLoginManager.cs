//using UnityEngine;
//using Google;
//using System.Threading.Tasks;
//using UnityEngine.Networking;
//using System.Text;

//public class GoogleLoginManager : MonoBehaviour
//{
//    private GoogleSignInConfiguration configuration;

//    void Awake()
//    {
//        configuration = new GoogleSignInConfiguration
//        {
//            WebClientId = "272854032499-uvoh7etrb27k4sp664qd3baj900l703l.apps.googleusercontent.com",
//            RequestIdToken = true
//        };
//    }

//    public void SignInWithGoogle()
//    {
//        GoogleSignIn.Configuration = configuration;
//        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
//    }

//    private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
//    {
//        if (task.IsFaulted)
//        {
//            Debug.LogError("Google Sign-in Error");
//            return;
//        }

//        if (task.IsCanceled)
//        {
//            Debug.Log("Google Sign-in Canceled");
//            return;
//        }

//        string idToken = task.Result.IdToken;
//        Debug.Log("Google ID Token: " + idToken);

//        SendTokenToBackend(idToken);
//    }

//    async void SendTokenToBackend(string idToken)
//    {
//        string url = "http://localhost:8080/api/v1/auth/google";

//        var json = JsonUtility.ToJson(new GoogleTokenRequest { idToken = idToken });

//        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
//        {
//            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
//            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            request.downloadHandler = new DownloadHandlerBuffer();
//            request.SetRequestHeader("Content-Type", "application/json");

//            await request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError("Backend Error: " + request.error);
//            }
//            else
//            {
//                Debug.Log("Backend Response: " + request.downloadHandler.text);

//                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

//                PlayerPrefs.SetString("accessToken", response.accessToken);
//                PlayerPrefs.SetString("refreshToken", response.refreshToken);
//                PlayerPrefs.Save();

//                Debug.Log("Login Success!");
//            }
//        }
//    }
//}

//[System.Serializable]
//public class GoogleTokenRequest
//{
//    public string idToken;
//}

//[System.Serializable]
//public class AuthResponse
//{
//    public string accessToken;
//    public string refreshToken;
//    public string email;
//    public string name;
//    public string role;
//}
