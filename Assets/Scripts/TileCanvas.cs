using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileCanvas : MonoBehaviour {

    public static void ShowFor(Tile tile)
    {
        //TODO: Fix so matches current player
        int playerId = 0;

        if (tile.HasPopulation(playerId))
        {
            _canvas.transform.position = new Vector3(
                tile.transform.position.x,
                tile.transform.position.y,
                tile.transform.position.z + zOffset);
            _canvas._editingTile = tile;
            _canvas._editingPop = tile.GetPlayerPopulation(playerId);
            _canvas.gameObject.SetActive(true);
            _canvas.UpdateSliders();
        }
    }

    [SerializeField]
    static float zOffset = 5;

    static TileCanvas _canvas;

    private void OnEnable()
    {
        _canvas = this;    
    }

    private void OnDestroy()
    {
        _canvas = null;    
    }

    [SerializeField]
    Button[] buttons;

    [SerializeField]
    GameObject[] helpTexts;

    [SerializeField]
    GameObject[] controls;

    [SerializeField]
    Slider[] metabolismSliders;

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

    public void LetsGetItOn()
    {
        _editingPop.activeAction = ActionMode.Procreation;
        HideCanvas();
    }

    public void EatAndLive()
    {
        _editingPop.activeAction = ActionMode.Metabolism;
        HideCanvas();
    }

    void HideCanvas()
    {
        gameObject.SetActive(false);
        _editingPop = null;
        _editingTile = null;
    }

    Tile _editingTile;
    PlayerPopulation _editingPop;

    public void UpdateSlider(Slider me)
    {
        MetabolismSlider mSlider = me.GetComponent<MetabolismSlider>();
        int requested = Mathf.FloorToInt(_editingPop.TotalEnergy * me.value);
        float updateSlideValue = me.value;

        int allowed = GetAllowance(mSlider, requested);
        if (allowed < requested)
        {
            updateSlideValue = (float) allowed / _editingPop.TotalEnergy;
            me.value = updateSlideValue;
        }

        //Debug.Log(string.Format("{0} {1} {2} {3}", requested, allowed, updateSlideValue, _editionPop.TotalEnergy));

        UpdateSliders();
    }

    void UpdateSliders()
    {
        float total = _editingPop.TotalEnergy;
        foreach (Slider s in metabolismSliders)
        {
            var ms = s.GetComponent<MetabolismSlider>();
            s.value = _editingPop.GetPlanningCostOf(ms.key) / total;
        }
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

    public void ShowSliderTooltip(Slider me)
    {
        MetabolismSlider mSlider = me.GetComponent<MetabolismSlider>();
        //mSlider.tooltip;
    }
}
