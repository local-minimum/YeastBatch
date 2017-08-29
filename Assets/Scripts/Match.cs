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

    [SerializeField]
    UIWinMeters winMeters;

    [SerializeField]
    GameObject ui;

    private void Start()
    {
        ui.SetActive(true);
        _match = this;
        players = Player.GetPlayers();
        totalPlayers = players.Length;
        board.SetupGame(totalPlayers);
        board.SetMediaComposition();
        board.CastMedia();
        SetPlayerTurn(activePlayer);
        winMeters.UpdateWinner(board);
        winMeters.SetPlayerColors(players[0].playerColor, players[1].playerColor);
        board.SetDominionColors(players[0].playerColor, players[1].playerColor);
        board.RebalanceMetabolism();
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

            board.RebalanceMetabolism();
            board.SetDominionColors(players[0].playerColor, players[1].playerColor);
            activePlayer = 0;
            Winner winner = winMeters.UpdateWinner(board);
            if (winner != Winner.None)
            {
                UIWinner.SetWinner(winner);
            } else
            {
                SetPlayerTurn(activePlayer);

            }
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
