using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockBehaviour : ViewBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private GameObject _lockedImage;
    [SerializeField] private GameObject _unlockedImage;
    [SerializeField] private Text _distanceText;

    [SerializeField] private GpsPosition _targetPosition;

    [SerializeField] private float _checkGpsDelayInSec = 1f;
    [SerializeField] private float _unlockDistanceInKm = 0.05f;
    #pragma warning restore 0649

    private bool _checkGps = false;
    private float _timeSinceLastCheck = 0;

    public override void Enter()
    {
        base.Enter();

        if (PlayerPrefs.GetString("unlocked") == "yeahbaby")
        {
            //TODO: Go to the next page
        }
        else
        {
            // Start GPS
            Input.location.Start();
            _checkGps = true;
        }
    }

    public override void Leave(string target = null)
    {
        base.Leave(target);

        Input.location.Stop();
    }

    private void CheckDistance()
    {
        if (!_checkGps)
            return;

        _timeSinceLastCheck += Time.deltaTime;

        if (_timeSinceLastCheck < _checkGpsDelayInSec)
            return;

        _timeSinceLastCheck -= _checkGpsDelayInSec;

        if (Input.location.status == LocationServiceStatus.Running)
        {
            var locationData = new GpsPosition(Input.location.lastData.latitude, Input.location.lastData.longitude);
            var distance = GpsHelper.DistanceTo(locationData, _targetPosition);

            _distanceText.text = GpsHelper.DistanceToString(distance);

            if (distance < _unlockDistanceInKm)
            {
                UnlockAnimation();
            }
        }
        else if (Input.location.status == LocationServiceStatus.Stopped)
        {
            Input.location.Start();
        }
        else if (Input.location.status == LocationServiceStatus.Failed)
        {
            _distanceText.text = "NO GPS";

            _checkGps = false;
        }
    }

    public void UnlockAnimation()
    {
        _checkGps = false;

        _distanceText.text = "WELCOME!";

        _lockedImage.gameObject.SetActive(false);
        _unlockedImage.gameObject.SetActive(true);

        Handheld.Vibrate();
        LeanTween.rotateZ(_unlockedImage, 0, 0.5f).setEase(LeanTweenType.easeOutElastic).setFrom(-20f).setOnComplete(() => 
        {
            // Ungly wait to leave page
            LeanTween.rotateY(_unlockedImage, 0, 0).setDelay(1f).setOnComplete(() => { Leave(); });
        });
    }

    // Update is called once per frame
    private void Update()
    {
        CheckDistance();
    }
}
