using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Letter : MonoBehaviour, IPointerClickHandler
{
    [Header("Variables")]
    [SerializeField] private Transform visuals;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;

    [HideInInspector] public bool interactable = true;

    private Color backgroundInitialColor;

    public Transform Visuals => visuals;

    public float TextSize {
        get { return text.fontSize; }
        set {  text.fontSize = value; }
    }

    private void Awake()
    {
        backgroundInitialColor = background.color;
    }

    public string GetLetter() => text.text;

    public char GetChar() => text.text[0];

    public void SetLetter(char letter, Color color = default)
    {
        text.text = letter.ToString();
        text.color = color == default ? Color.white : color;
    }

    public void ResetLetter()
    {
        SetLetter('\0');
        background.color = backgroundInitialColor;
        ResetVisualsPosition();
    }

    public void SetVisualsActive(bool choice)
    {
        SetTextActive(choice);
        background.gameObject.SetActive(choice);
    }
    

    public void SetTextActive(bool choice) => text.gameObject.SetActive(choice);

    public void ResetVisualsPosition() => visuals.localPosition = Vector3.zero;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
            return;
        PuzzleManager.instance.AddLetter(this);
    }
}
