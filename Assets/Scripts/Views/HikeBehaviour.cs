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

    [SerializeField] private List<GpsPosition> _hikeTargets;
    [SerializeField] private GameObject _hikeTargetPrefab;
    [SerializeField] private GameObject _hikeLinePrefab;
    [SerializeField] private RectTransform _hikeProgressRoot;
    #pragma warning restore 0649

    private int _hikeTargetIndex = 0;
    private List<HikeTargetBehaviour> _hikeTargetObjects;
    private List<HikeLineBehaviour> _hikeLineObjects;
    private float _progressWidth;

    private bool _hiking = true;
    private float _timeSinceLastCompassCheck = 0;
    private float _timeSinceLastDistanceCheck = 0;

    public override void Enter()
    {
        base.Enter();

        Input.location.Start();
        Input.compass.enabled = true;

        _northTransform.eulerAngles = new Vector3(0f, 0f, 0f);
        _distanceText.text = "START GPS...";
        
        _progressWidth = ((RectTransform)_hikeProgressRoot.parent.parent).sizeDelta.x;

        _hikeProgressRoot.anchoredPosition = new Vector2((_progressWidth * 0.5f) - 40f, 0f);
        CreateProgressBar();
    }

    public override void Leave(string target = null)
    {
        base.Leave(target);

        Input.compass.enabled = false;
        Input.location.Stop();
    }

    private void CreateProgressBar()
    {
        // Reset list
        _hikeTargetObjects = new List<HikeTargetBehaviour>();
        _hikeLineObjects = new List<HikeLineBehaviour>();

        // Remove all childs
        foreach (Transform child in _hikeProgressRoot)
            Destroy(child.gameObject);

        // Create new targets and lines
        for (int i = 0; i < _hikeTargets.Count; i++)
        {
            var target = Instantiate(_hikeTargetPrefab, _hikeProgressRoot);
            _hikeTargetObjects.Add(target.GetComponent<HikeTargetBehaviour>());

            if (i < _hikeTargets.Count - 1)
            {
                var line = Instantiate(_hikeLinePrefab, _hikeProgressRoot);
                var lineBehaviour = line.GetComponent<HikeLineBehaviour>();
                _hikeLineObjects.Add(lineBehaviour);

                var distance = GpsHelper.DistanceTo(_hikeTargets[i], _hikeTargets[i + 1]);
                lineBehaviour.SetDistance(distance);
            }
        }
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
        var angle = GpsHelper.AngleTo(localPosition, _hikeTargets[_hikeTargetIndex]);
        
        _needleTransform.LeanRotateZ(Input.compass.trueHeading + angle, _checkCompassDelayInSec);

        // Check angle for north
        _northTransform.LeanRotateZ(Input.compass.trueHeading, _checkCompassDelayInSec);
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
        var distance = GpsHelper.DistanceTo(localPosition, _hikeTargets[_hikeTargetIndex]);
        _distanceText.text = GpsHelper.DistanceToString(distance);

        // Check progress
        if (_hikeTargetIndex > 0)
        {
            _hikeLineObjects[_hikeTargetIndex - 1].SetDistance(distance);
        }

        if (distance < _unlockDistanceInKm)
        {
            // Update progress
            _hikeTargetObjects[_hikeTargetIndex].SetComplete();

            if (_hikeTargetIndex > 0)
                _hikeLineObjects[_hikeTargetIndex - 1].SetComplete();

            // Get next target
            _hikeTargetIndex++;

            // Update progress position
            var progressPosition = (_progressWidth * 0.5f) - ((_hikeTargetIndex * 224) + 40);
            LeanTween.moveX(_hikeProgressRoot, progressPosition, 0.5f).setEaseInOutSine();
            
            // Update UI next frame
            if (_hikeTargetIndex < _hikeTargets.Count)
            {
                //_timeSinceLastCompassCheck = _checkCompassDelayInSec;
                //_timeSinceLastDistanceCheck = _checkDistanceDelayInSec;
            }
            // Done
            else
            {
                _hiking = false;
                _northTransform.rotation = Quaternion.identity;
                _needleTransform.rotation = Quaternion.identity;

                // TODO: Done
            }
        }
    }

    private void Update()
    {
        UpdateCompass();
        UpdateDistance();

    }
}
