using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextGroup : MonoBehaviour {

    [SerializeField]
    Text[] items;

    Vector2 screenRes;

	void Update () {
		if (needRefit)
        {
            Refit();
        } else if (updateFit)
        {
            UpdateFit();
        }
	}

    bool needRefit
    {
        get
        {
            return screenRes != new Vector2(Screen.width, Screen.height);
        }
    }

    bool updateFit;
    int size;

    void Refit()
    {        
        for (int i = 0; i < items.Length; i++)
        {
            items[i].resizeTextForBestFit = true;
        }
        updateFit = true;

        screenRes = new Vector2(Screen.width, Screen.height);
    }
    
    void UpdateFit()
    {
        size = -1;
        for (int i = 0; i < items.Length; i++)
        {
            int cur = items[i].fontSize;
            if (size < 0 || cur < size)
            {
                size = cur;
            }
        }

        //Debug.Log("Fitting size " + size);

        for (int i = 0; i < items.Length; i++)
        {
            items[i].resizeTextForBestFit = false;
            items[i].fontSize = size + 3;
        }
        updateFit = false;
    }
}
