// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEditor;
using UnityEngine;
using Ray.Services;

namespace BlockPuzzleGameToolkit.Scripts.System
{
    public static class GameDataManager
    {
        private static Level _level;

        public static bool isTestPlay = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _level = null;
            isTestPlay = false;
        }

        private const string SubLevelKey = "SubLevelIndex";

        public static int GetSubLevelIndex()
        {
            return PlayerPrefs.GetInt(SubLevelKey, 1);
        }

        public static void SetSubLevelIndex(int index)
        {
            PlayerPrefs.SetInt(SubLevelKey, index);
            PlayerPrefs.Save();
        }

        public static void ResetSubLevelIndex()
        {
            SetSubLevelIndex(1);
        }

        public static void ClearPlayerProgress()
        {
            Database.UserData.SetLevel(1);
        }

        public static void ClearALlData()
        {
            #if UNITY_EDITOR
            // clear variables ResourceObject from Resources/Variables
            var resourceObjects = Resources.LoadAll<ResourceObject>("Variables");
            foreach (var resourceObject in resourceObjects)
            {
                resourceObject.Set(0);
            }

            AssetDatabase.SaveAssets();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Database.UserData.SetLevel(1);
            #endif
        }

        public static void UnlockLevel(int currentLevel)
        {
            int savedLevel = Database.UserData.Stats.Level;
            if (savedLevel < currentLevel)
            {
                Database.UserData.SetLevel(currentLevel);
            }
        }

        public static void UnlockGroup(int groupIndex)
        {
            int savedGroup = Database.UserData.Stats.GroupIndex;
            if (savedGroup < groupIndex)
            {
                Database.UserData.SetGroupIndex(groupIndex);
            }
        }

        public static int GetLevelNum()
        {
            return Database.UserData.Stats.Level;
        }

        public static int GetGroupIndex()
        {
            return Database.UserData.Stats.GroupIndex;
        }

        public static Level GetLevel()
        {
            if (_level != null && isTestPlay)
            {
                return _level;
            }

            if (_level != null)
            {
                return _level;
            }

            var gameMode = GetGameMode();
            if (gameMode == EGameMode.Classic)
            {
                _level = Resources.Load<Level>("Misc/ClassicLevel");
            }
            else if (gameMode == EGameMode.Timed)
            {
                _level = Resources.Load<Level>("Misc/TimeLevel");

                if (_level == null)
                {
                    Debug.LogError("Timed level not found.");
                    return null;
                }
            }
            else
            {
                _level = Resources.Load<Level>("Levels/Level_" + GetLevelNum());
            }

            return _level;
        }

        public static void SetLevel(Level level)
        {
            _level = level;
        }

        public static EGameMode GetGameMode()
        {
            return (EGameMode)PlayerPrefs.GetInt("GameMode");
        }

        public static void SetGameMode(EGameMode gameMode)
        {
            PlayerPrefs.SetInt("GameMode", (int)gameMode);
            PlayerPrefs.Save();
            // Clear cached level so the appropriate level for the new game mode is loaded
            _level = null;
        }

        public static void SetAllLevelsCompleted()
        {
            var levels = Resources.LoadAll<Level>("Levels").Length;
            Database.UserData.SetLevel(levels);
            Database.UserData.SetGroupIndex(Mathf.CeilToInt(levels / 3f));
        }

        internal static bool HasMoreLevels()
        {
            int currentLevel = GetLevelNum();
            int totalLevels = Resources.LoadAll<Level>("Levels").Length;
            return currentLevel < totalLevels;
        }
    }
}