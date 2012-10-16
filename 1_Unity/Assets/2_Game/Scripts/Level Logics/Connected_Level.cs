using UnityEngine;
using System.Collections;

public class Connected_Level : MonoBehaviour
{
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	private float levelRunTime = 0.0f;
	private float linkTimeEnabled = 5.0f;
	
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
		
		Vector2 v1 = player1.transform.position;
		Vector2 v2 = player2.transform.position;
		
		if(linkTimeEnabled > 0.0f &&  (p1finger == PlayerScript.FingerState.Single && p2finger == PlayerScript.FingerState.Single))
		{
			player1.SetDoLinkInk(true);
			player2.SetDoLinkInk(true);
		}
		else
		{
			player1.SetDoLinkInk(false);
			player2.SetDoLinkInk(false);
		}
		
		linkTimeEnabled -= Time.deltaTime;
		if(linkTimeEnabled < 0)
			linkTimeEnabled = 0;
				
		GameObject[] connectors = GameObject.FindGameObjectsWithTag("line connector");
		int id = 0;
		
		foreach(GameObject lc in connectors)
		{
			LineConnectorScript linesscript = lc.GetComponent<LineConnectorScript>();
			Vector2 lcp = lc.transform.position;
			
			float dist1 = (lcp-v1).magnitude;
			float dist2 = (lcp-v2).magnitude;			
			
			if(id == 0 && dist1 < 30)
				linesscript.LineLinkEnabled = true;
			else if(id == 0 && dist1 > 10)
				linesscript.LineLinkEnabled = false;
			
			if(id == 1 && dist2 < 30)
				linesscript.LineLinkEnabled = true;
			else if(id == 1 && dist2 > 10)
				linesscript.LineLinkEnabled = false;
			
			if(linesscript.LineLinkEnabled)
				linkTimeEnabled = 5;
			
			if(++id > 1)
				id = 0;
		}
		
		if(levelRunTime > 5.0f && connectors.Length == 0)
			GameLogicController.instance.MoveToNextLevel();
	}	
	
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		float ltimeEnabled = linkTimeEnabled;		
		
		if(stream.isWriting)
		{			
			stream.Serialize(ref ltimeEnabled);
		}
		else if(stream.isReading)
		{
			stream.Serialize(ref ltimeEnabled);			
		}
		
		linkTimeEnabled = ltimeEnabled;
	}
}