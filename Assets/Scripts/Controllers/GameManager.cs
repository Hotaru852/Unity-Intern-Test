using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES,
        AUTOWIN,
        AUTOLOSE
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WIN
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    //Nguyen Duy Quy
    public void LoadLevel(eLevelMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_uiMenu.GetLevelConditionView(), this, m_boardController);
            m_boardController.IsTimerMode = true;
        }
        else if (mode == eLevelMode.AUTOWIN)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelAutoWin>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
            m_boardController.IsAutoWinMode = true;
        }
        else if (mode == eLevelMode.AUTOLOSE)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelAutoLose>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
            m_boardController.IsAutoLoseMode = true;
        }
        m_levelCondition.ConditionCompleteEvent += OnLevelConditionComplete;

        State = eStateGame.GAME_STARTED;
    }

    private void OnLevelConditionComplete(LevelCondition.LevelResult result)
    {
        if (result == LevelCondition.LevelResult.WIN)
        {
            GameWin();
        }
        else
        {
            GameOver();
        }
    }

    public void GameWin()
    {
        StartCoroutine(WaitBoardController(true));
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController(false));
    }

    private IEnumerator WaitBoardController(bool isWin)
    {
        while (m_boardController.IsBusy)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        State = isWin ? eStateGame.GAME_WIN : eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= OnLevelConditionComplete;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
    //

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }
}
