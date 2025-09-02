using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using Ray.Services;
using System.Threading;
using System.Threading.Tasks;
[FirestoreData]
public class UserData
{
    [FirestoreProperty] public ApplicationData Application { get; set; } = new ApplicationData();
    [FirestoreProperty] public SecurityData Security { get; set; } = new SecurityData();
    [FirestoreProperty] public StatsData Stats { get; set; } = new StatsData();
    [FirestoreProperty] public TenjinData Tenjin { get; set; } = new TenjinData();
    [FirestoreProperty] public FeaturesData Features { get; set; } = new FeaturesData();
    [FirestoreProperty] public brdData bightdData { get; set; } = new brdData();

    public event Action<int> TotalCurrencyChanged;

    public int TotalCurrency
    {
        get => Stats.TotalCurrency;
        set
        {
            if (Stats.TotalCurrency != value)
            {
                Stats.TotalCurrency = value;
                TotalCurrencyChanged?.Invoke(value);
            }
        }
    }

    [FirestoreData]
    public class ApplicationData
    {
        [FirestoreProperty] public int CurrentVersion { get; set; } = 1;

        [FirestoreProperty]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp LastTimestamp { get; set; }
    }

    [FirestoreData]
    public class SecurityData
    {
        [FirestoreProperty] public bool Cheater { get; set; } = false;
    }

    [FirestoreData]
    public class StatsData
    {
        [FirestoreProperty] public int TotalCurrency { get; set; } = 0;
        [FirestoreProperty] public int Level { get; set; } = 1;
        [FirestoreProperty] public int GroupIndex { get; set; } = 1;
        [FirestoreProperty] public int HighestLevelReached { get; set; } = 1;
        [FirestoreProperty] public int RvCount { get; set; } = 0;
        [FirestoreProperty] public int HighestReachEvent { get; set; } = 0;
        [FirestoreProperty] public int TotalSessions { get; set; } = 0;
        [FirestoreProperty] public int Power_1 { get; set; } = 0;
        [FirestoreProperty] public int Power_2 { get; set; } = 0;
        [FirestoreProperty] public int Power_3 { get; set; } = 0;
        [FirestoreProperty] public int Power_4 { get; set; } = 0;
    }

    [FirestoreData]
    public class TenjinData
    {
        [FirestoreProperty] public string AdNetwork { get; set; } = string.Empty;
        [FirestoreProperty] public string AdvertisementId { get; set; } = string.Empty;
        [FirestoreProperty] public string ClickId { get; set; } = string.Empty;
    }

    [FirestoreData]
    public class FeaturesData
    {
        [FirestoreProperty]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp LastDailyGiftClaim { get; set; }
    }
    [FirestoreData]
    public class brdData
    {
        [FirestoreProperty] public bool RewardClaimed { get; set; }

    }

    public UserData Copy()
    {
        string json = JsonConvert.SerializeObject(this);

        string prettyJson = JsonConvert.SerializeObject(this, Formatting.Indented);
        var _rayDebug = ServiceAllocator.Instance.GetDebugService(ConfigType.Services);
        //_rayDebug.LogWarning($"SAVE DATA COPY{prettyJson}", _rayDebug);

        return JsonConvert.DeserializeObject<UserData>(json);
    }

    public async Task AddCurrency(int amount)
    {
        var saveData = Database.UserData.Copy();
        saveData.TotalCurrency += amount;
        await Database.Instance?.QueueSave(saveData);
    }

    public async Task AddScoreAsCurrency(int score)
    {
        var saveData = Database.UserData.Copy();
        saveData.Stats.TotalCurrency += score;
        saveData.Stats.TotalSessions++;
        await Database.Instance?.QueueSave(saveData);
    }

    public async Task<bool> SpendCurrency(int amount)
    {
        if (TotalCurrency >= amount)
        {
            var saveData = Database.UserData.Copy();
            saveData.TotalCurrency -= amount;
            await Database.Instance?.QueueSave(saveData);
            return true;
        }
        return false;
    }

    public void SetLevel(int value)
    {
        var saveData = Database.UserData.Copy();

        if (value > 3)
        {
            saveData.Stats.GroupIndex = ((value - 1) / 3) + 1;
            saveData.Stats.Level = ((value - 1) % 3) + 1;
        }
        else
        {
            saveData.Stats.Level = value;
        }

        int absoluteLevel = ((saveData.Stats.GroupIndex - 1) * 3) + saveData.Stats.Level;
        if (absoluteLevel > saveData.Stats.HighestLevelReached)
        {
            saveData.Stats.HighestLevelReached = absoluteLevel;
        }

        // Update the runtime copy immediately so subsequent code sees the new level
        Database.UserData = saveData;
        Database.Instance?.QueueSave(saveData);
    }

    public void SetGroupIndex(int value)
    {
        var saveData = Database.UserData.Copy();
        saveData.Stats.GroupIndex = value;

        int absoluteLevel = ((saveData.Stats.GroupIndex - 1) * 3) + saveData.Stats.Level;
        if (absoluteLevel > saveData.Stats.HighestLevelReached)
        {
            saveData.Stats.HighestLevelReached = absoluteLevel;
        }

        // Ensure local data reflects the new group before saving asynchronously
        Database.UserData = saveData;
        Database.Instance?.QueueSave(saveData);
    }
}
