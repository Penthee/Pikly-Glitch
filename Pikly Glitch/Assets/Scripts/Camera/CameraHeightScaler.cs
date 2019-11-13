using UnityEngine;
using System.Collections;

public class CameraHeightScaler : MonoBehaviour {

    public float minHeight = 1024, maxHeight = 1200;
    public float minCamSize = 15.7f, maxCamSize = 12;

	void Update () {
        //float screenHeight = Screen.height;

        //float heightDifference = maxHeight - minHeight;
        //float sizeDifference = minCamSize - maxCamSize;
        //float heightSizeRatio = sizeDifference / heightDifference;
        //float difference = screenHeight - minHeight;

        //float size = Camera.main.orthographicSize;

        ////float size = maxCamSize - ((screenHeight - minHeight) * heightSizeRatio);

        ////size = (screenHeight - minHeight) * (maxCamSize - minCamSize) / (minHeight - maxHeight) + minCamSize;

        //size = minCamSize - (difference * heightSizeRatio);

        //print(heightSizeRatio + ", " + size);

        //Camera.main.orthographicSize = size;

        

        float TARGET_WIDTH = 2560f;
        float TARGET_HEIGHT = 1400f;
        int PIXELS_TO_UNITS = 30; // 1:1 ratio of pixels to units

        float desiredRatio = TARGET_WIDTH / TARGET_HEIGHT;
        float currentRatio = (float)Screen.width / (float)Screen.height;

        if (currentRatio >= desiredRatio) {
            // Our resolution has plenty of width, so we just need to use the height to determine the camera size
            Camera.main.orthographicSize = TARGET_HEIGHT / 4 / PIXELS_TO_UNITS;
        } else {
            // Our camera needs to zoom out further than just fitting in the height of the image.
            // Determine how much bigger it needs to be, then apply that to our original algorithm.
            float differenceInSize = desiredRatio / currentRatio;
            Camera.main.orthographicSize = TARGET_HEIGHT / 4 / PIXELS_TO_UNITS * differenceInSize;
        }
    }
}
