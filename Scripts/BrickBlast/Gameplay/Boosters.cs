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

                Debug.Log("[Codex] J key pressed");
                ToggleDeleteRow();
            }

            if (!deleteRowMode || fieldManager == null || levelManager == null)
            {
                Debug.Log("[Codex] Booster inactive or managers missing");

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

                Debug.Log("[Codex] Pointer inside field");
                if (rowVisual == null)
                {
                    if (rowVisualPrefab != null)
                    {
                        rowVisual = Instantiate(rowVisualPrefab, fieldManager.field);
                        Debug.Log("[Codex] Row visual instantiated from prefab");
                    }
                    else
                    {
                        Debug.Log("[Codex] No prefab provided, creating default row visual");
                        rowVisual = CreateDefaultRowVisual();
                    }

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

                float pivotOffset = fieldManager.field.rect.height * fieldManager.field.pivot.y;
                int rowIndex = Mathf.Clamp(
                    Mathf.FloorToInt((local.y + pivotOffset) / cellSize),
                    0,
                    fieldManager.cells.GetLength(0) - 1);
                Debug.Log($"[Codex] Hovering row {rowIndex}");

                float yPos = rowIndex * cellSize - pivotOffset;

                int rowIndex = Mathf.Clamp(Mathf.FloorToInt(local.y / cellSize), 0, fieldManager.cells.GetLength(0) - 1);

                rowVisual.anchoredPosition = new Vector2(0, rowIndex * cellSize);

                rowVisual.sizeDelta = new Vector2(fieldManager.field.rect.width, cellSize);

                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {

                    Debug.Log($"[Codex] Deleting row {rowIndex}");
                    ApplyDeleteRow(rowIndex);
                }
            }
            else
            {
                if (rowVisual != null)
                {
                    rowVisual.gameObject.SetActive(false);
                }
                Debug.Log("[Codex] Pointer outside field");

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

            Debug.Log($"[Codex] Delete row mode {(deleteRowMode ? "enabled" : "disabled")}");

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


            Debug.Log($"[Codex] ApplyDeleteRow called for row {row}");


            var cells = new List<Cell>();
            for (int x = 0; x < fieldManager.cells.GetLength(1); x++)
            {
                cells.Add(fieldManager.cells[row, x]);
            }

            StartCoroutine(levelManager.DestroyLines(new List<List<Cell>> { cells }, null));

            Debug.Log("[Codex] DestroyLines coroutine started");

            int score = GameManager.instance.GameSettings.ScorePerLine;
            levelManager.OnScored?.Invoke(score);
            Debug.Log($"[Codex] Score added {score}");
        }

        private RectTransform CreateDefaultRowVisual()
        {
            var go = new GameObject("RowVisual", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(fieldManager.field, false);
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(1f, 0f, 0f, 0.3f);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            return rt;


            int score = GameManager.instance.GameSettings.ScorePerLine;
            levelManager.OnScored?.Invoke(score);

        }
    }
}
