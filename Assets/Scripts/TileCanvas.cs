﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileCanvas : MonoBehaviour {

    public static bool Showing {
        get
        {
            return _showing && !_canvas.isDelayHiding;
        }
    }
    static bool _showing;

    public static void ShowFor(Tile tile)
    {
        int playerId = Match.ActivePlayer;

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
            _canvas.HideHelpTexts();
            _canvas.HideControls();
            _showing = true;
        }
    }

    public static void HideIfNotHovered()
    {
        if (!hovered)
        {
            _canvas.DelayHiding();
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
        for (int i = 0; i < helpTexts.Length; i++)
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
        for (int i = 0; i < controls.Length; i++)
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
        HideCanvas();
    }

    public void LetsGetItOn()
    {
        _editingPop.activeAction = ActionMode.Procreation;
        _editingTile.SetPlayerAction(Match.ActivePlayer, PlayerAction.Population);
        _editingTile.ShowSelectedPopAction(Match.ActivePlayer);
        HideCanvas();
    }

    public void EatAndLive()
    {
        _editingPop.activeAction = ActionMode.Metabolism;
        _editingTile.SetPlayerAction(Match.ActivePlayer, PlayerAction.Population);
        _editingTile.ShowSelectedPopAction(Match.ActivePlayer);
        HideCanvas();
    }

    public void DelayHiding() {
        if (!isDelayHiding && gameObject.activeSelf)
        {
            StartCoroutine(_DelayHiding());
        }
    }

    public void AbortDelayHiding()
    {
        isDelayHiding = false; 
    }

    bool isDelayHiding = false;
    IEnumerator<WaitForSeconds> _DelayHiding() {
        isDelayHiding = true;
        yield return new WaitForSeconds(0.5f);
        if (isDelayHiding)
        {
            HideCanvas();

            isDelayHiding = false;
        }
    }

    public void HideCanvas()
    {
        gameObject.SetActive(false);
        if (_editingTile)
        {
            Brick.RemoveLeftSelect(_editingTile.brick);
        }
        _editingPop = null;
        _editingTile = null;
        _showing = false;
        UIPopViewer.ClearPop();
    }

    Tile _editingTile;
    PlayerPopulation _editingPop;

    static bool updatingSliders;
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

        if (!updatingSliders)
        {
            UpdateSliders();
        }
    }

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

    static bool hovered;

    private void OnMouseOver()
    {
        if (isDelayHiding)
        {
            isDelayHiding = false;
        }
        hovered = true;
    }

    public void CanvasEnter()
    {        
        hovered = true;
    }

    public void CanvasExit()
    {
        hovered = false;
        DelayHiding();
    }
}
