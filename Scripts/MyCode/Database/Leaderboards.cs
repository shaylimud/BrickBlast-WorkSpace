using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public static class Leaderboards
    {
        private static FirebaseFirestore Firestore => FirebaseFirestore.DefaultInstance;

        public static async Task UpdateClassicLeaderboard(int score, int level)
        {
            try
            {
                var auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser == null)
                {
                    Debug.LogWarning("Leaderboards: no authenticated user.");
                    return;
                }

                var docRef = Firestore.Collection("Leaderboards").Document(auth.CurrentUser.UserId);
                var data = new Dictionary<string, object>
                {
                    { "Score", score },
                    { "Level", level }
                };

                await docRef.SetAsync(data, SetOptions.MergeAll);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Leaderboards: failed to update leaderboard - {e}");
            }
        }
    }
}
