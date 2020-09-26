using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowBehviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private ViewBehaviour _splash;
    [SerializeField] private ViewBehaviour _login;
    [SerializeField] private ViewBehaviour _unlock;
    [SerializeField] private ViewBehaviour _menu;
    [SerializeField] private ViewBehaviour _hike;
    #pragma warning restore 0649
    private void OnEnable()
    {
        _splash.OnLeaveEvent += SplashLeave;
        _login.OnLeaveEvent += LoginLeave;
        _unlock.OnLeaveEvent += UnlockLeave;
        _menu.OnLeaveEvent += MenuLeave;
        _hike.OnLeaveEvent += HikeLeave;
    }

    private void OnDestroy()
    {
        _splash.OnLeaveEvent -= SplashLeave;
        _login.OnLeaveEvent -= LoginLeave;
        _unlock.OnLeaveEvent -= UnlockLeave;
        _menu.OnLeaveEvent -= MenuLeave;
        _hike.OnLeaveEvent -= HikeLeave;
    }

    private void Start()
    {
        _splash.Enter();
    }

    private void SplashLeave(string taget)
    {
        //HACK
        _hike.Enter();
        return;

        // Not logged in yet
        if (!PlayerPrefs.HasKey("person"))
            _login.Enter();
        // Not unlocked yet
        else if (!PlayerPrefs.HasKey("unlocked"))
            _unlock.Enter();
        else
            _menu.Enter();
    }

    private void LoginLeave(string taget)
    {
        _unlock.Enter();
    }

    private void UnlockLeave(string taget)
    {
        _menu.Enter();
    }

    private void MenuLeave(string taget)
    {

    }

    private void HikeLeave(string target)
    {
        _menu.Enter();
    }
}
