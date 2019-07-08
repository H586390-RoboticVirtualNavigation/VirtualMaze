using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas to display the individual sample points. An object pool is used for efficiency.
/// Attach this to a canvas component.
/// </summary>
public class GazePointPool : MonoBehaviour {
    // Image to represent the gaze points (Prefab)
    public Image gazePointImage;

    // Private object pool holding the images
    private List<Image> pool = new List<Image>(0);

    private readonly int initalPool = 25;
    private int activeNow = 0;

    /// <summary>
    /// Positions a gaze point on the intended canvas.
    /// </summary>
    /// <param name="canvasRect">RectTransform of the canvas to place the image</param>
    /// <param name="camera">Camera which renders what the subject sees</param>
    /// <param name="gazePoint">X and Y position of the gaze sample</param>
    public void AddGazePoint(RectTransform canvasRect, Camera camera, Vector2 gazePoint) {
        Image i = GetPooledGazePoint();

        //calculate and posiiton the gaze point on the intended canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, gazePoint, camera, out Vector2 localpoint)) {
            i.gameObject.SetActive(true);
            activeNow++;

            i.rectTransform.localPosition = localpoint;
        }
    }

    /// <summary>
    /// Creates an initial pool of Image objects
    /// </summary>
    public void PreparePool() {
        if (pool.Count < initalPool) {
            return;
        }
        for (int i = 0; i < initalPool; i++) {
            Image img = Instantiate(gazePointImage, transform);
            img.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Retrieves a unused Image object
    /// </summary>
    /// <returns></returns>
    private Image GetPooledGazePoint() {
        Image result = null;
        foreach (Image i in pool) {
            if (!i.gameObject.activeInHierarchy) {
                result = i;
                break;
            }
        }
        if (result == null) {
            result = Instantiate(gazePointImage, transform);
            pool.Add(result);
        }
        return result;
    }

    /// <summary>
    /// Clears screen of all active images.
    /// </summary>
    public void ClearScreen() {
        foreach (Image i in pool) {
            if (i.gameObject.activeInHierarchy) {
                i.gameObject.SetActive(false);
            }
        }
        activeNow = 0;
    }

    /// <summary>
    /// Removes and destroys all Images in the pool.
    /// </summary>
    public void FlushPool() {
        foreach (Image i in pool) {
            Destroy(i);
        }
        pool.Clear();
    }
}
