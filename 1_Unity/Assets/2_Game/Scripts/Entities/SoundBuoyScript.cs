using UnityEngine;
using System.Collections;

public class SoundBuoyScript : MonoBehaviour
{
	static ArrayList WorldBuoysList = new ArrayList();
	
	private FluidFieldGenerator fluidField = null;
	public tk2dAnimatedSprite sprite = null;
	public GameObject flareTemplate = null;
	
	public float riverPower = 2.0f;
	
	public bool BurstActivated = false;
	public bool SoundActivated = false;
	private SoundBuoyScript ActivatedWithOther = null;
	
	private float DeactiveTimer = 0.0f;
	private int activatedFrames = -1;
	private int numCirclesCompleted = 0;
	private Vector3 originalScale = Vector3.one;
	
	private PlayerScript player1 = null;
	private PlayerScript player2 = null;
	private GameObject p1 = null;
	private GameObject p2 = null;
	
	private ArrayList p1MovementPoints = new ArrayList(250);
	
	void Start()
	{
		int maxBuoys = 4;	//max limit available
		if(WorldBuoysList.Count > 0)
		{
			bool tooCloseToOThers = false;
			foreach(SoundBuoyScript sbs in WorldBuoysList)
			{
				if((sbs.transform.position-this.transform.position).magnitude < 100)
				{
					tooCloseToOThers = true;
					break;
				}
			}
			
			if(WorldBuoysList.Count >= maxBuoys || tooCloseToOThers)
			{
				DestroyImmediate(gameObject);
				return;
			}
		}		
		
		WorldBuoysList.Add(this);
		fluidField = GameObject.FindGameObjectWithTag("fluidField").GetComponent<FluidFieldGenerator>();
	}	
	
	void OnEnable()
	{
		networkView.observed = this;
		originalScale = transform.localScale;
	}
	
	void OnDisable()
	{
		WorldBuoysList.Remove(this);
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
		Vector3 centre = transform.position;
		
		float compundedMangle = 0.0f;
        float totalmangle = 0.0f;

        for (int i = 1; i < p1MovementPoints.Count; ++i)
        {
            Vector2 old = (Vector2)p1MovementPoints[i - 1];
            Vector2 cur = (Vector2)p1MovementPoints[i];

            float yDistCur = cur.y - centre.y;
            float xDistCur = cur.x - centre.x;

            float yDistOld = old.y - centre.y;
            float xDistOld = old.x - centre.x;

            float maxRadius = 250;
            float minRadius = 15;

            float curLen = Mathf.Sqrt((xDistCur * xDistCur) + (yDistCur * yDistCur));
            //float oldLen = (float)System.Math.Sqrt((xDistOld * xDistOld) + (yDistOld * yDistOld));

            if (curLen > minRadius && curLen < maxRadius)
            {
                float degsCur = Mathf.Atan2(yDistCur, xDistCur);
                float degresOld = Mathf.Atan2(yDistOld, xDistOld);

                if (System.Math.Sign(degresOld) == System.Math.Sign(degsCur))
                {
                    compundedMangle += Mathf.Abs(Mathf.Abs(degsCur) - Mathf.Abs(degresOld));
                    totalmangle += degsCur - degresOld;
                }
            }
        }
		
		return (int)(Mathf.Abs(ToDegrees(totalmangle)) / 270.0f);	//less than 360 because we want to count looping-sorta
	}
	
	public float ToDegrees(float radians)
	{
		return radians * (180.0f / 3.14159265359f);
	}
	
	void FixedUpdate()
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
		
		float dt = Time.fixedDeltaTime;
		
		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		//PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		int circles = NumCirclesAroundBuoy();
		int newcircles = circles - numCirclesCompleted;
		numCirclesCompleted = circles;
		Vector2 vMe = transform.position;
		Vector2 v1 = p1.transform.position;
		//Vector2 v2 = p2.transform.position;
		
		Vector3 scale = transform.localScale;
		scale = Vector2.Lerp(scale, originalScale*(Mathf.Abs(Mathf.Cos(Time.realtimeSinceStartup*7)) * 1.3f + 0.4f), 3.0f*dt);
		transform.localScale = scale;
		
		float increaseRate = 3.0f;
		float decreaseRate = 5.0f;
		Vector2 vBurstDirection = v1-vMe;
		
		if(SoundActivated)
		{
			if(ActivatedWithOther != null)
			{
				Vector2 vOther = ActivatedWithOther.transform.position;
				vBurstDirection = vOther-vMe;
				float power = UnityEngine.Random.Range(0.0f, 0.065f);
				fluidField.DropVelocityInDirection(vMe.x, vMe.y, vBurstDirection.x, vBurstDirection.y, power);
				sprite.color = Color.Lerp(sprite.color, Color.white, dt*increaseRate);
			}
		}		
		
		vBurstDirection.Normalize();
		for(int i = 0; i < newcircles; ++i)
		{
			Network.Instantiate(flareTemplate, transform.position, Quaternion.identity, 0);
			fluidField.DropVelocityInDirection(vMe.x, vMe.y, vBurstDirection.x, vBurstDirection.y, riverPower);
		}		
		
		if(BurstActivated || SoundActivated)
		{
			++activatedFrames;
			if(BurstActivated)
				sprite.color = Color.Lerp(sprite.color, Color.blue, dt*increaseRate);
			DeactiveTimer -= Time.fixedDeltaTime;
			if(DeactiveTimer < 0.0f)
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
		}
		
		
		int p1MovementsCount = p1MovementPoints.Count;
		p1MovementPoints.Add(v1);
		if(p1MovementsCount > 249)
			p1MovementPoints.RemoveAt(0);	//then remove the last one!		
		
		//Connect to the nextly created buoy
		int cnt = WorldBuoysList.Count;		
		if(cnt > 1 && !SoundActivated && circles > 5)
		{
			if(ActivatedWithOther == null)
			{
				int myIdx = WorldBuoysList.IndexOf(this);				
				if(myIdx < cnt-1)
				{
					ActivatedWithOther = (SoundBuoyScript)WorldBuoysList[myIdx+1];
					SoundActivated = true;
				}
				else if(myIdx > 0)
					ActivatedWithOther = (SoundBuoyScript)WorldBuoysList[0];
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
