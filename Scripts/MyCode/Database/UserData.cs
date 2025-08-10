using Firebase.Firestore;
using Newtonsoft.Json;
using System;

[FirestoreData]
public class UserData
{
    [FirestoreProperty] public ApplicationData Application { get; set; } = new ApplicationData();
    [FirestoreProperty] public SecurityData Security { get; set; } = new SecurityData();
    [FirestoreProperty] public StatsData Stats { get; set; } = new StatsData();
    [FirestoreProperty] public TenjinData Tenjin { get; set; } = new TenjinData();
    [FirestoreProperty] public FeaturesData Features { get; set; } = new FeaturesData();
    [FirestoreProperty] public brdData bightdData { get; set; } = new brdData();

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
        [FirestoreProperty] public int ReachLevel { get; set; } = 0;
        [FirestoreProperty] public int SpaceLevel { get; set; } = 0;
        [FirestoreProperty] public int RvCount { get; set; } = 0;
        [FirestoreProperty] public int HighestReachEvent { get; set; } = 0;
        [FirestoreProperty] public int TotalSessions { get; set; } = 0;
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
}