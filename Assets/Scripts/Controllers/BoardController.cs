using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    public bool IsTimerMode { get; set; }

    public bool IsAutoWinMode { get; set; }

    public bool IsAutoLoseMode { get; set; }

    private Board m_board;

    private GameManager m_gameManager;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    private float autoPlayDelay = 0.5f;

    private float autoPlayTimer = 0f;

    private Dictionary<NormalItem.eNormalType, List<Cell>> autoPlayGroups;

    private List<NormalItem.eNormalType> autoPlayTypes;

    private int currentTypeIndex = 0;

    private bool autoPlayInitialized = false;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    //Nguyen Duy Quy
    private void Fill()
    {
        m_board.FillMultipleOf3();
    }

    public bool IsBoardCleared()
    {
        return m_board.IsCleared();
    }

    public bool IsBoardQueueFull()
    {
        return m_board.IsQueueFull();
    }

    public Board GetBoard()
    {
        return m_board;
    }

    public void StartAutoPlay()
    {
        autoPlayGroups = m_board.GetCellsGroupedByType();
        autoPlayTypes = new List<NormalItem.eNormalType>(autoPlayGroups.Keys);
        currentTypeIndex = 0;
        autoPlayTimer = 0f;
        autoPlayInitialized = true;
    }
    //

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        //Nguyen Duy Quy
        if (IsAutoWinMode || IsAutoLoseMode)
        {
            if (!autoPlayInitialized)
            {
                StartAutoPlay();
            }

            autoPlayTimer -= Time.deltaTime;

            if (autoPlayTimer <= 0f)
            {
                if (IsAutoWinMode)
                {
                    while (currentTypeIndex < autoPlayTypes.Count &&
                      autoPlayGroups[autoPlayTypes[currentTypeIndex]].Count == 0)
                    {
                        currentTypeIndex++;
                    }
                    if (currentTypeIndex < autoPlayTypes.Count)
                    {
                        NormalItem.eNormalType fishType = autoPlayTypes[currentTypeIndex];
                        List<Cell> cellsOfType = autoPlayGroups[fishType];
                        if (cellsOfType.Count > 0)
                        {
                            Cell cellToMove = cellsOfType[0];
                            m_board.MoveItemToQueue(cellToMove);

                            StartCoroutine(WaitAndMatchCoroutine(cellToMove));
                            cellsOfType.RemoveAt(0);
                        }
                        autoPlayTimer = autoPlayDelay;
                    }
                } 
                else
                {
                    if (m_board.IsQueueFull())
                    {
                        m_gameManager.GameOver();
                        return;
                    }

                    NormalItem.eNormalType fishType = autoPlayTypes[currentTypeIndex];
                    List<Cell> cellsOfType = autoPlayGroups[fishType];
                    Cell cellToMove = cellsOfType[0];
                    m_board.MoveItemToQueue(cellToMove);
                    cellsOfType.RemoveAt(0);
                    currentTypeIndex++;
                    autoPlayTimer = autoPlayDelay;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    Cell c = hit.collider.GetComponent<Cell>();

                    if (m_board.IsQueueCell(c))
                    {
                        if (IsTimerMode)
                        {
                            m_board.ReturnItemFromQueue(c);

                            StartCoroutine(ShiftLeftQueueItemsCoroutine());
                        }
                    }
                    else
                    {
                        m_board.MoveItemToQueue(c);

                        StartCoroutine(WaitAndMatchCoroutine(c));
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                ResetRayCast();
            }
        }
        //

        /*if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                ShowHint();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                m_isDragging = true;
                m_hitCollider = hit.collider;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetRayCast();
        }

        if (Input.GetMouseButton(0) && m_isDragging)
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                if (m_hitCollider != null && m_hitCollider != hit.collider)
                {
                    StopHints();

                    Cell c1 = m_hitCollider.GetComponent<Cell>();
                    Cell c2 = hit.collider.GetComponent<Cell>();
                    if (AreItemsNeighbor(c1, c2))
                    {
                        IsBusy = true;
                        SetSortingLayer(c1, c2);
                        m_board.Swap(c1, c2, () =>
                        {
                            FindMatchesAndCollapse(c1, c2);
                        });

                        ResetRayCast();
                    }
                }
            }
            else
            {
                ResetRayCast();
            }
        }*/
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    {
        if (cell1.Item is BonusItem)
        {
            cell1.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else if (cell2.Item is BonusItem)
        {
            cell2.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else
        {
            List<Cell> cells1 = GetMatches(cell1);
            List<Cell> cells2 = GetMatches(cell2);

            List<Cell> matches = new List<Cell>();
            matches.AddRange(cells1);
            matches.AddRange(cells2);
            matches = matches.Distinct().ToList();

            if (matches.Count < m_gameSettings.MatchesMin)
            {
                m_board.Swap(cell1, cell2, () =>
                {
                    IsBusy = false;
                });
            }
            else
            {
                OnMoveEvent();

                CollapseMatches(matches, cell2);
            }
        }
    }

    //Nguyen Duy Quy
    private void Match3(Cell cell)
    {
        List<Cell> matches = m_board.GetQueueMatches();

        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }
    }
    //

    private void FindMatchesAndCollapse()
    {
        List<Cell> matches = m_board.FindFirstMatch();

        if (matches.Count > 0)
        {
            CollapseMatches(matches, null);
        }
        else
        {
            m_potentialMatch = m_board.GetPotentialMatches();
            if (m_potentialMatch.Count > 0)
            {
                IsBusy = false;

                m_timeAfterFill = 0f;
            }
            else
            {
                //StartCoroutine(RefillBoardCoroutine());
                StartCoroutine(ShuffleBoardCoroutine());
            }
        }
    }

    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<Cell> listVert = m_board.GetVerticalMatches(cell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            m_board.ConvertNormalToBonus(matches, cellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    //Nguyen Duy Quy
    private IEnumerator WaitAndMatchCoroutine(Cell c)
    {
        yield return new WaitForSeconds(0.2f);

        Match3(c);
        OnMoveEvent();

        StartCoroutine(ShiftLeftQueueItemsCoroutine());
    }

    private IEnumerator ShiftLeftQueueItemsCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        m_board.ShiftLeftQueueItems();
    }
    //

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

       FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        FindMatchesAndCollapse();
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }
}
