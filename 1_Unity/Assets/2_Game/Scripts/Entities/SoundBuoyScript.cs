using UnityEngine;
using System.Collections;

public class SoundBuoyScript : MonoBehaviour
{	
	private FluidFieldGenerator fluidField = null;
	public tk2dAnimatedSprite sprite = null;
	public GameObject flareTemplate = null;
	
	public int radius = 30;
	public float velocityPower = 190.0f;
	
	public bool BurstActivated = false;
	public bool SoundActivated = false;
	private SoundBuoyScript ActivatedWithOther = null;
	
	private float DeactiveTimer = 0.0f;
	private int activatedFrames = -1;
	private int numCirclesCompleted = 0;
	public Vector3 spewingDirection = new Vector3(1, 1, 1);
	public float spewRotation = 0.0f;
	private Vector3 originalScale = Vector3.one;
	
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	private ArrayList p1MovementPoints = new ArrayList(250);
	
	void Start()
	{		
		fluidField = GameObject.FindGameObjectWithTag("fluidField").GetComponent<FluidFieldGenerator>();
	}	
	
	void OnEnable()
	{
		networkView.observed = this;
		originalScale = transform.localScale;
	}
	
	void OnDisable()
	{

	}
	
	public void AddToRotationSpeed(float additionalRot, int playerNm)
	{
		//networkView.RPC ("SetRotationSpeed", RPCMode.All, (RotationSpeed+additionalRot), playerNm);
	}
	
	[RPC]
	void SetRotationSpeed(float newRotationSpeed, int playerNm)
	{
		//if(playerNm == 1)
		//	HitByPlayer1 = true;
		//else if(playerNm == 2)
		//	HitByPlayer2 = true;
		
		//if(HitByPlayer1 && HitByPlayer2)
		//	RotationSpeed = newRotationSpeed;
	}
	
	
	int NumCirclesAroundBuoy()
	{
		Vector3 blackHoleCenter = (transform.position);
		
		float compundedMangle = 0.0f;
        float totalmangle = 0.0f;

        for (int i = 1; i < p1MovementPoints.Count; ++i)
        {
            Vector2 old = (Vector2)p1MovementPoints[i - 1];
            Vector2 cur = (Vector2)p1MovementPoints[i];

            float yDistCur = cur.y - blackHoleCenter.y;
            float xDistCur = cur.x - blackHoleCenter.x;

            float yDistOld = old.y - blackHoleCenter.y;
            float xDistOld = old.x - blackHoleCenter.x;

            float maxRadius = 150;
            float minRadius = 15;

            float curLen = (float)System.Math.Sqrt((xDistCur * xDistCur) + (yDistCur * yDistCur));
            //float oldLen = (float)System.Math.Sqrt((xDistOld * xDistOld) + (yDistOld * yDistOld));

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
		
		return (int)(Mathf.Abs(totalmangle) / 310.0f);
	}
	
	public float ToDegrees(float radians)
	{
		return radians * (180.0f / 3.14159265359f);
	}
	
	void Update()
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
	
		if(p1 == null)// || p2 == null)
			return;
		
		float dt = Time.deltaTime;
		
		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		//PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		int circles = NumCirclesAroundBuoy();
		int newcircles = circles - numCirclesCompleted;
		numCirclesCompleted = circles;
		Vector2 vMe = transform.position;
		Vector2 v1 = p1.transform.position;
		//Vector2 v2 = p2.transform.position;
		Vector2 v1Diff = v1-vMe;
		
		for(int i = 0; i < newcircles; ++i)
		{
			Network.Instantiate(flareTemplate, transform.position, Quaternion.identity, 0);
			fluidField.DropVelocityInDirection(vMe.x, vMe.y, v1Diff.x, v1Diff.y, velocityPower);
		}
		
		Vector3 scale = transform.localScale;
		scale = Vector2.Lerp(scale, originalScale*(Mathf.Abs(Mathf.Cos(Time.realtimeSinceStartup*7)) * 1.3f + 0.4f), 3.0f*dt);
		transform.localScale = scale;
		
		float increaseRate = 3.0f;
		float decreaseRate = 5.0f;
		
		if(SoundActivated)
		{
			if(ActivatedWithOther != null)
			{	
				Vector2 vOther = ActivatedWithOther.transform.position;
				Vector2 adiff = vMe - vOther;
				adiff.Normalize();
				
				
				float power = UnityEngine.Random.Range(10.0f, 25.0f);
				fluidField.DropVelocityInDirection(vMe.x, vMe.y, adiff.x, adiff.y, power);
			}
		}
		
		if(BurstActivated || SoundActivated)
		{
			++activatedFrames;
			sprite.color = Color.Lerp(sprite.color, Color.white, dt*increaseRate);
			DeactiveTimer -= Time.fixedDeltaTime;
			if(DeactiveTimer < 0.0f && !SoundActivated)
			{
				activatedFrames = -1;
				BurstActivated = false;
			}
		}
		else
		{
			sprite.color = Color.Lerp (sprite.color, Color.yellow, dt*decreaseRate);
		}
		
	
		if(p1finger == PlayerScript.FingerState.None)// && p2finger == PlayerScript.FingerState.None)
		{
			numCirclesCompleted = 0;
			p1MovementPoints.Clear();
			return;
		}
		
		int p1MovementsCount = p1MovementPoints.Count;
		p1MovementPoints.Add(v1);
		if(p1MovementsCount > 249)
			p1MovementPoints.RemoveAt(0);	//then remove the last one!		
		
		if(!SoundActivated && circles > 10)
		{
			SoundActivated = true;
			
			if(ActivatedWithOther == null)
			{
				GameObject[] otherList = GameObject.FindGameObjectsWithTag("buoy");
				
				foreach(GameObject go in otherList)
				{
					SoundBuoyScript sbs = go.GetComponent<SoundBuoyScript>();	
					if(go == gameObject)
						continue;
					
					if(sbs != null && sbs.SoundActivated && sbs.ActivatedWithOther != this)
					{
						DebugStreamer.message = "FOUND ANOTHER!";
						//sbs.ActivatedWithOther = this;
						this.ActivatedWithOther = sbs;						
						break;
					}						
				}
			}			
		}
		/*
		else
		{
			if(SoundActivated)
			{
				if(ActivatedWithOther != null)
				{
					ActivatedWithOther.ActivatedWithOther = null;
					ActivatedWithOther.SoundActivated = false;
				}
				ActivatedWithOther = null;
			}
			SoundActivated = false;
		}
		*/
		
		
		if(!BurstActivated && Mathf.Abs(newcircles) > 0)// && v1Diff.magnitude < minDist)
		{
			BurstActivated = true;
			activatedFrames = (int)UnityEngine.Random.Range(0, 3);
			DeactiveTimer = 1.5f;
		}
	}

	
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		/*
		float rotSpeed = RotationSpeed;
		if(stream.isWriting)
		{
			stream.Serialize(ref rotSpeed);
		}
		else if(stream.isReading)
		{
			stream.Serialize(ref rotSpeed);
		}
		RotationSpeed = rotSpeed;
	*/
	}
}
