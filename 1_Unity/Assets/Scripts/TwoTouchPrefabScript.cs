using UnityEngine;
using System.Collections;

public class TwoTouchPrefabScript : MonoBehaviour 
{
	
	// NB: http://answers.unity3d.com/questions/11113/buffered-rpcs-from-networkinstantiate-not-removed.html
	// Network.Destroy is not buffered as is Network.Instantiate. this is why previouslydestroyed objects appear inappropriately!
	public NetworkViewID touch1_view_id;
	public NetworkViewID touch2_view_id;
	
	public float TriggeredSeconds; // time in seconds at which the event was triggered, as reported by Time.time
	 
	//void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) 
	//{		
        
	//	stream.Serialize(ref TriggeredSeconds);
		
		// do I really need different behavior for receiving?
		/*
		//int triggered_seconds = 0;
        if (stream.isWriting) {
            //health = currentHealth;
            stream.Serialize(ref TriggeredSeconds);
        } else {
            stream.Serialize(ref TriggeredSeconds);
            //currentHealth = health;
        }*/
    //}
	
	
	
	// Use this for initialization
	void Start () 
	{
		//TriggeredSeconds = Time.time;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
		
	/*	// check that objects still appears
		if (null == NetworkView.Find(touch1_view_id) || null == NetworkView.Find(touch2_view_id))
			Destroy(this.gameObject);

		transform.localScale += new Vector3(.1f, .1f, 0);
	*/
		
		
		
		/*if (Time.time - TriggeredSeconds > 5)
		{
			this.transform.localScale.
		}*/
	
		
	
	}
}
