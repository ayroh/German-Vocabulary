using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utilities.Enums;

public class AddAnswerPanelController : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField answerInputField;
    public TMP_InputField sentenceInputField;
    public TMP_InputField germanMeaningInputField;
    public TMP_InputField englishMeaningInputField;
    [HideInInspector] public ArtikelType artikelInputField = ArtikelType.der;
    [SerializeField] private ArtikelButtonController artikelButtonController;
    [SerializeField] private GameObject savedPanel;
    [SerializeField] private TextMeshProUGUI savedPanelText;

    public void InitializeEditPanel()
    {
        Answer currentAnswer = PuzzleManager.instance.currentAnswer;
        if (currentAnswer == null)
        {
            Debug.LogError("AddAnswerPanelController: InitializeEditPanel, current answer is null!");
            return;
        }
        answerInputField.text = currentAnswer.answer;
        sentenceInputField.text = currentAnswer.answerInASentence;
        englishMeaningInputField.text = currentAnswer.englishMeaning;
        germanMeaningInputField.text = currentAnswer.germanMeaning;
        artikelButtonController.PressButton((int)currentAnswer.artikelType);
    }

    public void OpenSavedPanel(bool isEdited = false)
    {
        StartCoroutine(OpenSavedPanelCoroutine(isEdited));
    }

    private IEnumerator OpenSavedPanelCoroutine(bool isEdited = false)
    {
        savedPanelText.text = isEdited ? "Edited" : "Saved";
        savedPanel.SetActive(true);

        yield return new WaitForSeconds(1f);

        savedPanel.SetActive(false);
    }

    private void OnDisable()
    {
        savedPanel.SetActive(false);
    }

}
