using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HikeQuestionBehaviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private CanvasGroup _questionCanvas;
    [SerializeField] private Text _questionText;
    [SerializeField] private QuestionButton[] _buttons;
    #pragma warning restore 0649

    private HikeAnswer[] _answers;

    public Action<int> OnAnswerClicked;

    public void SetQuestion(string question, HikeAnswer[] answers)
    {
        _answers = answers;
        _questionText.text = question;

        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i].Button == null || _buttons[i].ButtonText == null)
                continue;

            if (answers.Length > i)
            {
                _buttons[i].Button.SetActive(true);
                _buttons[i].ButtonText.text = answers[i].Answer;
            }
            else
            {
                _buttons[i].Button.SetActive(false);
            }
        }

        _questionCanvas.alpha = 1;
        _questionCanvas.blocksRaycasts = true;
    }

    public void OnSelection(int index)
    {
        _questionCanvas.alpha = 0;
        _questionCanvas.blocksRaycasts = false;

        var score = 0;

        if (index < _answers.Length)
            score = _answers[index].Points;

        OnAnswerClicked?.Invoke(score);
    }


    [Serializable]
    private struct QuestionButton
    {
        public GameObject Button;
        public Text ButtonText;

        public QuestionButton(GameObject button, Text buttonText)
        {
            Button = button;
            ButtonText = buttonText;
        }
    }
}
