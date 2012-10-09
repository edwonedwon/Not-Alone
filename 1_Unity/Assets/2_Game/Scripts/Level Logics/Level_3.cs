using UnityEngine;
using System.Collections;

public class Level_3 : MonoBehaviour
{
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	private float levelRunTime = 0.0f;
	
	void Start ()
	{
	
	}
	
	void Update ()
	{
		levelRunTime += Time.deltaTime;
		
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
		PlayerScript.FingerState p2finger = player2.MouseFingerDown();
		
		if(p1finger == PlayerScript.FingerState.Single && p2finger == PlayerScript.FingerState.Single)
		{
			player1.SetDoLinkInk(true);
			player2.SetDoLinkInk(true);
		}
		else
		{
			player1.SetDoLinkInk(false);
			player2.SetDoLinkInk(false);
		}
		
		GameObject[] blackHoles = GameObject.FindGameObjectsWithTag("blackhole");
		foreach(GameObject bh in blackHoles)
		{
			BlackHoleScript blackHoleScript = bh.GetComponent<BlackHoleScript>();
			if(Network.isServer)
				player1.UpdateAgainstBlackHole(blackHoleScript);
			else if(Network.isClient)
				player2.UpdateAgainstBlackHole(blackHoleScript);			
		}
		
		if(levelRunTime > 5.0f && blackHoles.Length == 0)
			GameLogicController.instance.MoveToNextLevel();
	}
}