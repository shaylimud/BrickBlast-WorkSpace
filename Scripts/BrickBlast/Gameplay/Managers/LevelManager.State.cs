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

using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Popups;
using Ray.Services;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public partial class LevelManager
    {
        private void HandleGameStateChange(EGameState newState)
        {
            var currentLevel = GetCurrentLevel();
            var levelType = currentLevel != null ? currentLevel.levelType : null;
            var stateHandler = levelType != null ? levelType.stateHandler : null;
            
            if (stateHandler != null)
            {
                stateHandler.HandleState(newState, this);
            }
            else if (gameMode == EGameMode.Timed)
            {
                switch (newState)
                {
                    case EGameState.PrepareGame:
                        EventManager.GameStatus = EGameState.Playing;
                        return;
                    case EGameState.PreFailed:
                        EventManager.GameStatus = EGameState.Failed;
                        return;
                    case EGameState.Failed:
                        var failedTimedPrefab = RayBrickMediator.Instance?.FailedTimedPopup;
                        if (failedTimedPrefab != null)
                        {
                            MenuManager.instance.ShowPopup(failedTimedPrefab);
                        }
                        else
                        {
                            MenuManager.instance.ShowPopup<FailedTimed>();
                        }
                        break;
                }
            }

            if (newState == EGameState.Failed)
            {
                failedLevel = this.currentLevel;
                failedSubLevelIndex = GameDataManager.GetSubLevelIndex();
                HandleLevelFail();
            }
        }

        private void HandleLevelFail()
        {
            int subLevelIndex = GameDataManager.GetSubLevelIndex();
            if (subLevelIndex > 1)
            {
                int groupIndex = (currentLevel - 1) / 3;
                int newLevel = groupIndex * 3 + 1;
                currentLevel = newLevel;
                Database.UserData.SetLevel(newLevel);
                GameDataManager.ResetSubLevelIndex();
                GameDataManager.SetLevel(null);
                GameManager.instance.RestartLevel();
            }
        }
    }
}