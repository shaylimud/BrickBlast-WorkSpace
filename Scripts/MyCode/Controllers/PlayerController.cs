using Ray.Services;
using Ray.Views;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField, RequireReference] private PlayerMovementConfig _playerMovementConfig;

        [Header("References")]
        [SerializeField, RequireReference] private PlayerView _playerView;
        [SerializeField, RequireReference] private BobberView _bobberView;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private void Awake()
        {
            //_playerView.Initialize(_playerMovementConfig);
            //_bobberView.Initialize(_playerView.transform);
        }

        private void OnEnable()
        {
            EventService.Level.OnStart += InitialPath;
            EventService.Level.OnEnd += EndPath;

            EventService.Resource.OnLevelResourceChanged += RefreshBobberSpace;

            EventService.Item.OnItemCollected += PulseBobber;
            EventService.Item.OnObstacleCollected += PlayerHit;

            EventService.Ad.OnReviveWatched += RevivePlayer;

            EventService.UI.OnShowExtraSpace += PauseTravel;
            EventService.Ad.OnExtraSpaceWatched += ResumeTravel;

            //_playerView.NewTrackedReach += HandleNewTrackedReach;
            //_playerView.OutOfLevel += PlayerOutOfLevel;
        }

        private void OnDisable()
        {
            EventService.Level.OnStart -= InitialPath;
            EventService.Level.OnEnd -= EndPath;

            EventService.Resource.OnLevelResourceChanged -= RefreshBobberSpace;

            EventService.Item.OnItemCollected -= PulseBobber;
            EventService.Item.OnObstacleCollected -= PlayerHit;

            EventService.Ad.OnReviveWatched -= RevivePlayer;

            EventService.UI.OnShowExtraSpace -= PauseTravel;
            EventService.Ad.OnExtraSpaceWatched -= ResumeTravel;

            //_playerView.NewTrackedReach -= HandleNewTrackedReach;
            //_playerView.OutOfLevel -= PlayerOutOfLevel;
        }

        private async void InitialPath(Component c)
        {
            _rayDebug.Event("InitialPath", c, this);

            Vector2 targetPos = new Vector2(0, -Database.UserData.Stats.ReachLevel);

            HandleNewTrackedReach(-Database.UserData.Stats.ReachLevel);

            await _playerView.AutoPath(targetPos);

            await TryShowFirstTutorial();

            RefreshBobberSpace(this);
            _bobberView.ToggleBobber();

            _playerView.StartTravel();
        }

        private async Task TryShowFirstTutorial()
        {
            var save = SaveSystem.Load();
            if (!save.showTutorial) return;

            EventService.UI.OnToggleTutorial.Invoke(this);
            save.showTutorial = false;
            SaveSystem.Save(save);

            var tcs = new TaskCompletionSource<bool>();
            EventService.UI.OnToggleTutorial += (sender) => tcs.TrySetResult(true);

            await tcs.Task;
        }

        private async void EndPath(Component c)
        {
            _rayDebug.Event("EndPath", c, this);

            _playerView.StopTravel();

            Vector2 targetPos = _playerView.InitialPosition;

            await _playerView.AutoPath(targetPos);

            _bobberView.ToggleBobber();

            EventService.Player.OnParked.Invoke(this);
        }

        private void PlayerHit(Component c)
        {
            _rayDebug.Event("PlayerHit", c, this);

            _playerView.StopTravel();
            EventService.Player.OnHit.Invoke(this);
        }

        private void RevivePlayer(Component c)
        {
            _rayDebug.Event("RevivePlayer", c, this);
            if (_playerView == null)
            {
                _rayDebug.LogWarning("PlayerView is null. Cannot revive player.", this);
                return;
            }

            _playerView.StartTravel();
        }

        private void PlayerOutOfLevel()
        {
            _rayDebug.Event("PlayerOutOfLevel", _playerView, this);

            _playerView.StopTravel();
            EventService.Player.OnOutOfLevel(this);
        }

        private void HandleNewTrackedReach(int newReach)
        {
            EventService.Player.OnNewTrackedReach(this, newReach);
        }

        private void RefreshBobberSpace(Component c)
        {
            _rayDebug.Event("RefreshBobberSpace", c, this);

            _bobberView.RefreshSpaceText(ResourceService.Instance.LevelSpace.Value);
        }

        private void PulseBobber(Component c, ItemType itemType, Vector2 collectionPos)
        {
            _bobberView.Pulse();
        }

        private void PauseTravel(Component c)
        {
            _rayDebug.Event("PauseTravel", c, this);

            _playerView.StopTravel();
        }
        private void ResumeTravel(Component c)
        {
            _rayDebug.Event("ResumeTravel", c, this);

            _playerView.StartTravel();
        }
    }
}