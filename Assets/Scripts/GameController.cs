using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Game Setup")]
    [Range(10, 100)]
    [SerializeField] int pieceScore = 10;
    [Range(1, 200)]
    [SerializeField] int moves = 50;

    [Space(20)]
    [SerializeField] Text txtMoves;
    [SerializeField] Text txtScore;
    [SerializeField] Text txtBestScore;
    [SerializeField] Button btnRestart;
    [SerializeField] GameObject lockBoard;
    [SerializeField] GameObject gameOver;

    int movesLeft;
    int score;
    int bestScore = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadBestScore();
        Play();
    }

    void Play()
    {
        gameOver.SetActive(false);
        score = 0;
        movesLeft = moves;
        txtScore.text = score.ToString();
        txtMoves.text = movesLeft.ToString();
        PieceManager.Instance.SetupBoard();
        UnlockBoard();
    }

    public void MoveUsed()
    {
        movesLeft--;
        txtMoves.text = movesLeft.ToString();

        if (movesLeft == 0)
        {
            gameOver.SetActive(true);
        }
    }

    public void UpdateScore(int matches)
    {
        score += pieceScore * matches;
        txtScore.text = score.ToString();

        if (score > bestScore)
        {
            bestScore = score;
            txtBestScore.text = score.ToString();
        }
    }

    public void LockBoard()
    {
        btnRestart.interactable = false;
        lockBoard.SetActive(true);
    }

    public void UnlockBoard()
    {
        btnRestart.interactable = true;

        if (movesLeft == 0)
            return;

        lockBoard.SetActive(false);
    }

    void SaveBestScore()
    {
        PlayerPrefs.SetInt("bestScore", bestScore);
    }

    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("bestScore", 0);
        txtBestScore.text = bestScore.ToString();
    }

    #region Events

    public void OnButtonRestartClick()
    {
        SaveBestScore();
        Play();
    }

    public void OnButtonQuitClick()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        SaveBestScore();
    }

    #endregion
}
