using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Winner { None, PlayerOne, PlayerTwo};

public class UIWinMeter : MonoBehaviour {

    [SerializeField]
    Image playerOneImage;

    [SerializeField]
    Image playerTwoImage;

    [SerializeField]
    AnimationCurve winCondition;
    
    [SerializeField]
    RectTransform[] winBars;

    public void Show(int playerOne, int playerTwo)
    {
        playerOneImage.fillAmount = playerOne / (float)(playerOne + playerTwo);
    }

    public void SetPlayerColors(Color playerOne, Color playerTwo)
    {
        playerOneImage.color = playerOne;
        playerTwoImage.color = playerTwo;
    }

    public Winner GetWinner()
    {
        float playerOne = playerOneImage.fillAmount;
        float winCondition = this.winCondition.Evaluate(Match.turn);
        if (playerOne >= winCondition)
        {
            return Winner.PlayerOne;
        } else if (playerOne <= 1 - winCondition)
        {
            return Winner.PlayerTwo;
        } else
        {
            return Winner.None;
        }
    }

    public float PlayerOne
    {
        get
        {
            return playerOneImage.fillAmount;
        }
    }

    public float PlayerTwo
    {
        get
        {
            return 1 - playerOneImage.fillAmount;
        }
    }
    public void SetWinCondition()
    {
        float winCondition = this.winCondition.Evaluate(Match.turn);
        winBars[0].anchorMin = new Vector2(winCondition, 0);
        winBars[0].anchorMax = new Vector2(winCondition, 1);

        winBars[1].anchorMin = new Vector2(1 - winCondition, 0);
        winBars[1].anchorMax = new Vector2(1 - winCondition, 1);

    }

    private void Start()
    {
        SetWinCondition();
    }

    private void OnEnable()
    {
        Match.OnPlayerTurn += HandlePlayerTurn;
    }

    int turnUpdate = -1;
    void HandlePlayerTurn(Player player, int pId)
    {
        if (turnUpdate != Match.turn)
        {
            turnUpdate = Match.turn;
            SetWinCondition();
        }
    }
}
