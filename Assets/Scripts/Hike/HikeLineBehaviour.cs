using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HikeLineBehaviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Image _progressImage;
    [SerializeField] private GameObject _dotsContainer;
    #pragma warning restore 0649

    private float _distance;

    public void SetDistance(float distance)
    {
        _distance = distance;
    }

    public void SetProgress(float currentDistance)
    {
        var done =_distance - currentDistance;

        _progressImage.fillAmount = done / _distance;
    }

    public void SetComplete()
    {
        _progressImage.fillAmount = 1f;
        _dotsContainer.SetActive(false);
    }
}
