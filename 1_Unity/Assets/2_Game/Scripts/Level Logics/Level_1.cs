using UnityEngine;
using System.Collections;

public class Level_1 : MonoBehaviour
{
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;

	private float timeTogether = 0.0f;
	
	void Start ()
	{
	
	}
	
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
		
		int p1finger = player1.MouseFingerDown();
		int p2finger = player1.MouseFingerDown();
		
		if(p1finger == -1 && p2finger == -1)
			return;
		
		//Find out if they are touching...
		Vector3 v1 = p1.transform.position;
		Vector3 v2 = p2.transform.position;		
		
		float difference = (v1-v2).magnitude;		
		
		if(difference < 50.0f)
		{
			timeTogether += Time.deltaTime;
			
			if(timeTogether > 5.0f)
				GameLogicController.instance.MoveToNextLevel();		
		}
		else
		{
			timeTogether = 0.0f;
		}
	}
}