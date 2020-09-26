using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoginBehaviour : ViewBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Person[] _people;
    [SerializeField] private InputField _input;
    [SerializeField] private Text _messageBox;
    [SerializeField] private CanvasGroup _messageCg;
    [SerializeField] private Button _validateButton;
    #pragma warning restore 0649

    public override void Enter()
    {
        base.Enter();

        _validateButton.interactable = true;
    }

    public void ValidatePerson()
    {
        var person = _people.FirstOrDefault(x => x.code == _input.text.Trim());

        _validateButton.interactable = false;

        // No valid code provided
        if (person.name == null)
        {
            AnimateErrorMessage();
        }
        else
        {
            AnimateValidMessage(person);
        }
    }

    private void AnimateValidMessage(Person person)
    {
        _messageBox.text = "Welcome " + person.name;

        PlayerPrefs.SetString("person", person.name);

        if (person.isAdmin)
            PlayerPrefs.SetString("admin", "yes");

        LeanTween.cancel(_messageBox.gameObject);

        LeanTween.alphaCanvas(_messageCg, 1, 1).setFrom(0).setOnComplete(() =>
        {
            LeanTween.alphaCanvas(_messageCg, 0, 1).setDelay(2f).setOnComplete(() =>
            {
                Leave();
            });
        });
    }

    private void AnimateErrorMessage()
    {
        _messageBox.text = "Invalid code";

        LeanTween.cancel(_messageBox.gameObject);

        LeanTween.alphaCanvas(_messageCg, 1, 1).setFrom(0).setOnComplete(() =>
        {
            LeanTween.alphaCanvas(_messageCg, 0, 1).setDelay(2f).setOnComplete(() =>
            {
                _validateButton.interactable = true;
            });
        });
    }
}

[System.Serializable]
public struct Person
{
    public string name;
    public string code;
    public bool isAdmin;
}
