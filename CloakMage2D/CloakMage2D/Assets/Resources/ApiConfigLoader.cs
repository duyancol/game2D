using UnityEngine;

public static class ApiConfigLoader
{
    private static ApiConfigSO _config;

    public static ApiConfigSO Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<ApiConfigSO>("ApiConfig");

                if (_config == null)
                {
                    Debug.LogError("❌ Không tìm thấy ApiConfig trong Resources!");
                }
            }

            return _config;
        }
    }
}