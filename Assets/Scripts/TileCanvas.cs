using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCanvas : MonoBehaviour {

    [SerializeField]
    GameObject[] helpTexts;

    [SerializeField]
    GameObject[] controls;

    int activeControl = -1;

    public void ShowHoverText(GameObject help)
    {
        for (int i=0; i<helpTexts.Length; i++)
        {
            bool isThis = helpTexts[i] == help;
            helpTexts[i].SetActive(isThis && (i != activeControl));
        }
    }

    public void HideHelpTexts()
    {
        for (int i = 0; i < helpTexts.Length; i++)
        {
            helpTexts[i].SetActive(false);
        }

    }

    public void SetActiveControl(GameObject control)
    {
        for (int i = 0; i < controls.Length; i++)
        {
            if (controls[i] == control)
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
}
