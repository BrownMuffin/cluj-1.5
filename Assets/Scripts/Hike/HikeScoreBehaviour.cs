using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HikeScoreBehaviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private CanvasGroup _scoreCanvas;
    [SerializeField] private Text _timeText;
    [SerializeField] private Text _distanceText;
    [SerializeField] private Text _speedText;
    [SerializeField] private Text _scoreText;
    #pragma warning restore 0649

    public Action OnBackClicked;
    
    public void SetScore(float time, float distance, int score)
    {
        _scoreCanvas.alpha = 1;

        // Time
        var timeSpan = TimeSpan.FromSeconds(time);
        _timeText.text = "Time: " + timeSpan.ToString(@"hh\:mm\:ss");

        // Distance
        _distanceText.text = "Distance: " + GpsHelper.DistanceToString(distance);

        // Speed
        var kph = distance / (time / 3600f);
        _speedText.text = "Speed: " + kph + " KM/H";

        // Score
        _scoreText.text = "Score: " + score;
    }

    public void OnBackButton()
    {
        _scoreCanvas.alpha = 0;
        OnBackClicked?.Invoke();
    }
}
