using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour {

    [SerializeField] Text playerNameUI;
    [SerializeField] Image playerColorUI;

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
        playerNameUI.text = player.GetName();
        playerNameUI.fontStyle = player.playerType == PlayerType.Computer ? FontStyle.Italic : FontStyle.Normal;
        playerColorUI.color = player.playerColor;
    }
}

