﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour {

    public void SetPlayerDominion(int player, float dominon)
    {

    }

    [SerializeField] SpriteRenderer popRend;

    Tile tile;

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
    }

    bool overMe = false;

    private void OnMouseEnter()
    {
        overMe = true;
    }

    private void OnMouseExit()
    {
        overMe = false;
    }

    private void OnEnable()
    {
        Match.OnPlayerTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        Match.OnPlayerTurn -= HandlePlayerTurn;
        if (ButtonDown == this)
        {
            ButtonDown = null;
        }
    }

    private void OnDestroy()
    {
        Match.OnPlayerTurn -= HandlePlayerTurn;
        if (ButtonDown == this)
        {
            ButtonDown = null;
        }
    }

    private void HandlePlayerTurn(Player player, int playerId)
    {
        if (tile.HasPopulation(playerId))
        {
            popRend.color = player.playerColor;
        } else
        {
            popRend.color = new Color(0, 0, 0, 0);
        }
        tile.ShowSelectedAction(playerId);
    }

    [SerializeField] GameObject modeAnim;

    public void ShowAction(ActionMode mode)
    {
        modeAnim.GetComponent<Animator>().SetTrigger(System.Enum.GetName(typeof(ActionMode), mode));
    }

    static Brick ButtonDown;
    static float buttonDownTime;
    float pressForDiffusion = 1.5f;

    private void Update()
    {
        //TODO: OverMe propagates through UIs
        if (overMe)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ButtonDown = this;
                buttonDownTime = Time.timeSinceLevelLoad;
            }

            if (Input.GetMouseButtonUp(0))
            {
                ButtonDown.IllustrateSelection(this, true);
                ButtonDown.SetTileAction(this);                

            } else if (Input.GetMouseButton(0))
            {
                ButtonDown.IllustrateSelection(this, false);
                
            } else if (Input.GetMouseButtonDown(1))
            {
                ButtonDown = this;
            } else if (Input.GetMouseButtonUp(1))
            {
                if (ButtonDown == this)
                {
                    tile.ClearPlan();
                    ClearPlanIllustration();
                }
                ButtonDown = null;
            }
        }
    }

    private void ClearPlanIllustration()
    {
        Animator anim = modeAnim.GetComponent<Animator>();
        anim.SetTrigger("None");        

    }

    TileActions prevAction = TileActions.None;
    private void IllustrateSelection(Brick target, bool selectionDone)
    {
        TileActions current = GetTileAction(target, selectionDone);
        if (current == prevAction)
        {
            return;
        }

        Animator anim = modeAnim.GetComponent<Animator>();
        switch (current)
        {
            case TileActions.None:
                //TODO: This need to revert if not performed or something
                anim.SetTrigger("None");
                break;
            case TileActions.NotYetDiffusion:
                //TODO: Should fade in or something
            case TileActions.Diffusion:
                anim.SetTrigger("Diffusion");
                break;
            case TileActions.Migration:
                anim.SetTrigger("None");
                //TODO: Should illustrate thingy to target
                break;
        }

        prevAction = current;
    }

    enum TileActions {None, Diffusion, Migration, NotYetDiffusion};

    private TileActions GetTileAction(Brick other, bool selectionDone)
    {
        if (other == null)
        {
            return TileActions.None;
        } else if (this == other)
        {
            return TileActions.Diffusion;
        } else if (Time.timeSinceLevelLoad - buttonDownTime > pressForDiffusion)
        {
            return TileActions.Migration;
        } else if (selectionDone)
        {
            return TileActions.None;
        }
        {
            return TileActions.NotYetDiffusion;
        }
    }

    void SetTileAction(Brick other)
    {
        //TODO: Needs to be plans rather than actions directly and needs negating pop actions
        switch (GetTileAction(other, true))
        {
            case TileActions.Diffusion:
                tile.PlanDiffusion();
                modeAnim.GetComponent<Animator>().SetTrigger("Diffusion");
                break;
            case TileActions.Migration:
                tile.PlanMigration(other.tile);
                break;
        }
    }
}
