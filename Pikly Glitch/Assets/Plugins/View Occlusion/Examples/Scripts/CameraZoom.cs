using UnityEngine;
using System.Collections;
using LOS;

public class CameraZoom : MonoBehaviour {

    public float zoomMagnitude, lerpSpeed, minZoom, maxZoom;

    new Camera camera;

	void Start () {
        camera = GetComponent<Camera>();
    }
	
	void Update () {
        float input = Input.GetAxis("Mouse ScrollWheel");

        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, camera.orthographicSize + (input * zoomMagnitude), lerpSpeed * Time.deltaTime);

        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, minZoom, maxZoom);
	}
}
