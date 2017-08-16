using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType { Human, Computer};

public class Player : MonoBehaviour {

    public static Player[] GetPlayers()
    {
        return FindObjectOfType<Player>().transform.GetComponents<Player>();
    }

    public PlayerType playerType;
    public Color playerColor;
    public string playerName;
    public bool isLocalPlayer;
    public string GetName()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            if (playerType == PlayerType.Computer)
            {
                playerName = GetAComputerName();
            } else
            {
                playerName = GetBoringPlayerName();
            }
        }
        return playerName;
    }

    public string GetAComputerName() {
        var options = new string[] {"hog1", "pbs2", "tor1", "ste12", "msn4", "snf1", "yak8"};
        return options[Random.Range(0, options.Length)];
    }

    public string GetBoringPlayerName()
    {
        var players = transform.GetComponents<Player>();
        for (int i=0; i < players.Length; i++)
        {
            if (players[i] == this)
            {
                return string.Format("Player {0}", i + 1);
            }
        }
        return "Player X";
    }
}
