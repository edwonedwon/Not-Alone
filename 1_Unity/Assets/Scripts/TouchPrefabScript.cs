using UnityEngine;
using System.Collections;

public class TouchPrefabScript : MonoBehaviour {
	
	public int zOffset;
	
	ParticleSystem particlesPS;
	Transform particlesTF;
	FluidFieldGenerator fluidGenerator;
	
	void Start () {
		
//		print("(start) touch with id: " + networkView.viewID);

		particlesPS = GameObject.Find("Particles").GetComponent<ParticleSystem>();
		particlesTF = GameObject.Find("Particles").GetComponent<Transform>();
		
		particlesPS.particleSystem.enableEmission = false;
		
		
		fluidGenerator = GameObject.Find ("heightfield mesh").GetComponent<FluidFieldGenerator>();
		
	}
	
	void Update () {
				
	}
	
	#region Events
	
	void OnEnable () {
		FingerGestures.OnFingerMove += OnFingerMove; 
		FingerGestures.OnFingerUp += OnFingerUp;
		FingerGestures.OnFingerDown += OnFingerDown;
	}
	
	void OnDisable () {
		FingerGestures.OnFingerMove -= OnFingerMove;
		FingerGestures.OnFingerUp -= OnFingerUp;
		FingerGestures.OnFingerDown -= OnFingerDown;
	}
	
	#endregion
	
	#region Finger Gestures
	
	void OnFingerDown (int finger, Vector2 pos) {
	
	}
	
	void OnFingerMove (int finger, Vector2 pos)
	{
		//PhotonView pv = PhotonView.Get(this);
		Vector3 posWorld;
		posWorld = Camera.main.ScreenToWorldPoint(new Vector3(pos.x,pos.y,zOffset));
		
		// enable particle emission
		particlesPS.particleSystem.enableEmission = true;
		// make particles move same as touch
		particlesTF.position = new Vector3(posWorld.x,posWorld.y,zOffset);
		
		if (this.networkView.isMine)
		{
			transform.position = new Vector3(posWorld.x,posWorld.y,zOffset);
		}
		
			
		fluidGenerator.OnMouseDown(finger, pos);
		
		
	}
	
	void OnFingerUp (int finger, Vector2 pos, float timeHeldDown)
	{
		
		// disable particle emission
		particlesPS.particleSystem.enableEmission = false;
		
		if (networkView.isMine) 
		{
			Network.Destroy(gameObject);
//			print("Destroy touch with id: " + networkView.viewID.ToString());
		} 
		
		fluidGenerator.OnMouseUp(finger, pos);
		
	}
	
	#endregion

}
