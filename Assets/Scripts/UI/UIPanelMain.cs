using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Nguyen Duy Quy
public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimeAttack;

    [SerializeField] private Button btnNormal;

    [SerializeField] private Button btnAutoWin;

    [SerializeField] private Button btnAutoLose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnNormal.onClick.AddListener(OnClickMoves);
        btnTimeAttack.onClick.AddListener(OnClickTimer);
        btnAutoWin.onClick.AddListener(OnClickAutoWin);
        btnAutoLose.onClick.AddListener(OnClickAutoLose);
    }

    private void OnDestroy()
    {
        if (btnNormal) btnNormal.onClick.RemoveAllListeners();
        if (btnTimeAttack) btnTimeAttack.onClick.RemoveAllListeners();
        if (btnAutoWin) btnAutoWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickAutoWin()
    {
        m_mngr.LoadLevelAutoWin();
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAutoLose();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
//
