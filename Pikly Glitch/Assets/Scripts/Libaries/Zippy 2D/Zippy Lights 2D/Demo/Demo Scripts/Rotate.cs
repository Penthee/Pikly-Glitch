using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	public float speed = 100;
	public bool rectTransform;
	
	RectTransform _rect;

	void Awake() {
		if (rectTransform)
			_rect = GetComponent<RectTransform>();
	}
	void Update () {
		if (rectTransform)
			_rect.Rotate(Time.deltaTime * speed * Vector3.forward);
		else
			transform.Rotate(Time.deltaTime * speed * Vector3.forward);
	}
}
