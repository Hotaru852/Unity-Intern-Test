using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Nguyen Duy Quy
public class LevelAutoLose : LevelCondition
{
    private BoardController m_board;

    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt);

        m_board = board;

        UpdateText();
    }

    private void Update()
    {
        if (m_conditionCompleted) return;

        UpdateText();

        if (m_board.IsBoardQueueFull())
        {
            OnConditionComplete(LevelResult.LOSE);
        }
    }

    protected override void UpdateText()
    {
        m_txt.text = "AUTO";
    }
}