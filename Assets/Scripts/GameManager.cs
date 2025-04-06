using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Board board;
    public GameObject[] buttonsS; 
    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;

    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;

    public GameObject[,] pieces;
    public List<GameObject> movedPawns;

    public Player white;
    public Player black;
    public Player currentPlayer;
    public Player otherPlayer;

    MiniMax mma;

    GameObject[] buttons;
    bool gameOver = false;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    };

    public Difficulty currentDifficulty;

    
    public Text winText;
    public GameObject gameOverPanel;

    int depth = 4;

    bool firstMoveMade;
    

    void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        pieces = new GameObject[8, 8];
        movedPawns = new List<GameObject>();

        white = new Player("white", true);
        black = new Player("black", false);

        currentPlayer = white;
        otherPlayer = black;

        InitialSetup();

        buttons = GameObject.FindGameObjectsWithTag("Button");
        
        mma = new MiniMax();
        currentDifficulty = Difficulty.Medium;
    }

    private void InitialSetup()
    {
        AddPiece(whiteRook, white, 0, 0);
        AddPiece(whiteKnight, white, 1, 0);
        AddPiece(whiteBishop, white, 2, 0);
        AddPiece(whiteQueen, white, 3, 0);
        AddPiece(whiteKing, white, 4, 0);
        AddPiece(whiteBishop, white, 5, 0);
        AddPiece(whiteKnight, white, 6, 0);
        AddPiece(whiteRook, white, 7, 0);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(whitePawn, white, i, 1);
        }

        AddPiece(blackRook, black, 0, 7);
        AddPiece(blackKnight, black, 1, 7);
        AddPiece(blackBishop, black, 2, 7);
        AddPiece(blackQueen, black, 3, 7);
        AddPiece(blackKing, black, 4, 7);
        AddPiece(blackBishop, black, 5, 7);
        AddPiece(blackKnight, black, 6, 7);
        AddPiece(blackRook, black, 7, 7);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(blackPawn, black, i, 6);
        }
    }

    public void AddPiece(GameObject prefab, Player player, int col, int row)
    {
        GameObject pieceObject = board.AddPiece(prefab, col, row);
        player.pieces.Add(pieceObject);
        pieces[col, row] = pieceObject;
    }

    public void SelectPieceAtGrid(Vector2Int gridPoint)
    {
        GameObject selectedPiece = pieces[gridPoint.x, gridPoint.y];
        if (selectedPiece)
        {
            board.SelectPiece(selectedPiece);
        }
    }

    public List<Vector2Int> MovesForPiece(GameObject pieceObject, bool realMove = true, bool maximisingPlayer = true)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        Vector2Int gridPoint = GridForPiece(pieceObject);
        List<Vector2Int> locations = piece.MoveLocations(gridPoint);

        locations.RemoveAll(gp => gp.x < 0 || gp.x > 7 || gp.y < 0 || gp.y > 7);

        if (realMove)
        {
            locations.RemoveAll(gp => FriendlyPieceAt(gp));
        }
        else
        {
            locations.RemoveAll(gp => FriendlyPieceAt(gp, false, maximisingPlayer ? white : black));
        }
        

        return locations;
    }

    public void Move(GameObject piece, Vector2Int gridPoint, bool realMove = true)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent.type == PieceType.Pawn && !HasPawnMoved(piece) && realMove)
        {
            movedPawns.Add(piece);
        }

        Vector2Int startGridPoint = GridForPiece(piece);

        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece;
        if (realMove)
        {
            board.MovePiece(piece, gridPoint);
        }
    }

    public void PawnMoved(GameObject pawn)
    {
        movedPawns.Add(pawn);
    }

    public bool HasPawnMoved(GameObject pawn)
    {
        return movedPawns.Contains(pawn);
    }

    public GameObject CapturePieceAt(Vector2Int gridPoint, bool realMove = true, Player player = null)
    {       
        GameObject pieceToCapture = PieceAtGrid(gridPoint);
        if (pieceToCapture.GetComponent<Piece>().type == PieceType.King && realMove)
        {
            Destroy(board.GetComponent<TileSelector>());
            Destroy(board.GetComponent<MoveSelector>());
            GameOver(currentPlayer);
        }

        if (realMove)
        {
            currentPlayer.capturedPieces.Add(pieceToCapture);
        }
        else
        {
            player.capturedPieces.Add(pieceToCapture);
        }


        pieces[gridPoint.x, gridPoint.y] = null;

        if (realMove)
        {
            Destroy(pieceToCapture);
        }

        return pieceToCapture.gameObject;
        
    }

    public void SelectPiece(GameObject piece)
    {
        board.SelectPiece(piece);
    }

    public void DeselectPiece(GameObject piece)
    {
        board.DeselectPiece(piece);
    }

    public bool DoesPieceBelongToCurrentPlayer(GameObject piece)
    {
        return currentPlayer.pieces.Contains(piece);
    }

    public GameObject PieceAtGrid(Vector2Int gridPoint)
    {
        if (gridPoint.x > 7 || gridPoint.y > 7 || gridPoint.x < 0 || gridPoint.y < 0)
        {
            return null;
        }
        return pieces[gridPoint.x, gridPoint.y];
    }

    public Vector2Int GridForPiece(GameObject piece)
    {
        for (int i = 0; i < 8; i++) 
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public bool FriendlyPieceAt(Vector2Int gridPoint, bool realMove = true, Player other = null)
    {
        GameObject piece = PieceAtGrid(gridPoint);

        if (realMove)
        {
            if (piece == null)
            {
                return false;
            }

            if (otherPlayer.pieces.Contains(piece))
            {
                return false;
            }

            return true;
        }
        else
        {
            if(piece == null)
            {
                return false;
            }

            if (other.pieces.Contains(piece))
            {
                return false;
            }

            return true;
        }
    }

    public void NextPlayer()
    {
        if(currentPlayer == white)
        {
            currentPlayer = black;
            otherPlayer = white;
        }

        if(currentPlayer.name == "black" && !gameOver)
        {
            if (currentDifficulty == Difficulty.Easy)
            {
                depth = 1;
            }
            else if (currentDifficulty == Difficulty.Medium)
            {
                depth = 3;
            }
            else
            {
                depth = 5;
            }

            StartCoroutine(AITurn());
            CheckmateCheck();
        }
        else
        {
            
            if (!firstMoveMade)
            {
                DisableButtons();
                firstMoveMade = true;
                foreach (var button in buttonsS){
            button.SetActive(false); 
        }
            }
        }
    }

    IEnumerator AITurn()
    {
        yield return new WaitForSeconds(0.5f);


        (float, Move) miniMaxCall = mma.MinMax(depth, true);


        Move moveToMake = miniMaxCall.Item2;

        mma.EvaluatedMovesCount();

        if (PieceAtGrid(moveToMake.dest) == null)
        {
            Move(moveToMake.piece, moveToMake.dest);
        }
        else
        {
            CapturePieceAt(moveToMake.dest);
            Move(moveToMake.piece, moveToMake.dest);
        }

        currentPlayer = white;
        otherPlayer = black;
    }

    public List<Move> GenerateAllMoves(Player player)
    {
        string playerStr;

        if (player.name == "white")
        {
            playerStr = "White";
        }
        else {
            playerStr = "Black";
        }

        List<Move> moves = new List<Move>();

        for (int i = 0; i < pieces.GetLength(0); i++)
        {
            for(int j = 0; j < pieces.GetLength(1); j++)
            {
                if(pieces[i,j] != null)
                {
                    if (pieces[i, j].name.Contains(playerStr))
                    {
                        Vector2Int gridPos = GridForPiece(pieces[i, j]);

                        bool maximising = false;
                        if(player.name == "black")
                        {
                            maximising = true;
                        }

                        List<Vector2Int> movePos = MovesForPiece(pieces[i, j], false, maximising);

                        foreach(Vector2Int pos in movePos)
                        {
                            moves.Add(new Move(gridPos, pos, pieces[i, j]));
                        }
                    }
                }
            }
        }

        return moves;
    }

    public void UndoDelete(GameObject piece, Vector2Int position, Player player)
    {
        GameObject test = player.capturedPieces[player.capturedPieces.Count - 1];
        player.capturedPieces.Remove(test);
        
        pieces[position.x, position.y] = test;
    }

    public void ChangeDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
    }

    private void DisableButtons()
    {
        foreach(GameObject buttonObj in buttons)
        {
            buttonObj.SetActive(false);
        }
    }

    public void CheckmateCheck()
    {
        bool checkMate = false;

        List<List<Vector2Int>> gameMoves = new List<List<Vector2Int>>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] != null)
                {
                    GameObject piece = pieces[i, j];

                    if (piece.name.Contains("Black"))
                    {
                        gameMoves.Add(MovesForPiece(piece));
                    }
                }
            }
        }

        foreach(List<Vector2Int> moveList in gameMoves)
        {
            foreach(Vector2Int move in moveList)
            {
                GameObject piece = PieceAtGrid(move);
                if(piece != null)
                {
                    if(piece.GetComponent<Piece>().type == PieceType.King)
                    {
                        checkMate = true;
                    }
                }
            }
        }

    }

    private void GameOver(Player player)
    {
        gameOverPanel.SetActive(true);

        gameOver = true;

        if(player.name == "white")
        {
            winText.text = "White Wins!";
        }
        else
        {
            winText.text = "Black Wins!";
        }
    }
}
