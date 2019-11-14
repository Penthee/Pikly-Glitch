using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {
    public float timeout;

    void Start() {
        GameObject.Destroy(gameObject, timeout);
    }
}
