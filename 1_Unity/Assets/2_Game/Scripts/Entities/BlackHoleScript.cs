using UnityEngine;
using System.Collections;

public class BlackHoleScript : MonoBehaviour
{	
	public static ArrayList WorldBlackHoles = new ArrayList();
	
	private FluidFieldGenerator fluidField = null;
	public int radius = 30;
	public float velocityPower = 190.0f;
	public float holePower = 100.0f;
	public float inkSpit = 0.0f;
	public float RotationSpeed = 100;
	
	private bool shrinkAndDissapear = false;
	public AudioSource audio = null;
	public AudioClip blackHoleInstantiated = null;
	public Vector3 spewingDirection = new Vector3(1, 1, 1);
	public float spewRotation = 0.0f;
	
	private bool HitByPlayer1 = false;
	private bool HitByPlayer2 = false;
	
	public static bool CloseToOthers(GameObject go, float minDistance)
	{
		if(WorldBlackHoles.Count > 0)
		{
			bool tooCloseToOThers = false;
			foreach(BlackHoleScript bh in WorldBlackHoles)
			{
				if(bh.gameObject != go)
				{
					if((bh.transform.position-go.transform.position).magnitude < minDistance)
						return true;
				}
			}
		}
		return false;
	}
	
	void Start()
	{
		int maxBlackHoles = 4;
		bool tooCloseToOthers = SoundBuoyScript.CloseToOthers(gameObject, 100.0f);
		if(!tooCloseToOthers)
			tooCloseToOthers = BlackHoleScript.CloseToOthers(gameObject, 100.0f);
			
		if(WorldBlackHoles.Count >= maxBlackHoles || tooCloseToOthers)
		{
			DestroyImmediate(gameObject);
			return;
		}
		
		WorldBlackHoles.Add(this);
		
	}	
	
	void OnEnable()
	{
		audio.PlayOneShot(blackHoleInstantiated);
		networkView.observed = this;
	}
	
	void OnDisable()
	{
		WorldBlackHoles.Remove(this);
	}
	
	public void AddToRotationSpeed(float additionalRot, int playerNm)
	{
		if(!shrinkAndDissapear)
			networkView.RPC ("SetRotationSpeed", RPCMode.All, (RotationSpeed+additionalRot), playerNm);
	}
	
	[RPC]
	void SetRotationSpeed(float newRotationSpeed, int playerNm)
	{
		if(playerNm == 1)
			HitByPlayer1 = true;
		else if(playerNm == 2)
			HitByPlayer2 = true;
		
		if(HitByPlayer1 && HitByPlayer2)
			RotationSpeed = newRotationSpeed;
	}
	
	void FixedUpdate()
	{
		if(fluidField == null)
		{
			fluidField = GameObject.Find("fluid field").GetComponent<FluidFieldGenerator>();
		}
		else
		{
			if(shrinkAndDissapear)
			{
				Vector3 curScale = transform.localScale * 0.9f;	
				float curmag = Mathf.Sqrt((curScale.x*curScale.x)+(curScale.y*curScale.y));
				if(curmag < 0.1f)
					Network.Destroy(gameObject);
				else
					transform.localScale = curScale;
			}
			else
			{
				fluidField.UpdateBasedOnBlackHole(this);
				RotationSpeed = Mathf.Min(1000, RotationSpeed);
				this.transform.Rotate(Vector3.forward, Time.deltaTime * Mathf.Max(50, RotationSpeed));
				if(RotationSpeed < 0.0f)
					shrinkAndDissapear = true;
			}
			
			
			spewRotation = RotationSpeed * Time.deltaTime * 5.0f;
			spewingDirection = Quaternion.AngleAxis(spewRotation, Vector3.forward) * spewingDirection;
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
