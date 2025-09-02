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
using BlockPuzzleGameToolkit.Scripts.GUI.Labels;

using BlockPuzzleGameToolkit.Scripts.System;

using BlockPuzzleGameToolkit.Scripts.Gameplay;

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class FailedScore : Failed
    {
        public Transform scorePosition;

        private void Start()
        {
            var modeHandler = FindObjectOfType<BaseModeHandler>(true);

            var scoreLabel = FindObjectOfType<TargetsUIHandler>().ScoreLabel;
            var labelComponent = scoreLabel.GetComponent<TargetScoreGUIElement>();
            labelComponent.enabled = false;

            var scoreObject = Instantiate(scoreLabel, transform);
            scoreObject.transform.position = scorePosition.position;

            var scoreComponent = scoreObject.GetComponent<TargetScoreGUIElement>();

            if (scoreComponent != null)
            {
                int finalScore = GameManager.instance.LastScore;
                scoreComponent.countText.text = finalScore.ToString();
                scoreComponent.scoreSlider.maxValue = Mathf.Max(scoreComponent.scoreSlider.maxValue, finalScore);
                scoreComponent.scoreSlider.value = finalScore;

                if (modeHandler != null && scoreComponent != null)
                {
                    scoreComponent.countText.text = modeHandler.score.ToString();
                    scoreComponent.scoreSlider.maxValue =
                        Mathf.Max(scoreComponent.scoreSlider.maxValue, modeHandler.score);
                    scoreComponent.scoreSlider.value = modeHandler.score;

                }
            }
        }
    }
}