using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BufferService : MonoBehaviour
{
    public static bool IsBuffering => bufferRequests > 0;
    private static int bufferRequests = 0;

    // Buffer Display
    [SerializeField] float _bufferIconRotateSpeed = 360f;

    [SerializeField] GameObject _bufferCanvas;
    [SerializeField] GameObject _bufferIcon;

    // Input/Interactions
    [SerializeField] CanvasGroup _uiCanvasGroup;
    [SerializeField] EventSystem _eventSystem;

    public static BufferService Instance;

    private void Awake()
    {
        // Singleton guard + persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Try to bind in the first scene too
        TryFindReferences();
        UpdateBufferState(); // ensure correct UI state on boot
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scene-specific objects will change; reacquire them
        TryFindReferences();
        UpdateBufferState(); // keep UI/input consistent across scene changes
    }

    private void TryFindReferences()
    {
        if (_bufferCanvas == null)
            _bufferCanvas = GameObject.Find("Canvas - Buffer");

        if (_bufferIcon == null)
            _bufferIcon = GameObject.Find("Icon - Buffer");

        if (_uiCanvasGroup == null)
        {
            var canvasGO = GameObject.Find("Canvas - Buffer");
            if (canvasGO != null)
                _uiCanvasGroup = canvasGO.GetComponent<CanvasGroup>();
        }

        if (_eventSystem == null)
            _eventSystem = FindObjectOfType<EventSystem>();
    }

    public void RequestBuffer()
    {
        bufferRequests++;
        UpdateBufferState();
    }

    public void ReleaseBuffer()
    {
        bufferRequests = Mathf.Max(0, bufferRequests - 1);
        UpdateBufferState();
    }

    private void UpdateBufferState()
    {
        bool shouldBuffer = IsBuffering;
        SetBufferDisplay(shouldBuffer);
        SetInputState(!shouldBuffer);
    }

    private void SetBufferDisplay(bool isDisplayed)
    {
        if (_bufferCanvas != null)
            _bufferCanvas.SetActive(isDisplayed);
    }

    private void SetInputState(bool isInteractable)
    {
        if (_uiCanvasGroup != null)
        {
            _uiCanvasGroup.interactable = isInteractable;
            _uiCanvasGroup.blocksRaycasts = isInteractable;
        }

        if (_eventSystem != null)
        {
            var inputModule = _eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule != null)
                inputModule.enabled = isInteractable;
        }
    }

    private void Update()
    {
        if (_bufferCanvas != null && _bufferCanvas.activeSelf && _bufferIcon != null)
        {
            _bufferIcon.transform.Rotate(Vector3.forward, -_bufferIconRotateSpeed * Time.unscaledDeltaTime);
        }
    }
}
