using UnityEngine;
using System.Collections;

public class ET_Finger_Touch : MonoBehaviour
{
	private GameObject player1 = null;
	private GameObject player2 = null;
	
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(player1 == null)
		{
			player1 = GameObject.FindGameObjectWithTag("PLAYER1");
			//if(p1 != null)
			//	player1 = p1.GetComponent<PlayerScript>();
		}				
		if(player2 == null)
		{
			player2 = GameObject.FindGameObjectWithTag("PLAYER2");
			//if(p2 != null)
			//	player2 = p2.GetComponent<PlayerScript>();
		}
		
		
		if(player1 == null || player2 == null)
			return;
		
		//Find out if they are touching...
		Vector3 v1 = player1.transform.position;
		Vector3 v2 = player2.transform.position;		
		Vector3 vD = (v2-v1);
		float difference = vD.magnitude;		
		
		Vector3 halfwayPoint = v1 + (vD * 0.5f);
		transform.position = halfwayPoint;
		
		ParticleSystem psystem = GetComponent<ParticleSystem>();		
		if(difference < 50.0f)
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
