using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	public float speed;

	private Transform _trans;

	// Use this for initialization
	void Start () {
		_trans = transform;
	}
	
	// Update is called once per frame
	void Update () {

        transform.Translate(new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, Input.GetAxis("Vertical") * speed * Time.deltaTime), Space.World);

		//Vector3 direction = Vector3.zero;

		//if (Input.GetKey(KeyCode.W)) {
		//	direction.y = 1;
		//}
		//else if (Input.GetKey(KeyCode.S)) {
		//	direction.y = -1;
		//}

		//if (Input.GetKey(KeyCode.A)) {
		//	direction.x = -1;
		//}
		//else if (Input.GetKey(KeyCode.D)) {
		//	direction.x = 1;
		//}

		//direction = speed * direction.normalized;

		//_trans.position = _trans.position + direction;
	}
}
