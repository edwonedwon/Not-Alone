using UnityEngine;
using System.Collections;

public class ET_Finger_Touch : MonoBehaviour
{
	
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(p1 == null)
		{
			p1 = GameObject.FindGameObjectWithTag("PLAYER1");
			if(p1 != null)
				player1 = p1.GetComponent<PlayerScript>();
		}				
		if(p2 == null)
		{
			p2 = GameObject.FindGameObjectWithTag("PLAYER2");
			if(p2 != null)
				player2 = p2.GetComponent<PlayerScript>();
		}
		
		
		if(p1 == null || p2 == null)
			return;
		
		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		//Find out if they are touching...
		Vector3 v1 = p1.transform.position;
		Vector3 v2 = p2.transform.position;		
		Vector3 vD = (v2-v1);
		float difference = vD.magnitude;		
		
		Vector3 halfwayPoint = v1 + (vD * 0.5f);
		transform.position = halfwayPoint;
		
		ParticleSystem psystem = GetComponent<ParticleSystem>();		
		if(difference < 50.0f && (p1finger == 0 || p2finger == 0))
		{
			psystem.enableEmission = true;			
			psystem.startSize += 10.0f;
			if(psystem.startSize > 20000)
				psystem.startSize = 20000;	
		}
		else
		{
			psystem.startSize -= 50.0f;
			if(psystem.startSize < 10)
				psystem.startSize = 10;
			psystem.enableEmission = false;
			
			
		}
	}
}
