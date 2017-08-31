using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DominionData
{
    public float playerOne;
    public float playerTwo;

    public DominionData(float playerOne, float playerTwo)
    {
        this.playerOne = playerOne;
        this.playerTwo = playerTwo;
    }
}

public struct DominionStatus
{
    public DominionData tiles;
    public DominionData population;

    public DominionStatus(float tilesPlayerOne, float tilesPlayerTwo, float popPlayerOne, float popPlayerTwo)
    {
        tiles = new DominionData(tilesPlayerOne, tilesPlayerTwo);
        population = new DominionData(popPlayerOne, popPlayerTwo);
    }
}

public class UIWinMeters : MonoBehaviour {

    [SerializeField]
    UIWinMeter popSize;

    [SerializeField]
    UIWinMeter tileDominion;

    public void SetPlayerColors(Color playerOne, Color playerTwo)
    {
        popSize.SetPlayerColors(playerOne, playerTwo);
        tileDominion.SetPlayerColors(playerOne, playerTwo);
    }

    public Winner UpdateWinner(Board board)
    {
        Winner popSizeWinner = UpdateAndGetPopSizeWinner(board);
        Winner tileDominonWinner = UpdateAndGetTileDominionWinner(board);

        if (popSizeWinner != Winner.None && (tileDominonWinner == popSizeWinner || tileDominonWinner == Winner.None))
        {
            return popSizeWinner;
        } else if (tileDominonWinner != Winner.None && (tileDominonWinner == popSizeWinner || popSizeWinner == Winner.None)) {
            return tileDominonWinner;
        }
        return Winner.None;
    }

    public DominionStatus GetDominions()
    {
        return new DominionStatus(tileDominion.PlayerOne, tileDominion.PlayerTwo, popSize.PlayerOne, popSize.PlayerTwo);
    }

    Winner UpdateAndGetPopSizeWinner(Board board)
    {
        int totalSize;
        int[] popSizes = board.TotalPlayerPopulations(out totalSize, Match.TotalPlayers);
        popSize.Show(popSizes[0], popSizes[1]);
        return popSize.GetWinner();
    }

    [SerializeField]
    int minPopForDominion = 10;

    Winner UpdateAndGetTileDominionWinner(Board board)
    {
        int[,] pops = board.GetPlayerPopulationsPerTile();
        int tiles = pops.GetLength(0);
        int players = pops.GetLength(1);
        int[] dominions = new int[players];

        for (int tile=0; tile<tiles; tile++)
        {
            int idBest = -1;
            int best = -1;

            for (int player = 0; player<players; player++)
            {
                if (pops[tile, player] > best)
                {
                    best = pops[tile, player];
                    if (best > minPopForDominion && idBest > -2)
                    {
                        idBest = player;
                    }
                } else if (pops[tile, player] == best)
                {
                    idBest = -2;
                }
            }

            if (idBest > -1)
            {
                dominions[idBest]++;
            }
        }

        tileDominion.Show(dominions[0], dominions[1]);
        return tileDominion.GetWinner();
    }
}
