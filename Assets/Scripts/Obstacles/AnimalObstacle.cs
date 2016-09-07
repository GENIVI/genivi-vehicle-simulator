/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public enum AnimalState { WALK_IN, IDLE, WALK_OUT, RAGDOLL }

public class AnimalObstacle : BaseObstacle
{
    public bool spawnAnywhere = true;
    public bool raycastToGround = false;
    public LayerMask raycastIgnore;

	[SerializeField] Animator animator;
	// Use this for initialization
	void Start () {
		animator.SetBool("Idle", true);
	}
	
	//Call ChooseAnimation(animChoice) where animChoice is an integer to enter an animation state
	//If 0 or 1 is chosen it sets the controller boolean to either idle = 0 or walk = 1
	//if 2 is chosen it triggers a leap and will transit back to whichever state was set prior using the 0 or 1 boolean
	void ChooseAnimation (int animChoice) {
		if (animChoice == 0) {
			animator.SetBool("Idle", true);
			animator.SetBool("Walk", false);
		} else if (animChoice == 1) {
			animator.SetBool("Walk", true);
			animator.SetBool("Idle", false);
		} else if (animChoice == 2) {
			animator.SetTrigger("Jump");
		}
	}
	
	public float walkDistance = 4f;

	private AnimalState state;


	void Awake()
	{

	}

	protected override void Update()
	{
		base.Update();

        if(raycastToGround && state != AnimalState.RAGDOLL) {
            RaycastHit hit;
            Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 50f, raycastIgnore);
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }

	}

	public override void OnTrigger()
	{
		StartCoroutine("WalkOverRoad");
	}

	IEnumerator WalkOverRoad()
	{
		var audio = GetComponent<AudioSource>();
		if(audio != null)
			audio.Play();
			
		state = AnimalState.WALK_IN;
		ChooseAnimation(1);
		Vector3 startPosition = transform.position;
		while((transform.position - startPosition).magnitude < walkDistance)
		{
			yield return new WaitForFixedUpdate();
		}

		state = AnimalState.IDLE;
		ChooseAnimation(0);
	}


	IEnumerator Jump()
	{
		ChooseAnimation(2);
		yield return null;
	}

	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Player" && state != AnimalState.RAGDOLL)
		{
			StopCoroutine("WalkOverRoad");
			ChooseAnimation(0);

			state = AnimalState.RAGDOLL;

			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().freezeRotation = false;

			var vFinal = collision.rigidbody.mass * collision.relativeVelocity / (GetComponent<Rigidbody>().mass + collision.rigidbody.mass);
			var impulse = vFinal * GetComponent<Rigidbody>().mass;

			GetComponent<Rigidbody>().AddForceAtPosition(impulse, collision.contacts[0].point, ForceMode.Impulse);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.root.name == "DeerTrigger")
		{
			StartCoroutine(Jump());
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
	}

    public override bool SpawnAnywhere()
    {
        return spawnAnywhere;
    }
}

