using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopViewer : MonoBehaviour {

    [SerializeField]
    Text popSize;

    [SerializeField]
    Text aaNutr;

    [SerializeField]
    Text cNutr;

    [SerializeField]
    Text nNutr;

    [SerializeField]
    Text waste;

    [SerializeField]
    Text damage;

    [SerializeField]
    Text energy;

    public static void ShowPop(PlayerPopulation pop)
    {
        var v = viewer;
        v.popSize.text = pop.Size.ToString();
        v.aaNutr.text = pop.GetNutrientState(Nutrients.AA).ToString();
        v.cNutr.text = pop.GetNutrientState(Nutrients.C).ToString();
        v.nNutr.text = pop.GetNutrientState(Nutrients.N).ToString();
        v.waste.text = pop.GetWasteState().ToString();
        v.damage.text = pop.GetDamageState().ToString();
        v.energy.text = pop.GetEnergyState().ToString();
    }

    public static void ClearPop()
    {
        var v = viewer;
        v.popSize.text = "---";
        v.aaNutr.text = "---";
        v.cNutr.text = "---";
        v.nNutr.text = "---";
        v.waste.text = "---";
        v.damage.text = "---";
        v.energy.text = "---";
    }

    static UIPopViewer _viewer;

    static UIPopViewer viewer
    {
        get
        {
            if (_viewer == null)
            {
                _viewer = FindObjectOfType<UIPopViewer>();
            }
            return _viewer;
        }
    }

	void Start () {
        _viewer = this;		
	}

    private void OnDisable()
    {
        _viewer = null;
    }

    private void OnDestroy()
    {
        _viewer = null;
    }

    void Update () {
		
	}
}
