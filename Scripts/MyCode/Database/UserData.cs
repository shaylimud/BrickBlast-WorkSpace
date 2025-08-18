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
    public event Action<int> LevelChanged;
    public event Action<int> GroupIndexChanged;
    private int level = 1;
    private int groupIndex = 1;

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

    [FirestoreProperty]
    public int Level
    {
        get => level;
        set
        {
            if (level != value)
            {
                level = value;
                if (level % 4 == 0)
                {
                    GroupIndex++;
                    level = 1;
                }
                LevelChanged?.Invoke(level);
            }
        }
    }

    [FirestoreProperty]
    public int GroupIndex
    {
        get => groupIndex;
        set
        {
            if (groupIndex != value)
            {
                groupIndex = value;
                GroupIndexChanged?.Invoke(value);
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
        [FirestoreProperty] public int SpaceLevel { get; set; } = 0;
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
        await Database.Instance?.Save(saveData);
    }

    public async Task AddScoreAsCurrency(int score)
    {
        var saveData = Database.UserData.Copy();
        saveData.Stats.TotalCurrency += score;
        saveData.Stats.TotalSessions++;
        await Database.Instance?.Save(saveData);
    }

    public async Task<bool> SpendCurrency(int amount)
    {
        if (TotalCurrency >= amount)
        {
            var saveData = Database.UserData.Copy();
            saveData.TotalCurrency -= amount;
            await Database.Instance?.Save(saveData);
            return true;
        }
        return false;
    }

    public void SetLevel(int value)
    {
        var saveData = Database.UserData.Copy();
        saveData.Level = value;
        Database.Instance?.Save(saveData);
    }

    public void SetGroupIndex(int value)
    {
        var saveData = Database.UserData.Copy();
        saveData.GroupIndex = value;
        Database.Instance?.Save(saveData);
    }
}