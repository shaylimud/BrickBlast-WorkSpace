using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public class Database : MonoBehaviour
    {
        public static GameSettingsRay GameSettings;
        public static UserData UserData;

        private FirebaseFirestore _firestore;
        private string _userID;
        public string UserId => _userID;
        private readonly SemaphoreSlim _saveSemaphore = new SemaphoreSlim(1, 1);

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static Database Instance;
        private void Awake()
        {
            Instance = this;
        }

        public async Task Initialize()
        {
            _rayDebug.Log("Database.Initialize() - Start", this);

            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            _rayDebug.Log("Database.Initialize() - Firebase dependency check complete: " + dependencyStatus, this);

            if (dependencyStatus == DependencyStatus.Available)
            {
                _firestore = FirebaseFirestore.DefaultInstance;

                _rayDebug.Log("Database.Initialize() - Firebase initialized. Proceeding to authorize user...", this);
                await AutorizeUser();
                _rayDebug.Log("Database.Initialize() - User authorized. Proceeding to load GameSettings...", this);
                await LoadGameSettings();
                _rayDebug.Log("Database.Initialize() - GameSettings loaded. Proceeding to load UserData...", this);
                await LoadUserData();
                _rayDebug.Log("Database.Initialize() - Initialization complete", this);
            }
            else
            {
                _rayDebug.LogError("Firebase dependencies are not available: " + dependencyStatus, this);
            }
        }

        private async Task AutorizeUser()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            if (auth.CurrentUser == null)
            {
                try
                {
                    var result = await auth.SignInAnonymouslyAsync();
                    _userID = result.User.UserId;
                    Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, _userID);
                }
                catch (Exception e)
                {
                    _rayDebug.LogError("SignInAnonymouslyAsync encountered an error: " + e, this);
                    return;
                }
            }
            else
            {
                _userID = auth.CurrentUser.UserId;
            }
        }

    private async Task LoadGameSettings()
    {
        _rayDebug.Log("LoadGameSettings() - Start", this);
        _firestore = FirebaseFirestore.DefaultInstance;
        var gameSettings = _firestore.Collection("GameConfig").Document("Game Settings");
    
        try
        {
            DocumentSnapshot snapshot = await gameSettings.GetSnapshotAsync();
            _rayDebug.Log("LoadGameSettings() - Firestore snapshot received. Exists: " + snapshot.Exists, this);
    
            if (snapshot.Exists)
            {
                Dictionary<string, object> rawData = snapshot.ToDictionary();
                string jsonData = JsonConvert.SerializeObject(rawData, Formatting.Indented);
                
                // 🔍 Full raw Game Settings from Firestore
                _rayDebug.Log("Full GameSettings document from Firestore:\n" + jsonData, this);
    
                GameSettings = JsonConvert.DeserializeObject<GameSettingsRay>(jsonData);
    
                if (GameSettings == null)
                    _rayDebug.LogError("GameSettings is null after deserialization", this);
                else if (GameSettings.Application == null)
                    _rayDebug.LogError("GameSettings.Application is null after deserialization", this);
                else
                    _rayDebug.Log("Game Settings object deserialized successfully.", this);
            }
            else
            {
                _rayDebug.LogWarning("Game Settings document does not exist.", this);
            }
        }
        catch (Exception e)
        {
            _rayDebug.LogError($"Database: Error loading Game Settings - {e}", this);
        }
    }

        private async Task LoadUserData()
        {
            _rayDebug.Log("LoadUserData() - Start", this);
            var userDoc = _firestore.Collection("UserData").Document(_userID);

            try
            {
                DocumentSnapshot serverUserData = await userDoc.GetSnapshotAsync();
                _rayDebug.Log("LoadUserData() - Firestore snapshot received. Exists: " + serverUserData.Exists, this);

                if (serverUserData.Exists)
                {
                    UserData = serverUserData.ConvertTo<UserData>();
                    _rayDebug.Log("LoadUserData() - UserData converted from Firestore", this);

                    if (UserData == null)
                    {
                        _rayDebug.LogError("UserData is null after ConvertTo<UserData>()", this);
                        return;
                    }

                    if (UserData.Application == null)
                    {
                        _rayDebug.LogWarning("UserData.Application is null. Assigning default ApplicationData.", this);
                        UserData.Application = new UserData.ApplicationData();
                    }

                    EnsureAllFieldsExist(UserData);

                    _rayDebug.Log($"LoadUserData() - UserData.Application.CurrentVersion (before set): {UserData.Application.CurrentVersion}", this);
                    UserData.Application.CurrentVersion = int.Parse(Application.version);
                    _rayDebug.Log($"LoadUserData() - Application version set to {UserData.Application.CurrentVersion}", this);

                    await MigrateLegacyProgress();

                    await Save(UserData);
                    _rayDebug.Log("User Data Loaded and Saved: " + JsonUtility.ToJson(UserData, true), this);
                }
                else
                {
                    _rayDebug.Log("User data not found. Creating new user data...", this);
                    await CreateNewUserData(userDoc);
                }
            }
            catch (Exception e)
            {
                _rayDebug.LogError($"Database: Error loading User Data - {e}", this);
            }
        }

        private async Task MigrateLegacyProgress()
        {
            if (PlayerPrefs.GetInt("MigratedToUserData", 0) == 1)
            {
                return;
            }

            bool changed = false;

            if (PlayerPrefs.HasKey("Coins"))
            {
                UserData.TotalCurrency = PlayerPrefs.GetInt("Coins");
                changed = true;
            }

            if (PlayerPrefs.HasKey("Level"))
            {
                UserData.Stats.Level = PlayerPrefs.GetInt("Level");
                changed = true;
            }

            if (changed)
            {
                await Save(UserData);
            }

            PlayerPrefs.SetInt("MigratedToUserData", 1);
            PlayerPrefs.Save();
        }
        private void EnsureAllFieldsExist(object obj)
        {
            // Checks if the Server UserData is missing a field set in the Client UserData
            // If so, it is Created and the default value is the one set in the Client UserData
            Type type = obj.GetType();
            object defaultInstance = Activator.CreateInstance(type);

            foreach (PropertyInfo prop in type.GetProperties())
            {
                var currentValue = prop.GetValue(obj);
                if (currentValue == null)
                {
                    prop.SetValue(obj, prop.GetValue(defaultInstance));
                }
                else if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string))
                {
                    EnsureAllFieldsExist(currentValue);
                }
            }
        }

        private async Task CreateNewUserData(DocumentReference userDoc)
        {
            var defaultDataDoc = _firestore.Collection("GameConfig").Document("Default Stats");

            try
            {
                DocumentSnapshot defaultSnapshot = await defaultDataDoc.GetSnapshotAsync();
                if (defaultSnapshot.Exists)
                {
                    Dictionary<string, object> defaultData = defaultSnapshot.ToDictionary();
                    int defaultLevel = defaultData.ContainsKey("Level") ? Convert.ToInt32(defaultData["Level"]) : 1;
                    int defaultGroupIndex = defaultData.ContainsKey("GroupIndex") ? Convert.ToInt32(defaultData["GroupIndex"]) : 1;
                    int defaultPower1 = defaultData.ContainsKey("Power_1") ? Convert.ToInt32(defaultData["Power_1"]) : 0;
                    int defaultPower2 = defaultData.ContainsKey("Power_2") ? Convert.ToInt32(defaultData["Power_2"]) : 0;
                    int defaultPower3 = defaultData.ContainsKey("Power_3") ? Convert.ToInt32(defaultData["Power_3"]) : 0;
                    int defaultPower4 = defaultData.ContainsKey("Power_4") ? Convert.ToInt32(defaultData["Power_4"]) : 0;

                    Timestamp currentTime = await TimeApiService.Instance.GetCurrentTime();

                    // Add Default values to new Server User Data
                    UserData = new UserData
                    {
                        Application = new UserData.ApplicationData
                        {
                            LastTimestamp = currentTime,
                            CurrentVersion = int.Parse(Application.version)
                        },

                        Stats = new UserData.StatsData
                        {
                            Level = defaultLevel,
                            GroupIndex = defaultGroupIndex,
                            HighestLevelReached = ((defaultGroupIndex - 1) * 3) + defaultLevel,
                            Power_1 = defaultPower1,
                            Power_2 = defaultPower2,
                            Power_3 = defaultPower3,
                            Power_4 = defaultPower4
                        }
                    };

                    // Save new user data to Firestore
                    await userDoc.SetAsync(UserData);
                    _rayDebug.Log("New user data created with default values.", this);
                }
                else
                {
                    _rayDebug.LogError("Default Data document not found in Firestore! are you sure the Path correct?", this);
                }
            }
            catch (Exception e)
            {
                _rayDebug.LogError($"Error creating new user data - {e}", this);
            }
        }

        public async Task Save(UserData saveData)
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                BufferService.Instance.RequestBuffer();

                var userDoc = _firestore.Collection("UserData").Document(_userID);
                DocumentSnapshot serverSnapshot = await userDoc.GetSnapshotAsync();
                UserData serverUserData = serverSnapshot.ConvertTo<UserData>();

                // Cheat Detection
                List<string> mismatchedFields = new List<string>();
                Dictionary<string, (int oldValue, int newValue)> mismatchedValues = new Dictionary<string, (int, int)>();

                foreach (PropertyInfo prop in typeof(UserData.StatsData).GetProperties())
                {
                    // Level, GroupIndex and HighestLevelReached are updated locally before a save occurs,
                    // which causes a false mismatch when compared against the server snapshot.
                    // Skip these fields when performing the cheat/mismatch detection.
                    if (prop.Name == nameof(UserData.StatsData.Level) ||
                        prop.Name == nameof(UserData.StatsData.GroupIndex) ||
                        prop.Name == nameof(UserData.StatsData.HighestLevelReached))
                    {
                        continue;
                    }

                    if (prop.PropertyType == typeof(int)) // Ensure property is an int before casting
                    {
                        int clientValue = (int)prop.GetValue(UserData.Stats);
                        int serverValue = (int)prop.GetValue(serverUserData.Stats);

                        if (!Equals(clientValue, serverValue))
                        {
                            mismatchedFields.Add(prop.Name);
                            mismatchedValues[prop.Name] = (serverValue, clientValue);
                        }
                    }
                    else
                    {
                        _rayDebug.LogWarning($"Skipping property {prop.Name} because it is not an int. Type: {prop.PropertyType}", this);
                    }
                }

                if (mismatchedFields.Count > 0)
                {
                    _rayDebug.LogWarning($"Stats data mismatch detected in: {string.Join(", ", mismatchedFields)}!", this);
                    foreach (var field in mismatchedValues)
                    {
                        _rayDebug.LogWarning($"{field.Key} - Server: {field.Value.oldValue}, Client: {field.Value.newValue}", this);

                        // Ensure there is a listener before invoking to avoid null reference exceptions
                        EventService.Database.OnMismatchDetected?.Invoke(this);
                        TenjinService.Instance.SendCheatEvent(field.Key, field.Value.oldValue.ToString(), field.Value.newValue.ToString());
                    }

                    saveData.Stats = serverUserData.Stats;
                    saveData.Security.Cheater = true;
                }

                UserData = saveData; // Transfer modifications to client after cheat check

                int absoluteLevel = (UserData.Stats.GroupIndex - 1) * 3 + UserData.Stats.Level;
                if (absoluteLevel > UserData.Stats.HighestLevelReached)
                {
                    UserData.Stats.HighestLevelReached = absoluteLevel;
                }

                // Check and update Highest Reach Event
                if (UserData.Stats.Level > serverUserData.Stats.Level)
                {
                    List<int> sortedReachEvents = GameSettings.Events.SortedReachEvents();
                    int highestValidEvent = sortedReachEvents.Where(e => e <= UserData.Stats.Level).DefaultIfEmpty(0).Max();
                    if (highestValidEvent > UserData.Stats.HighestReachEvent)
                    {
                        UserData.Stats.HighestReachEvent = highestValidEvent;

                        TenjinService.Instance.SendReachEvent(UserData.Security.Cheater, highestValidEvent);

                        _rayDebug.Log($"Updated HighestReachEvent to {highestValidEvent}", this);
                    }
                }

                if (ResourceService.Instance.IncreaseRvCount.Value) UserData.Stats.RvCount++;
                ResourceService.Instance.IncreaseRvCount.Value = false;

                UserData.Application.LastTimestamp = await TimeApiService.Instance.GetCurrentTime();

                await userDoc.SetAsync(UserData);
                _rayDebug.Log("Server User Data Updated", this);

                if (mismatchedFields.Count > 0) EventService.Resource.OnMenuResourceChanged.Invoke(this); // Used to change Upgrade Cost Icons to RV for penalty
            }
            finally
            {
                BufferService.Instance.ReleaseBuffer();
                _saveSemaphore.Release();
            }
        }

        // QA
        public void MarkAsCheat() => MarkUserAsCheat();
        public void Add100Mil() => ModifyUserCurrency();
        public void ResetData() => ResetUserData();
        public async Task ModifyUserCurrency()
        {
            BufferService.Instance.RequestBuffer();

            var userDoc = _firestore.Collection("UserData").Document(_userID);

            try
            {
                DocumentSnapshot serverSnapshot = await userDoc.GetSnapshotAsync();
                if (serverSnapshot.Exists)
                {
                    UserData serverUserData = serverSnapshot.ConvertTo<UserData>();

                    // Add 100 million to Total Currency
                    serverUserData.Stats.TotalCurrency += 100_000_000;

                    // Save back to Firestore
                    await userDoc.SetAsync(serverUserData);
                    UserData = serverUserData;

                    _rayDebug.Log("User currency updated: +100 Million added.", this);
                }
                else
                {
                    _rayDebug.LogWarning("User data not found in Firestore.", this);
                }
            }
            catch (Exception e)
            {
                _rayDebug.LogError($"Error modifying user currency: {e}", this);
            }

            BufferService.Instance.ReleaseBuffer();

            EventService.Resource.OnMenuResourceChanged(this);
        }
        public async Task ResetUserData()
        {
            BufferService.Instance.RequestBuffer();

            var userDoc = _firestore.Collection("UserData").Document(_userID);

            try
            {
                // Delete existing user data
                await userDoc.DeleteAsync();
                _rayDebug.Log("User data deleted from Firestore.", this);

                // Re-create new user data from default stats
                await CreateNewUserData(userDoc);
                _rayDebug.Log("User data has been reset.", this);
            }
            catch (Exception e)
            {
                _rayDebug.LogError($"Error resetting user data: {e}", this);
            }

            BufferService.Instance.ReleaseBuffer();

            EventService.Resource.OnMenuResourceChanged(this);
        }
        public async Task MarkUserAsCheat()
        {
            _rayDebug.Log("Marking User As Cheat", this);

            UserData.Stats.TotalCurrency++;

            var saveData = Database.UserData.Copy();
            await Database.Instance.Save(saveData);
        }

        // Development
        public void UpdateFirestoreDocument()
        {
            // Used to Create Firestore Documents using Client Stracture (Saves Time)

            _firestore = FirebaseFirestore.DefaultInstance;

            var gameSettings = new GameSettingsRay();
            _firestore.Document($"Test/0").SetAsync(gameSettings);
        }
        public void ShowFirestoreDocument()
        {
            // Used to fetch and see Firestore Documents (Useful when you created a Firestore structure and need ChatGPT to create a client version)

            _firestore = FirebaseFirestore.DefaultInstance;

            _firestore.Collection("GameConfig").Document("Default Stats").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    _rayDebug.LogError("Failed to fetch document: " + task.Exception, this);
                    return;
                }

                if (task.Result.Exists)
                {
                    Dictionary<string, object> gameSettings = task.Result.ToDictionary();
                    string prettyJson = JsonConvert.SerializeObject(gameSettings, Formatting.Indented);
                    _rayDebug.Log("Default Stats:\n" + prettyJson, this);
                }
                else
                {
                    _rayDebug.LogWarning("Document does not exist.", this);
                }
            });
        }
    }
}