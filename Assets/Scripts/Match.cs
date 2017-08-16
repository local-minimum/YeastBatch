using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerTurn(Player player, int pId);

public class Match : MonoBehaviour {

    public static PlayerTurn OnPlayerTurn;

    static Match _match;

    public static void EndTurn()
    {
        _match._EndTurn();
    }

    public static int ActivePlayer
    {
        get
        {
            return _match.activePlayer;
        }
    }

    //TODO: Implement game modes

    int activePlayer;
    Player[] players;
    int nPlayers;

    [SerializeField]
    Board board;

    private void Start()
    {
        _match = this;
        players = Player.GetPlayers();
        nPlayers = players.Length;
        board.SetupGame(nPlayers);
        SetPlayerTurn(activePlayer);
        //TODO: why not do this: board.InitiateBatch(nPlayers);
    }

    void _EndTurn()
    {
        activePlayer++;
        if (activePlayer < nPlayers)
        {
            SetPlayerTurn(activePlayer);
        }
        else
        {
            //TODO: Run through actions
            activePlayer = 0;
        }
    }

    void SetPlayerTurn(int pId)
    {
        if (OnPlayerTurn != null)
        {
            OnPlayerTurn(players[activePlayer], activePlayer);

        }
    }

}
