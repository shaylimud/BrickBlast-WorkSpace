using Ray.Services;
using Ray.Views;
using UnityEngine;

namespace Ray.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField, RequireReference] private CameraMovementConfig _cameraMovementConfig;

        [Header("References")]
        [SerializeField, RequireReference] private CameraView _cameraView;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private void Awake()
        {
            _cameraView.Initialize(_cameraMovementConfig);
        }
    }
}