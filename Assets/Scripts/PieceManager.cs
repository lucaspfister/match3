using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;

    [SerializeField] float pieceMovementDuration = 0.25f;
    [SerializeField] AnimationHelper animationHelper;
    [SerializeField] CanvasScaler canvasScaler;
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
        Up,
        Down,
        Left,
        Right
    }

    void Awake()
    {
        Instance = this;

        board = new Piece[8, 8];
        boardLength = grid.constraintCount;
        pieceSize = grid.cellSize.x;
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
        } while (CheckSideValue(x, y, value, Direction.Up) > 1 ||
                 CheckSideValue(x, y, value, Direction.Left) > 1);

        return value;
    }

    int CheckSideValue(int x, int y, int value, Direction direction, int total = 0)
    {
        bool isMatch = false;

        switch (direction)
        {
            case Direction.Up:
                {
                    if (y > 0 && board[x, y - 1]?.value == value)
                    {
                        isMatch = true;
                        total++;
                        y -= 1;
                    }
                    break;
                }
            case Direction.Down:
                {
                    if (y < boardLength - 1 && board[x, y + 1]?.value == value)
                    {
                        isMatch = true;
                        total++;
                        y += 1;
                    }
                    break;
                }
            case Direction.Left:
                {
                    if (x > 0 && board[x - 1, y]?.value == value)
                    {
                        isMatch = true;
                        total++;
                        x -= 1;
                    }
                    break;
                }
            case Direction.Right:
                {
                    if (x < boardLength - 1 && board[x + 1, y]?.value == value)
                    {
                        isMatch = true;
                        total++;
                        x += 1;
                    }
                    break;
                }
        }

        if (isMatch)
        {
            return CheckSideValue(x, y, value, direction, total);
        }

        return total;
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

        StartCoroutine(CheckSelectedPieces(selectedPiece, piece, isDrag));
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
    }

    IEnumerator CheckSelectedPieces(Piece piece1, Piece piece2, bool isDrag)
    {
        LockBoard();
        DeselectPiece();
        piece1.Deselect();
        piece2.Deselect();

        yield return StartCoroutine(SwitchPieces(piece1, piece2, isDrag));

        if (piece1.value == piece2.value)
        {
            yield return StartCoroutine(SwitchPieces(piece1, piece2, isDrag));
        }
        else
        {
            List<Vector2Int> lstMatches1 = ListMatches(piece1);
            List<Vector2Int> lstMatches2 = ListMatches(piece2);

            if (lstMatches1.Count == 0 && lstMatches2.Count == 0)
            {
                yield return StartCoroutine(SwitchPieces(piece1, piece2, isDrag));
            }
            else
            {
                //TODO
                foreach (var item in lstMatches1)
                {
                    animationHelper.Shrink(board[item.x, item.y].gameObject);
                }
            }
        }

        //GameController.Instance.MoveUsed();
        //canvasGroup.interactable = GameController.Instance.hasMovesLeft;

        UnlockBoard();
    }

    IEnumerator SwitchPieces(Piece piece1, Piece piece2, bool animate)
    {
        Vector2Int piece1Coord = piece1.boardCoordinate;
        piece1.boardCoordinate = piece2.boardCoordinate;
        piece2.boardCoordinate = piece1Coord;

        float duration = animate ? 0 : pieceMovementDuration;
        animationHelper.MoveTo(piece1.gameObject, Util.BoardToLocalPosition(piece1.boardCoordinate, pieceSize), duration);
        animationHelper.MoveTo(piece2.gameObject, Util.BoardToLocalPosition(piece2.boardCoordinate, pieceSize), duration);

        yield return new WaitForSeconds(duration);
    }

    List<Vector2Int> ListMatches(Piece piece)
    {
        int x = piece.boardCoordinate.x;
        int y = piece.boardCoordinate.y;

        int up = CheckSideValue(x, y, piece.value, Direction.Up);
        int down = CheckSideValue(x, y, piece.value, Direction.Down);
        int left = CheckSideValue(x, y, piece.value, Direction.Left);
        int right = CheckSideValue(x, y, piece.value, Direction.Right);

        int totalVertical = up + down;
        int totalHorizontal = left + right;

        List<Vector2Int> lstMatches = new List<Vector2Int>();

        if (totalVertical < 2 && totalHorizontal < 2)
            return lstMatches;

        if (totalVertical >= totalHorizontal)
        {
            for (int i = 0; i < up; i++)
            {
                lstMatches.Add(new Vector2Int(x, --y));
            }

            y = piece.boardCoordinate.y;
            lstMatches.Add(new Vector2Int(x, y));

            for (int i = 0; i < down; i++)
            {
                lstMatches.Add(new Vector2Int(x, ++y));
            }
        }
        else
        {
            for (int i = 0; i < left; i++)
            {
                lstMatches.Add(new Vector2Int(--x, y));
            }

            x = piece.boardCoordinate.x;
            lstMatches.Add(new Vector2Int(x, y));

            for (int i = 0; i < right; i++)
            {
                lstMatches.Add(new Vector2Int(++x, y));
            }
        }

        return lstMatches;
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
