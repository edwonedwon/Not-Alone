using UnityEngine;
using System.Collections;

public class Chase_Level : MonoBehaviour
{
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;

	private float timeTogether = 0.0f;
	
	public InkBouncerScript InkBouncer = null;
	
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

		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		if(p1finger == PlayerScript.FingerState.None && p2finger == PlayerScript.FingerState.None)
			return;
		
		//Find out if they are touching...
		Vector2 v1 = p1.transform.position;
		Vector2 v2 = p2.transform.position;	
		Vector2 vMiddle = v1-v2;
		float difference = vMiddle.magnitude;
		
		if(difference < 150.0f)
		{
			if(InkBouncer != null)
			{
				Vector2 inkPos = InkBouncer.transform.position;
				float differenceToInk = (inkPos-vMiddle).magnitude;
				if(differenceToInk < 50)
					InkBouncer.ReduceSpitRate(0.075f * Time.deltaTime);
			}
			else if(InkBouncer == null)
				GameLogicController.instance.MoveToNextLevel();		
		}
	}
}