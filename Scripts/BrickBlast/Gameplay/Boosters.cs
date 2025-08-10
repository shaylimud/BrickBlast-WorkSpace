using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class Boosters : MonoBehaviour
    {
        [SerializeField] private RectTransform rowVisualPrefab;

        private RectTransform rowVisual;
        private bool deleteRowMode;
        private FieldManager fieldManager;
        private LevelManager levelManager;

        private void Awake()
        {
            fieldManager = FindObjectOfType<FieldManager>();
            levelManager = FindObjectOfType<LevelManager>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
            {
                Debug.Log("This : 1");
                ToggleDeleteRow();
            }
            Debug.Log("This : 2");

            if (!deleteRowMode || fieldManager == null || levelManager == null)
            {
                Debug.Log("This : 3");

                return;
            }

            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(fieldManager.field, pointer, null, out var local))
            {
                if (rowVisual == null)
                {
                    if (rowVisualPrefab == null)
                    {
                        return;
                    }
                    rowVisual = Instantiate(rowVisualPrefab, fieldManager.field);
                }

                rowVisual.gameObject.SetActive(true);

                float cellSize = fieldManager.GetCellSize();
                int rowIndex = Mathf.Clamp(Mathf.FloorToInt(local.y / cellSize), 0, fieldManager.cells.GetLength(0) - 1);

                rowVisual.anchoredPosition = new Vector2(0, rowIndex * cellSize);
                rowVisual.sizeDelta = new Vector2(fieldManager.field.rect.width, cellSize);

                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ApplyDeleteRow(rowIndex);
                }
            }
            else if (rowVisual != null)
            {
                rowVisual.gameObject.SetActive(false);
            }
        }

        private void ToggleDeleteRow()
        {
            deleteRowMode = !deleteRowMode;
            if (!deleteRowMode && rowVisual != null)
            {
                rowVisual.gameObject.SetActive(false);
            }
        }

        private void ApplyDeleteRow(int row)
        {
            deleteRowMode = false;
            if (rowVisual != null)
            {
                rowVisual.gameObject.SetActive(false);
            }

            var cells = new List<Cell>();
            for (int x = 0; x < fieldManager.cells.GetLength(1); x++)
            {
                cells.Add(fieldManager.cells[row, x]);
            }

            StartCoroutine(levelManager.DestroyLines(new List<List<Cell>> { cells }, null));

            int score = GameManager.instance.GameSettings.ScorePerLine;
            levelManager.OnScored?.Invoke(score);
        }
    }
}
