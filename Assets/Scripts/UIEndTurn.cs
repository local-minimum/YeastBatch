using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEndTurn : MonoBehaviour {

    [SerializeField] Button actionButton;

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
        if (player.isLocalPlayer && player.playerType == PlayerType.Human)
        {
            actionButton.interactable = true;
        } else
        {
            //TODO: Handle effects of game mode such as hotseat or online
            actionButton.interactable = false;
        }
    }

    [SerializeField] Plate plate;

    public void EndTurn()
    {
        actionButton.interactable = false;
        plate.EndTurn();
    }
}
