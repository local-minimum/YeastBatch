using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour {

    [SerializeField] SpriteRenderer popRend;

    Tile tile;
    SpriteRenderer sRend;

    private void Start()
    {
        anim = modeAnim.GetComponent<Animator>();
        tile = GetComponentInParent<Tile>();
        sRend = GetComponent<SpriteRenderer>();
    }

    private void OnMouseEnter()
    {
        if (UIPopAction.Showing)
        {
            return;
        }
        sRend.color = new Color(0.8f, .6f, 0.5f);
        UITileStatus.ShowFor(tile);
        UIPopViewer.ShowPop(tile.GetPlayerPopulation(Match.ActivePlayer));
        HoverBrick = this;
    }

    bool clearBrickWithDelay;
    float clearTime;

    private void OnMouseExit()
    {

        sRend.color = new Color(1f, 1f, 1f);
        clearBrickWithDelay = true;
        clearTime = Time.timeSinceLevelLoad + 0.5f;
        
        if (HoverBrick == this)
        {
            HoverBrick = null;
        }
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
        if (HoverBrick == this)
        {
            HoverBrick = null;
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
        if (HoverBrick == this)
        {
            HoverBrick = null;
        }
        UITileStatus.Clear(tile);
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
            popRend.gameObject.SetActive(false);
        }

        tile.ShowSelectedPopAction(Match.ActivePlayer);
    }

    [SerializeField] GameObject modeAnim;

    public void ShowPopAction(ActionMode mode)
    {
        modeAnim.GetComponent<Animator>().SetTrigger(System.Enum.GetName(typeof(ActionMode), mode));
        arrow.Hide();
    }

    static Brick HoverBrick;
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
        if (UIPopAction.Showing)
        {
            return;
        }

        if (HoverBrick == this)
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
        }

        if (clearBrickWithDelay)
        {
            if (HoverBrick == null)
            {
                if (Time.timeSinceLevelLoad > clearTime)
                {
                    UITileStatus.Clear(tile);
                    clearBrickWithDelay = false;
                }
            } else
            {
                clearBrickWithDelay = false;
            }
         }
    }

    public void ClearPlanIllustration()
    {
        anim.SetTrigger("None");
        arrow.Hide();

    }

    public void ShowMigration(Tile target)
    {
        anim.SetTrigger("None");
        arrow.ShowMigration(target.transform);
    }

    public void ShowDiffusion()
    {
        anim.SetTrigger("Diffusion");
        arrow.Hide();

    }

    [SerializeField] UIMigrationArrow arrow;

    TileActions prevAction = TileActions.None;

    Animator anim;

    private void IllustrateSelection(Brick target, bool selectionDone)
    {
        TileActions current = GetTileAction(target, selectionDone);

        if (current == prevAction)
        {
            return;
        }

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
                if (LeftSelectBrick.tile.IsNeighbour(target.tile))
                {
                    anim.SetTrigger("None");
                    LeftSelectBrick.arrow.ShowMigration(target.transform);
                }
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
