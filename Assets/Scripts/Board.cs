using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    // References to essential game object
    public TetrisManager tetrisManager;
    public Tilemap tilemap;
    public Piece prefabPiece;
    public TetronimoData[] tetronimos; // Input Tetronimo Data in the editor

    // Reference to the puzzle tiles and piece order
    public Puzzle puzzle;

    // Game setup variables
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector2Int startPosition = new Vector2Int(-1, 8);
    public float dropInterval = 0.5f;

    // In-game variables
    int left { get { return -boardSize.x / 2; } }
    int right { get { return boardSize.x / 2; } }
    int bottom { get { return -boardSize.y / 2; } }
    int top { get { return boardSize.y / 2; } }   
    
    float dropTimer = 0.0f;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>(); // Maps tilemap position to a Piece GameObject
    Piece activePiece;
    List<Tetronimo> pieceOrder = new List<Tetronimo>();

    // Update is called once per frame
    private void Update()
    {
        if (tetrisManager.gameOver) return;

        dropTimer += Time.deltaTime;

        if (dropTimer >= dropInterval)
        {
            dropTimer = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult) 
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }

    // Spawn the next piece in the puzzle's piece order
    // If the last piece has been placed without a Game Over, the player wins
    public void SpawnPiece()
    {
        activePiece = Instantiate(prefabPiece);

        Tetronimo t = Tetronimo.Donut;

        if (pieceOrder.Count > 0)
        {
            t = pieceOrder[0];
            pieceOrder.RemoveAt(0);
        }
        else
        {
            tetrisManager.WinGame();
        }

        activePiece.Initialize(this, t);

        CheckEndGame();

        Set(activePiece);
    }

    // Check if the current piece can be placed. If not, it's a game over.
    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            // End game when there is not a valid position for the new piece
            tetrisManager.SetGameOver(true);
        }
    }

    // Call this function from the TetrisManager OnGameOver event
    public void UpdateGameOver()
    {
        // If gameOver is false, the game just started or reset
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    // Reset the board and reinitialize the puzzle
    void ResetBoard()
    {
        // Clear the pieces on the board
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        // Clear the active piece
        activePiece = null;

        // Clear the tilemap
        tilemap.ClearAllTiles();

        // Clear the pieces dictionary
        pieces.Clear();

        // Reset the start order
        pieceOrder.Clear();
        foreach (Tetronimo t in puzzle.puzzleOrder)
        {
            pieceOrder.Add(t);
        }

        // Spawn the first piece of the new game
        SpawnPiece();

        // Initialize the puzzle and reset the game won flag
        InitializePuzzle();
    }

    // Initialize the puzzle by reading from the puzzle prefab and making it into a piece
    void InitializePuzzle()
    {
        // Create a new piece and set its essential data
        Piece puzzlePiece = Instantiate(prefabPiece);
        puzzlePiece.board = this;
        puzzlePiece.position = Vector2Int.zero;
        puzzlePiece.data = new TetronimoData();
        puzzlePiece.data.tile = puzzle.puzzleTile;
        puzzlePiece.freeze = true;

        // Set the cells of the puzzle piece to match the tiles set in the puzzle tilemap
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int y = bottom; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                if (puzzle.puzzleTilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }

        puzzlePiece.cells = cells.ToArray();
        puzzlePiece.activeCellCount = puzzlePiece.cells.Length;

        Set(puzzlePiece);
    }

    // Set board tiles and update the pieces dictionary
    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);
            pieces[cellPosition] = piece;
        }
    }

    // Colour in the piece on the board
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    // Clear a piece from the board by setting its tiles to null
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    // Check if a piece can be placed in a position by ensuring its tiles are not overlapping with existing tiles
    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // Check bounds
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top)
            {
                return false;
            }

            // Check if occupied
            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    // Check if every tile in a row has a tile
    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            if (!tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }
        return true;
    }

    // Clear all tiles on a line
    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];
                piece.ReduceActiveCount();
                SetTile(cellPosition, null);
            }
        }
    }

    // Shift all rows above the cleared row downwards
    void ShiftsRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                // Record the piece of the tile that will move
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                { 
                    Piece currentPiece = pieces[cellPosition];

                    // Clear the tile that will move
                    SetTile(cellPosition, null);

                    // Move the tile down one row
                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    // Check if a line if full and can be destroyed
    // If no lines can be destroyed after dropping a piece, the player loses the puzzle
    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShifted = 0;
        foreach (int y in destroyedLines)
        {
            ShiftsRowsDown(y - rowsShifted);
            rowsShifted++;
        }

        int scoreToAdd = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(scoreToAdd);

        // If no lines were destroyed, the game is over in Puzzle Mode
        if (destroyedLines.Count == 0) tetrisManager.SetGameOver(true);
    }
}
