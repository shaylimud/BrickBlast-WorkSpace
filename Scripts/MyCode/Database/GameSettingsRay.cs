using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[FirestoreData]
public class GameSettingsRay
{
    [FirestoreProperty] public AdvertisingData Advertising { get; set; }
    [FirestoreProperty] public ApplicationData Application { get; set; }
    [FirestoreProperty] public EventsData Events { get; set; }
    [FirestoreProperty] public MultipliersData Multipliers { get; set; }
    [FirestoreProperty] public SecurityData Security { get; set; }
    [FirestoreProperty] public InAppPurchasesData InAppPurchases { get; set; }
    [FirestoreProperty] public FreeGiftData FreeGift { get; set; }
    [FirestoreProperty] public ExtraSpaceData ExtraSpace { get; set; }
    [FirestoreProperty] public BrdData BrightData { get; set; }
    public class AdvertisingData
    {
        [FirestoreProperty] public string Banner { get; set; }
        [FirestoreProperty] public RewardedAdUnits Rewarded { get; set; }
        [FirestoreProperty] public InterstitialAdUnits Interstitial { get; set; }
        [FirestoreProperty] public AdFreqs Freqs { get; set; }
    }

    [FirestoreData]
    public class AdFreqs
    {
        [FirestoreProperty] public int InterFreq { get; set; }
        [FirestoreProperty] public int NoEnemiesFreq { get; set; }
    }

    [FirestoreData]
    public class RewardedAdUnits
    {
        [FirestoreProperty] public string NoEnemies { get; set; }
        [FirestoreProperty] public string Revive { get; set; }
        [FirestoreProperty] public string Triple { get; set; }
        [FirestoreProperty] public string Penalty { get; set; }
        [FirestoreProperty] public string FreeGift { get; set; }
        [FirestoreProperty] public string ExtraSpace { get; set; }
    }

    [FirestoreData]
    public class InterstitialAdUnits
    {
        [FirestoreProperty] public string Regular { get; set; }
        [FirestoreProperty] public string Penalty { get; set; }
    }

    [FirestoreData]
    public class ApplicationData
    {
        [FirestoreProperty] public int MinimalVersion { get; set; }
    }

    [FirestoreData]
    public class EventsData
    {
        [FirestoreProperty] public Dictionary<string, object> ReachEvents { get; set; }

        public List<int> SortedReachEvents()
        {
            return ReachEvents.Keys
                .Select(key => int.Parse(key))
                .OrderBy(num => num)
                .ToList();
        }
    }

    [FirestoreData]
    public class MultipliersData
    {
        [FirestoreProperty] public Dictionary<string, float> Reach { get; set; }
        [FirestoreProperty] public Dictionary<string, float> Space { get; set; }

        public float MultiplierByLevel(Dictionary<string, float> multipliers, int level)
        {
            return multipliers
                .Where(m => int.Parse(m.Key) <= level)
                .OrderByDescending(m => int.Parse(m.Key))
                .Select(m => m.Value)
                .FirstOrDefault();
        }
    }

    [FirestoreData]
    public class SecurityData
    {
        [FirestoreProperty] public bool AllowPenalties { get; set; }
    }

    [FirestoreData]
    public class FreeGiftData
    {
        [FirestoreProperty] public int CooldownMinutes { get; set; }
        [FirestoreProperty] public Dictionary<string, float> RewardByReachCostMultiplier { get; set; }
        [FirestoreProperty] public int RewardCap { get; set; }

        public float RandomRewardByReachCost()
        {
            float min = RewardByReachCostMultiplier["Min"];
            float max = RewardByReachCostMultiplier["Max"];

            return UnityEngine.Random.Range(min, max);
        }
    }

    [FirestoreData]
    public class InAppPurchasesData
    {
        [FirestoreProperty] public Dictionary<string, int> Consumables { get; set; }
        [FirestoreProperty] public string SubscriptionNoAds { get; set; }
        [FirestoreProperty] public BundleData Bundle_1 { get; set; }

        public int ConsumableRewardById(string productId)
        {
            if (Consumables != null && Consumables.TryGetValue(productId, out int reward))
            {
                return reward;
            }
            return 0;
        }
    }

    [FirestoreData]
    public class BundleData
    {
        [FirestoreProperty] public int Booster_Col { get; set; }
        [FirestoreProperty] public int Booster_Row { get; set; }
        [FirestoreProperty] public int Booster_Shape { get; set; }
        [FirestoreProperty] public int Booster_Square { get; set; }
        [FirestoreProperty] public string ID { get; set; }
    }

    [FirestoreData]
    public class BrdData
    {
        [FirestoreProperty] public int Reward { get; set; }
        [FirestoreProperty] public bool Enable { get; set; }
    }

    [FirestoreData]
    public class ExtraSpaceData
    {
        [FirestoreProperty] public int Freq { get; set; }
        [FirestoreProperty] public float LevelRatioCap { get; set; }
        [FirestoreProperty] public int ShowPerLevel { get; set; }
    }
}