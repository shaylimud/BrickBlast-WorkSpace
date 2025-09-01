using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ray.Views
{
    /// <summary>
    /// Lightweight utility for toggling UI elements.
    /// Safely checks references before operating to avoid
    /// MissingReferenceException when GameObjects are destroyed.
    /// </summary>
    public class UIView : MonoBehaviour
    {
        public void Show(params GameObject[] gameObjects)
        {
            foreach (var go in gameObjects)
            {
                if (go)
                {
                    go.SetActive(true);
                }
            }
        }

        public void Hide(params GameObject[] gameObjects)
        {
            foreach (var go in gameObjects)
            {
                if (go)
                {
                    go.SetActive(false);
                }
            }
        }

        public void FadeOff(params GameObject[] gameObjects)
        {
            // Placeholder fade logic – simply hides the objects.
            Hide(gameObjects);
        }

        public void ShowHideViaStatus(bool status, params GameObject[] gameObjects)
        {
            if (status)
            {
                Show(gameObjects);
            }
            else
            {
                Hide(gameObjects);
            }
        }

        public void ToggleOnTop(GameObject primary, GameObject secondary, bool status)
        {
            if (primary)
            {
                primary.SetActive(status);
            }
            if (secondary)
            {
                secondary.SetActive(!status);
            }
        }

        public void SetText(TextMeshProUGUI text, object value)
        {
            if (text)
            {
                text.text = value?.ToString();
            }
        }

        public void SetText(TextMeshPro text, object value)
        {
            if (text)
            {
                text.text = value?.ToString();
            }
        }

        public void PulseCurrency(TextMeshProUGUI text, int value)
        {
            if (text)
            {
                text.text = value.ToString();
            }
        }

        public void ButtonInteractableState(bool state, GameObject buttonObj)
        {
            if (buttonObj)
            {
                var btn = buttonObj.GetComponent<Button>();
                if (btn)
                {
                    btn.interactable = state;
                }
            }
        }

        public IEnumerator DepleteMeter(Image meter, float duration, Action onComplete)
        {
            if (meter)
            {
                float timer = 0f;
                while (timer < duration)
                {
                    meter.fillAmount = 1f - timer / duration;
                    timer += Time.deltaTime;
                    yield return null;
                }
                meter.fillAmount = 0f;
            }
            onComplete?.Invoke();
        }

        public IEnumerator StandBy(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration);
            onComplete?.Invoke();
        }
    }
}
