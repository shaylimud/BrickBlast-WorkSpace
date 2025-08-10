using BlockPuzzleGameToolkit.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class BoosterManager : MonoBehaviour
    {
        private FieldManager fieldManager;

        private void Awake()
        {
            fieldManager = FindObjectOfType<FieldManager>();
        }

        private void Update()
        {
            if (Mouse.current?.leftButton.wasPressedThisFrame != true || fieldManager == null)
            {
                return;
            }

            Vector2 pointer = Mouse.current.position.ReadValue();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fieldManager.field, pointer, null, out var local))
            {
                return;
            }

            float cellSize = fieldManager.GetCellSize();
            float pivotOffsetY = fieldManager.field.rect.height * fieldManager.field.pivot.y;
            float pivotOffsetX = fieldManager.field.rect.width * fieldManager.field.pivot.x;
            int rows = fieldManager.cells.GetLength(0);
            int cols = fieldManager.cells.GetLength(1);

            int rowFromBottom = Mathf.Clamp(Mathf.FloorToInt((local.y + pivotOffsetY) / cellSize), 0, rows - 1);
            int rowIndex = rows - 1 - rowFromBottom;
            int colIndex = Mathf.Clamp(Mathf.FloorToInt((local.x + pivotOffsetX) / cellSize), 0, cols - 1);
            int gridIndex = rowIndex * cols + colIndex;

            Debug.Log($"Clicked cell -> row {rowIndex}, col {colIndex}, grid {gridIndex}");
            Debug.Log($"Number of rows: {rows}");
        }
    }
}

