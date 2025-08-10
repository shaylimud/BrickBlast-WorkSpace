using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectionService : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField, RequireReference] private ConnectionSettingsConfig _ConnectionSettingsConfig;

    private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

    private static bool _isConnected = false;
    private float _currentRetryDelay;
    private CancellationTokenSource _cancellationTokenSource;

    private static readonly HttpClient httpClient = new HttpClient();

    public static ConnectionService Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _ = MonitorConnection(_cancellationTokenSource.Token);
    }

    private async Task MonitorConnection(CancellationToken token)
    {
        var retryProp = _ConnectionSettingsConfig.RetryProp;
        _currentRetryDelay = retryProp.RetryMode == RetryMode.Linear ? retryProp.LinearRetryInterval : 1f;

        while (!token.IsCancellationRequested)
        {
            bool wasConnected = _isConnected;
            _isConnected = await CheckConnection();

            if (_isConnected)
            {
                _currentRetryDelay = retryProp.RetryMode == RetryMode.Linear ? retryProp.LinearRetryInterval : 1f;

                // ?? Release Buffer when connection is restored
                if (!wasConnected)
                {
                    BufferService.Instance.ReleaseBuffer();
                }
            }
            else
            {
                // ?? Request Buffer when connection is lost
                if (wasConnected)
                {
                    BufferService.Instance.RequestBuffer();
                }
            }

            if (wasConnected != _isConnected)
            {
                _rayDebug.Log($"[ConnectionService] Connection State Changed: {_isConnected}", this);
            }

            await Task.Delay(TimeSpan.FromSeconds(_currentRetryDelay), token);

            if (!_isConnected && retryProp.RetryMode == RetryMode.Exponential)
            {
                _currentRetryDelay = Mathf.Min(_currentRetryDelay * retryProp.ExponentialBase, retryProp.ExponentialCap);
            }
        }
    }


    private async Task<bool> CheckConnection()
    {
        try
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_ConnectionSettingsConfig.RetryProp.RequestTimeout)))
            {
                var response = await httpClient.GetAsync("https://www.google.com", cts.Token);
                return response.IsSuccessStatusCode;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task WaitForConnection()
    {
        while (!_isConnected)
        {
            await Task.Delay(500);
        }
    }
}