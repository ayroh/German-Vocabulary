using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Enums;

public class ArtikelButtonController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private List<Button> artikelButtons;
    [SerializeField] private AddAnswerPanelController addAnswerPanelController;

    private ColorBlock standardColorBlock;
    private ColorBlock pressedColorBlock;


    private void Awake()
    {
        standardColorBlock = artikelButtons[0].colors;
        pressedColorBlock = artikelButtons[0].colors;
        pressedColorBlock.normalColor = pressedColorBlock.pressedColor;

        PressButton((int)ArtikelType.der);
    }

    public void PressButton(int artikelTypeInt)
    {
        ArtikelType artikelType = (ArtikelType)artikelTypeInt;

        for (int i = 0;i < artikelButtons.Count;i++)
            artikelButtons[i].colors = standardColorBlock;

        artikelButtons[artikelTypeInt].colors = pressedColorBlock;

        addAnswerPanelController.artikelInputField = artikelType;
    }
}
