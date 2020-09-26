using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ViewBehaviour : MonoBehaviour
{
    public Action OnEnterEvent;
    public Action<string> OnLeaveEvent;

    private CanvasGroup _cg;

    private CanvasGroup canvasGroup
    {
        get
        {
            if (_cg == null)
                _cg = GetComponent<CanvasGroup>();

            return _cg;
        }
    }

    public virtual void Enter()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        OnEnterEvent?.Invoke();
    }

    public virtual void Leave(string target = null)
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;

        OnLeaveEvent?.Invoke(target);
    }
}
