using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashBehaviour : ViewBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private CanvasGroup _content;
    [SerializeField] private Text _titleText;
    #pragma warning restore 0649

    private float _fadeDelay = 0.75f;
    private float _stepDelay = 0.5f;

    private string[] _steps = { "Cluj", "Cluj 1", "Cluj 1.", "Cluj 1.5", "Cluj 1.5", "Cluj 1.5", "Cluj 1.5" };

    private bool _stepping = false;
    private int _stepIndex = 0;
    private float _timeSinceLastStep = 0;

    public override void Enter()
    {
        base.Enter();

        _stepping = false;
        _stepIndex = 0;
        _titleText.text = "";

        LeanTween.cancel(gameObject);

        LeanTween.alphaCanvas(_content, 1, _fadeDelay).setFrom(0).setOnComplete(() => { _stepping = true; });
    }
    
    private void Update()
    {
        if (_stepping)
            DoSteps();
    }

    private void DoSteps()
    {
        _timeSinceLastStep += Time.deltaTime;

        if (_timeSinceLastStep > _stepDelay)
        {
            _timeSinceLastStep -= _stepDelay;
            _titleText.text = _steps[_stepIndex];
            _stepIndex++;

            if (_stepIndex == _steps.Length)
            {
                _stepping = false;
                LeanTween.alphaCanvas(_content, 0, _fadeDelay).setOnComplete(() => { Leave(); });
            }
        }
    }
}
