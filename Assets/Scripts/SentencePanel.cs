using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Utilities.Constants;
using Utilities.Enums;
using Unity.VisualScripting;

public class SentencePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Image gridBackground;

    [Header("Values")]
    [SerializeField] private bool resize = false;
    public SentenceType type;

    public List<Letter> letters { get; private set; } = new();

    private RectTransform gridLayoutRectTransform;

    private const int MaxLetterCount = 10;
    private readonly Vector2 gridCellSizeOffset = new Vector2(15, 15);

    private int currentLength;

    private void Awake()
    {
        gridLayoutRectTransform = gridLayout.GetComponent<RectTransform>();

        for(int i = 0;i < transform.childCount;i++)
            letters.Add(transform.GetChild(i).GetComponent<Letter>());
    }

    public void SetSentence(int length, List<int> spaces = null)
    {
        for (int i = 0;i < currentLength;++i) 
        { 
            letters[i].ResetLetter();
            letters[i].gameObject.SetActive(false);
        }

        currentLength = length;

        for (int i = 0;i < currentLength;++i)
        {
            letters[i].gameObject.SetActive(true);
            letters[i].SetVisualsActive(true);
        }

        if(spaces != null)
        {
            for (int i = 0;i < spaces.Count;++i)
            {
                letters[spaces[i]].SetVisualsActive(false);
                letters[spaces[i]].SetLetter(' ');
            }
        }

        ResizeGrid();
    }

    private void ResizeGrid()
    {
        if (!resize)
            return;

        Vector2 letterSize = Constants.LetterStandardSize;

        if (currentLength > MaxLetterCount)
            letterSize *= (float)MaxLetterCount / currentLength;

        gridLayout.cellSize = letterSize;
        if (gridLayoutRectTransform == null)
            print(name);
        gridLayoutRectTransform.sizeDelta = new Vector2(currentLength * letterSize.x + gridCellSizeOffset.x * 2, letterSize.y + gridCellSizeOffset.y * 2);
    }

}

