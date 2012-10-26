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
		Application.targetFrameRate = 30;
		Time.fixedDeltaTime = 1.0f / 30.0f;	
	}
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	
	public void DoSomethingHere(float additionalRot, int playerNm)
	{
		networkView.RPC ("FunctionToDoSomething", RPCMode.All, additionalRot, playerNm);
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
		Network.SetSendingEnabled(0, false);
		Network.isMessageQueueRunning = false;
	
		if(Network.isServer)
		{
			//Destroy buoy objects
			GameObject[] soundBuoys = GameObject.FindGameObjectsWithTag("buoy");		
			foreach(GameObject go in soundBuoys)
				Network.Destroy(go);		
			
			//Remove all spirit particles
			fluidField.IncreaseSpiritParticles(-1, 0);
			
			//clear out the black holes!
			foreach(BlackHoleScript bhs in BlackHoleScript.WorldBlackHoles)
				Network.Destroy(bhs.gameObject);
		}
		
		//clear static lists
		SoundBuoyScript.RiverList.Clear();
		SoundBuoyScript.WorldBuoysList.Clear();
		BlackHoleScript.WorldBlackHoles.Clear();
		
		//drop an explosion in the fluid field!
		fluidField.ExplosionFromTheCentre();
		
		Network.isMessageQueueRunning = true;
		Network.SetSendingEnabled(0, true);
	}
	
	bool playTogetherAudio = false;
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
	
		bool doInkLink = false;
		bool riverComplete = SoundBuoyScript.CheckForRiverCompletion();
		
		if(p1 != null && p2 != null)
		{
			PlayerScript.FingerState p1finger = player1.MouseFingerDown();
			PlayerScript.FingerState p2finger = player2.MouseFingerDown();
			
			foreach(BlackHoleScript bh in BlackHoleScript.WorldBlackHoles)
			{
				if(Network.isServer)
					player1.UpdateAgainstBlackHole(bh);
				else if(Network.isClient)
					player2.UpdateAgainstBlackHole(bh);			
			}
			
			
			bool canCreateRiverBetweenActivatedBuoys = false;
			int buoysWithFingersDownActivated = 0;
			
			//if(Network.isServer)
			{
				if(player1.DoLinkInk() && player1.DoLinkInk())
				{
					SoundBuoyScript buoy1 = null;
					SoundBuoyScript buoy2 = null;
					
					float minDist = 160.0f;
					float best1 = 9999.0f;
					float best2 = 9999.0f;
					
					Vector2 p1v = player1.transform.position;
					Vector2 p2v = player2.transform.position;
					
					foreach(SoundBuoyScript sbs in SoundBuoyScript.WorldBuoysList)
					{
						Vector2 sbspos = sbs.transform.position;
						float mag1 = (sbspos-p1v).magnitude;
						float mag2 = (sbspos-p2v).magnitude;
						
						if(mag1 < minDist && mag1 < best1)
						{
							best1 = mag1;
							buoy1 = sbs;
						}
						if(mag2 < minDist && mag2 < best2)
						{
							best2 = mag2;
							buoy2 = sbs;
						}
					}
					
					if(buoy1 != null && buoy2 != null)
					{
						if(buoy1 != buoy2)
						{	
							buoy1.ActivatedWithOther = buoy2;
							buoy2.ActivatedWithOther = buoy1;
						}
					}
				}
			}
			
			//Find out if they are touching...
			Vector3 v1 = p1.transform.position;
			Vector3 v2 = p2.transform.position;
			
			float difference = (v1-v2).magnitude;		
			
			if(difference < 50.0f)
			{
				if(p1finger == PlayerScript.FingerState.Single && p2finger == PlayerScript.FingerState.Single)
				{
					timeTogetherNotMoving += Time.deltaTime;
				}
				if(timeTogetherNotMoving > 1.0f && !playTogetherAudio)
				{
					playTogetherAudio = true;
					audio.Play();
				}
				if(timeTogetherNotMoving > 5.0f)
				{
					playTogetherAudio = false;
					timeTogetherNotMoving = -5;
					ClearAllGameEntitiesOut();
					//GameLogicController.instance.MoveToNextLevel();		//not anymore!
				}
			}
			else
			{
				if(playTogetherAudio)
				{
					playTogetherAudio = false;
					audio.Stop();
				}
				
				if(timeTogetherNotMoving >= 1.0f)
				{
					
					player1.SetDoLinkInk(true);
					player2.SetDoLinkInk(true);
				}
				timeTogetherNotMoving = 0.0f;
			}
			
			if(p1finger != PlayerScript.FingerState.Single || p2finger != PlayerScript.FingerState.Single)
			{
				timeTogetherNotMoving = 0;
				player1.SetDoLinkInk(false);
				player2.SetDoLinkInk(false);
			}
		}
	}
}