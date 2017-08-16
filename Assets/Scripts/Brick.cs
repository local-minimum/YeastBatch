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
        Plate.OnPlayerTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        Plate.OnPlayerTurn -= HandlePlayerTurn;
    }

    private void OnDestroy()
    {
        Plate.OnPlayerTurn -= HandlePlayerTurn;
    }

    private void HandlePlayerTurn(Player player, int playerId)
    {
        popRend.color = player.playerColor;
    }
    }
