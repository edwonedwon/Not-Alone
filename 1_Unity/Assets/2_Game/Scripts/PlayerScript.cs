using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	public int zOffset;
	
	private int mouseFingerDown = -1;		//neither!
	private Vector2 lastMousePos = new Vector2(0,0);
	
	
	
	public int MouseFingerDown()
	{
		return mouseFingerDown;
	}
	
	
	void Start()
	{		
		//print("(start) touch with id: " + networkView.viewID);
		//particlesPS = GameObject.Find("Particles").GetComponent<ParticleSystem>();
		//particlesTF = GameObject.Find("Particles").GetComponent<Transform>();//		
		//particlesPS.particleSystem.enableEmission = false;
	}
	
	void Update()
	{
		
		
	}
	
	#region Events
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	
	#endregion
	
	
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		int mouseState = mouseFingerDown;
		Vector3 pos = transform.position;
		
		if(stream.isWriting)
		{			
			stream.Serialize(ref pos);
			stream.Serialize(ref mouseState);
		}
		else if(stream.isReading)
		{
			stream.Serialize(ref pos);
			stream.Serialize(ref mouseState);			
		}
		
		mouseFingerDown = mouseState;
		transform.position = pos;
	}
	
	
	#region Finger Gestures
	
	
	
	public void OnPlayerFingerDown (int finger, Vector2 pos)
	{
		mouseFingerDown = finger;	//either 0, or 1 i believe..
		lastMousePos = pos;
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(lastMousePos.x, lastMousePos.y, zOffset));
	}
	
	public void OnPlayerFingerMove (int finger, Vector2 pos)
	{
		mouseFingerDown = finger;	//either 0, or 1 i believe..
		lastMousePos = pos;
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(lastMousePos.x, lastMousePos.y, zOffset));
	}
	
	public void OnPlayerFingerUp (int finger, Vector2 pos, float timeHeldDown)
	{
		mouseFingerDown = -1;	//up!		
	}
	
	
	
	
	
	#endregion

}
