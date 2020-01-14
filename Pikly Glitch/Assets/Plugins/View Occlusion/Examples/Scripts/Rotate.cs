using UnityEngine;
using System.Collections;
using UnityEngine.Animations;

public class Rotate : MonoBehaviour {

	public Transform _trans;
	public float speed;
	public Axis axis = Axis.Z;

	void Start () {
		_trans = transform;
	}


	void Update () {
		Vector3 angle = _trans.eulerAngles;
		switch (axis)
		{
			case Axis.X:
				angle.x += speed * Time.deltaTime;
				break;
			case Axis.Y:
				angle.y += speed * Time.deltaTime;
				break;
			default:
				angle.z += speed * Time.deltaTime;
				break;
		}

		_trans.eulerAngles = angle;
	}
}
