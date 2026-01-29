// StaqsMainMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI elements like Button, Text
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for Lists

public class StaqsMainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startGameButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button exitButton;
    public GameObject fallingBlockPrefab; // Assign a UI Image or Panel prefab here
    public RectTransform fallingBlocksContainer; // Assign an empty RectTransform as parent for falling blocks

    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Falling Block Settings")]
    public int numberOfBlocks = 30;
    public float minBlockSize = 20f;
    public float maxBlockSize = 60f;
    public float minFallSpeed = 200f; // Pixels per second
    public float maxFallSpeed = 500f; // Pixels per second
    public float spawnInterval = 0.5f; // How often to try and spawn a new block

    // Tetris-like colors (RGBA values for Unity's Color)
    private Color[] blockColors = new Color[]
    {
        new Color(0f, 1f, 1f, 0.3f),   // Cyan
        new Color(0f, 0f, 1f, 0.3f),   // Blue
        new Color(1f, 0.65f, 0f, 0.3f),// Orange
        new Color(1f, 1f, 0f, 0.3f),   // Yellow
        new Color(0f, 0.5f, 0f, 0.3f), // Green
        new Color(0.5f, 0f, 0.5f, 0.3f),// Purple
        new Color(1f, 0f, 0f, 0.3f)    // Red
    };

    void Start()
    {
        // Ensure UI elements are assigned
        if (startGameButton == null) Debug.LogError("Start Game Button not assigned!");
        if (optionsButton == null) Debug.LogError("Options Button not assigned!");
        if (creditsButton == null) Debug.LogError("Credits Button not assigned!");
        if (exitButton == null) Debug.LogError("Exit Button not assigned!");
        if (fallingBlockPrefab == null) Debug.LogError("Falling Block Prefab not assigned!");
        if (fallingBlocksContainer == null) Debug.LogError("Falling Blocks Container not assigned!");
        if (creditsPanel == null) Debug.LogError("Credits Panel not assigned!");


        // Add listeners to buttons
        startGameButton.onClick.AddListener(OnStartGameClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        creditsButton.onClick.AddListener(OnCreditsClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        if (optionsPanel != null)
        {
            Button closeButton = optionsPanel.transform.Find("CloseOptionsButton")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseOptionsClicked);
            }
            else
            {
                Debug.LogWarning("CloseOptionsButton not found as a child of OptionsPanel. Please ensure it's named 'CloseOptionsButton'.");
            }

            // Ensure options panel is hidden at start
            optionsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            Button closeCreditsButton = creditsPanel.transform.Find("CloseCreditsButton")?.GetComponent<Button>();
            if (closeCreditsButton != null)
            {
                closeCreditsButton.onClick.AddListener(OnCloseCreditsClicked);
            }
            else
            {
                Debug.LogWarning("CloseCreditsButton not found as a child of CreditsPanel. Please ensure it's named 'CloseCreditsButton'.");
            }

            // Ensure credits panel is hidden at start
            creditsPanel.SetActive(false);
        }

        // Start the falling block background effect
        StartCoroutine(SpawnFallingBlocks());
    }

    void OnStartGameClicked()
    {
        Debug.Log("Start Game clicked!");
        // Load your game scene here
        SceneManager.LoadScene("GameScene");
    }

    void OnOptionsClicked()
    {
        Debug.Log("Options clicked! Showing options Panel");
        // Open options panel/menu
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    void OnCloseOptionsClicked()
    {
        Debug.Log("Closing options panel.");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    void OnCreditsClicked()
    {
        Debug.Log("Credits clicked! Showing credits panel.");
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            // Optionally, hide the main menu buttons
            SetMainMenuButtonsActive(false);
        }
    }

    void OnCloseCreditsClicked()
    {
        Debug.Log("Closing credits panel.");
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
            // Optionally, re-show the main menu buttons
            SetMainMenuButtonsActive(true);
        }
    }

    void OnExitClicked()
    {
        Debug.Log("Exit clicked!");
        // Quit the application (only works in built games, not in editor)
        Application.Quit();
        // For editor, you might stop play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void SetMainMenuButtonsActive(bool isActive)
    {
        startGameButton.gameObject.SetActive(isActive);
        optionsButton.gameObject.SetActive(isActive);
        creditsButton.gameObject.SetActive(isActive);
        exitButton.gameObject.SetActive(isActive);
    }

    // Coroutine to spawn falling blocks continuously
    IEnumerator SpawnFallingBlocks()
    {
        while (true)
        {
            // Only spawn if we haven't reached the max number of blocks
            if (fallingBlocksContainer.childCount < numberOfBlocks)
            {
                CreateFallingBlock();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void CreateFallingBlock()
    {
        // Instantiate the block prefab
        GameObject blockGO = Instantiate(fallingBlockPrefab, fallingBlocksContainer);
        RectTransform blockRect = blockGO.GetComponent<RectTransform>();
        Image blockImage = blockGO.GetComponent<Image>();

        // Set random size
        float size = Random.Range(minBlockSize, maxBlockSize);
        blockRect.sizeDelta = new Vector2(size, size);

        // Set random horizontal position (within the container's width)
        float containerWidth = fallingBlocksContainer.rect.width;
        float randomX = Random.Range(-containerWidth / 2 + size / 2, containerWidth / 2 - size / 2);
        blockRect.anchoredPosition = new Vector2(randomX, fallingBlocksContainer.rect.height / 2 + size); // Start above container

        // Set random color
        blockImage.color = blockColors[Random.Range(0, blockColors.Length)];

        // Set random fall speed
        float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

        // Start the falling animation for this block
        StartCoroutine(FallAnimation(blockRect, fallSpeed));
    }

    IEnumerator FallAnimation(RectTransform blockRect, float speed)
    {
        // Get the bottom edge of the container in local space
        float containerBottomY = -fallingBlocksContainer.rect.height / 2 - blockRect.sizeDelta.y;

        while (blockRect.anchoredPosition.y > containerBottomY)
        {
            // Move the block down based on speed and time
            blockRect.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);
            yield return null; // Wait for next frame
        }

        // Once off-screen, reset its position to the top
        float containerTopY = fallingBlocksContainer.rect.height / 2 + blockRect.sizeDelta.y;
        float containerWidth = fallingBlocksContainer.rect.width;
        float randomX = Random.Range(-containerWidth / 2 + blockRect.sizeDelta.x / 2, containerWidth / 2 - blockRect.sizeDelta.x / 2);
        blockRect.anchoredPosition = new Vector2(randomX, containerTopY);

        // Restart the falling animation for continuous loop
        StartCoroutine(FallAnimation(blockRect, Random.Range(minFallSpeed, maxFallSpeed)));
    }
}
