using UnityEngine;
using System.Collections;

public class ChasePlayer : MonoBehaviour {

	Transform playertransform;
	public float movespeed;
	public float followdistance;

	// Use this for initialization
	void Start () {
		playertransform = GameObject.FindGameObjectWithTag ("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(transform.position, playertransform.position) < followdistance) {
			transform.position = Vector3.MoveTowards (transform.position, playertransform.position, movespeed * Time.deltaTime);
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		// TODO: Stop enemies from floating through box colliders
	}
}
