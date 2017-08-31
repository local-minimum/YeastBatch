using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWinScoreMeter : MonoBehaviour {

    [SerializeField]
    Image playerOne;

    [SerializeField]
    Image playerTwo;

    int playerOneScore;

    int playerTwoScore;

    [SerializeField]
    int winScore;

    static UIWinScoreMeter _instance;

    public static UIWinScoreMeter instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIWinScoreMeter>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void SetScore(int playerOne, int playerTwo)
    {
        playerOneScore = playerOne;
        playerTwoScore = playerTwo;

        this.playerOne.fillAmount = (float)playerOneScore / winScore;
        this.playerTwo.fillAmount = (float)playerTwoScore / winScore;
    }

    public static void AddPoints(int playerOne, int playerTwo)
    {
        instance.SetScore(instance.playerOneScore + playerOne, instance.playerTwoScore + playerTwo);
    }

    public static void AddPointsFromBoardStatus(UIWinMeters winMeters)
    {
        DominionStatus dominions = winMeters.GetDominions();
        int playerOne = 0;
        int playerTwo = 0;
        if (dominions.tiles.playerOne > dominions.tiles.playerTwo)
        {
            playerOne += Mathf.FloorToInt((dominions.tiles.playerOne - dominions.tiles.playerTwo) / instance.diffPointsTiles);
        } else
        {
            playerTwo += Mathf.FloorToInt((dominions.tiles.playerTwo - dominions.tiles.playerOne) / instance.diffPointsTiles);
        }
        if (dominions.population.playerOne > dominions.population.playerTwo)
        {
            playerOne += Mathf.FloorToInt((dominions.population.playerOne - dominions.population.playerTwo) / instance.diffPointsPop);
        } else
        {
            playerOne += Mathf.FloorToInt((dominions.population.playerTwo - dominions.population.playerOne) / instance.diffPointsPop);
        }

        AddPoints(playerOne, playerTwo);
    }

    [SerializeField, Range(0, 1)]
    float diffPointsPop = 0.2f;

    [SerializeField, Range(0, 1)]
    float diffPointsTiles = 0.2f;

}
