using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

// Main game manager for Tetris-like game
public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform previewArea; // Assign in inspector
    private int nextTetrominoIndex;
    private GameObject previewTetromino;
    [SerializeField] private float[] tetrominoWeights = { 0.15f, 0.20f, 0.10f, 0.05f, 0.25f, 0.10f, 0.15f }; // Editable in inspector
    public GameObject[] Tetrominos; // Array of Tetromino prefabs
    public float movementFrequency = 0.8f; // Time between automatic downward moves
    private float passedTime = 0; // Tracks time since last move
    private GameObject currentTetromino; // Currently active Tetromino
    private bool gameloop; // Is the game running?
    public GameObject UpgradePanel; // Reference to upgrade UI panel
    public GameObject GameoverPanel; // Reference to game over UI panel
    private float lastMoveTime; // Last time a horizontal move was made
    private float repeatRate = 0.1f; // Time between repeated horizontal moves when holding key

    // Add these lines back to restore your ghost block and hold feature variables:
    private GameObject ghostTetromino; // The ghost block for quick drop preview
    public Material ghostMaterial; // Assign a grey transparent material in inspector

    private GridScript gridScript;

    // Add these fields at the top of your GameManager class:
    private int currentScore = 0;
    private int baseGoal = 1000; // Base score goal for round 1
    private float goalMultiplier = 1.5f; // Multiplier for goal increase per round
    private int goalScore;
    private int currentRound = 1;
    private int linesCompleted = 0;

    // Difficulty settings
    private float baseMovementFrequency = 0.8f; // Base drop speed
    private float speedIncrement = 0.05f; // How much faster per round
    private float minMovementFrequency = 0.1f; // Minimum drop speed

    [Header("UI Text")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI goalText;
    public TMPro.TextMeshProUGUI roundText;
    public TMPro.TextMeshProUGUI linesText;

    // Add a reference to your grid GameObject at the top:
    [SerializeField] private GameObject grid; // Assign your grid GameObject in the inspector

    // Called when the game starts
    void Start()
    {
        gameloop = true;
        gridScript = GetComponent<GridScript>();
        CalculateDifficulty();
        SpawnTetromino();
        UpdateUI(); // Initialize UI
    }

    // Called every frame
    void Update()
    {
        if (!gameloop) return; // Stop all game logic if game is paused/upgrades

        CheckForLines();
        passedTime += Time.deltaTime;
        if (passedTime >= movementFrequency)
        {
            passedTime -= movementFrequency;
            MoveTetromino(Vector3.down);
        }
        UserInput();
        UpdateGhost();
    }

    // Handles all user input for movement and rotation
    void UserInput()
    {
        if (!gameloop) return; // Prevent input when game is paused/upgrades

        // Quick Drop (Space Bar): Instantly drop Tetromino to the lowest valid position
        if (Input.GetKeyDown(KeyCode.Space))
        {
            while (IsValidPosition())
            {
                currentTetromino.transform.position += Vector3.down;
            }
            // Move back up one space after invalid position
            currentTetromino.transform.position += Vector3.up;

            // Lock Tetromino in grid, check for lines, and spawn new Tetromino immediately
            GetComponent<GridScript>().UpdateGrid(currentTetromino.transform);
            CheckForLines();
            SpawnTetromino();
            return; // Prevent further input this frame
        }
    
        // Horizontal Movement (Left/Right) with slow repeat
        // Initial key press
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveTetromino(Vector3.left);
            lastMoveTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveTetromino(Vector3.right);
            lastMoveTime = Time.time;
        }

        // Holding key for repeated movement
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (Time.time > lastMoveTime + repeatRate)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    MoveTetromino(Vector3.left);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    MoveTetromino(Vector3.right);
                }
                lastMoveTime = Time.time;
            }
        }

        // Rotation (Up Arrow): Rotate Tetromino 90 degrees
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentTetromino.transform.Rotate(0, 0, 90);
            if (!IsValidPosition())
            {
                // Undo rotation if position is invalid
                currentTetromino.transform.Rotate(0, 0, -90);
            }
        }

        // Fast Drop (Down Arrow): Increase drop speed while holding down
        if (Input.GetKey(KeyCode.DownArrow))
        {
            movementFrequency = 0.2f; // Faster drop
        }
        else
        {
            movementFrequency = 0.8f; // Default drop speed
        }
    }

    // Spawns a new Tetromino with weighted probabilities
    void SpawnTetromino()
    {
        if (gameloop)
        {
            int index = nextTetrominoIndex; // Use the previewed Tetromino
            currentTetromino = Instantiate(Tetrominos[index], new Vector3(5, 18, 0), Quaternion.identity);

            // Prepare next Tetromino
            nextTetrominoIndex = GetRandomTetrominoIndex();
            

            if (!IsValidPosition())
            {
                Gameover();
            }
        }
    }
    
int GetRandomTetrominoIndex()
{
    float rand = Random.value;
    float cumulative = 0f;
    for (int i = 0; i < tetrominoWeights.Length; i++)
    {
        cumulative += tetrominoWeights[i];
        if (rand < cumulative)
            return i;
    }
    return tetrominoWeights.Length - 1;
}
    // Handles game over logic and upgrade menu
    private void Gameover()
{
    gameloop = false;
    if (ghostTetromino != null)
        Destroy(ghostTetromino); // Destroy ghost explicitly
    grid.SetActive(false);
    GameoverPanel.SetActive(true);
    DeleteAllTetrominos();
}

    // Called from game over restart button
    public void RestartGame()
    {
        currentRound = 1;
        currentScore = 0;
        linesCompleted = 0;
        CalculateDifficulty();
        UpdateUI();
        gameloop = true;
        grid.SetActive(true);
        GameoverPanel.SetActive(false);
        SpawnTetromino();
    }

    // Call this when the goal score is reached
    void ShowShopScreen()
{
    gameloop = false;
    if (ghostTetromino != null)
        Destroy(ghostTetromino); // Destroy ghost explicitly
    DeleteAllTetrominos(); // Clean up all Tetromino clones
    grid.SetActive(false);
    UpgradePanel.SetActive(true);
}

    // Called from shop continue button to start next round
    public void StartNextRound()
    {
        currentRound++;
        currentScore = 0;
        linesCompleted = 0;
        CalculateDifficulty();
        UpdateUI();
        gameloop = true;
        grid.SetActive(true);
        UpgradePanel.SetActive(false);
        SpawnTetromino();
    }

    // Utility to delete all Tetromino clones in the scene
    void DeleteAllTetrominos()
    {
        foreach (var tetromino in GameObject.FindGameObjectsWithTag("Tetromino"))
        {
            Destroy(tetromino);
        }
        if (ghostTetromino != null)
        {
            Destroy(ghostTetromino);
            ghostTetromino = null;
        }
        if (previewTetromino != null)
            Destroy(previewTetromino);
    }

    // Moves the current Tetromino in the given direction
    void MoveTetromino(Vector3 direction)
    {
        currentTetromino.transform.position += direction;
        if (!IsValidPosition())
        {
            // Undo move if position is invalid
            currentTetromino.transform.position -= direction;
            if (direction == Vector3.down)
            {
                // Lock Tetromino in grid, check for lines, and spawn new Tetromino
                GetComponent<GridScript>().UpdateGrid(currentTetromino.transform);
                CheckForLines();
                SpawnTetromino();
            }
        }
    }

    // Checks if the current Tetromino's position is valid
    bool IsValidPosition()
    {
        return GetComponent<GridScript>().IsValidPosition(currentTetromino.transform);
    }

    // Checks for and clears completed lines
    void CheckForLines()
    {
        int linesCleared = gridScript.CheckForLinesAndReturnCount();
        if (linesCleared > 0)
        {
            linesCompleted += linesCleared;
            switch (linesCleared)
            {
                case 1: currentScore += 40; break;
                case 2: currentScore += 100; break;
                case 3: currentScore += 300; break;
                case 4: currentScore += 1200; break;
                default: break;
            }
            UpdateUI();

            if (currentScore >= goalScore)
            {
                ShowShopScreen(); // Load shop scene and clean up Tetrominos
            }
        }
    }

    // Updates the ghost Tetromino to show where the current piece will land
    void UpdateGhost()
    {
        if (!gameloop || currentTetromino == null) return; // Prevent ghost update when game is over or no piece

        // Destroy previous ghost if it exists
        if (ghostTetromino != null)
            Destroy(ghostTetromino);

        // Instantiate a copy of the current Tetromino
        ghostTetromino = Instantiate(currentTetromino, currentTetromino.transform.position, currentTetromino.transform.rotation);

        // Disable all scripts and colliders on the ghost
        foreach (var comp in ghostTetromino.GetComponents<MonoBehaviour>())
            comp.enabled = false;
        foreach (var collider in ghostTetromino.GetComponents<Collider>())
            collider.enabled = false;

        // Set ghost material (make sure ghostMaterial is assigned in inspector)
        foreach (Transform mino in ghostTetromino.transform)
        {
            var renderer = mino.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = ghostMaterial;
        }

        // Move ghost down until it can't go further
        while (GetComponent<GridScript>().IsValidPosition(ghostTetromino.transform))
        {
            ghostTetromino.transform.position += Vector3.down;
        }
        // Move back up one space after invalid position
        ghostTetromino.transform.position += Vector3.up;
    }

    // Calculates goal score and movement speed based on current round
    void CalculateDifficulty()
    {
        goalScore = Mathf.RoundToInt(baseGoal * Mathf.Pow(goalMultiplier, currentRound - 1));
        movementFrequency = Mathf.Max(minMovementFrequency, baseMovementFrequency - (currentRound - 1) * speedIncrement);
    }

    // Call this method whenever you update score, goal, round, or lines
    void UpdateUI()
    {
        scoreText.text = currentScore.ToString();
        goalText.text = goalScore.ToString();
        roundText.text = currentRound.ToString();
        linesText.text = linesCompleted.ToString();
    }
}