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
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Services;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.Settings;
using DG.Tweening;
using UnityEngine;
using Ray.Services;
using ResourceManager = BlockPuzzleGameToolkit.Scripts.Data.ResourceManager;

namespace BlockPuzzleGameToolkit.Scripts.System
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public Action<string> purchaseSucceded;
        public DebugSettings debugSettings;
        public GameSettings GameSettings;
        public SpinSettings luckySpinSettings;
        public CoinsShopSettings coinsShopSettings;
        private (string id, ProductTypeWrapper.ProductType productType)[] products;
        private int lastBackgroundIndex = -1;
        private bool isTutorialMode;
        public Action<bool, List<string>> OnPurchasesRestored;
        public ProductID noAdsProduct;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetTimeScale()
        {
            Time.timeScale = 1f;
        }

        public int Score { get=> ResourceManager.instance.GetResource("Score").GetValue(); set => ResourceManager.instance.GetResource("Score").Set(value); }

        public override void Awake()
        {
            base.Awake();
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            DOTween.SetTweensCapacity(1250, 512);
        }

        private void OnEnable()
        {

            if (!IsTutorialShown() && !GameDataManager.isTestPlay)
            {
                SetTutorialMode(true);
            }
        }

        private void OnDisable()
        {
            GameDataManager.isTestPlay = false; // Reset isTestPlay
            Time.timeScale = 1f; // Ensure time scale restored
        }

        private bool IsTutorialShown()
        {
            return PlayerPrefs.GetInt("tutorial", 0) == 1;
        }

        public void SetTutorialCompleted()
        {
            PlayerPrefs.SetInt("tutorial", 1);
            PlayerPrefs.Save();
        }

        private async void Start()
        {
            if (GameSettings.enableInApps) {
                products = Resources.LoadAll<ProductID>("ProductIDs")
                    .Select(p => (p.ID, p.productType))
                    .ToArray();

                // Initialize gaming services
                await InitializeGamingServices.instance?.Initialize(
                    OnInitializeSuccess,
                    OnInitializeError
                );
                // Initialize IAP directly if InitializeGamingServices is not used
            }
            

            if (GameDataManager.isTestPlay)
            {
                GameDataManager.SetLevel(GameDataManager.GetLevel());
            }
        }

        private void OnInitializeSuccess()
        {
            Debug.Log("Gaming services initialized successfully");
        }

        private void OnInitializeError(string errorMessage)
        {
            Debug.LogError($"Failed to initialize gaming services: {errorMessage}");
        }

        public void RestartLevel()
        {
            DOTween.KillAll();
            MenuManager.instance.CloseAllPopups();
            EventManager.GetEvent(EGameEvent.RestartLevel).Invoke();
        }

        public void RemoveAds()
        {
            if (GameSettings.enableAds) {
                MenuManager.instance.ShowPopup<NoAds>();
            }
        }

        public void MainMenu()
        {
            DOTween.KillAll();
            if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Adventure)
            {
                SceneLoader.instance.StartMapScene();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Map)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                MenuManager.instance.ShowPopup<Quit>();
            }
            else
            {
                SceneLoader.instance.GoMain();
            }
        }

        public void OpenMap()
        {
            if (GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.StartGameSceneClassic();
            }
            else if (GetGameMode() == EGameMode.Timed)
            {
                SceneLoader.instance.StartGameSceneTimed();
            }
            else
            {
                GameDataManager.ResetSubLevelIndex();
                SceneLoader.instance.StartMapScene();
            }
        }

        public void OpenGame()
        {
            SceneLoader.instance.StartGameScene();
        }

        public void PurchaseSucceeded(string id)
        {
            purchaseSucceded?.Invoke(id);
        }



        public void SetGameMode(EGameMode gameMode)
        {
            GameDataManager.SetGameMode(gameMode);
        }

        private EGameMode GetGameMode()
        {
            return GameDataManager.GetGameMode();
        }

        public int GetLastBackgroundIndex()
        {
            return lastBackgroundIndex;
        }

        public void SetLastBackgroundIndex(int index)
        {
            lastBackgroundIndex = index;
        }

        public void NextLevel()
        {
            OpenGame();
            RestartLevel();
        }

        public void SetTutorialMode(bool tutorial)
        {
            Debug.Log("Tutorial mode set to " + tutorial);
            isTutorialMode = tutorial;
        }

        public bool IsTutorialMode()
        {
            return isTutorialMode;
        }

        internal void RestorePurchases(Action<bool, List<string>> OnPurchasesRestored)
        {
            if (!GameSettings.enableInApps) return;
            
            this.OnPurchasesRestored = OnPurchasesRestored;
        }


    }
}