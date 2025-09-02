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

using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.System;
using Ray.Services;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class PreFailed : PopupWithCurrencyLabel
    {
        public TextMeshProUGUI continuePrice;
        public TextMeshProUGUI timerText;
        public Button continueButton;
        public Button reviveButton;
        public TextMeshProUGUI timeLeftText;
        protected int timer;
        protected int price;
        protected bool hasContinued = false;

        protected virtual void OnEnable()
        {
            price = GameManager.instance.GameSettings.continuePrice;
            if (continuePrice != null)
            {
                continuePrice.text = price.ToString();
            }

            continueButton?.onClick.AddListener(Continue);
            RayBrickMediator.Instance?.SetReviveButton(reviveButton);

            InitializeTimer();

            if (timerText != null)
            {
                timerText.text = timer.ToString();
            }
            SoundBase.instance.PlaySound(SoundBase.instance.warningTime);
            InvokeRepeating(nameof(UpdateTimer), 1, 1);
            //rewardButton.gameObject.SetActive(GameManager.instance.GameSettings.enableAds);
            if(GameDataManager.GetLevel().enableTimer && timeLeftText != null)
            {
                timeLeftText.gameObject.SetActive(true);
            }
        }

        protected virtual void InitializeTimer()
        {
            timer = GameManager.instance.GameSettings.failedTimerStart;
        }

        protected virtual void UpdateTimer()
        {
            if (MenuManager.instance.GetLastPopup() == this)
            {
                timer--;
                SaveTimerState();
            }
            else
            {
                timer = GameManager.instance.GameSettings.failedTimerStart;
            }

            if (timerText != null)
            {
                timerText.text = timer.ToString();
            }
            if (timer <= 0)
            {
                if (continueButton != null)
                {
                    continueButton.interactable = false;
                }
                hasContinued = true;

                CancelInvoke(nameof(UpdateTimer));
                EventManager.GameStatus = EGameState.Failed;
                Close();
            }
        }

        protected virtual void SaveTimerState() { }

        public void PauseTimer()
        {
            CancelInvoke(nameof(UpdateTimer));
        }

        protected virtual async void Continue()
        {
            if (timer <= 0 || hasContinued)
            {
                return;
            }

            hasContinued = true;
            if (continueButton != null)
            {
                continueButton.interactable = false;
            }


            CancelInvoke(nameof(UpdateTimer));
            StopInteration();
            OnContinue();
        }

        public void OnContinue()
        {
            DOTween.Kill(this);
            DOVirtual.DelayedCall(0.5f, ContinueGame);
        }

        private async void ContinueGame()
        {
            result = EPopupResult.Continue;
            await Database.UserData.AddScoreAsCurrency(GameManager.instance.LastScore);

            // Restart from the first level within the current group
            Database.UserData.SetLevel(1);
            GameDataManager.ResetSubLevelIndex();
            GameDataManager.SetLevel(null);

            GameManager.instance.RestartLevel();
            Close();
        }
    }
}