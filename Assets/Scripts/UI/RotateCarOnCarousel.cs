/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class RotateCarOnCarousel : MonoBehaviour {

    public float frontRotation = 200f;
    public float backRotation = 270f;
    public float backScale = 0.8f;
    public float frontScale = 1f;

    private const float backX = 5f;
    private const float frontX = -5f;

    private Quaternion front;
    private Quaternion back;

    void Awake()
    {
        front = Quaternion.Euler(0f, frontRotation, 0f);
        back = Quaternion.Euler(0f, backRotation, 0f);
    }

	void Update () {
        front = Quaternion.Euler(0f, frontRotation, 0f);
        back = Quaternion.Euler(0f, backRotation, 0f);

        float t = (transform.position.z + backX) / (backX - frontX);
        transform.rotation = Quaternion.Lerp(back, front, t);
        transform.localScale = Vector3.one * Mathf.Lerp(backScale, frontScale, t);
	}
}
