using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazePointPool : MonoBehaviour {
    public static GazePointPool gazePointPooler;

    public Image gazePointImage;
    private List<Image> pool = new List<Image>(0);
    private readonly int initalPool = 25;

    public void addGazePoint(RectTransform canvasRect, Camera camera, Vector2 gazePoint) {
        Image i = getPooledGazePoint();
        i.gameObject.SetActive(true);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, gazePoint, camera, out Vector2 localpoint)) {

            i.rectTransform.localPosition = localpoint;
        }
    }

    public void preparePool() {
        if (pool.Count < initalPool) {
            return;
        }
        for (int i = 0; i < initalPool; i++) {
            Image img = Instantiate(gazePointImage, transform);
            img.gameObject.SetActive(false);
        }
    }

    private void Awake() {
        gazePointPooler = this;
    }

    private Image getPooledGazePoint() {
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

    public void ClearScreen() {
        foreach (Image i in pool) {
            if (i.gameObject.activeInHierarchy) {
                i.gameObject.SetActive(false);
            }
        }
    }

    public void FlushPool() {
        foreach (Image i in pool) {
            Destroy(i);
        }
        pool.Clear();
    }
}
