using UnityEngine;
using System.Collections;

public class ConstantRotate : MonoBehaviour {

    public float speed = 1f;

	// Update is called once per frame
	void Update () {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
	}
}
