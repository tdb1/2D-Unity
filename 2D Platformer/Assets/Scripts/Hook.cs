﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
public class Hook : MonoBehaviour {
	//public Transform parentTransform;
	private RaycastHit hit;
	private Rigidbody rb;
	public bool hooked = false;
	private float momentum;
	public float speed;
	private LineRenderer line;
	Animator animator;
	private Vector3 ropeTarget;
	public GameObject hookCube;
	public GameObject ropeNode;
	private float ropeNodeSize;

	void Start () {
		line = GetComponent<LineRenderer> ();
		animator = this.GetComponent<Animator> ();
		ropeNodeSize = ropeNode.GetComponent<Collider> ().bounds.size.x;
		rb = this.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			Debug.DrawRay (cubeTransformPosition (), hookCube.transform.rotation * Vector3.forward * 10, Color.black, 2);

			if(Physics.Raycast(cubeTransformPosition(), hookCube.transform.rotation * Vector3.forward, out hit, 10, ~(1<<2))) {
				ropeTarget = hit.point;
				ropeTarget.z = 0;
				GameObject ropeTargetObject = (GameObject)Instantiate (ropeNode, hit.point, new Quaternion (0f, 0f, 0f, 0f));
//				Debug.Log ("hooked");
				hooked = true;
				animator.SetBool ("Hooked", true);
				//generateRope (hookCube, ropeTargetObject);
			}
		}
		if (Input.GetButtonUp ("Fire1")) {
			if (hooked) {
				hooked = false;
				animator.SetBool ("Hooked", false);
				rb.velocity = hookCube.transform.rotation * Vector3.forward * momentum;
			}
		}
		if (hooked) {
			hookCube.transform.LookAt (hit.point);
			line.SetPosition (0, cubeTransformPosition());
			line.SetPosition (1, hit.point);
		} else {
			// keep the distance locked, so that the player can swing like a pendulum
			var v3 = Input.mousePosition;
			v3.z = 0;
			v3 = Camera.main.ScreenToWorldPoint (v3);
			v3.z = 0;
			hookCube.transform.LookAt (v3);
			line.SetPosition (0, cubeTransformPosition());
			line.SetPosition (1, cubeTransformPosition());
		}
			
		if (hooked && Input.GetKey(KeyCode.E)) {
			momentum += Time.deltaTime * speed;
			//step = momentum * Time.deltaTime;
			rb.AddForce (Vector3.Normalize(hit.point - cubeTransformPosition()) * 10000 * Time.deltaTime);
			//parentTransform.position = Vector3.MoveTowards (parentTransform.position, hit.point, step);
		}
		else if (momentum >= 0) {
			momentum = 0;
		}
	}

	Vector3 cubeTransformPosition(){
		return new Vector3 (hookCube.transform.position.x, hookCube.transform.position.y, 0);
	}

	void OnAnimatorIK(){
		if (animator.GetBool ("Hooked")) {
			if (ropeTarget != null) {
				animator.SetLookAtWeight (0.4f);
				animator.SetLookAtPosition (ropeTarget);
				animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 0.8f);
				animator.SetIKPosition (AvatarIKGoal.LeftHand, ropeTarget);
			}
		}
	}

	void generateRope(GameObject origin, GameObject target){
		Debug.Log ("Generating Rope");
		if (Vector3.Distance (origin.transform.position, target.transform.position) > ropeNodeSize) {
			origin.transform.LookAt (target.transform);
			GameObject nextNode = (GameObject)Instantiate (ropeNode, origin.transform.forward * ropeNodeSize, new Quaternion (0f, 0f, 0f, 0f));
			nextNode.GetComponent<HingeJoint>().connectedBody = origin.GetComponent<Rigidbody>();
			generateRope (nextNode, target);
		}
		else{
			origin.GetComponent<HingeJoint>().connectedBody = target.GetComponent<Rigidbody>();
		}

	}
}
