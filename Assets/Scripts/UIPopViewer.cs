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
        _viewer.popSize.text = pop.Size.ToString();
        _viewer.aaNutr.text = pop.GetNutrientState(Nutrients.AA).ToString();
        _viewer.cNutr.text = pop.GetNutrientState(Nutrients.C).ToString();
        _viewer.nNutr.text = pop.GetNutrientState(Nutrients.N).ToString();
        _viewer.waste.text = pop.GetWasteState().ToString();
        _viewer.damage.text = pop.GetDamageState().ToString();
        _viewer.energy.text = pop.GetEnergyState().ToString();
    }

    static UIPopViewer _viewer;


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
