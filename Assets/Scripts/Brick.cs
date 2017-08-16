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

    private void OnMouseEnter()
    {
        TileCanvas.ShowFor(tile);
    }

    private void OnMouseExit()
    {
        
    }

    private void OnEnable()
    {
        Match.OnPlayerTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        Match.OnPlayerTurn -= HandlePlayerTurn;
    }

    private void OnDestroy()
    {
        Match.OnPlayerTurn -= HandlePlayerTurn;
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
}
