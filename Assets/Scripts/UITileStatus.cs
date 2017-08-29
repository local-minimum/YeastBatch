using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITileStatus : MonoBehaviour {

    [SerializeField]
    Text aaText;

    [SerializeField]
    Text nText;

    [SerializeField]
    Text cText;

    [SerializeField]
    Image cProgress;

    [SerializeField]
    Image aaProgress;

    [SerializeField]
    Image nProgress;

    public static void ShowFor(Tile tile)
    {
        _instance.cText.text = tile.nutrientState.GetNutrientState(Nutrients.C).ToString();
        _instance.nText.text = tile.nutrientState.GetNutrientState(Nutrients.N).ToString();
        _instance.aaText.text = tile.nutrientState.GetNutrientState(Nutrients.AA).ToString();

        _instance.aaProgress.fillAmount = tile.nutrientState.GetSaturation(Nutrients.AA);
        _instance.cProgress.fillAmount = tile.nutrientState.GetSaturation(Nutrients.N);
        _instance.nProgress.fillAmount = tile.nutrientState.GetSaturation(Nutrients.C);

        _instance.aaProgress.transform.parent.gameObject.SetActive(true);
        _instance.nProgress.transform.parent.gameObject.SetActive(true);
        _instance.cProgress.transform.parent.gameObject.SetActive(true);

        _tile = tile;
    }

    public static void Clear(Tile tile)
    {
        if (_tile == tile)
        {
            Clear();
        }
    }

    public static void Clear()
    {
        _instance.cText.text = "---";
        _instance.nText.text = "---";
        _instance.aaText.text = "---";

        _instance.aaProgress.transform.parent.gameObject.SetActive(false);
        _instance.nProgress.transform.parent.gameObject.SetActive(false);
        _instance.cProgress.transform.parent.gameObject.SetActive(false);

        _tile = null;
    }

    static UITileStatus _instance;
    static Tile _tile;

	void Start () {
        _instance = this;
        Clear();
	}

    private void OnDestroy()
    {
        _instance = null;
        _tile = null;
    }

    private void OnDisable()
    {
        _instance = null;
        _tile = null;
    }
}
