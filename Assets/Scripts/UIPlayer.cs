using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour {

    Text playerNameUI;
    Image playerColorUI;

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
        playerNameUI.text = player.GetName();
        playerNameUI.fontStyle = player.playerType == PlayerType.Computer ? FontStyle.Italic : FontStyle.Normal;
        playerColorUI.color = player.playerColor;
    }
}

