using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileCanvas : MonoBehaviour {

    [SerializeField]
    Button[] buttons;

    [SerializeField]
    GameObject[] helpTexts;

    [SerializeField]
    GameObject[] controls;

    int activeControl = -1;

    int GetButtonIndex(Button button)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == button)
            {
                return i;
            }
        }
        throw new System.ArgumentException("Button not known: " + button);
    }

    public void ShowHoverText(Button button)
    {
        int idx = GetButtonIndex(button);
        ShowHoverText(idx);       
    }

    void ShowHoverText(int idx)
    {
        for (int i=0; i<helpTexts.Length; i++)
        {
            helpTexts[i].SetActive((i == idx) && (i != activeControl));
        }
    }

    public void HideHelpTexts()
    {
        for (int i = 0; i < helpTexts.Length; i++)
        {
            helpTexts[i].SetActive(false);
        }

    }

    public void HideControls()
    {
        for (int i = 0; i<controls.Length; i++)
        {
            controls[i].SetActive(false);
        }
        activeControl = -1;
    }

    public void SetActiveControl(Button button)
    {
        int idx = GetButtonIndex(button);
        SetActiveControl(idx);
    }

    public void SetActiveControl(int index)
    {
        for (int i = 0; i < controls.Length; i++)
        {
            if (i == index)
            {
                activeControl = i;
                controls[i].SetActive(true);
                HideHelpTexts();
            } else
            {
                controls[i].SetActive(false);
            }
        }
    }

    private void Start()
    {
        HideHelpTexts();
        HideControls();        
    }
}
