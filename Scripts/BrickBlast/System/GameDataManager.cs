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
        public static int LevelNum;

        private static Level _level;

        public static bool isTestPlay = false;

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
            Database.UserData.SetGroupIndex(1);
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
            Database.UserData.SetGroupIndex(1);
            #endif
        }

        public static void UnlockLevel(int currentLevel)
        {
            int savedLevel = GetLevelNum();
            if (savedLevel < currentLevel)
            {
                SetLevelNum(currentLevel);
            }
        }

        public static void UnlockGroup(int groupIndex)
        {
            int savedGroup = Database.UserData.GroupIndex;
            if (savedGroup < groupIndex)
            {
                Database.UserData.SetGroupIndex(groupIndex);
            }
        }

        public static int GetLevelNum()
        {
            return (Database.UserData.GroupIndex - 1) * 3 + Database.UserData.Level;
        }

        public static int GetGroupIndex()
        {
            return Database.UserData.GroupIndex;
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
        }

        public static void SetAllLevelsCompleted()
        {
            var levels = Resources.LoadAll<Level>("Levels").Length;
            int groupIndex = Mathf.CeilToInt(levels / 3f);
            int levelInGroup = levels - (groupIndex - 1) * 3;
            Database.UserData.SetGroupIndex(groupIndex);
            Database.UserData.SetLevel(levelInGroup);
        }

        internal static bool HasMoreLevels()
        {
            int currentLevel = GetLevelNum();
            int totalLevels = Resources.LoadAll<Level>("Levels").Length;
            return currentLevel < totalLevels;
        }

        public static void SetLevelNum(int stateCurrentLevel)
        {
            LevelNum = stateCurrentLevel;
            int groupIndex = Mathf.CeilToInt(stateCurrentLevel / 3f);
            int levelInGroup = stateCurrentLevel - (groupIndex - 1) * 3;
            Database.UserData.SetGroupIndex(groupIndex);
            Database.UserData.SetLevel(levelInGroup);
        }
    }
}