using Ray.Services;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Controllers
{
    public class LevelController : MonoBehaviour
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private void OnEnable()
        {
            EventService.Ad.OnNoEnemiesDismissed += StartLevel;
            EventService.Resource.OnNoEnemiesReceived += StartLevel;

            EventService.Player.OnOutOfLevel += EndLevel;
            EventService.Ad.OnReviveDismissed += EndLevel;
            EventService.Ad.OnExtraSpaceDismissed += EndLevel;
        }

        private void StartLevel(Component c)
        {
            _rayDebug.Event("StartLevel", c, this);

            EventService.Level.OnStart.Invoke(this);
        }

        private void EndLevel(Component c)
        {
            _rayDebug.Event("EndLevel", c, this);

            EventService.Level.OnEnd.Invoke(this);
        }
    }
}