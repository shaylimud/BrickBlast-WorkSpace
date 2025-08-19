using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public class LeaderboardService : MonoBehaviour
    {
        public static LeaderboardService Instance;

        private void Awake()
        {
            Instance = this;
        }

        public async void UpdateClassicLeaderboard(int score, int level)
        {
            string userId = Database.Instance.UserId;

            var data = new Dictionary<string, object>
            {
                { "Score", score },
                { "Level", level }
            };

            await FirebaseFirestore.DefaultInstance
                .Collection("Leaderboards")
                .Document(userId)
                .SetAsync(data);
        }
    }
}

