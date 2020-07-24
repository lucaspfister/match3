using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;

    [SerializeField] float pieceMovementDuration = 0.25f;
    [SerializeField] AnimationHelper animationHelper;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Piece piecePrefab;
    [SerializeField] Transform piecesParent;
    [SerializeField] Color[] pieceColors;

    float pieceSize;
    int boardLength;
    Piece[,] board;
    Piece selectedPiece;

    enum Direction
    {
        None,
        Top,
        Down,
        Left,
        Right
    }

    void Awake()
    {
        Instance = this;

        pieceSize = grid.cellSize.x;
        boardLength = grid.constraintCount;
        board = new Piece[8, 8];
    }

    public void SetupBoard()
    {
        Vector2 pos;
        int value = 0;

        for (int x = 0; x < boardLength; x++)
        {
            for (int y = 0; y < boardLength; y++)
            {
                pos = Util.BoardToLocalPosition(x, y, pieceSize);
                board[x, y] = Instantiate(piecePrefab, piecesParent);
                value = RandomValidValue(x, y);
                board[x, y].Set(value, pieceColors[value], new Vector2Int(x, y), pos);
            }
        }
    }

    int RandomValidValue(int x, int y)
    {
        int value;

        do
        {
            value = Random.Range(0, pieceColors.Length);
        } while
        ((CheckSideValue(x, y, value, Direction.Top) && CheckSideValue(x, y - 1, value, Direction.Top)) ||
        (CheckSideValue(x, y, value, Direction.Left) && CheckSideValue(x - 1, y, value, Direction.Left)));

        return value;
    }

    bool CheckSideValue(int x, int y, int value, Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                {
                    if (y > 0 && board[x, y - 1]?.value == value)
                        return true;

                    return false;
                }
            case Direction.Down:
                {
                    if (y < boardLength - 1 && board[x, y + 1]?.value == value)
                        return true;

                    return false;
                }
            case Direction.Left:
                {
                    if (x > 0 && board[x - 1, y]?.value == value)
                        return true;

                    return false;
                }
            case Direction.Right:
                {
                    if (x < boardLength - 1 && board[x + 1, y]?.value == value)
                        return true;

                    return false;
                }
        }

        return false;
    }

    bool ValidateMovement(Piece piece1, Piece piece2)
    {
        if (piece1.boardCoordinate.x == piece2.boardCoordinate.x)
        {
            if (piece1.boardCoordinate.y + 1 == piece2.boardCoordinate.y ||
                piece1.boardCoordinate.y - 1 == piece2.boardCoordinate.y)
            {
                return true;
            }
        }
        else if (piece1.boardCoordinate.y == piece2.boardCoordinate.y)
        {
            if (piece1.boardCoordinate.x + 1 == piece2.boardCoordinate.x ||
                piece1.boardCoordinate.x - 1 == piece2.boardCoordinate.x)
            {
                return true;
            }
        }

        return false;
    }

    public void SelectPiece(Piece piece, bool isDrag = false)
    {
        //First selection
        if (!selectedPiece)
        {
            selectedPiece = piece;
            return;
        }

        //Same piece
        if (selectedPiece == piece)
        {
            return;
        }

        //Invalid movement
        if (!ValidateMovement(selectedPiece, piece))
        {
            selectedPiece.Deselect();
            selectedPiece = piece;
            return;
        }

        //Valid movement
        if (isDrag)
        {
            selectedPiece.DragSuccess();
        }

        StartCoroutine(MoveSelectedPieces(selectedPiece, piece, isDrag));
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
    }

    IEnumerator MoveSelectedPieces(Piece piece1, Piece piece2, bool isDrag)
    {
        LockBoard();
        piece1.Deselect();
        piece2.Deselect();
        DeselectPiece();

        Vector2Int piece1Coord = piece1.boardCoordinate;
        piece1.boardCoordinate = piece2.boardCoordinate;
        piece2.boardCoordinate = piece1Coord;

        float duration = isDrag ? 0 : pieceMovementDuration;
        animationHelper.MoveTo(piece1.gameObject, Util.BoardToLocalPosition(piece1.boardCoordinate, pieceSize), duration);
        animationHelper.MoveTo(piece2.gameObject, Util.BoardToLocalPosition(piece2.boardCoordinate, pieceSize), duration);

        yield return new WaitForSeconds(duration);
        
        //verificar se pontuou

        //GameController.Instance.MoveUsed();
        //canvasGroup.interactable = GameController.Instance.hasMovesLeft;

        UnlockBoard();
    }

    public void LockBoard()
    {
        canvasGroup.interactable = false;
    }

    public void UnlockBoard()
    {
        canvasGroup.interactable = true;
    }
}
