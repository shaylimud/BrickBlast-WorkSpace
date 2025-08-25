using Ray.Services;
using Ray.Views;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ray.Controllers
{
    public class ItemController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField, RequireReference] private ItemTypeConfig _itemTypeConfig;
        [SerializeField, RequireReference] private ItemMovementConfig _itemMovementConfig;
        [SerializeField, RequireReference] private ItemSpawnConfig _itemSpawnConfig;

        [Header("Item Collection")]
        [SerializeField, RequireReference] private ItemCollection _itemCollection;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private List<ItemView> _existingItemViews = new List<ItemView>();

        private void OnEnable()
        {
            EventService.Level.OnStart += SpawnAllItemsAtOnce;

            EventService.Resource.OnCollectedItemValueProccessed += SpawnTextMesh;

            EventService.Player.OnParked += DeleteAllItems;
        }

        private void SpawnAllItemsAtOnce(Component c)
        {
            _rayDebug.Event("SpawnAllItemsAtOnce", c, this);

            for (int y = _itemSpawnConfig.FirstSpawnY; y > -Database.UserData.Stats.Level; y -= _itemSpawnConfig.IncrementSpawnY)
            {
                SpawnItem(c, y);
            }
        }

        private void SpawnItem(Component c, int initialY)
        {
            ItemType rolledType = _itemTypeConfig.GetRandomItemTypeByChance();

            if (rolledType == ItemType.Tier0 && ResourceService.Instance.NoEnemies.Value) rolledType = ItemType.Tier1;

            Sprite rolledSprite = _itemCollection.GetRandomSpriteByType(rolledType);

            GameObject itemGo = Instantiate(_itemTypeConfig.GetPrefabForType(rolledType));
            ItemView itemView = itemGo.AddComponent<ItemView>();

            itemView.Initialize(rolledType, rolledSprite, _itemMovementConfig, _itemTypeConfig, initialY);

            var appearanceProp = _itemTypeConfig.GetAppearancePropertiesForType(rolledType);
            if (appearanceProp.ShaderMaterial != null) itemGo.GetComponent<SpriteRenderer>().material = appearanceProp.ShaderMaterial;
            if (appearanceProp.ParticleSystem != null) Instantiate(appearanceProp.ParticleSystem, itemGo.transform);

            _existingItemViews.Add(itemView);

            itemView.OnItemCollected += HandleItemCollected;
            itemView.OnObstacleCollected += HandleObstacleCollected;
        }

        private void DeleteAllItems(Component c)
        {
            _rayDebug.Event("DeleteAllItems", c, this);

            for (int i = _existingItemViews.Count - 1; i >= 0; i--)
            {
                DeleteItem(_existingItemViews[i]);
            }
        }

        private void HandleItemCollected(ItemView itemView, Vector2 itemPos)
        {
            _rayDebug.Event("HandleItemCollected", itemView, this);

            itemView.StopBehavior();
            EventService.Item.OnItemCollected?.Invoke(this, itemView.ItemType, itemView.transform.position);
        }

        private void HandleObstacleCollected(ItemView itemView)
        {
            _rayDebug.Event("HandleObstacleCollected", itemView, this);

            itemView.StopBehavior();
            EventService.Item.OnObstacleCollected?.Invoke(this);
            DeleteItem(itemView);
        }

        private void DeleteItem(ItemView itemView)
        {
            _rayDebug.Log("DeleteItem", itemView);

            itemView.OnItemCollected -= HandleItemCollected;
            itemView.OnObstacleCollected -= HandleObstacleCollected;

            _existingItemViews.Remove(itemView);
            Destroy(itemView.gameObject);
        }

        private void SpawnTextMesh(Component c, ItemType itemType, int textMeshValue, Vector2 collectionPos)
        {
            _rayDebug.Event("SpawnTextMesh", c, this);

            var textMesh = _itemTypeConfig.GetValueTextMeshForType(itemType);

            if (textMesh != null)
            {
                var textMeshGo = Instantiate(textMesh, collectionPos, Quaternion.identity);
                textMeshGo.GetComponent<TextMeshPro>().text = "+" + textMeshValue.ToString("N0");
            }
        }
    }
}