
using Firebase.Firestore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Ray.Features;

namespace Ray.Services
{
    public class ResourceService : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField, RequireReference] private ResourceEvaluationConfig _resourceEvaluationConfig;

        [HideInInspector] public EncryptedField<int> LevelCurrency = new EncryptedField<int>(0);

        // Transient score submitted by gameplay handlers, merged with currency at level end


        [HideInInspector] public EncryptedField<int> LevelScore = new EncryptedField<int>(0);
        [HideInInspector] public EncryptedField<int> LevelSpace = new EncryptedField<int>(0);
        [HideInInspector] public EncryptedField<int> LevelsPlayed = new EncryptedField<int>(0);
        [HideInInspector] public EncryptedField<bool> NoEnemies = new EncryptedField<bool>(false);
        [HideInInspector] public EncryptedField<bool> IncreaseRvCount = new EncryptedField<bool>(false);

        private UserData _pendingSaveData;
        private UserData SaveData => _pendingSaveData ??= Database.UserData.Copy();
        
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static ResourceService Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventService.UI.OnSpaceUpgradeBtn += ProcessSpaceUpgrade;
            EventService.UI.OnReachUpgradeBtn += ProcessReachUpgrade;
            EventService.UI.OnBackToMenu += HandleBackToMenu;
            EventService.UI.OnBoosterPurchaseBtn += ProcessBoosterPurchase;

            EventService.Level.OnStart += ResetLevelResources;

            EventService.Player.OnParked += RewardEndCurrency;

            EventService.Item.OnItemCollected += ProcessItemValueUsingY;

            EventService.Ad.OnFreeGiftWatched += ClaimFreeGift;
            EventService.Ad.OnNoEnemiesWatched += RewardNoEnemies;
            EventService.Ad.OnTripleWatched += RewardEndTriple;

            // ExtraSpace rewarded ads are repurposed for boosters, so no listener here

            EventService.Brd.OnFirstTimeConsent += RewardBrightData;
        }

        private void ProcessSpaceUpgrade(Component c) => ProcessUpgrade(c, UpgradeType.Space);

        private void ProcessReachUpgrade(Component c) => ProcessUpgrade(c, UpgradeType.Reach);

        public void SubmitLevelScore(int score)
        {
            LevelScore.Value = score;
        }

        private async Task CommitEndGameChanges()
        {
            if (_pendingSaveData == null) return;
            await Database.Instance.Save(_pendingSaveData);
            _pendingSaveData = null;
        }

        private void ProcessUpgrade(Component c, UpgradeType upgradeType)
        {
            _rayDebug.Event($"ProcessUpgrade - {upgradeType}", c, this);

            if (PanalizedUser() && RewardedService.Instance.IsRewardedReady(RewardedType.Penalty)) EventService.UI.OnRewardedBtn(this, RewardedType.Penalty);

            var upgradeProp = upgradeType == UpgradeType.Reach ? _resourceEvaluationConfig.ReachUpgradeProperties : _resourceEvaluationConfig.SpaceUpgradeProperties;
            int upgradeCost = UpgradeCost(upgradeType);

            if (Database.UserData.Stats.TotalCurrency < upgradeCost)
            {
                EventService.UI.OnToggleInsufficient.Invoke(this);
                return;
            }

            var saveData = SaveData;
            saveData.Stats.TotalCurrency -= upgradeCost;
            Database.UserData.TotalCurrency = Database.UserData.TotalCurrency - upgradeCost;

            if (upgradeType == UpgradeType.Reach)
            {
                saveData.Level += upgradeProp.LevelIncrement;
                Database.UserData.Level += upgradeProp.LevelIncrement;
            }
            else
            {
                saveData.Stats.SpaceLevel += upgradeProp.LevelIncrement;
                Database.UserData.Stats.SpaceLevel += upgradeProp.LevelIncrement;
            }

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        public int UpgradeCost(UpgradeType upgradeType)
        {
            var upgradeProp = upgradeType == UpgradeType.Reach ? _resourceEvaluationConfig.ReachUpgradeProperties : _resourceEvaluationConfig.SpaceUpgradeProperties;
            int level = upgradeType == UpgradeType.Reach ? Database.UserData.Level : Database.UserData.Stats.SpaceLevel;
            var multiplierDic = upgradeType == UpgradeType.Reach ? Database.GameSettings.Multipliers.Reach : Database.GameSettings.Multipliers.Space;

            // Sort multipliers by level breakpoint (ascending order)
            var sortedMultipliers = multiplierDic
                .Select(kv => new { Level = int.Parse(kv.Key), Multiplier = kv.Value })
                .OrderBy(m => m.Level)
                .ToList();

            int upgradesDone = (level - upgradeProp.BaseLevel) / upgradeProp.LevelIncrement;
            float currentCost = upgradeProp.BaseCost;
            int currentLevel = upgradeProp.BaseLevel;
            float currentMultiplier = sortedMultipliers[0].Multiplier; // Start with the lowest level multiplier

            foreach (var entry in sortedMultipliers)
            {
                if (entry.Level > level) break; // Stop if we reached the user's level

                while (currentLevel < entry.Level && currentLevel < level)
                {
                    currentCost *= currentMultiplier; // Apply the previous multiplier
                    currentLevel += upgradeProp.LevelIncrement;
                }

                currentMultiplier = entry.Multiplier; // Update multiplier to the new breakpoint
            }

            // Apply the final multiplier for the last few levels
            while (currentLevel < level)
            {
                currentCost *= currentMultiplier;
                currentLevel += upgradeProp.LevelIncrement;
            }

            return Mathf.FloorToInt(currentCost);
        }

        private void ProcessBoosterPurchase(Component c, BoosterType boosterType, int cost)
        {
            _rayDebug.Event($"ProcessBoosterPurchase - {boosterType}", c, this);

            if (Database.UserData.Stats.TotalCurrency < cost)
            {
                EventService.UI.OnToggleInsufficient.Invoke(this);
                return;
            }

            var saveData = SaveData;

            switch (boosterType)
            {
                case BoosterType.ClearRow:
                    if (saveData.Stats.Power_1 >= 99) return;
                    saveData.Stats.Power_1 = Mathf.Clamp(saveData.Stats.Power_1 + 1, 0, 99);
                    Database.UserData.Stats.Power_1 = saveData.Stats.Power_1;
                    break;
                case BoosterType.ClearColumn:
                    if (saveData.Stats.Power_2 >= 99) return;
                    saveData.Stats.Power_2 = Mathf.Clamp(saveData.Stats.Power_2 + 1, 0, 99);
                    Database.UserData.Stats.Power_2 = saveData.Stats.Power_2;
                    break;
                case BoosterType.ClearSquare:
                    if (saveData.Stats.Power_3 >= 99) return;
                    saveData.Stats.Power_3 = Mathf.Clamp(saveData.Stats.Power_3 + 1, 0, 99);
                    Database.UserData.Stats.Power_3 = saveData.Stats.Power_3;
                    break;
                case BoosterType.ChangeShape:
                    if (saveData.Stats.Power_4 >= 99) return;
                    saveData.Stats.Power_4 = Mathf.Clamp(saveData.Stats.Power_4 + 1, 0, 99);
                    Database.UserData.Stats.Power_4 = saveData.Stats.Power_4;
                    break;
            }

            saveData.Stats.TotalCurrency -= cost;
            Database.UserData.TotalCurrency = Database.UserData.TotalCurrency - cost;

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        public void RewardBooster(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.ClearRow:
                    Database.UserData.Stats.Power_1 = Mathf.Clamp(Database.UserData.Stats.Power_1 + 1, 0, 99);
                    break;
                case BoosterType.ClearColumn:
                    Database.UserData.Stats.Power_2 = Mathf.Clamp(Database.UserData.Stats.Power_2 + 1, 0, 99);
                    break;
                case BoosterType.ClearSquare:
                    Database.UserData.Stats.Power_3 = Mathf.Clamp(Database.UserData.Stats.Power_3 + 1, 0, 99);
                    break;
                case BoosterType.ChangeShape:
                    Database.UserData.Stats.Power_4 = Mathf.Clamp(Database.UserData.Stats.Power_4 + 1, 0, 99);
                    break;
            }

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        public void ConsumeBooster(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.ClearRow:
                    Database.UserData.Stats.Power_1 = Mathf.Clamp(Database.UserData.Stats.Power_1 - 1, 0, 99);
                    break;
                case BoosterType.ClearColumn:
                    Database.UserData.Stats.Power_2 = Mathf.Clamp(Database.UserData.Stats.Power_2 - 1, 0, 99);
                    break;
                case BoosterType.ClearSquare:
                    Database.UserData.Stats.Power_3 = Mathf.Clamp(Database.UserData.Stats.Power_3 - 1, 0, 99);
                    break;
                case BoosterType.ChangeShape:
                    Database.UserData.Stats.Power_4 = Mathf.Clamp(Database.UserData.Stats.Power_4 - 1, 0, 99);
                    break;
            }

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        public void RewardNoEnemies(Component c)
        {
            _rayDebug.Event("RewardNoEnemies", c, this);

            var saveData = SaveData;

            NoEnemies.Value = true;

            EventService.Resource.OnNoEnemiesReceived.Invoke(this);
        }

        private void ResetLevelResources(Component c)
        {
            _rayDebug.Event("ResetLevelResources", c, this);

            LevelCurrency.Value = 0;
            LevelScore.Value = 0;

            LevelSpace.Value = Database.UserData.Stats.SpaceLevel;

            EventService.Resource.OnLevelResourceChanged.Invoke(this);
            EventService.Resource.OnEndCurrencyChanged.Invoke(this);
        }

        private void ProcessItemValueUsingY(Component c, ItemType itemType, Vector2 itemPos)
        {
            _rayDebug.Event("ProcessItemValueUsingY", c, this);

            var collectedItemProp = _resourceEvaluationConfig.CollectedItemProperties;
            var itemTypeProp = _resourceEvaluationConfig.GetItemTypeEvaluationForType(itemType);

            int itemValue = collectedItemProp.BaseValue;

            int increments = Mathf.FloorToInt(-itemPos.y / collectedItemProp.VerticalIncrement);
            itemValue += increments * collectedItemProp.VerticalIncrementValue;
            itemValue = Mathf.FloorToInt(itemValue * itemTypeProp.Multiplier);
            EventService.Resource.OnCollectedItemValueProccessed.Invoke(this, itemType, itemValue, itemPos);

            LevelCurrency.Value += itemValue;
            LevelSpace.Value--;
            EventService.Resource.OnLevelResourceChanged.Invoke(this);

            if (LevelSpace.Value <= 0)
            {
                EventService.Resource.OnSpaceLimitReached?.Invoke(this);
            }
        }

        private void RewardEndCurrency(Component c)
        {
            _rayDebug.Event("RewardEndCurrency", c, this);

            int total = LevelCurrency.Value + LevelScore.Value;

            LevelCurrency.Value = total;

            var saveData = SaveData;
            saveData.Stats.TotalCurrency += total;
            saveData.Stats.TotalSessions++;
            Database.UserData.TotalCurrency = Database.UserData.TotalCurrency + total;
            Database.UserData.Stats.TotalSessions++;
            LevelScore.Value = 0;

            EventService.Resource.OnEndCurrencyChanged(this);
        }

        private void RewardEndTriple(Component c)
        {
            _rayDebug.Event("RewardEndTriple", c, this);

            int DoubleAmount = LevelCurrency.Value * 2;

            var saveData = SaveData;
            saveData.Stats.TotalCurrency += DoubleAmount;
            Database.UserData.TotalCurrency = Database.UserData.TotalCurrency + DoubleAmount;
            LevelCurrency.Value *= 3;

            EventService.Resource.OnEndCurrencyChanged(this);
        }
        private void RewardBrightData(Component c)
        {
            _rayDebug.Event("RewardBrightData", c, this);

            int Amount = Database.GameSettings.BrightData.Reward;

            var saveData = SaveData;
            saveData.bightdData.RewardClaimed = true;
            saveData.Stats.TotalCurrency += Amount;
            Database.UserData.bightdData.RewardClaimed = true;
            Database.UserData.TotalCurrency = Database.UserData.TotalCurrency + Amount;

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        private async void HandleBackToMenu(Component c)
        {
            LevelsPlayed.Value++;
            NoEnemies.Value = false;
            await CommitEndGameChanges();
        }

        public async Task<(bool canClaim, string cooldownTime)> CanClaimFreeGift()
        {
            if (Database.UserData.Features.LastDailyGiftClaim == default)
            {
                return (true, "--:--:--"); // First-time claim, no cooldown needed
            }

            Timestamp lastClaimTime = Database.UserData.Features.LastDailyGiftClaim;
            Timestamp currentTime = await TimeApiService.Instance.GetCurrentTime();

            double minutesSinceLastClaim = (currentTime.ToDateTime() - lastClaimTime.ToDateTime()).TotalMinutes;
            double cooldownMinutes = Database.GameSettings.FreeGift.CooldownMinutes;

            if (minutesSinceLastClaim >= cooldownMinutes)
            {
                return (true, "--:--:--"); // Available
            }

            // Calculate time remaining
            double minutesLeft = cooldownMinutes - minutesSinceLastClaim;

            int hours = Mathf.Max(0, (int)(minutesLeft / 60));
            int minutes = Mathf.Max(0, (int)(minutesLeft % 60));
            int seconds = Mathf.Max(0, (int)((minutesLeft - (hours * 60) - minutes) * 60));

            string formattedTime = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

            return (false, formattedTime);
        }

        public async void ClaimFreeGift(Component c)
        {
            _rayDebug.Event("ClaimFreeGift", c, this);

            BufferService.Instance.RequestBuffer(); // Because we await TimeAPI get current time before a save

            int giftAmount = Mathf.Clamp(
                (int)(UpgradeCost(UpgradeType.Reach) * Database.GameSettings.FreeGift.RandomRewardByReachCost()), 0, Database.GameSettings.FreeGift.RewardCap);

            var saveData = Database.UserData.Copy();
            saveData.Stats.TotalCurrency += giftAmount;
            saveData.Features.LastDailyGiftClaim = await TimeApiService.Instance.GetCurrentTime();

            BufferService.Instance.ReleaseBuffer();
            await Database.Instance.Save(saveData);
            _rayDebug.Log($"Free gift claimed: +{giftAmount} coins!", this);

            EventService.Resource.OnMenuResourceChanged.Invoke(this);
        }

        public bool PanalizedUser()
        {
            return (Database.UserData.Security.Cheater && Database.GameSettings.Security.AllowPenalties);
        }

        private void RewardExtraSpace(Component c)
        {
            _rayDebug.Event("RewardExtraSpace", c, this);

            LevelSpace.Value += ExtraSpaceFeature.Instance.ExtraSpaceReward.Value;

            EventService.Resource.OnExtraSpaceReceived.Invoke(this);

            EventService.Resource.OnLevelResourceChanged.Invoke(this);
        }
    }
}