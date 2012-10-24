using UnityEngine;
using System.Collections;

public class SoundBuoyScript : MonoBehaviour
{
	class BuoyRiver
	{
		public ArrayList buoys = new ArrayList();
	}
	
	public static ArrayList WorldBuoysList = new ArrayList();
	public static ArrayList RiverList = new ArrayList();
	static float allBuoysConnectedTimer = -1.0f;	//ready-to-go!
	
	private FluidFieldGenerator fluidField = null;
	public tk2dAnimatedSprite sprite = null;
	private string[] buoyAnimations = new string[7];
	
	public GameObject flareTemplate = null;
	
	public AudioSource audio = null;
	public AudioClip[] ringingSounds = new AudioClip[5];
	
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
	
	public static bool CloseToOthers(GameObject go, float minDistance)
	{
		if(WorldBuoysList.Count > 0)
		{
			foreach(SoundBuoyScript sbs in WorldBuoysList)
			{
				if(sbs.gameObject != go)
				{
					if((sbs.transform.position-go.transform.position).magnitude < minDistance)
						return true;
				}
			}
		}
		return false;
	}
	
	void Start()
	{
		int maxBuoys = 5;	//max limit available
		bool tooCloseToOthers = SoundBuoyScript.CloseToOthers(gameObject, 100.0f);
		if(!tooCloseToOthers)
			tooCloseToOthers = BlackHoleScript.CloseToOthers(gameObject, 100.0f);
			
		if(WorldBuoysList.Count >= maxBuoys || tooCloseToOthers)
		{
			DestroyImmediate(gameObject);
			return;
		}
		
		
		buoyAnimations[0] = "appear";
		buoyAnimations[1] = "moving";
		buoyAnimations[2] = "drop";
		buoyAnimations[3] = "ringing";
		buoyAnimations[4] = "ding";
		buoyAnimations[5] = "sink";
		buoyAnimations[6] = "underwater";
	
		sprite.animationCompleteDelegate += AnimationComplete;
		
		WorldBuoysList.Add(this);
		fluidField = GameObject.FindGameObjectWithTag("fluidField").GetComponent<FluidFieldGenerator>();
	}	
	
	public void SetCurrentAnimation(int idx)
	{
		sprite.Play(buoyAnimations[idx]);
	}
	
	public void AnimationComplete (tk2dAnimatedSprite anim, int clipId)
	{
		switch (clipId)
		{
		case 0:	//appear
			anim.Play(buoyAnimations[2]);
			break;
		case 1: //moving
			anim.Play(buoyAnimations[2]);
			break;
		case 2:	//drop
			anim.Play(buoyAnimations[2]);
			break;
		case 3:	//ringing
			anim.Play(buoyAnimations[2]);
			break;
		case 4:	//ding
			anim.Play(buoyAnimations[2]);
			break;
		case 5:	//sink
			anim.Play(buoyAnimations[6]);
			break;
		case 6:	//underwater
			anim.Play(buoyAnimations[6]);
			break;
		}
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
	
	
	
	public static bool CheckForRiverCompletion()
	{
		int connects = 0;
		foreach(SoundBuoyScript sbs in WorldBuoysList)
		{
			if(sbs.ActivatedWithOther != null && sbs.SoundActivated)
				++connects;
		}
		if(connects > 3 && connects == WorldBuoysList.Count-1)
		{
			if(allBuoysConnectedTimer < 0.0f)	//zero
			{
				//Play river completed sound/effect here
				allBuoysConnectedTimer = 0.0f;
			}
			
			allBuoysConnectedTimer += Time.fixedDeltaTime;
			if(allBuoysConnectedTimer > 5.0f && WorldBuoysList.Count > 0)
			{								
				BuoyRiver br = new BuoyRiver();
				foreach(SoundBuoyScript sbs in WorldBuoysList)
				{
					sbs.Submerge();
					br.buoys.Add(sbs.gameObject);		
				}
				WorldBuoysList.Clear();
				RiverList.Add(br);
				return true;
			}
		}
		else
		{
			allBuoysConnectedTimer = -1.0f;
		}
		return false;
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
	
	bool CheckForInkLineOverBuoy()
	{
		if(player1.DoLinkInk() && player2.DoLinkInk())
		{
			Vector2 v1 = p1.transform.position;
			Vector2 v2 = p2.transform.position;
			Vector2 closest = Vector2.zero;
			Vector2 pos = transform.position;
			GetClosetPointOnLine(ref closest, v1, v2, pos, true);
			
			Vector2 difference = pos - closest;
			if(difference.magnitude < 10.0f)
				return true;
		}
		return false;
	}
	
	public void GetClosetPointOnLine(ref Vector2 closestPointOut, Vector2 A, Vector2 B, Vector2 P, bool segmentClamp)
    {
        float apx = P.x - A.x;
        float apy = P.y - A.y;
        float abx = B.x - A.x;
        float aby = B.y - A.y;

        float ab2 = abx * abx + aby * aby;
        float ap_ab = apx * abx + apy * aby;
        float t = ap_ab / ab2;
        if (segmentClamp)
        {
            if (t < 0.0f)
                t = 0.0f;
            else if (t > 1.0f)
                t = 1.0f;
        }

        closestPointOut.x = A.x + abx * t;
        closestPointOut.y = A.y + aby * t;
    }
	
	private bool submerged = false;
	public void Submerge()
	{
		submerged = true;
		originalScale = Vector2.zero;
		SetCurrentAnimation(5);	//sink...
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
	
		if(p1 == null || p2 == null)
			return;
				
		float dt = Time.fixedDeltaTime;
		
		PlayerScript.FingerState p1finger = player1.MouseFingerDown();
		PlayerScript.FingerState p2finger = player1.MouseFingerDown();
		
		int circles = NumCirclesAroundBuoy();
		int newcircles = circles - numCirclesCompleted;
		numCirclesCompleted = circles;
		Vector2 vMe = transform.position;
		Vector2 v1 = p1.transform.position;
		Vector2 v2 = p2.transform.position;
		
		//Vector3 scale = transform.localScale;
		//scale = Vector2.Lerp(scale, originalScale*(Mathf.Abs(Mathf.Cos(Time.realtimeSinceStartup*7)) * 1.3f + 0.4f), 3.0f*dt);
		//transform.localScale = scale;
		
		float increaseRate = 3.0f;
		float decreaseRate = 5.0f;
		Vector2 vBurstDirection = v1-vMe;
		int cnt = WorldBuoysList.Count;
		
		if(ActivatedWithOther == null)
		{
			int myIdx = WorldBuoysList.IndexOf(this);				
			if(myIdx < cnt-1)
			{
				ActivatedWithOther = (SoundBuoyScript)WorldBuoysList[myIdx+1];
			}
			//else if(myIdx > 0)
			//	ActivatedWithOther = (SoundBuoyScript)WorldBuoysList[0];
		}	
				
		if(!SoundActivated && CheckForInkLineOverBuoy())
			SoundActivated = true;
		if(!SoundActivated && circles > 5)
			SoundActivated = true;
		
		if(SoundActivated)
		{
			if(ActivatedWithOther != null)
			{
				Vector2 vOther = ActivatedWithOther.transform.position;
				vBurstDirection = vOther-vMe;
				float power = UnityEngine.Random.Range(0.0f, 0.065f);
				if(submerged)
					power *= 0.2f;
				fluidField.DropVelocityInDirection(vMe.x, vMe.y, vBurstDirection.x, vBurstDirection.y, power);
				//sprite.color = Color.Lerp(sprite.color, Color.white, dt*increaseRate);
			}
		}		
		
		if(!submerged)
		{
			vBurstDirection.Normalize();
			if(newcircles > 0)
			{
				audio.PlayOneShot(ringingSounds[WorldBuoysList.IndexOf(this)]);
				SetCurrentAnimation(3);
			}
			//for(int i = 0; i < newcircles; ++i)
			//{
			//	Network.Instantiate(flareTemplate, transform.position, Quaternion.identity, 0);
			//	fluidField.DropVelocityInDirection(vMe.x, vMe.y, vBurstDirection.x, vBurstDirection.y, riverPower);
			//}
		}
		
			if(BurstActivated || SoundActivated)
			{
				++activatedFrames;
				if(BurstActivated)
				{
					//sprite.color = Color.Lerp(sprite.color, Color.blue, dt*increaseRate);
				}
				DeactiveTimer -= Time.fixedDeltaTime;
				if(DeactiveTimer < 0.0f)
				{
					activatedFrames = -1;
					BurstActivated = false;
				}
			}
			else
			{
				//sprite.color = Color.Lerp (sprite.color, Color.yellow, dt*decreaseRate);
			}
		
		if(!submerged)
		{
			if(Network.isServer)
			{
				p1MovementPoints.Add(v1);
				if(p1finger == PlayerScript.FingerState.None)// && p2finger == PlayerScript.FingerState.None)
				{
					numCirclesCompleted = 0;
					p1MovementPoints.Clear();
				}
				
			}
			else if(Network.isClient)
			{
				p1MovementPoints.Add(v2);
				if(p2finger == PlayerScript.FingerState.None)// && p2finger == PlayerScript.FingerState.None)
				{
					numCirclesCompleted = 0;
					p1MovementPoints.Clear();
				}
			}
			
			if(p1MovementPoints.Count > 90)
				p1MovementPoints.RemoveAt(0);	//then remove the last one!				
		}
		
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
