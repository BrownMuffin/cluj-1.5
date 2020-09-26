using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HikeTargetBehaviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite _completeSprite;
    #pragma warning restore 0649

    public void SetComplete()
    {
        _targetImage.sprite = _completeSprite;
    }
}
