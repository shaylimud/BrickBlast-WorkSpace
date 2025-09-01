using Ray.Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ray.Controllers
{
    public class SoundController : MonoBehaviour
    {
        // Singleton Instance
        public static SoundController Instance { get; private set; }

        [Header("Configs")]
        [SerializeField, RequireReference] private SoundCollection soundCollection;

        [Header("Cap Audio Instances")]
        [SerializeField] int poolSize = 10;
        private Queue<AudioSource> _audioPool = new Queue<AudioSource>();

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private AudioSource _bgm;
        private bool _muteState;

        private void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Optional: Persist between scenes
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Instance = null;
        }

        private void OnEnable()
        {
            EventService.UI.OnToggleSound += ToggleSounds;

            EventService.UI.OnStartBtn += Click;
            EventService.UI.OnSpaceUpgradeBtn += Click;
            EventService.UI.OnReachUpgradeBtn += Click;
            EventService.UI.OnToggleSound += Click;

            EventService.UI.OnToggleInsufficient += Error;
            EventService.UI.OnToggleDataMismatch += Error;
            EventService.UI.OnToggleTutorial += OpenPopup;

            EventService.UI.OnMeterStart += RushMeter;

            EventService.Resource.OnMenuResourceChanged += Purchase;

            EventService.Level.OnStart += StartLevel;

            EventService.Ad.OnReviveWatched += AdReward;
            EventService.Ad.OnTripleWatched += AdReward;
            EventService.Ad.OnNoEnemiesWatched += AdReward;
            EventService.Ad.OnFreeGiftWatched += AdReward;
            EventService.Ad.OnExtraSpaceWatched += AdReward;

            EventService.Item.OnItemCollected += PlayItemCollection;
            EventService.Item.OnObstacleCollected += PlayerHit;
        }

        private void OnDisable()
        {
            EventService.UI.OnToggleSound -= ToggleSounds;

            EventService.UI.OnStartBtn -= Click;
            EventService.UI.OnSpaceUpgradeBtn -= Click;
            EventService.UI.OnReachUpgradeBtn -= Click;
            EventService.UI.OnToggleSound -= Click;

            EventService.UI.OnToggleInsufficient -= Error;
            EventService.UI.OnToggleDataMismatch -= Error;
            EventService.UI.OnToggleTutorial -= OpenPopup;

            EventService.UI.OnMeterStart -= RushMeter;

            EventService.Resource.OnMenuResourceChanged -= Purchase;

            EventService.Level.OnStart -= StartLevel;

            EventService.Ad.OnReviveWatched -= AdReward;
            EventService.Ad.OnTripleWatched -= AdReward;
            EventService.Ad.OnNoEnemiesWatched -= AdReward;
            EventService.Ad.OnFreeGiftWatched -= AdReward;
            EventService.Ad.OnExtraSpaceWatched -= AdReward;

            EventService.Item.OnItemCollected -= PlayItemCollection;
            EventService.Item.OnObstacleCollected -= PlayerHit;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Play Methods
        private void AdReward(Component c) => Play(SoundType.AdReward);
        private void Click(Component c) => Play(SoundType.Click);
        private void Error(Component c) => Play(SoundType.Error);
        private void Purchase(Component c) => Play(SoundType.Purchase);
        private void RushMeter(Component c) => Play(SoundType.RushMeter);
        private void StartLevel(Component c) => Play(SoundType.StartLevel);
        private void OpenPopup(Component c) => Play(SoundType.OpenPopup);

        private void PlayItemCollection(Component c, ItemType itemType, Vector2 collectionPos)
        {
            if (itemType == ItemType.Tier5) Play(SoundType.Tier5Item);
            else Play(SoundType.LowTierItem);
        }
        private void PlayerHit(Component c) => Play(SoundType.PlayerHit);

        // Functionality
        void Start()
        {
            Initialize();
        }
        private void Initialize()
        {
            InitializeAudiosourcePool();
            var save = SaveSystem.Load();
            _muteState = save.muteSounds;
            CreateBGM();
            SetMuteState();
        }
        private void CreateBGM()
        {
            if (soundCollection.BgmInfo.AudioClip == null)
            {
                _rayDebug.LogError($"BGM is missing from SoundCollection!.", this);
                return;
            }

            var audioSource = new GameObject("BGM").AddComponent<AudioSource>();
            audioSource.transform.SetParent(transform);
            audioSource.clip = soundCollection.BgmInfo.AudioClip;
            audioSource.volume = soundCollection.BgmInfo.Volume;
            audioSource.pitch = soundCollection.BgmInfo.Pitch;
            audioSource.loop = true;
            audioSource.Play();

            _bgm = audioSource;
        }
        private void InitializeAudiosourcePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
                audioSource.transform.SetParent(transform);
                _audioPool.Enqueue(audioSource);
            }
        }
        public void Play(SoundType soundType)
        {
            var soundInfo = soundCollection.GetSoundInfoForType(soundType);

            if (_muteState) return;

            if (_audioPool.Count == 0)
            {
                _rayDebug.LogWarning($"No empty space left in _audioPool , Sound play is Ignored!.", this);
                return;
            }

            AudioSource source = _audioPool.Dequeue();

            if (!source)
            {
                source = new GameObject("AudioSource").AddComponent<AudioSource>();
                source.transform.SetParent(transform);
            }

            source.clip = soundInfo.AudioClips[Random.Range(0, soundInfo.AudioClips.Length)];

            var rndVolumePitch = RandomizedVolumePitch(soundInfo.RndVolumePitch, soundInfo.Volume, soundInfo.Pitch);
            source.volume = rndVolumePitch.Item1;
            source.pitch = rndVolumePitch.Item2;

            source.Play();

            if (source)
            {
                StartCoroutine(ReturnToPool(source));
            }
        }
        private (float, float) RandomizedVolumePitch(RndAmount randomization, float originalValume, float originalPitch)
        {
            float rndVolume = originalValume;
            float rndPitch = originalPitch;

            switch (randomization)
            {
                case RndAmount.None:
                    break;
                case RndAmount.Low:
                    rndVolume += Random.Range(-0.1f, 0.1f);
                    rndPitch += Random.Range(-0.1f, 0.1f);
                    break;
                case RndAmount.Mid:
                    rndVolume += Random.Range(-0.15f, 0.15f);
                    rndPitch += Random.Range(-0.2f, 0.2f);
                    break;
                case RndAmount.High:
                    rndVolume += Random.Range(-0.2f, 0.2f);
                    rndPitch += Random.Range(-0.35f, 0.35f);
                    break;
            }

            return (rndVolume, rndPitch);
        }
        private IEnumerator ReturnToPool(AudioSource source)
        {
            if (!source) yield break;

            yield return new WaitWhile(() => source && source.isPlaying);

            if (source)
            {
                _audioPool.Enqueue(source);
            }
        }
        public void ToggleSounds(Component c)
        {
            _rayDebug.Event("ToggleSounds", c, this);

            var save = SaveSystem.Load();
            save.muteSounds = !save.muteSounds;
            SaveSystem.Save(save);

            _muteState = save.muteSounds;

            SetMuteState();
        }
        void SetMuteState()
        {
            if (_bgm != null) _bgm.mute = _muteState;

            foreach (var source in _audioPool)
            {
                if (source) source.mute = _muteState;
            }
        }
    }
}
