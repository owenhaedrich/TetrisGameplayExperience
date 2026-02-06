using UnityEngine;
using UnityEngine.Tilemaps;

public class Puzzle : MonoBehaviour
{
    // Create the fields required to define a puzzle in the editor
    public Tile puzzleTile; // Tile to draw the puzzle with
    public Tilemap puzzleTilemap; // Arrangement of tiles for the puzzle's initial state
    public Tetronimo[] puzzleOrder; // Piece order for the puzzle
}
