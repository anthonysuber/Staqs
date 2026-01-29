// ScrollingBackground.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple script to create a scrolling background effect by
/// manipulating the UV coordinates of a RawImage.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class ScrollingBackground : MonoBehaviour
{
    // The RawImage component that this script will control.
    // We get this automatically from the required component.
    private RawImage rawImage;

    [Tooltip("The speed at which the texture scrolls.")]
    public float scrollSpeed = 0.1f;

    [Tooltip("The direction of the scroll. " +
             "x=1 for right, x=-1 for left. " +
             "y=1 for up, y=-1 for down.")]
    public Vector2 scrollDirection = new Vector2(0, -1); // Default to scrolling down

    void Awake()
    {
        // Get a reference to the RawImage component attached to this GameObject.
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        // Check if the RawImage and its texture are valid.
        if (rawImage == null || rawImage.texture == null)
        {
            Debug.LogWarning("RawImage or its texture is not assigned on " + gameObject.name);
            return;
        }

        // Get the current UV rectangle.
        Rect uvRect = rawImage.uvRect;

        // Calculate the new UV position based on time, speed, and direction.
        // Time.deltaTime ensures the scroll speed is frame-rate independent.
        uvRect.x += scrollDirection.x * scrollSpeed * Time.deltaTime;
        uvRect.y += scrollDirection.y * scrollSpeed * Time.deltaTime;

        // Apply the new UV rectangle to the RawImage.
        // This causes the texture to appear to scroll.
        rawImage.uvRect = uvRect;
    }
}
