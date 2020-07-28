using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;

    [SerializeField] AnimationHelper animationHelper;
    [SerializeField] CanvasScaler canvasScaler;
    [SerializeField] GridLayoutGroup grid;
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

        boardLength = grid.constraintCount;
        board = new Piece[boardLength, boardLength];
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
                if (board[x, y] == null)
                {
                    board[x, y] = Instantiate(piecePrefab, piecesParent);
                }

                pos = Util.BoardToLocalPosition(x, y, pieceSize);
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

    int CheckSideValue(int x, int y, int value, Direction direction, List<Piece> lstIgnore = null, int total = 0)
    {
        lstIgnore = lstIgnore ?? new List<Piece>();
        bool isMatch = false;

        switch (direction)
        {
            case Direction.Up:
                {
                    if (y > 0 && board[x, y - 1]?.value == value && !lstIgnore.Contains(board[x, y - 1]))
                    {
                        isMatch = true;
                        y -= 1;
                    }
                    break;
                }
            case Direction.Down:
                {
                    if (y < boardLength - 1 && board[x, y + 1]?.value == value)
                    {
                        isMatch = true;
                        y += 1;
                    }
                    break;
                }
            case Direction.Left:
                {
                    if (x > 0 && board[x - 1, y]?.value == value)
                    {
                        isMatch = true;
                        x -= 1;
                    }
                    break;
                }
            case Direction.Right:
                {
                    if (x < boardLength - 1 && board[x + 1, y]?.value == value)
                    {
                        isMatch = true;
                        x += 1;
                    }
                    break;
                }
        }

        if (isMatch && !lstIgnore.Contains(board[x, y]))
        {
            total++;
            return CheckSideValue(x, y, value, direction, lstIgnore, total);
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

        if (isDrag)
        {
            selectedPiece.DragSuccess();
        }

        //Check selected pieces for matches
        StartCoroutine(CheckSelectedPieces(selectedPiece, piece, isDrag));
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
    }

    IEnumerator CheckSelectedPieces(Piece piece1, Piece piece2, bool isDrag)
    {
        GameController.Instance.LockBoard();
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
            List<Piece> lstPendingPieces = new List<Piece>();
            lstPendingPieces.Add(piece1);
            lstPendingPieces.Add(piece2);
            bool isUsefulMove = false;

            while (lstPendingPieces.Count > 0)
            {
                List<Piece> lstMatches = new List<Piece>();

                foreach (Piece item in lstPendingPieces)
                {
                    lstMatches.AddRange(ListMatches(item, lstMatches));
                }

                //No matches
                if (lstMatches.Count == 0)
                {
                    if (!isUsefulMove)
                    {
                        yield return StartCoroutine(SwitchPieces(piece1, piece2, isDrag));
                    }

                    break;
                }

                isUsefulMove = true;

                Queue<Piece> availablePieces = new Queue<Piece>();
                List<int> lstModifiedColumns = new List<int>();

                //Shrink matches
                foreach (Piece item in lstMatches)
                {
                    animationHelper.Shrink(item.gameObject);
                    availablePieces.Enqueue(item);
                    board[item.boardCoordinate.x, item.boardCoordinate.y] = null;

                    if (!lstModifiedColumns.Contains(item.boardCoordinate.x))
                    {
                        lstModifiedColumns.Add(item.boardCoordinate.x);
                    }
                }

                yield return new WaitForSeconds(animationHelper.ShrinkDuration);

                GameController.Instance.UpdateScore(lstMatches.Count);
                //Fill empty slots and receive pending pieces
                lstPendingPieces = FillEmptySlots(lstModifiedColumns, availablePieces);

                if (lstPendingPieces.Count > 0)
                {
                    yield return new WaitForSeconds(animationHelper.MoveDuration);
                }
            }

            if (isUsefulMove)
            {
                GameController.Instance.MoveUsed();
            }
        }

        GameController.Instance.UnlockBoard();
    }

    IEnumerator SwitchPieces(Piece piece1, Piece piece2, bool isDrag)
    {
        Vector2Int piece1Coord = piece1.boardCoordinate;
        piece1.boardCoordinate = piece2.boardCoordinate;
        piece2.boardCoordinate = piece1Coord;
        board[piece1.boardCoordinate.x, piece1.boardCoordinate.y] = piece1;
        board[piece2.boardCoordinate.x, piece2.boardCoordinate.y] = piece2;

        animationHelper.MoveTo(piece1.gameObject, Util.BoardToLocalPosition(piece1.boardCoordinate, pieceSize), !isDrag);
        animationHelper.MoveTo(piece2.gameObject, Util.BoardToLocalPosition(piece2.boardCoordinate, pieceSize), !isDrag);

        if (!isDrag)
        {
            yield return new WaitForSeconds(animationHelper.MoveDuration);
        }
    }

    List<Piece> ListMatches(Piece piece, List<Piece> lstIgnore)
    {
        List<Piece> lstMatches = new List<Piece>();

        if (lstIgnore.Contains(piece))
            return lstMatches;

        int x = piece.boardCoordinate.x;
        int y = piece.boardCoordinate.y;

        int up = CheckSideValue(x, y, piece.value, Direction.Up, lstIgnore);
        int down = CheckSideValue(x, y, piece.value, Direction.Down, lstIgnore);
        int left = CheckSideValue(x, y, piece.value, Direction.Left, lstIgnore);
        int right = CheckSideValue(x, y, piece.value, Direction.Right, lstIgnore);

        int totalVertical = up + down;
        int totalHorizontal = left + right;


        if (totalVertical < 2 && totalHorizontal < 2)
            return lstMatches;

        if (totalVertical >= totalHorizontal)
        {
            for (int i = 0; i < up; i++)
            {
                lstMatches.Add(board[x, --y]);
            }

            y = piece.boardCoordinate.y;
            lstMatches.Add(board[x, y]);

            for (int i = 0; i < down; i++)
            {
                lstMatches.Add(board[x, ++y]);
            }
        }
        else
        {
            for (int i = 0; i < left; i++)
            {
                lstMatches.Add(board[--x, y]);
            }

            x = piece.boardCoordinate.x;
            lstMatches.Add(board[x, y]);

            for (int i = 0; i < right; i++)
            {
                lstMatches.Add(board[++x, y]);
            }
        }

        return lstMatches;
    }

    List<Piece> FillEmptySlots(List<int> lstModifiedColumns, Queue<Piece> availablePieces)
    {
        List<Piece> lstPendingPieces = new List<Piece>();

        foreach (int x in lstModifiedColumns)
        {
            int emptySlots = 0;

            for (int y = boardLength - 1; y >= 0; y--)
            {
                if (board[x, y] == null)
                {
                    emptySlots++;
                    continue;
                }

                if (emptySlots > 0)
                {
                    Piece piece = board[x, y];
                    int newY = y + emptySlots;
                    board[x, newY] = piece;
                    board[x, y] = null;
                    piece.boardCoordinate = new Vector2Int(x, newY);
                    animationHelper.MoveTo(piece.gameObject, Util.BoardToLocalPosition(x, newY, pieceSize));
                    lstPendingPieces.Add(piece);
                }
            }

            int usedPieces = 0;

            while (emptySlots > 0)
            {
                Piece piece = availablePieces.Dequeue();
                int newY = emptySlots - 1;
                Vector2 pos = Util.BoardToLocalPosition(x, -usedPieces - 1, pieceSize);
                Vector2 endPos = Util.BoardToLocalPosition(x, newY, pieceSize);
                int value = Random.Range(0, pieceColors.Length);
                board[x, newY] = piece;
                piece.Set(value, pieceColors[value], new Vector2Int(x, newY), pos);
                animationHelper.MoveTo(piece.gameObject, endPos);
                lstPendingPieces.Add(piece);
                emptySlots--;
                usedPieces++;
            }
        }

        return lstPendingPieces;
    }
}
