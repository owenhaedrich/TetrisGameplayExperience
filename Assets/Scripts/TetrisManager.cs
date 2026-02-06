using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    // Readable variables for other classes
    public int score { get; private set; }
    public bool gameOver { get; private set; }
    public bool gameWon { get; private set; }

    // Events
    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetGameOver(false);
    }

    // Calculate the score rewarded for clearing a certain number of rows
    public int CalculateScore(int clearedRows)
    {
        switch (clearedRows)
        {
            case 1:
                return 100;
            case 2:
                return 300;
            case 3:
                return 500;
            case 4:
                return 800;
            default:
                return 0;
        }
    }

    // Change the score by a certain amount and send the OnScoreChanged event
    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    // Set the gameWon flag and end the game
    public void WinGame()
    {
        gameWon = true;
        SetGameOver(true);
    }

    // Send the OnGameOver event, if the game is just starting or restarting we reset the score and gameWon flag
    public void SetGameOver(bool _gameOver)
    {
        gameOver = _gameOver;
        OnGameOver.Invoke();

        if (!_gameOver)
        {
            gameWon = false;
            score = 0;
            ChangeScore(0);
        }
    }
}
