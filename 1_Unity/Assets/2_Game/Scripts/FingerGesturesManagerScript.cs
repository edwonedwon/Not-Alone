using UnityEngine;
using System.Collections;

public class FingerGesturesManagerScript : MonoBehaviour {
		
	private GameObject player1 = null;
	private GameObject player2 = null;
	
	void Start ()
	{
		
	}
	
	void Update ()
	{
		if(player1 == null)
			player1 = GameObject.FindGameObjectWithTag("PLAYER1");
		if(player2 == null)
			player2 = GameObject.FindGameObjectWithTag("PLAYER2");
	}
	
	#region Events
	
	void OnEnable ()
	{
		FingerGestures.OnFingerDown += OnFingerDown;
		FingerGestures.OnFingerUp += OnFingerUp;
		FingerGestures.OnFingerMove += OnFingerMove;
	}
	
	void OnDisable ()
	{
		FingerGestures.OnFingerDown -= OnFingerDown;
		FingerGestures.OnFingerUp -= OnFingerUp;
		FingerGestures.OnFingerMove -= OnFingerMove;
	}
	
	#endregion
	
	#region Finger Gestures
	

	void OnFingerUp (int fingerIndex, Vector2 fingerPos, float timeHeldDown)
	{
		if(Network.isServer && player1 != null)
			player1.GetComponent<PlayerScript>().OnPlayerFingerUp(fingerIndex,  fingerPos, timeHeldDown);
		else if(Network.isClient && player2 != null)
			player2.GetComponent<PlayerScript>().OnPlayerFingerUp(fingerIndex, fingerPos, timeHeldDown);
	}
	
	void OnFingerMove (int fingerIndex, Vector2 fingerPos)
	{
		if(Network.isServer && player1 != null)
			player1.GetComponent<PlayerScript>().OnPlayerFingerMove(fingerIndex, fingerPos);
		else if(Network.isClient && player2 != null)
			player2.GetComponent<PlayerScript>().OnPlayerFingerMove(fingerIndex, fingerPos);
	}
	
	void OnFingerDown (int fingerIndex, Vector2 fingerPos)
	{
		if(Network.isServer && player1 != null)
			player1.GetComponent<PlayerScript>().OnPlayerFingerDown(fingerIndex, fingerPos);
		else if(Network.isClient && player2 != null)
			player2.GetComponent<PlayerScript>().OnPlayerFingerDown(fingerIndex, fingerPos);
		
		/*
		Vector3 posWorld = Camera.main.ScreenToWorldPoint(new Vector3(pos.x,pos.y,0));
		if(Network.isServer)
		{
			DebugStreamer.message = "player1 finger down";
			if(player1 == null)
				player1 = (GameObject)Network.Instantiate(player1Prefab, new Vector3(posWorld.x, posWorld.y, zOffset), transform.rotation, 0);
		}
		else if(Network.isClient)
		{
			DebugStreamer.message = "two players finger down";
			
			if(player2 == null)
				player2 = (GameObject)Network.Instantiate(player2Prefab, new Vector3(posWorld.x, posWorld.y, zOffset), transform.rotation, 0);
		}
		
		
		
		if (Network.isClient || Network.isServer) 
		{
			// convert touch position to world position
 			
			
			// instantiate a touchPrefab at finger position and make it = new_touch_prefab
			new_touch_prefab = (GameObject)Network.Instantiate(touchPrefab, new Vector3 (posWorld.x,posWorld.y,zOffset), transform.rotation, 0);
			
			// print the viewID of the touch
			print("instantiated touch with id: " + new_touch_prefab.networkView.viewID);
		}*/
		
		/*		
		// for each tps (tps = all touch prefabs in scene)
		foreach(TouchPrefabScript tps in GameObject.FindSceneObjectsOfType(typeof(TouchPrefabScript)))
		{
			// if touch is other players
			if (!tps.networkView.isMine)
			{
				// get distance between my touch and other players touch
				float distance = Vector3.Distance(tps.transform.position, new_touch_prefab.transform.position);
					
				print("Distance between touches is " + distance);
				
				// halfway_pos = half of (my touch position + their touch position)
				Vector3 halfway_pos = .5f*(tps.transform.position + new_touch_prefab.transform.position);
				DebugStreamer.message = "halfway_pos: " + halfway_pos.x.ToString() + ", " + halfway_pos.y.ToString();
				//TwoTouchPrefab_Spawn(halfway_pos, new_touch_prefab.networkView, tps.networkView);
				
			}
		}
		*/		
	}
	
	
//	public /*static*/ void TwoTouchPrefab_Spawn(Vector3 pos, NetworkView touch1, NetworkView touch2)
//	{
//		NetworkViewID nvID = Network.AllocateViewID();	
//		networkView.RPC("TwoTouchPrefab_Spawn_RPC", RPCMode.AllBuffered, Time.time, pos, nvID, touch1.viewID, touch2.viewID);		
//	}
	
	
	[RPC]
	public /*static*/ void TwoTouchPrefab_Spawn_RPC(float triggered_seconds, Vector3 pos, NetworkViewID twoTouchPrefab_view_id, NetworkViewID touch1_view_id, NetworkViewID touch2_view_id)
	{
		/*
		Transform twoTouchClone;
		
		var go = GameObject.Instantiate(twoTouchPrefab, pos, Quaternion.identity);
		
		twoTouchClone = (go as GameObject).transform;
        
		NetworkView twoTouchPrefab_nView;
		twoTouchPrefab_nView = twoTouchClone.GetComponent<NetworkView>();
        twoTouchPrefab_nView.viewID = twoTouchPrefab_view_id;
		
		var twoTouchPrefabScript = twoTouchClone.GetComponent<TwoTouchPrefabScript>();
		twoTouchPrefabScript.touch1_view_id = touch1_view_id;
		twoTouchPrefabScript.touch2_view_id = touch2_view_id;
		twoTouchPrefabScript.TriggeredSeconds = triggered_seconds;
		*/
	}
	
	#endregion
}
