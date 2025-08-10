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
    // Toggle booster mode with J
    if (Keyboard.current?.jKey.wasPressedThisFrame == true)
    {
        Debug.Log("[Codex] J key pressed");
        ToggleDeleteRow();
    }

    // If booster is off or managers are missing, hide any visual and bail
    if (!deleteRowMode || fieldManager == null || levelManager == null)
    {
        if (rowVisual != null)
            rowVisual.gameObject.SetActive(false);
        return;
    }

    // Read pointer position
    Vector2 pointer = Mouse.current?.position.ReadValue() ?? Vector2.zero;

    // Convert to local point; if pointer is outside the field, hide visual and bail
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fieldManager.field, pointer, null, out var local))
    {
        if (rowVisual != null)
            rowVisual.gameObject.SetActive(false);
        Debug.Log("[Codex] Pointer outside field");
        return;
    }

    Debug.Log("[Codex] Pointer inside field");

    // Ensure we have a row visual
    if (rowVisual == null)
    {
        rowVisual = rowVisualPrefab != null
            ? Instantiate(rowVisualPrefab, fieldManager.field)
            : CreateDefaultRowVisual();

        Debug.Log(rowVisualPrefab != null
            ? "[Codex] Row visual instantiated from prefab"
            : "[Codex] Default row visual created");
    }

    rowVisual.gameObject.SetActive(true);

    // Compute the hovered row
    float cellSize = fieldManager.GetCellSize();
    float pivotOffsetY = fieldManager.field.rect.height * fieldManager.field.pivot.y;
    int rows = fieldManager.cells.GetLength(0);

    int rowIndex = Mathf.Clamp(
        Mathf.FloorToInt((local.y + pivotOffsetY) / cellSize),
        0,
        rows - 1);

    Debug.Log($"[Codex] Hovering row {rowIndex}");

    // Position/size the highlight
    float yPos = rowIndex * cellSize - pivotOffsetY;
    rowVisual.anchoredPosition = new Vector2(0f, yPos);
    rowVisual.sizeDelta = new Vector2(fieldManager.field.rect.width, cellSize);

    // Apply on click
    if (Mouse.current?.leftButton.wasPressedThisFrame == true)
    {
        Debug.Log($"[Codex] Deleting row {rowIndex}");
        ApplyDeleteRow(rowIndex);
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
