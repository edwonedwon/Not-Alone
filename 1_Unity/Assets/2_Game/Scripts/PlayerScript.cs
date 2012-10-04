using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	public int zOffset;
	
	private int mouseFingerDown = -1;		//neither!
	private Vector2 lastMousePos = new Vector2(0,0);
	
	
	private bool mouseIsMovingWhileDown = false;
	
	private ArrayList currentMousePoints = new ArrayList();
	private BlackHoleScript blackHole = null;
	private float previousMangle = 0.0f;
	
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
		//DebugStreamer.message = "mouseIsMovingWhileDown: " + mouseIsMovingWhileDown.ToString();
		
		
		if(blackHole == null)
		{
			GameObject go = GameObject.FindGameObjectWithTag("blackhole");
			if(go != null)
				blackHole = go.GetComponent<BlackHoleScript>();
		}
		
		
		if(mouseIsMovingWhileDown && blackHole != null)
		{
			Camera camcam = Camera.main;
			
			Vector3 blackHoleCenter = camcam.WorldToScreenPoint(blackHole.transform.position);
			
			//DebugStreamer.message = "currentMousePoints: " + currentMousePoints.Count.ToString();
			
			float compundedMangle = 0.0f;
            float totalmangle = 0.0f;

            float direction = 1;

            for (int i = 1; i < currentMousePoints.Count; ++i)
            {
                Vector2 old = (Vector2)currentMousePoints[i - 1];
                Vector2 cur = (Vector2)currentMousePoints[i];

                float yDistCur = cur.y - blackHoleCenter.y;
                float xDistCur = cur.x - blackHoleCenter.x;

                float yDistOld = old.y - blackHoleCenter.y;
                float xDistOld = old.x - blackHoleCenter.x;

                float maxRadius = 100;
                float minRadius = 20;

                float curLen = (float)System.Math.Sqrt((xDistCur * xDistCur) + (yDistCur * yDistCur));
                float oldLen = (float)System.Math.Sqrt((xDistOld * xDistOld) + (yDistOld * yDistOld));

                if (curLen > minRadius && curLen < maxRadius)
                {
                    float degsCur = ToDegrees((float)System.Math.Atan2(yDistCur, xDistCur));
                    float degresOld = ToDegrees((float)System.Math.Atan2(yDistOld, xDistOld));

                    if (System.Math.Sign(degresOld) == System.Math.Sign(degsCur))
                    {
                        compundedMangle += System.Math.Abs(System.Math.Abs(degsCur) - System.Math.Abs(degresOld));
                        totalmangle += degsCur - degresOld;
                    }
                }
            }

            if(totalmangle < 0)
                direction = -1;
			
			
			blackHole.AddToRotationSpeed((totalmangle-previousMangle) * 0.1f);
			previousMangle = totalmangle;
			//DebugStreamer.message = "totalmangle: " + totalmangle.ToString();
		}		
	}
	
	#region Events
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	
	public float ToDegrees(float radians)
	{
		return (float)(radians * (180.0 / 3.14159265359f));
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
		
		currentMousePoints.Add(pos);
		lastMousePos = pos;
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(lastMousePos.x, lastMousePos.y, zOffset));
		mouseFingerDown = finger;	//either 0, or 1 i believe..
		mouseIsMovingWhileDown = true;
	}
	
	public void OnPlayerFingerMove (int finger, Vector2 pos)
	{
		//DebugStreamer.message = "mouse move pos: " + pos.ToString();
		
		currentMousePoints.Add(pos);
		lastMousePos = pos;
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(lastMousePos.x, lastMousePos.y, zOffset));
		mouseFingerDown = finger;	//either 0, or 1 i believe..
	}
	
	public void OnPlayerFingerUp (int finger, Vector2 pos, float timeHeldDown)
	{
		mouseIsMovingWhileDown = false;
		mouseFingerDown = -1;	//up!
		
		
		previousMangle = 0;
		currentMousePoints.Clear();
	}
	
	
	
	
	
	#endregion

}
