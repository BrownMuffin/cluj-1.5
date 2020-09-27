using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HikeBehaviour : ViewBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private RectTransform _needleTransform;
    [SerializeField] private RectTransform _northTransform;
    [SerializeField] private float _checkCompassDelayInSec = 0.5f;

    [SerializeField] private float _unlockDistanceInKm = 0.01f;
    [SerializeField] private float _checkDistanceDelayInSec = 1f;
    [SerializeField] private Text _distanceText;
    
    [SerializeField] private HikeTarget[] _hikeTargets;

    [SerializeField] private GameObject _hikeTargetPrefab;
    [SerializeField] private GameObject _hikeLinePrefab;
    [SerializeField] private RectTransform _hikeProgressRoot;

    [SerializeField] private CanvasGroup _overlayCanvas;
    [SerializeField] private HikeQuestionBehaviour _hqb;
    [SerializeField] private HikeScoreBehaviour _hsb;
    #pragma warning restore 0649

    private int _hikeTargetIndex = 0;
    private List<HikeTargetBehaviour> _hikeTargetObjects;
    private List<HikeLineBehaviour> _hikeLineObjects;
    private float _progressWidth;
    private float _targetWidth;
    private float _lineWidth;

    private bool _hiking = true;
    private float _timeSinceLastCompassCheck = 0;
    private float _timeSinceLastDistanceCheck = 0;

    private bool _hikingStarted = false;
    private float _hikingTime;
    private float _hikingDistance;
    private int _hikingScore;

    
    public override void Enter()
    {
        base.Enter();

        Input.location.Start();
        Input.compass.enabled = true;

        //Reset values
        _northTransform.eulerAngles = new Vector3(0f, 0f, 0f);
        _distanceText.text = "START GPS...";

        _hikingStarted = false;
        _hikingTime = 0f;
        _hikingDistance = 0f;
        _hikingScore = 0;

        _overlayCanvas.alpha = 0;
        _overlayCanvas.blocksRaycasts = false;

        _hqb.OnAnswerClicked += OnScoreUpdate;
        _hsb.OnBackClicked += OnBack;

        // Set progress in the correct position
        _targetWidth = ((RectTransform)_hikeTargetPrefab.transform).sizeDelta.x;
        _lineWidth = ((RectTransform)_hikeLinePrefab.transform).sizeDelta.x;

        _progressWidth = ((RectTransform)_hikeProgressRoot.parent.parent).sizeDelta.x;
        _hikeProgressRoot.anchoredPosition = new Vector2((_progressWidth - _targetWidth) * 0.5f, 0f);
        
        CreateProgressBar();
    }

    public override void Leave(string target = null)
    {
        base.Leave(target);

        Input.compass.enabled = false;
        Input.location.Stop();

        _hqb.OnAnswerClicked -= OnScoreUpdate;
    }

    private void CreateProgressBar()
    {
        // Reset list
        _hikeTargetIndex = 0;
        _hikeTargetObjects = new List<HikeTargetBehaviour>();
        _hikeLineObjects = new List<HikeLineBehaviour>();

        // Remove all childs
        foreach (Transform child in _hikeProgressRoot)
            Destroy(child.gameObject);

        // Need at least 2 targets
        if (_hikeTargets.Length < 2)
            return;

        // Create new targets and lines
        for (int i = 0; i < _hikeTargets.Length; i++)
        {
            var target = Instantiate(_hikeTargetPrefab, _hikeProgressRoot);
            
            _hikeTargetObjects.Add(target.GetComponent<HikeTargetBehaviour>());

            if (i < _hikeTargets.Length - 1)
            {
                var line = Instantiate(_hikeLinePrefab, _hikeProgressRoot);
                var lineBehaviour = line.GetComponent<HikeLineBehaviour>();
                _hikeLineObjects.Add(lineBehaviour);

                var distance = GpsHelper.DistanceTo(_hikeTargets[i].Position, _hikeTargets[i + 1].Position);
                lineBehaviour.SetDistance(distance);
                _hikingDistance += distance;
            }
        }

        Debug.Log(_hikingDistance);
    }

    private void UpdateCompass()
    {
        if (!_hiking)
            return;

        // Check GPS
        if (Input.location.status != LocationServiceStatus.Running)
            return;

        // Check time
        _timeSinceLastCompassCheck += Time.deltaTime;

        if (_timeSinceLastCompassCheck < _checkCompassDelayInSec)
            return;

        _timeSinceLastCompassCheck -= _checkCompassDelayInSec;

        // Check angle for the needle
        var localPosition = new GpsPosition(Input.location.lastData.latitude, Input.location.lastData.longitude);
        var angle = GpsHelper.AngleTo(localPosition, _hikeTargets[_hikeTargetIndex].Position);
        
        _needleTransform.LeanRotateZ(Input.compass.trueHeading + angle, _checkCompassDelayInSec * 2f);

        // Check angle for north
        _northTransform.LeanRotateZ(Input.compass.trueHeading, _checkCompassDelayInSec * 2f);
    }
    
    private void UpdateDistance()
    {
        if (!_hiking)
            return;

        // Check GPS
        if (Input.location.status != LocationServiceStatus.Running)
            return;

        // Check time
        _timeSinceLastDistanceCheck += Time.deltaTime;

        if (_timeSinceLastDistanceCheck < _checkDistanceDelayInSec)
            return;

        _timeSinceLastDistanceCheck -= _checkDistanceDelayInSec;

        // Check distance
        var localPosition = new GpsPosition(Input.location.lastData.latitude, Input.location.lastData.longitude);
        var distance = GpsHelper.DistanceTo(localPosition, _hikeTargets[_hikeTargetIndex].Position);
        _distanceText.text = GpsHelper.DistanceToString(distance);

        // Update progress line
        if (_hikeTargetIndex > 0)
            _hikeLineObjects[_hikeTargetIndex - 1].SetProgress(distance);

        // Check if unlocked
        if (distance < _unlockDistanceInKm)
        {
            // Start the time
            if (_hikeTargetIndex == 0)
                _hikingStarted = true;

            // Check for question
            if (!string.IsNullOrEmpty(_hikeTargets[_hikeTargetIndex].Question))
                ShowQuestion(_hikeTargets[_hikeTargetIndex].Question, _hikeTargets[_hikeTargetIndex].Answers);

            // Update progress
            _hikeTargetObjects[_hikeTargetIndex].SetComplete();

            if (_hikeTargetIndex > 0)
                _hikeLineObjects[_hikeTargetIndex - 1].SetComplete();

            // Get next target
            _hikeTargetIndex++;

            // Update progress position
            var progressPosition = (_progressWidth * 0.5f) - ((_hikeTargetIndex * (_targetWidth + _lineWidth)) + (_targetWidth * 0.5f));
            LeanTween.moveX(_hikeProgressRoot, progressPosition, 0.5f).setEaseInOutSine();
            
            // Update UI next frame
            if (_hikeTargetIndex < _hikeTargets.Length)
            {
                _timeSinceLastCompassCheck = _checkCompassDelayInSec;
                _timeSinceLastDistanceCheck = _checkDistanceDelayInSec;
            }
            // Done
            else
            {
                _hiking = false;
                _hikingStarted = false;
                _northTransform.rotation = Quaternion.identity;
                _needleTransform.rotation = Quaternion.identity;

                ShowScore();
            }
        }
    }

    private void ShowQuestion(string question, HikeAnswer[] answers)
    {
        _overlayCanvas.alpha = 1;
        _overlayCanvas.blocksRaycasts = true;

        _hqb.SetQuestion(question, answers);
    }

    private void ShowScore()
    {
        _overlayCanvas.alpha = 1;
        _overlayCanvas.blocksRaycasts = true;

        _hsb.SetScore(_hikingTime, _hikingDistance, _hikingScore);
    }

    private void OnScoreUpdate(int score)
    {
        _overlayCanvas.alpha = 0;
        _overlayCanvas.blocksRaycasts = false;

        _hikingScore += score;
    }

    private void OnBack()
    {
        _overlayCanvas.alpha = 0;
        _overlayCanvas.blocksRaycasts = false;

        Leave();
    }

    private void Update()
    {
        UpdateCompass();
        UpdateDistance();

        if (_hikingStarted)
            _hikingTime += Time.deltaTime;
    }
}

[Serializable]
public struct HikeTarget
{
    public GpsPosition Position;
    public string Question;
    public HikeAnswer[] Answers;

    public HikeTarget(GpsPosition position, string question = null, HikeAnswer[] answers = null)
    {
        Position = position;
        Question = question;
        Answers = answers;
    }
}

[Serializable]
public struct HikeAnswer
{
    public string Answer;
    public int Points;
}
