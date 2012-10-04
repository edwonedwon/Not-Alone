using UnityEngine;
using System.Collections;

public class BlackHoleScript : MonoBehaviour
{	
	private FluidFieldGenerator fluidField = null;
	
	public int radius = 30;
	public float velocityPower = 190.0f;
	public float holePower = 100.0f;
	public float inkSpit = 0.0f;
	public float RotationSpeed = 100;
	
	private bool shrinkAndDissapear = false;
	void Start()
	{		
		
	}	
	
	public void AddToRotationSpeed(float additionalRot)
	{
		if(!shrinkAndDissapear)
			networkView.RPC ("SetRotationSpeed", RPCMode.All, (RotationSpeed+additionalRot));		
	}
	
	[RPC]
	void SetRotationSpeed(float newRotationSpeed)
	{
		RotationSpeed = newRotationSpeed;
	}
	
	void Update()
	{
		if(fluidField == null)
		{
			fluidField = GameObject.Find("heightfield mesh").GetComponent<FluidFieldGenerator>();
		}
		else
		{
			if(shrinkAndDissapear)
			{
				Vector3 curScale = transform.localScale;
				curScale.x *= 0.9f;
				curScale.y *= 0.9f;
				if(curScale.magnitude < 0.02f)
					Destroy (gameObject);
				else
					transform.localScale = curScale;
			}
			else
			{
				fluidField.UpdateBasedOnBlackHole(this);
				RotationSpeed = Mathf.Min(1000, RotationSpeed);
				this.transform.Rotate(Vector3.forward, Time.deltaTime * Mathf.Max(50, RotationSpeed));
				
				//RotationSpeed -= 25.0f * Time.deltaTime;
				
				if(RotationSpeed < 0.0f)
					shrinkAndDissapear = true;
			}
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
	
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	

}
