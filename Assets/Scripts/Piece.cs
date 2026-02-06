using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Piece variables
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;
    public Vector2Int position;
    public bool freeze = false;
    public int activeCellCount = -1;

    // Initialize the piece on the board
    public void Initialize(Board board, Tetronimo tetronimo)
    {
        // Reference the board
        this.board = board;

        // Search for the tetronimo data in the board's tetronimos array
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // Copy the cells from the data to the piece
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = data.cells[i];
        }

        // Set the starting position
        position = board.startPosition;

        activeCellCount = cells.Length;
    }

    // Get inputs and manage the piece once per frame
    private void Update()
    {
        if (board.tetrisManager.gameOver) return;

        if (freeze) return;

        board.Clear(this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1);
            }
        }

        board.Set(this);

        // Check the board before spawning the piece
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }
    }

    // Rotate the piece 90 degress in a particular direction,
    // ensure that the rotation is on the board and not overlapping with another piece
    void Rotate(int direction)
    {
        Vector2Int[] originalCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++) originalCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks()) RevertRotation(originalCells);
        }
    }

    // Undo a rotation if it's invalid
    void RevertRotation(Vector2Int[] originalCells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = originalCells[i];
        }
    }

    // Try to find and apply a valid wall kick
    bool TryWallKicks()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), // down-left
            new Vector2Int(1, -1) // down-right
        };

        // 2 Rotation Wall Kicks for I Tetronimo
        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(new Vector2Int(-2, 0)); // two left
            wallKickOffsets.Add(new Vector2Int(2, 0)); // two right
        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    // Move the cells for a rotation
    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, direction * 90);

        for (int i = 0; i < cells.Length; i++)
        {
            // Convert cell to Vector3 for rotation
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            // Adjust for special rotation center
            bool isSpecial = data.tetronimo == Tetronimo.O || data.tetronimo == Tetronimo.I;           
            if (isSpecial)
            {
                cellPosition -= new Vector3(0.5f, 0.5f);
            }

            // Rotate the cell
            Vector3 result = rotation * cellPosition;

            // Assign the rotated position back to cells array
            if (isSpecial)
            {
                cells[i] = new Vector2Int(Mathf.CeilToInt(result.x), Mathf.CeilToInt(result.y));
            }
            else
            {
                cells[i] = new Vector2Int(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.y));
            }
        }
    }

    // Move to the bottom-most valid position immediately
    void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            // Keep moving down until we can't, the check and the move are both done in Move()
        }

        freeze = true;
    }

    // Apply a move if the move position is valid
    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position + translation;

        bool positionIsValid = board.IsPositionValid(this, newPosition);

        if (positionIsValid)
        {
            position += translation;
        }

        return positionIsValid;
    }

    // Keep track of how many cells are left on a piece,
    // if none are left, delete the piece
    public void ReduceActiveCount()
    {
        activeCellCount--;
        if (activeCellCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
