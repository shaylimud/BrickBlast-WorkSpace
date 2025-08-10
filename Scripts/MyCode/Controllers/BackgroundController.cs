using Ray.Services;
using System.Collections.Generic;
using UnityEngine;

namespace Ray.Controllers
{
    public class BackgroundController : MonoBehaviour
    {
        [Header("Canvas Spawn Parent")]
        [SerializeField, RequireReference] RectTransform _canvasParent;

        [Header("Configs")]
        [SerializeField, RequireReference] private BackgroundSpawnConfig _bgSpawnConfig;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private List<GameObject> _existingBgs = new List<GameObject>();
        private List<GameObject> _existingBgElements = new List<GameObject>();

        private void OnEnable()
        {
            EventService.Level.OnStart += SpawnAllBackgroundsAtOnce;

            EventService.Player.OnParked += DeleteAllBg;
        }

        private void SpawnAllBackgroundsAtOnce(Component c)
        {
            _rayDebug.Event("SpawnAllBackgroundsAtOnce", c, this);

            var bgSpawnProp = _bgSpawnConfig.BgSpawnProp;
            var bgElementSpawnProp = _bgSpawnConfig.BgElementSpawnProp;

            int bgCount = Mathf.CeilToInt(Database.UserData.Stats.ReachLevel / 10f);
            for (int i = 0; i < bgCount; i++)
            {
                int targetY = bgSpawnProp.FirstSpawnY + (bgSpawnProp.IncrementSpawnY * i);

                SpawnBg(c, targetY);
            }

            int bgElementCount = Mathf.CeilToInt(Database.UserData.Stats.ReachLevel / 5);
            for (int i = 0; i < bgElementCount; i++)
            {
                int targetY = bgElementSpawnProp.FirstSpawnY + (bgElementSpawnProp.IncrementSpawnY * i);

                SpawnBgElement(c, targetY);
            }
        }

        private void SpawnBg(Component c, int targetY)
        {
            _rayDebug.Event("SpawnBg", c, this);

            var bgSpawnProp = _bgSpawnConfig.BgSpawnProp;

            GameObject bgGo = Instantiate(bgSpawnProp.BgPrefab, _canvasParent);

            Sprite rolledSprite = _bgSpawnConfig.GetRandomBgSprite();
            bgGo.GetComponent<UnityEngine.UI.Image>().sprite = rolledSprite;

            RectTransform rect = bgGo.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, targetY);

            _existingBgs.Add(bgGo);
        }

        private void SpawnBgElement(Component c, int targetY)
        {
            _rayDebug.Event("SpawnBgElement", c, this);

            var bgElementSpawnProp = _bgSpawnConfig.BgElementSpawnProp;

            GameObject bgElementGo = Instantiate(bgElementSpawnProp.BgElementPrefab, _canvasParent);

            Sprite rolledSprite = _bgSpawnConfig.GetRandomBgElementSprite();
            bgElementGo.GetComponent<UnityEngine.UI.Image>().sprite = rolledSprite;

            bool isAttachedRight = Random.value > 0.5f ? true : false;

            RectTransform rect = bgElementGo.GetComponent<RectTransform>();

            SetAnchorAndScale(rect, isAttachedRight);

            float rndPosYOffset = Random.Range(-200f, 200f);

            rect.anchoredPosition = new Vector2(0, targetY + rndPosYOffset);

            _existingBgElements.Add(bgElementGo);
        }

        private void SetAnchorAndScale(RectTransform rect, bool isAttachedRight)
        {
            float rndScaleOffset = Random.Range(-0.5f, 0.5f);

            if (isAttachedRight)
            {
                // Center Right anchor preset
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.localScale = new Vector3(1 + rndScaleOffset, 1 + rndScaleOffset, 1);
            }
            else
            {
                // Center Left anchor preset
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.localScale = new Vector3(-1 - rndScaleOffset, 1 + rndScaleOffset, 1);
            }
        }

        private void DeleteAllBg(Component c)
        {
            _rayDebug.Event("DeleteAllBg", c, this);

            for (int i = _existingBgs.Count - 1; i >= 0; i--)
            {
                DeleteBg(_existingBgs[i]);
            }

            for (int i = _existingBgElements.Count - 1; i >= 0; i--)
            {
                DeleteBgElement(_existingBgElements[i]);
            }
        }

        private void DeleteBg(GameObject go)
        {
            _existingBgs.Remove(go);

            Destroy(go.gameObject);
        }

        private void DeleteBgElement(GameObject go)
        {
            _existingBgElements.Remove(go);

            Destroy(go.gameObject);
        }
    }
}