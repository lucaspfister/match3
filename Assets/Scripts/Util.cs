using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Vector2 BoardToLocalPosition(Vector2Int boardCoordinate, float pieceSize)
    {
        return (BoardToLocalPosition(boardCoordinate.x, boardCoordinate.y, pieceSize));
    }

    public static Vector2 BoardToLocalPosition(int x, int y, float pieceSize)
    {
        return new Vector2(x * pieceSize, -y * pieceSize);
    }
}
