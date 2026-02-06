using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    // References to UI objects
    public TextMeshProUGUI scoreText;
    public GameObject endGamePanel;
    public GameObject winGamePanel;
    public TetrisManager tetrisManager;

    // Update the on-screen score to match the Tetris Manager
    public void UpdateScore()
    {
        scoreText.text = $"Score: {tetrisManager.score.ToString():n0}";
    }

    // Enable and disable the Win/Loss game over screens as needed
    public void UpdateGameOver()
    {
        if (tetrisManager.gameWon)
        {
            winGamePanel.SetActive(tetrisManager.gameOver);
        }
        else
        {
            endGamePanel.SetActive(tetrisManager.gameOver);
        }
    }

    // Called by the Play Again button On-Click Event
    public void PlayAgain()
    {
        tetrisManager.SetGameOver(false);
    }
}
