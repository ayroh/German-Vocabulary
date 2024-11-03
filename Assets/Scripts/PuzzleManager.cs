using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;
using Utilities.Signals;
using Random = System.Random;

public class PuzzleManager : Singleton<PuzzleManager>
{
    [Header("Test")]
    public TextMeshProUGUI errorText;


    [Header("References")]
    [SerializeField] private SentencePanel artikelPanel;
    [SerializeField] private SentencePanel answerPanel;
    [SerializeField] private SentencePanel lettersPanel;
    [SerializeField] private TextMeshProUGUI answerInASentenceText;
    [SerializeField] private TextMeshProUGUI germanMeaningText;
    [SerializeField] private TextMeshProUGUI englishMeaningText;
    [SerializeField] private RectTransform answerInASentenceParent;
    [SerializeField] private RectTransform germanMeaningParent;
    [SerializeField] private RectTransform englishMeaningParent;
    [SerializeField] private AddAnswerPanelController addAnswerPanelController;
    [SerializeField] private TextMeshProUGUI questionCountText;

    private List<IEnumerator> letterNumerators = new();
    private IEnumerator checkAnswerNumerator;

    private List<Letter> artikel => artikelPanel.letters;
    private List<Letter> answer => answerPanel.letters;
    private List<Letter> letters => lettersPanel.letters;

    private int maxAnswerLength => currentAnswer.answer.Length;
    private int currentLetterIndex = 0;

    [HideInInspector] public Answer currentAnswer;
    private int currentAnswerIndex = -1;

    private List<Answer> allAnswers = new();

    private StringBuilder sb = new StringBuilder(20);

    private readonly Dictionary<ArtikelType, Color> artikelColor = new()
    {
        {ArtikelType.der, new Color(0, 0, 0.8f) },
        {ArtikelType.die, new Color(0.8f, 0, 0) },
        {ArtikelType.das, new Color(0.15f, 0.75f, 0.15f) },
        {ArtikelType.NULL, Color.white }
    };

    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 144;

        sb.Insert(0, "_", 20);
    }

    private void Start()
    {
        for (int i = 0;i < artikelPanel.transform.childCount;i++)
            artikel[i].interactable = false;

        for (int i = 0;i < answerPanel.transform.childCount;i++)
        {
            answer[i].interactable = false;
            letterNumerators.Add(null);
        }

        for (int i = 0;i < lettersPanel.transform.childCount;i++)
            letters[i].interactable = true;

        currentLetterIndex = 0;
        currentAnswerIndex = -1;
        allAnswers = SaveLoadManager.LoadAnswers();
        Random rng = new();
        allAnswers = allAnswers.OrderBy(item => rng.Next()).ToList();
        SetNewAnswer(true);
    }

    public void SetNewAnswer(bool isNext)
    {
        SetAnswer(currentAnswerIndex = Mathf.Clamp(currentAnswerIndex += (isNext ? 1 : -1), 0, allAnswers.Count - 1));
    }

    private async void SetAnswer(int answerIndex)
    {
        Answer newAnswer = allAnswers[answerIndex];
        if(newAnswer == null || newAnswer.answer == "")
        {
            Debug.LogError("PuzzleManager: SetAnswer, Answer is empty!");
            return;
        }

        ResetAnswer();

        currentAnswer = newAnswer;
        answerPanel.SetSentence(maxAnswerLength, SpaceIndexesInSentence(ref currentAnswer.answer));
        lettersPanel.SetSentence(maxAnswerLength);

        if(currentAnswer.artikelType == ArtikelType.NULL)
        {
            artikelPanel.gameObject.SetActive(false);
        }
        else
        {
            artikelPanel.gameObject.SetActive(true);
            string artikelString = currentAnswer.artikelType.ToString();
            artikelPanel.SetSentence(artikelString.Length);
            for (int i = 0;i < artikelString.Length;++i)
                artikel[i].SetLetter(artikelString[i], artikelColor[currentAnswer.artikelType]);
        }

        answerInASentenceText.text = HideWordFromSentence(ref currentAnswer.answerInASentence, currentAnswer.answer);

        germanMeaningText.text = currentAnswer.germanMeaning;
        englishMeaningText.text = currentAnswer.englishMeaning;


        int[] order = new int[maxAnswerLength];
        for (int i = 0;i < maxAnswerLength;++i)
            order[i] = i;

        Random rng = new();
        order = order.OrderBy(item => rng.Next()).ToArray();
        for (int i = 0;i < maxAnswerLength;++i)
        {
            letters[i].SetLetter(currentAnswer.answer[order[i]]);
            
            if(currentAnswer.answer[order[i]] == ' ')
                letters[i].gameObject.SetActive(false);
        }

        SetQuestionCount();

        await Task.Yield();
        await Task.Yield();

        answerInASentenceParent.sizeDelta = new Vector2(answerInASentenceText.mesh.bounds.size.x + 50, answerInASentenceParent.sizeDelta.y);
        //germanMeaningParent.sizeDelta = new Vector2(germanMeaningText.mesh.bounds.size.x + 50, germanMeaningParent.sizeDelta.y);
        englishMeaningParent.sizeDelta = new Vector2(englishMeaningText.mesh.bounds.size.x + 50, englishMeaningParent.sizeDelta.y);
    }

    private string HideWordFromSentence(ref string sentence, string word)
    {
        return sentence.Replace(word, sb.ToString().Substring(0, word.Length), StringComparison.OrdinalIgnoreCase);
    }

    private List<int> SpaceIndexesInSentence(ref string sentence)
    {
        List<int> spaces = new();
        for(int i = 0;i < sentence.Length;++i)
        {
            if (sentence[i] == ' ')
                spaces.Add(i);
        }
        return spaces.Count != 0 ? spaces : null;
    }

    public void AddLetter(Letter newLetter)
    {
        if (currentLetterIndex == maxAnswerLength)
            return;

        if (currentAnswer.answer[currentLetterIndex] == ' ')
            currentLetterIndex++;

        StartCoroutine(letterNumerators[currentLetterIndex] = MoveLetter(newLetter, answer[currentLetterIndex]));

        currentLetterIndex++;
    }

    public void ResetAnswer()
    {
        if (currentAnswer == null)
            return;

        for (int i = 0;i < maxAnswerLength;++i)
        {
            if (letterNumerators[i] != null )
                StopCoroutine(letterNumerators[i]);

            answer[i].ResetLetter();

            letters[i].SetVisualsActive(true);
            letters[i].ResetVisualsPosition();
            letters[i].interactable = true;
        }

        currentLetterIndex = 0;
    }

    private IEnumerator MoveLetter(Letter singleLetter, Letter answerLetter)
    {
        char singleLetterChar = singleLetter.GetChar();
        if(singleLetterChar == '\0')
        {
            Debug.LogError("PuzzleManager: MoveLetter, Single letter char is empty!");
            yield break;
        }
        answerLetter.SetLetter(singleLetterChar);
        answerLetter.SetTextActive(false);

        singleLetter.interactable = false;

        float timer = 0f;
        Transform singleLetterVisuals = singleLetter.Visuals;
        RectTransform singleLetterRectTransform = singleLetter.GetComponent<RectTransform>();

        Vector3 startPos = singleLetterVisuals.position;
        Vector2 startSize = singleLetterRectTransform.sizeDelta;
        Vector2 endSize = answerLetter.GetComponent<RectTransform>().sizeDelta;
        float startFontSize = singleLetter.TextSize;

        while (timer < Constants.LetterMoveTime)
        {
            float value = timer / Constants.LetterMoveTime;
            
            singleLetterVisuals.position = Vector3.Lerp(startPos, answerLetter.transform.position, value);
            singleLetterRectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, value);
            singleLetter.TextSize = Mathf.Lerp(startFontSize, answerLetter.TextSize, value);

            timer += Time.deltaTime;
            yield return null;
        }

        singleLetterVisuals.position = answerLetter.transform.position;
        singleLetter.SetVisualsActive(false);
        singleLetterRectTransform.sizeDelta = startSize;
        singleLetter.TextSize = startFontSize;

        answerLetter.SetTextActive(true);

        CheckAnswer();
    }

    public void NextCorrectLetter()
    {
        if (currentLetterIndex == maxAnswerLength)
            return;

        while (currentAnswer.answer[currentLetterIndex] == ' ')
            currentLetterIndex++;

        for (int i = 0;i < currentLetterIndex;++i)
        {
            if (answer[i].GetChar() != currentAnswer.answer[i])
            {
                ResetAnswer();
                break;
            }
        }

        for (int i = 0;i < letters.Count;++i)
        {
            if (letters[i].GetChar() == currentAnswer.answer[currentLetterIndex] && letters[i].interactable)
            {
                AddLetter(letters[i]);
                break;
            }
        }
    }

    private void CheckAnswer()
    {
        if (checkAnswerNumerator != null)
            StopCoroutine(checkAnswerNumerator);

        StartCoroutine(checkAnswerNumerator = CheckAnswerCoroutine());
    }

    private IEnumerator CheckAnswerCoroutine()
    {
        if (currentLetterIndex != maxAnswerLength)
            yield break;

        for(int i = 0;i < maxAnswerLength;++i)
        {
            if (answer[i].GetChar() != currentAnswer.answer[i])
            {
                ResetAnswer();
                yield break;
            }
        }

        answerInASentenceText.text = currentAnswer.answerInASentence;

        yield return new WaitForSeconds(.5f);

        SetNewAnswer(true);
    }

    public void ShowSentenceToggle()
    {
        currentAnswer.show = !currentAnswer.show;
        SaveLoadManager.SaveAnswers(allAnswers);
    }

    private void SetQuestionCount() => questionCountText.text = (currentAnswerIndex + 1) + "/" + allAnswers.Count;
    
    
    public void AddAnswerToSave()
    {
        if (addAnswerPanelController.answerInputField.text == "" || addAnswerPanelController.sentenceInputField.text == "")
            return;

        Answer newAnswer = new Answer
        {
            answer = addAnswerPanelController.answerInputField.text,
            answerInASentence = addAnswerPanelController.sentenceInputField.text,
            germanMeaning = addAnswerPanelController.germanMeaningInputField.text,
            englishMeaning = addAnswerPanelController.englishMeaningInputField.text,
            artikelType = addAnswerPanelController.artikelInputField,
            show = true
        };

        int i = 0;
        for(i = 0;i < allAnswers.Count;++i)
        {
            if (allAnswers[i].answer == newAnswer.answer)
            {
                allAnswers[i] = newAnswer;
                addAnswerPanelController.OpenSavedPanel(true);
                break;
            }
        }

        if(i == allAnswers.Count)
        {
            allAnswers.Add(newAnswer);
            addAnswerPanelController.OpenSavedPanel(false);
        }

        SaveLoadManager.SaveAnswers(allAnswers);

    }

    public void DeleteCurrentAnswer()
    {
        allAnswers.Remove(currentAnswer);
        SaveLoadManager.SaveAnswers(allAnswers);
        SetNewAnswer(true);
    }



    private void OnEnable()
    {
        Signals.OnSetNewAnswer += SetNewAnswer;
        Signals.OnResetAnswer += ResetAnswer;
    }

    private void OnDisable()
    {
        Signals.OnSetNewAnswer -= SetNewAnswer;
        Signals.OnResetAnswer -= ResetAnswer;
    }


    public void GetAnswersFromWindowsButton()
    {
        SaveLoadManager.ChangeAnswersFromWindows();
        currentLetterIndex = 0;
        currentAnswerIndex = -1;
        allAnswers = SaveLoadManager.LoadAnswers();
        Random rng = new();
        allAnswers = allAnswers.OrderBy(item => rng.Next()).ToList();
        SetNewAnswer(true);
    }
}

public class Answer
{
    public ArtikelType artikelType;
    public string answer;
    public string answerInASentence;
    public string germanMeaning;
    public string englishMeaning;
    public bool show = true;
}