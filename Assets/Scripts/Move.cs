using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Vector2Int source;
    public Vector2Int dest;
    public GameObject piece;

    public Move(Vector2Int src, Vector2Int dst, GameObject piece)
    {
        this.source = src;
        this.dest = dst;
        this.piece = piece;
    }
}
