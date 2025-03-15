using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    //Nguyen Duy Quy
    public enum LevelResult { WIN, LOSE }
    public event Action<LevelResult> ConditionCompleteEvent = delegate { };
    //

    protected Text m_txt;

    protected bool m_conditionCompleted = false;

    public virtual void Setup(float value, Text txt)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, GameManager mngr)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, BoardController board)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, GameManager mngr, BoardController board) {
        m_txt = txt;
    }

    protected virtual void UpdateText() { }

    //Nguyen Duy Quy
    protected void OnConditionComplete(LevelResult result)
    {
        if (m_conditionCompleted) return;

        m_conditionCompleted = true;
        ConditionCompleteEvent(result);
    }
    //

    protected virtual void OnDestroy()
    {

    }
}
