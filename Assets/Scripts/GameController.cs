using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [SerializeField] int pieceScore = 10;
    [SerializeField] int moves = 50;
    [SerializeField] TextMeshProUGUI txtScore;
    [SerializeField] TextMeshProUGUI txtMoves;

    public bool hasMovesLeft => movesLeft > 0;

    int movesLeft;
    int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        movesLeft = moves;
        txtScore.text = score.ToString();
        txtMoves.text = movesLeft.ToString();

        PieceManager.Instance.SetupBoard();
    }

    public void MoveUsed()
    {
        movesLeft--;
        txtMoves.text = movesLeft.ToString();
    }

}
