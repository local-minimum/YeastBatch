using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopAction : MonoBehaviour {

    [SerializeField]
    GameObject _view;
    static bool _showing;

    public static bool Showing {
        get { return _showing; }
    }

    static UIPopAction _instance;

    static UIPopAction instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIPopAction>();
            }
            return _instance;
        }
    }

    public static void ShowFor(Tile tile)
    {
        UITileStatus.ShowFor(tile);
        _instance._view.SetActive(true);
        _instance._editingTile = tile;
        _instance._editingPop = tile.GetPlayerPopulation(Match.ActivePlayer);
        _instance.UpdateSliders();
        _showing = true;
    }

    public void MaxOut()
    {
        float factor = 0;
        foreach (Slider s in metabolismSliders)
        {
            factor += s.value;        
        }

        if (factor > 0)
        {
            factor = 1f / factor;
            
            foreach (Slider s in metabolismSliders)
            {
                s.value *= factor;
            }
        } else
        {
            factor = 1f / (float) metabolismSliders.Length;
            foreach (Slider s in metabolismSliders)
            {
                s.value = factor;
            }
        }
    }

    public void HideView()
    {
        _view.SetActive(false);
        _editingPop = null;
        _editingTile = null;
        _showing = false;
    }

    public void LetsGetItOn()
    {
        _editingPop.activeAction = ActionMode.Procreation;
        _editingTile.SetPlayerAction(Match.ActivePlayer, PlayerAction.Population);
        _editingTile.ShowSelectedPopAction(Match.ActivePlayer);
        HideView();
    }

    public void EatAndLive()
    {
        _editingPop.activeAction = ActionMode.Metabolism;
        _editingTile.SetPlayerAction(Match.ActivePlayer, PlayerAction.Population);
        _editingTile.ShowSelectedPopAction(Match.ActivePlayer);
        HideView();
    }

    bool updatingSliders;
    public void UpdateSlider(Slider me)
    {

        MetabolismSlider mSlider = me.GetComponent<MetabolismSlider>();
        int requested = Mathf.FloorToInt(_editingPop.TotalEnergy * me.value);
        float updateSlideValue = me.value;

        int allowed = GetAllowance(mSlider, requested);
        if (allowed < requested)
        {
            updateSlideValue = (float)allowed / _editingPop.TotalEnergy;
            me.value = updateSlideValue;
        }

        //Debug.Log(string.Format("{0} {1} {2} {3}", requested, allowed, updateSlideValue, _editionPop.TotalEnergy));

        if (!updatingSliders)
        {
            UpdateSliders();
        }
    }

    [SerializeField]
    Slider[] metabolismSliders;

    void UpdateSliders()
    {
        updatingSliders = true;
        float total = _editingPop.TotalEnergy;
        foreach (Slider s in metabolismSliders)
        {
            var ms = s.GetComponent<MetabolismSlider>();
            s.value = _editingPop.GetPlanningCostOf(ms.key) / total;
        }
        updatingSliders = false;
    }

    int GetAllowance(MetabolismSlider mSlider, int requested)
    {

        if (mSlider.key == "Export")
        {
            return _editingPop.PlanExportWaste(requested, PlayerPopulation.ClampMode.Others);
        }
        else if (mSlider.key == "ImportC")
        {
            return _editingPop.PlanImport(Nutrients.C, requested, PlayerPopulation.ClampMode.Others);
        }
        else if (mSlider.key == "ImportN")
        {
            return _editingPop.PlanImport(Nutrients.N, requested, PlayerPopulation.ClampMode.Others);
        }
        else if (mSlider.key == "ImportAA")
        {
            return _editingPop.PlanImport(Nutrients.AA, requested, PlayerPopulation.ClampMode.Others);
        }
        else if (mSlider.key == "Maintenance")
        {
            return _editingPop.PlanMaintenance(requested, PlayerPopulation.ClampMode.Others);
        }
        else
        {
            throw new System.ArgumentException(string.Format("{0} not recognized action.", mSlider.key));
        }
    }

    Tile _editingTile;
    PlayerPopulation _editingPop;

    private void Start()
    {
        _instance = this;
        HideView();
    }
}
