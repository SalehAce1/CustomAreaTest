using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControl : MonoBehaviour {

	// Use this for initialization
	Animator _anim;
	void Start () 
	{
		_anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (transform.position.y < 0.94 || transform.position.y > 24.7) 
		{
			_anim.SetBool ("gnd", true);
		} else 
		{
			_anim.SetBool ("gnd", false);
		}
	}
}
