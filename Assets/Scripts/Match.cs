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

    public static int TotalPlayers
    {
        get
        {
            return _match.totalPlayers;
        }
    }

    //TODO: Implement game modes

    int activePlayer;
    Player[] players;
    int totalPlayers;

    [SerializeField]
    Board board;

    private void Start()
    {
        _match = this;
        players = Player.GetPlayers();
        totalPlayers = players.Length;
        board.SetupGame(totalPlayers);
        board.SetMediaComposition();
        board.CastMedia();
        SetPlayerTurn(activePlayer);
        
    }

    void _EndTurn()
    {
        activePlayer++;
        if (activePlayer < totalPlayers)
        {
            SetPlayerTurn(activePlayer);
        }
        else
        {
            board.EnactMetabolism();
            board.EnactProcreation();
            board.EnactMigration();
            board.EnactDiffusion();

            activePlayer = 0;
            SetPlayerTurn(activePlayer);
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
