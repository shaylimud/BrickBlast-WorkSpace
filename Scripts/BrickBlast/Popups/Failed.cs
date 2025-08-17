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

using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine.UI;
using TMPro;
using Ray.Services;
using static GameSettingsRay;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class Failed : Popup
    {
        public Button retryButton;
        public TextMeshProUGUI currencyText;
        public Button tripleRewardButton;

        private bool rewardGranted;

        protected virtual void OnEnable()
        {
            retryButton.onClick.AddListener(Retry);
            closeButton.onClick.AddListener(CollectAndExit);

            if (currencyText != null)
            {
                currencyText.text = GameManager.instance.Score.ToString();
            }

            if (tripleRewardButton != null)
            {
                tripleRewardButton.onClick.AddListener(TripleReward);
                tripleRewardButton.interactable = RewardedService.Instance.IsRewardedReady(RewardedType.Triple);
            }
        }

        private async void Retry()
        {
            if (rewardGranted) return;
            rewardGranted = true;
            StopInteration();

            await Database.UserData.AddScoreAsCurrency(GameManager.instance.Score);
            GameManager.instance.RestartLevel();
            Close();
        }

        private async void CollectAndExit()
        {
            if (rewardGranted) return;
            rewardGranted = true;
            StopInteration();

            await Database.UserData.AddScoreAsCurrency(GameManager.instance.Score);
            GameManager.instance.MainMenu();
            Close();
        }

        private void TripleReward()
        {
            if (rewardGranted) return;
            if (!RewardedService.Instance.IsRewardedReady(RewardedType.Triple)) return;

            tripleRewardButton.interactable = false;
            RewardedService.Instance.ShowRewarded(RewardedType.Triple, OnTripleRewarded);
        }

        private async void OnTripleRewarded()
        {
            if (rewardGranted) return;
            rewardGranted = true;

            await Database.UserData.AddScoreAsCurrency(GameManager.instance.Score * 3);
            GameManager.instance.MainMenu();
            Close();
        }
    }
}