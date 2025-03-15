using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMoves : LevelCondition
{
    private int m_moves;

    private BoardController m_board;

    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt);

        m_moves = (int)value;

        m_board = board;

        m_board.OnMoveEvent += OnMove;

        UpdateText();
    }

    //Nguyen Duy Quy
    private void OnMove()
    {
        if (m_conditionCompleted) return;

        //m_moves--;

        UpdateText();

        if (m_board.IsBoardCleared())
        {
            OnConditionComplete(LevelResult.WIN);
        }
        else if(m_board.IsBoardQueueFull())
        {
            OnConditionComplete(LevelResult.LOSE);
        }
    }
    //

    protected override void UpdateText()
    {
        m_txt.text = string.Format("NORMAL", m_moves);
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
