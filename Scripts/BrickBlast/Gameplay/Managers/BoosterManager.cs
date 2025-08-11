using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class BoosterManager : MonoBehaviour
    {
        [SerializeField]private FieldManager fieldManager;
        private Cell lastHighlightedCell;
        private Camera mainCamera;

        private void Awake()
        {
            Debug.Log("[BoosterManager] Awake");
            fieldManager = FindObjectOfType<FieldManager>();
            if (fieldManager == null)
            {
                Debug.Log("[BoosterManager] FieldManager not found.");
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.Log("[BoosterManager] Main camera not found.");
            }
        }

        private void Update()
        {
            if (fieldManager == null)
            {
                fieldManager = FindObjectOfType<FieldManager>();
            }
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("[BoosterManager] Mouse click detected.");
                ProcessInput(Input.mousePosition);
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("[BoosterManager] Touch detected.");
                ProcessInput(Input.GetTouch(0).position);
            }
        }

        private void ProcessInput(Vector2 screenPosition)
        {
            Debug.Log($"[BoosterManager] Screen position: {screenPosition}");
            if (mainCamera == null)
            {
                Debug.LogError("[BoosterManager] No camera available.");
                return;
            }

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            Debug.Log($"[BoosterManager] World position: {worldPosition}");

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Cell"))
            {
                Debug.Log("[BoosterManager] Cell collider hit.");
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    HighlightCell(cell);
                }
                else
                {
                    Debug.Log("[BoosterManager] Collider does not have Cell component.");
                }
            }
            else
            {
                Debug.Log("[BoosterManager] No cell detected under input.");
            }
        }

        private void HighlightCell(Cell cell)
        {
            Debug.Log("[BoosterManager] Highlighting cell.");
            if (lastHighlightedCell != null)
            {
                Debug.Log("[BoosterManager] Clearing previous highlighted cell.");
                lastHighlightedCell.ClearCell();
            }

            cell.HighlightCellTutorial();
            lastHighlightedCell = cell;

            if (fieldManager != null && fieldManager.cells != null)
            {
                for (int row = 0; row < fieldManager.cells.GetLength(0); row++)
                {
                    for (int col = 0; col < fieldManager.cells.GetLength(1); col++)
                    {
                        if (fieldManager.cells[row, col] == cell)
                        {
                            Debug.Log($"[BoosterManager] Cell coordinates -> Row: {row}, Col: {col}");
                            return;
                        }
                    }
                }
                Debug.Log("[BoosterManager] Cell not found in FieldManager grid.");
            }
            else
            {
                Debug.Log("[BoosterManager] FieldManager or cells array not initialized.");
            }
        }
    }
}

