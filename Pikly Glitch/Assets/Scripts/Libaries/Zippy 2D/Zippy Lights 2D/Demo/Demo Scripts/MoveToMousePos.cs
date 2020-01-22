using UnityEngine;
using System.Collections;
public class MoveToMousePos : MonoBehaviour {
	Vector3 newPosition;
	bool canMove = true;
	public Transform spriteTransform;
	public float moveSpeed = 25, accuracy = 0.25f, minDistance = 0.25f;
	float speed;

	void Start() {
		newPosition = transform.position;
	}


	void Update() {

		if (Input.GetMouseButton(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				newPosition = hit.point;
				newPosition.z = 0;
			}
		}

		//float angle = 0;

		//Vector3 relative = transform.InverseTransformPoint(newPosition);
		//angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
		//transform.Rotate(0, 0, -angle* Time.deltaTime * 50);

		//var rotation = Quaternion.LookRotation(Vector3.forward);
		//spriteTransform.rotation = rotation;

	

		if (canMove && Vector2.Distance(transform.position, newPosition) < accuracy) {
			canMove = false;
			
		} else if(!canMove && Vector2.Distance(transform.position, newPosition) > minDistance) {
			canMove = true;
			
		}

		transform.position += (newPosition - transform.position) * speed * Time.deltaTime * moveSpeed * .1f;

		if (canMove) {
			speed = Mathf.Clamp01(speed + .05f * Time.deltaTime * moveSpeed);
		} else {
			speed = Mathf.Clamp01(speed - .1f * Time.deltaTime * moveSpeed);
		}
	}
}