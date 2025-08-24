using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Popups;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    public abstract class LevelStateHandler : ScriptableObject
    {
        public virtual void HandleState(EGameState state, LevelManager levelManager)
        {
            switch (state)
            {
                case EGameState.PrepareGame:
                    HandlePrepareGame(levelManager);
                    break;
                case EGameState.Playing:
                    HandlePlaying(levelManager);
                    break;
                case EGameState.PreFailed:
                    HandlePreFailed(levelManager);
                    break;
                case EGameState.Failed:
                    HandleFailed(levelManager);
                    break;
                case EGameState.PreWin:
                    HandlePreWin(levelManager);
                    break;
                case EGameState.Win:
                    HandleWin(levelManager);
                    break;
            }
        }
        
        private protected virtual void HandlePrepareGame(LevelManager levelManager)
        {
            var level = levelManager.GetCurrentLevel();
            var prePlayPopup = level.levelType.prePlayPopup;

            if (prePlayPopup != null)
            {
                MenuManager.instance.ShowPopup(prePlayPopup, null, _ => EventManager.GameStatus = EGameState.Playing);
            }
            else
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }

        private protected virtual void HandlePlaying(LevelManager levelManager) {}

        private protected virtual void HandlePreFailed(LevelManager levelManager) {}

        private protected virtual void HandleFailed(LevelManager levelManager)
        {
            var failedPopup = levelManager.GetCurrentLevel().levelType.failedPopup;
            if (failedPopup != null)
            {
                MenuManager.instance.ShowPopup(failedPopup);
            }
            else if (levelManager.gameMode == EGameMode.Timed)
            {
                // Timed levels sometimes lack a failed popup on the LevelType,
                // leaving the game stuck on the gameplay screen. Fall back to
                // the default timed failure popup in that case.
                MenuManager.instance.ShowPopup<FailedTimed>();
            }
        }

        private protected virtual void HandlePreWin(LevelManager levelManager)
        {
            var preWinPopup = levelManager.GetCurrentLevel().levelType.preWinPopup;
            if (preWinPopup != null)
            {
                MenuManager.instance.ShowPopupDelayed(preWinPopup, null, _ => EventManager.GameStatus = EGameState.Win);
            }
            else
            {
                EventManager.GameStatus = EGameState.Win;
            }
        }

        private protected virtual void HandleWin(LevelManager levelManager)
        {
            var winPopup = levelManager.GetCurrentLevel().levelType.winPopup
                ?? RayBrickMediator.Instance?.WinPopup;

            void HandleWinResult(EPopupResult _)
            {
                // GameDataManager.GetSubLevelIndex returns the index for the next
                // level to play. A value of 1 means we've wrapped to the first
                // sub-level of the next group, so the player should return to the
                // map. Any other value means there are more sub-levels to play in
                // the current group, so we continue the game.
                var subLevelIndex = GameDataManager.GetSubLevelIndex();
                if (subLevelIndex == 1)
                {
                    GameManager.instance.OpenMap();
                }
                else
                {
                    EventManager.GameStatus = EGameState.Playing;
                    GameManager.instance.OpenGame();
                    GameManager.instance.RestartLevel();
                }
            }

            if (winPopup != null)
            {
                MenuManager.instance.ShowPopup(winPopup, null, HandleWinResult);
            }
            else
            {
                MenuManager.instance.ShowPopup<Win>(null, HandleWinResult);
            }
        }
    }
}