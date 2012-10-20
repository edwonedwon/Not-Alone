using UnityEngine;
using System.Collections;

public class ContinuousLevel : MonoBehaviour
{
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	public FluidFieldGenerator fluidField = null;
	
	//game-style: clear-screen-togetherness	
	private float timeTogetherNotMoving = 0.0f;
	
	
	//game-style: tapping on screen calling out plankton
	
	
	void Start ()
	{
	
	}
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	
	
	public void DoSomethingHere(float additionalRot, int playerNm)
	{
		networkView.RPC ("SetRotationSpeed", RPCMode.All, additionalRot, playerNm);
	}
	
	[RPC]
	void FunctionToDoSomething(float newRotationSpeed, int playerNm)
	{
		
	}
	
	
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		//int mouseState = (int)currentFingerState;
		//Vector3 pos = transform.position;
		
		if(stream.isWriting)
		{			
			//stream.Serialize(ref pos);
			//stream.Serialize(ref mouseState);
		}
		else if(stream.isReading)
		{
			//stream.Serialize(ref pos);
			//stream.Serialize(ref mouseState);			
		}
		
		//currentFingerState = (FingerState)mouseState;
		//transform.position = pos;
	}
	
	
	
	void ClearAllGameEntitiesOut()
	{
		GameObject[] soundBuoys = GameObject.FindGameObjectsWithTag("buoy");
		fluidField.IncreaseSpiritParticles(-1, 0);
		
		foreach(GameObject go in soundBuoys)
		{
			GameObject.Destroy(go);	
		}
		
		
		fluidField.ExplosionFromTheCentre();		
	}
	
	void FixedUpdate ()
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
	
		
		SoundBuoyScript.CheckForRiverCompletion();
		
		
		if(p1 != null)
		{
			if(SoundBuoyScript.WorldBuoysList.Count == 3)
				ClearAllGameEntitiesOut();
			return;
		}
		
		
		if(p1 == null || p2 == null)
			return;
		
		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		if(p1finger == PlayerScript.FingerState.None && p2finger == PlayerScript.FingerState.None)
			return;
		
		//Find out if they are touching...
		Vector3 v1 = p1.transform.position;
		Vector3 v2 = p2.transform.position;		
		
		float difference = (v1-v2).magnitude;		
		
		if(difference < 50.0f)
		{
			timeTogetherNotMoving += Time.deltaTime;			
			if(timeTogetherNotMoving > 5.0f)
			{
				ClearAllGameEntitiesOut();
				//GameLogicController.instance.MoveToNextLevel();		//not anymore!
			}
		}
		else
		{
			if(timeTogetherNotMoving >= 1.0f)
			{
				player1.SetDoLinkInk(true);
				player2.SetDoLinkInk(true);
			}
			timeTogetherNotMoving = 0.0f;
		}
		
		if(p1finger != PlayerScript.FingerState.Single || p2finger != PlayerScript.FingerState.Single)
		{
			player1.SetDoLinkInk(false);
			player2.SetDoLinkInk(false);
		}
		
		
		
		
	}
}