using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Ray.Services
{
    public class TimeApiService : MonoBehaviour
    {
        private const string timeApiUrl = "https://www.google.com/";

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static TimeApiService Instance;
        private void Awake()
        {
            Instance = this;
        }

        public async Task<Timestamp> GetCurrentTime()
        {
            await ConnectionService.Instance.WaitForConnection();

            int retryDelayMs = 1000;
            while (true)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(timeApiUrl))
                {
                    var asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string jsonResult = request.GetResponseHeader("date");
                        DateTime dateTimeUtc = DateTime.Parse(jsonResult, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                        return Timestamp.FromDateTime(dateTimeUtc);
                    }
                    else
                    {
                        _rayDebug.LogWarning("Failed to fetch online current time: " + request.error + ". Retrying...", this);
                        await Task.Delay(retryDelayMs);
                    }
                }
            }
        }
    }
}