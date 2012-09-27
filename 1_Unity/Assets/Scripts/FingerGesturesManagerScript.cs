using UnityEngine;
using System.Collections;

public class FingerGesturesManagerScript : MonoBehaviour {
	
	public GameObject touchPrefab;
	public GameObject twoTouchPrefab;
	
	public float zOffset;
	
	Vector3 moveToPos;
	
	void Start () {
		
	}
	
	void Update () {
		
	}
	
	#region Events
	
	void OnEnable () {
		FingerGestures.OnFingerDown += OnFingerDown; 
	}
	
	void OnDisable () {
		FingerGestures.OnFingerDown -= OnFingerDown;
	}
	
	#endregion
	
	#region Finger Gestures
	
	void OnFingerDown (int finger, Vector2 pos)
	{
		
		GameObject new_touch_prefab = null;
		if (Network.isClient || Network.isServer) 
		{
			// convert touch position to world position
 			Vector3 posWorld = Camera.main.ScreenToWorldPoint(new Vector3(pos.x,pos.y,0));
			
			// instantiate a touchPrefab at finger position and make it = new_touch_prefab
			new_touch_prefab = (GameObject)Network.Instantiate(touchPrefab, new Vector3 (posWorld.x,posWorld.y,zOffset), transform.rotation, 0);
			
			// print the viewID of the touch
			print("instantiated touch with id: " + new_touch_prefab.networkView.viewID);
		}
		
		// if there is a touch prefab in the scene
		if (new_touch_prefab != null)
		{
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
					
					// Spawn 
//					TwoTouchPrefab_Spawn(halfway_pos, new_touch_prefab.networkView, tps.networkView);
					
				}
			}
		}			
	}
	
	
	
//	public /*static*/ void TwoTouchPrefab_Spawn(Vector3 pos, NetworkView touch1, NetworkView touch2)
//	{
//		NetworkViewID nvID = Network.AllocateViewID();	
//		networkView.RPC("TwoTouchPrefab_Spawn_RPC", RPCMode.AllBuffered, Time.time, pos, nvID, touch1.viewID, touch2.viewID);		
//	}
	
	[RPC]
	public /*static*/ void TwoTouchPrefab_Spawn_RPC(float triggered_seconds, Vector3 pos, NetworkViewID twoTouchPrefab_view_id, NetworkViewID touch1_view_id, NetworkViewID touch2_view_id)
	{
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
	
	}
	
	#endregion
}
