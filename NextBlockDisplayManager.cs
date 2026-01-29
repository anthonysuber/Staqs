// NextBlockDisplayManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NextBlockDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nextBlockSlotPrefab; // Assign your NextBlockSlotPrefab here
    public Transform nextBlocksContainer; // Assign the NextBlockPanel (which has the Vertical Layout Group)

    private List<GameObject> currentDisplayedBlocks = new List<GameObject>();

    // This method will be called by your GameManager to update the display
    // 'blockTypes' would be a list of integers or enums representing the upcoming Tetromino types
    public void UpdateNextBlocksDisplay(List<int> nextBlockTypes)
    {
        // 1. Clear existing displayed blocks
        foreach (GameObject blockGO in currentDisplayedBlocks)
        {
            Destroy(blockGO); // Destroy the GameObject
        }
        currentDisplayedBlocks.Clear(); // Clear the list reference

        // 2. Instantiate new blocks based on the provided list
        foreach (int blockType in nextBlockTypes)
        {
            if (nextBlockSlotPrefab == null)
            {
                Debug.LogError("NextBlockSlotPrefab is not assigned in NextBlockDisplayManager!");
                return;
            }

            GameObject newSlot = Instantiate(nextBlockSlotPrefab, nextBlocksContainer);
            currentDisplayedBlocks.Add(newSlot);

            // Here, you would update the visual of the 'newSlot'
            // For now, let's just change its background color based on blockType
            Image slotImage = newSlot.GetComponent<Image>();
            if (slotImage != null)
            {
                // This is a placeholder. You'd map 'blockType' to actual Tetris colors.
                // Example: 0=I, 1=O, 2=T, etc.
                switch (blockType)
                {
                    case 0: slotImage.color = new Color(0f, 1f, 1f, 0.7f); break; // Cyan (I-block)
                    case 1: slotImage.color = new Color(1f, 1f, 0f, 0.7f); break; // Yellow (O-block)
                    case 2: slotImage.color = new Color(0.5f, 0f, 0.5f, 0.7f); break; // Purple (T-block)
                    case 3: slotImage.color = new Color(0f, 1f, 0f, 0.7f); break; // Green (S-block)
                    case 4: slotImage.color = new Color(1f, 0f, 0f, 0.7f); break; // Red (Z-block)
                    case 5: slotImage.color = new Color(0f, 0f, 1f, 0.7f); break; // Blue (J-block)
                    case 6: slotImage.color = new Color(1f, 0.65f, 0f, 0.7f); break; // Orange (L-block)
                    default: slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); break; // Default gray
                }
            }
            else
            {
                Debug.LogWarning("NextBlockSlotPrefab does not have an Image component.");
            }

            // If you had a Text child for numbering:
            // Text numberText = newSlot.GetComponentInChildren<Text>();
            // if (numberText != null)
            // {
            //     numberText.text = $"{currentDisplayedBlocks.Count}";
            // }
        }
    }

    // Example usage (for testing purposes, you'd remove this later)
    void Start()
    {
        // Simulate showing 1 next block
        // UpdateNextBlocksDisplay(new List<int> { 0 }); // Show an I-block

        // Simulate showing 3 next blocks after an upgrade
        // StartCoroutine(TestDisplayChange());
    }

    /*
    IEnumerator TestDisplayChange()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Simulating upgrade: showing 3 next blocks.");
        UpdateNextBlocksDisplay(new List<int> { 1, 2, 3 }); // Show O, T, S blocks
        yield return new WaitForSeconds(3f);
        Debug.Log("Simulating another change: showing 2 next blocks.");
        UpdateNextBlocksDisplay(new List<int> { 4, 5 }); // Show Z, J blocks
    }
    */
}
