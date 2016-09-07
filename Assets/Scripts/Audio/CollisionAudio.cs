/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public enum CollisionStrength { SOFT, MEDIUM, HARD }

public class CollisionAudio : MonoBehaviour {

    public AudioClip[] sidesHard;
    public AudioClip[] frontHard;
    public AudioClip[] sidesMed;
    public AudioClip[] frontMed;
    public AudioClip[] sidesSoft;
    public AudioClip[] frontSoft;

    private AudioSource source;

    public float hardVelocity;
    public float mediumVelocity;

    public float hardVol;
    public float mediumVol;
    public float softVol;

    public void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private static AudioClip SelectRandom(AudioClip[] clips)
    {
        return clips[Mathf.RoundToInt(Random.Range(0, clips.Length - 1))];
    }

    public void PlayCollision(Vector3 worldPosition, float relativeVelocity)
    {
        Vector3 collisionVec = (worldPosition - transform.root.position);
        collisionVec.y = 0;
        Vector3 flatForward = transform.root.forward;
        flatForward.y = 0;

        if (collisionVec == Vector3.zero || flatForward == Vector3.zero)
            return;

        bool front = Mathf.Abs(Vector3.Dot(collisionVec.normalized, flatForward.normalized)) > 0.5f;

        transform.position = worldPosition;

        CollisionStrength strength;
        float vol = 1f;
        if (relativeVelocity > hardVelocity)
        {
            strength = CollisionStrength.HARD;
            vol = hardVol;
        }
        else if (relativeVelocity > mediumVelocity)
        {
            strength = CollisionStrength.MEDIUM;
            vol = mediumVol;
        }
        else
        {
            strength = CollisionStrength.SOFT;
            vol = softVol;
        }



        AudioClip clip = frontSoft[0];
        if (front)
        {
            switch(strength)
            {
                case CollisionStrength.HARD:
                    clip = SelectRandom(frontHard);
                    break;
                case CollisionStrength.MEDIUM:
                    clip = SelectRandom(frontMed);
                    break;
                case CollisionStrength.SOFT:
                    clip = SelectRandom(frontSoft);
                    break;
            }         
        } else
        {
            switch (strength)
            {
                case CollisionStrength.HARD:
                    clip = SelectRandom(sidesHard);
                    break;
                case CollisionStrength.MEDIUM:
                    clip = SelectRandom(sidesMed);
                    break;
                case CollisionStrength.SOFT:
                    clip = SelectRandom(sidesSoft);
                    break;
            }
        }
        
        source.PlayOneShot(clip, vol);

    }
}
