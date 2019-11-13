using UnityEngine;
using System.Collections;

public class MoveTowards : MonoBehaviour {

	void Start () {
	
	}
	
	void Update () {

        transform.Translate(-transform.position * 10 * Time.deltaTime);
	}
}
