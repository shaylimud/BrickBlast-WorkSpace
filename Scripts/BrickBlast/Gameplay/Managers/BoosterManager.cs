using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.System;
using Ray.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GameSettingsRay;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class BoosterManager : MonoBehaviour
    {
        public static BoosterManager Instance { get; private set; }

        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private Button rowButton;
        [SerializeField] private Button columnButton;
        [SerializeField] private Button squareButton;
        [SerializeField] private Button changeShapeButton;

        [SerializeField] private TextMeshProUGUI rowUsesText;
        [SerializeField] private TextMeshProUGUI columnUsesText;
        [SerializeField] private TextMeshProUGUI squareUsesText;
        [SerializeField] private TextMeshProUGUI changeShapeUsesText;

        private GameObject rowOutline;
        private GameObject columnOutline;
        private GameObject squareOutline;
        private GameObject changeShapeOutline;

        private Cell lastHighlightedCell;
        private Camera mainCamera;
        private BoosterType? activeBooster;

        private Button[] boosterButtons;
        private readonly Dictionary<Button, Color> originalButtonColors = new();

        private void Awake()
        {
            Instance = this;
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

            rowOutline = rowButton?.transform.Find("outline")?.gameObject;
            columnOutline = columnButton?.transform.Find("outline")?.gameObject;
            squareOutline = squareButton?.transform.Find("outline")?.gameObject;
            changeShapeOutline = changeShapeButton?.transform.Find("outline")?.gameObject;

            boosterButtons = new[] { rowButton, columnButton, squareButton, changeShapeButton };
            foreach (var btn in boosterButtons)
            {
                if (btn != null)
                    originalButtonColors[btn] = btn.image.color;
            }

            if (rowButton != null)
                rowButton.onClick.AddListener(() => SelectBooster(BoosterType.ClearRow));
            if (columnButton != null)
                columnButton.onClick.AddListener(() => SelectBooster(BoosterType.ClearColumn));
            if (squareButton != null)
                squareButton.onClick.AddListener(() => SelectBooster(BoosterType.ClearSquare));
            if (changeShapeButton != null)
                changeShapeButton.onClick.AddListener(() => SelectBooster(BoosterType.ChangeShape));

            UpdateBoosterTexts();
            SetOutlineVisibility(null);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public async void SelectBooster(BoosterType booster)
        {
            // Prevent using boosters when the player has none available
            if (GetBoosterCount(booster) <= 0)
            {
                if (RewardedService.Instance != null && RewardedService.Instance.IsRewardedReady(RewardedType.ExtraSpace))
                {
                    RewardedService.Instance.ShowRewarded(RewardedType.ExtraSpace, () =>
                    {
                        ResourceService.Instance?.RewardBooster(booster);
                        UpdateBoosterTexts();
                        RayBrickMediator.Instance?.RefreshShop(this);
                    });
                }
                else
                {
                    EventService.UI.OnToggleInsufficient?.Invoke(this);
                }
                return;
            }

            Button selectedButton = booster switch
            {
                BoosterType.ClearRow => rowButton,
                BoosterType.ClearColumn => columnButton,
                BoosterType.ClearSquare => squareButton,
                BoosterType.ChangeShape => changeShapeButton,
                _ => null
            };

            SetBoosterButtonsState(selectedButton);

            if (booster == BoosterType.ChangeShape)
            {
                await ChangeShapes();
                SetOutlineVisibility(null);
                return;
            }

            if (booster == BoosterType.ClearSquare)
            {
                ClearRandomSquare();
                if (ResourceService.Instance != null)
                    await ResourceService.Instance.ConsumeBooster(BoosterType.ClearSquare);
                UpdateBoosterTexts();
                SetOutlineVisibility(null);
                ResetBoosterButtons();
                return;
            }

            activeBooster = booster;
            SetOutlineVisibility(activeBooster);
        }

        private int GetBoosterCount(BoosterType booster)
        {
            switch (booster)
            {
                case BoosterType.ClearRow:
                    return Database.UserData.Stats.Power_1;
                case BoosterType.ClearColumn:
                    return Database.UserData.Stats.Power_2;
                case BoosterType.ClearSquare:
                    return Database.UserData.Stats.Power_3;
                case BoosterType.ChangeShape:
                    return Database.UserData.Stats.Power_4;
                default:
                    return 0;
            }
        }

        private void UpdateBoosterTexts()
        {
            rowUsesText?.SetText(GetBoosterCount(BoosterType.ClearRow).ToString());
            columnUsesText?.SetText(GetBoosterCount(BoosterType.ClearColumn).ToString());
            squareUsesText?.SetText(GetBoosterCount(BoosterType.ClearSquare).ToString());
            changeShapeUsesText?.SetText(GetBoosterCount(BoosterType.ChangeShape).ToString());
        }

        private void SetOutlineVisibility(BoosterType? booster)
        {
            if (rowOutline != null)
                rowOutline.SetActive(booster == BoosterType.ClearRow);
            if (columnOutline != null)
                columnOutline.SetActive(booster == BoosterType.ClearColumn);
            if (squareOutline != null)
                squareOutline.SetActive(booster == BoosterType.ClearSquare);
            if (changeShapeOutline != null)
                changeShapeOutline.SetActive(booster == BoosterType.ChangeShape);
        }

        private void SetBoosterButtonsState(Button selected)
        {
            foreach (var btn in boosterButtons)
            {
                if (btn == null) continue;
                btn.interactable = false;
                if (originalButtonColors.TryGetValue(btn, out var color))
                {
                    btn.image.color = btn == selected ? color : Color.gray;
                }
            }
        }

        private void ResetBoosterButtons()
        {
            foreach (var btn in boosterButtons)
            {
                if (btn == null) continue;
                btn.interactable = true;
                if (originalButtonColors.TryGetValue(btn, out var color))
                    btn.image.color = color;
            }
        }

        private async Task ChangeShapes()
        {
            var deckManager = FindObjectOfType<CellDeckManager>();
            if (deckManager != null)
            {
                deckManager.UpdateCellDeckAfterFail();
            }
            if (ResourceService.Instance != null)
                await ResourceService.Instance.ConsumeBooster(BoosterType.ChangeShape);
            UpdateBoosterTexts();
            activeBooster = null;
            ResetBoosterButtons();
        }

        private void Update()
        {
            if (activeBooster == null)
                return;

            if (fieldManager == null)
                fieldManager = FindObjectOfType<FieldManager>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            // Get input position (mouse or first touch)
            bool pressed = false;
            bool released = false;
            Vector3 screenPos = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                pressed = true;
                screenPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                released = true;
            }
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    pressed = true;
                    screenPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    released = true;
                }
            }
#endif

            if (released && lastHighlightedCell != null)
            {
                lastHighlightedCell.ClearCell();
                lastHighlightedCell = null;
            }

            if (!pressed || mainCamera == null)
                return;

            // Convert to world position (z=0 for 2D)
            Vector3 worldPos3 = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos3.z = 0f;
            Vector2 worldPosition = worldPos3;

            Debug.Log($"[BoosterManager] World position: {worldPosition}");

            // Point check at that position
            Collider2D col = Physics2D.OverlapPoint(worldPosition);
            if (col != null && col.CompareTag("Cell"))
            {
                Debug.Log("[BoosterManager] Cell collider hit.");
                var cell = col.GetComponent<Cell>();
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


        private async void HighlightCell(Cell cell)
        {
            Debug.Log("[BoosterManager] Highlighting cell.");
            if (lastHighlightedCell != null)
            {
                Debug.Log("[BoosterManager] Clearing previous highlighted cell.");
                lastHighlightedCell.ClearCell();
            }

            //cell.HighlightCellTutorial();
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
                            switch (activeBooster)
                            {
                                case BoosterType.ClearRow:
                                    FillRow(row);
                                    break;
                                case BoosterType.ClearColumn:
                                    FillColumn(col);
                                    break;
                                case BoosterType.ChangeShape:
                                    await ChangeShapes();
                                    break;
                            }
                            UpdateBoosterTexts();

                            if (activeBooster.HasValue)
                            {
                                if (ResourceService.Instance != null)
                                    await ResourceService.Instance.ConsumeBooster(activeBooster.Value);
                                UpdateBoosterTexts();
                                activeBooster = null;
                                SetOutlineVisibility(null);
                                ResetBoosterButtons();
                            }
                            lastHighlightedCell = null;
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

        private void FillRow(int rowIndex)
        {
            var itemTemplate = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
            if (itemTemplate == null)
            {
                Debug.Log("[BoosterManager] ItemTemplate 0 not found.");
                return;
            }

            for (int col = 0; col < fieldManager.cells.GetLength(1); col++)
            {
                var targetCell = fieldManager.cells[rowIndex, col];
                if (targetCell != null && targetCell.IsEmpty())
                {
                    targetCell.FillCell(itemTemplate);
                }
            }

            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(null);
        }

        private void FillColumn(int columnIndex)
        {
            var itemTemplate = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
            if (itemTemplate == null)
            {
                Debug.Log("[BoosterManager] ItemTemplate 0 not found.");
                return;
            }

            for (int row = 0; row < fieldManager.cells.GetLength(0); row++)
            {
                var targetCell = fieldManager.cells[row, columnIndex];
                if (targetCell != null && targetCell.IsEmpty())
                {
                    targetCell.FillCell(itemTemplate);
                }
            }

            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(null);
        }

        private void ClearRandomSquare()
        {
            if (fieldManager == null || fieldManager.cells == null)
                return;

            int rows = fieldManager.cells.GetLength(0);
            int cols = fieldManager.cells.GetLength(1);
            if (rows < 3 || cols < 3)
                return;

            int rowIndex = Random.Range(1, rows - 1);
            int columnIndex = Random.Range(1, cols - 1);
            ClearSquare(rowIndex, columnIndex);
        }

        private void ClearSquare(int rowIndex, int columnIndex)
        {
            for (int row = rowIndex - 1; row <= rowIndex + 1; row++)
            {
                for (int col = columnIndex - 1; col <= columnIndex + 1; col++)
                {
                    if (row < 0 || col < 0 || row >= fieldManager.cells.GetLength(0) || col >= fieldManager.cells.GetLength(1))
                        continue;

                    var targetCell = fieldManager.cells[row, col];
                    if (targetCell != null)
                    {
                        targetCell.ClearCell();
                    }
                }
            }

            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(null);
        }
    }

}
