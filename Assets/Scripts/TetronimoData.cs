using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// Name all the possible tetronimos
// The Donut is the unique piece
public enum Tetronimo { I, O, T, J, L, S, Z, Donut }

// Make this data available in the editor to create all our tetronimos
[Serializable]
public struct TetronimoData
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;
}
