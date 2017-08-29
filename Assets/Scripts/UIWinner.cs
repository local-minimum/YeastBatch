using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIWinner : MonoBehaviour {

    static UIWinner _instance;

    public static UIWinner instance
    {
        get
        {            
            return _instance;
        }
    }

    public static void SetWinner(Winner winner)
    {
        Player player = GetWinner(winner);
        if (player == null)
        {
            return;
        }
        instance.winnerTxt.text = player.playerName + " wins!";
        instance.winnerTxt.transform.parent.gameObject.SetActive(true);
        _hasWinner = true;
        UIEndTurn.SetInteractable(false);
    }

    public static Player GetWinner(Winner winner)
    {
        switch (winner)
        {
            case Winner.PlayerOne:
                return Player.GetPlayers()[0];
            case Winner.PlayerTwo:
                return Player.GetPlayers()[1];
            default:
                return null;
        }
    }

    [SerializeField]
    Text winnerTxt;

    static bool _hasWinner;

    public static bool hasWinner
    {
        get { return _hasWinner; }
    }

    private void Start()
    {
        _instance = this;
    }
}
