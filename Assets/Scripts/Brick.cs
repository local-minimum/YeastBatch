using System.Collections;
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
        if (LeftSelectBrick == this)
        {
            LeftSelectBrick = null;
        }
        if (RightSelectBrick == this)
        {
            RightSelectBrick = null;
        }
    }

    private void OnDestroy()
    {
        Match.OnPlayerTurn -= HandlePlayerTurn;
        if (LeftSelectBrick == this)
        {
            LeftSelectBrick = null;
        }
        if (RightSelectBrick == this)
        {
            RightSelectBrick = null;
        }
    }

    private void HandlePlayerTurn(Player player, int playerId)
    {
        if (tile.HasPopulation(playerId))
        {
            popRend.color = player.playerColor;
            popRend.gameObject.SetActive(true);
        }
        else
        {
            //popRend.color = new Color(0, 0, 0, 0);
            popRend.gameObject.SetActive(false);
        }
        //TODO: Use exactly what was planned, not only player action
        //tile.ShowSelectedPopAction(playerId);
    }

    [SerializeField] GameObject modeAnim;

    public void ShowPopAction(ActionMode mode)
    {
        modeAnim.GetComponent<Animator>().SetTrigger(System.Enum.GetName(typeof(ActionMode), mode));
        arrow.Hide();
    }

    static Brick LeftSelectBrick;
    static Brick RightSelectBrick;
    static float buttonDownTime;
    static float pressForDiffusion = 0.5f;

    public static void SetLeftSelect(Brick brick)
    {
        //Debug.Log("Select " + brick);
        LeftSelectBrick = brick;
    }

    public static void RemoveLeftSelect(Brick brick)
    {
        if (brick == LeftSelectBrick)
        {
            LeftSelectBrick = null;
            //Debug.Log("UnSelect " + brick);
        }
    }

    private void Update()
    {
        //TODO: OverMe propagates through UIs
        if (overMe && !TileCanvas.Showing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LeftSelectBrick = this;
                buttonDownTime = Time.timeSinceLevelLoad;                
            }

            if (LeftSelectBrick)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    LeftSelectBrick.IllustrateSelection(this, true);
                    LeftSelectBrick.SetTileAction(this);

                }
                else if (Input.GetMouseButton(0))
                {
                    LeftSelectBrick.IllustrateSelection(this, false);

                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                RightSelectBrick = this;
                Debug.Log("Right Select " + RightSelectBrick);
            } else if (RightSelectBrick) {
                if (Input.GetMouseButtonUp(1))
                {
                    Debug.Log("Right Select Complete " + RightSelectBrick);
                    if (RightSelectBrick == this)
                    {
                        Debug.Log("Clear");
                        tile.ClearPlan();
                        ClearPlanIllustration();
                    }
                    RightSelectBrick = null;
                }
            }
        } else if (TileCanvas.Showing && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            TileCanvas.HideIfNotHovered();
        }
    }

    private void ClearPlanIllustration()
    {
        Animator anim = modeAnim.GetComponent<Animator>();
        anim.SetTrigger("None");
        arrow.Hide();

    }

    [SerializeField] UIMigrationArrow arrow;

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
                var resetAction = tile.GetPlayerAction(Match.ActivePlayer);
                if (resetAction == PlayerAction.None)
                {
                    anim.SetTrigger("None");
                    LeftSelectBrick.arrow.Hide();
                }
                else if (resetAction == PlayerAction.Migration)
                {
                    LeftSelectBrick.arrow.ReShow();
                    anim.SetTrigger("None");
                } else if (resetAction == PlayerAction.Diffusion)
                {
                    anim.SetTrigger("Diffusion");
                    LeftSelectBrick.arrow.Hide();
                } else
                {
                    LeftSelectBrick.arrow.Hide();
                    tile.ShowSelectedPopAction(Match.ActivePlayer);
                }
                
                break;
            case TileActions.NotYetDiffusion:
                //TODO: Should fade in or something
            case TileActions.Diffusion:
                LeftSelectBrick.arrow.Hide();
                anim.SetTrigger("Diffusion");
                break;
            case TileActions.Migration:
                anim.SetTrigger("None");
                LeftSelectBrick.arrow.ShowMigration(target.transform);
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
            if (Time.timeSinceLevelLoad - buttonDownTime > pressForDiffusion)
            {
                return TileActions.Diffusion;
            } else if (selectionDone)
            {
                return TileActions.None;
            } else
            {
                return TileActions.NotYetDiffusion;
            }
        } else 
        {
            return TileActions.Migration;
        }
    }

    void SetTileAction(Brick other)
    {
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
