using UnityEngine;
using System.Collections;

public class InkBouncerScript : MonoBehaviour
{	
	public FluidFieldGenerator fluidField = null;
		
	private bool shrinkAndDissapear = false;	
	private float currentCosX = 0;
	private float currentSinY = 0;
	private float currentXRadius = 300;
	private float currentYRadius = 300;
	private float fluidSpitRate = 2.5f;
	
	private int framesOfInkDropped = 0;
	public float Speed = 0.4f;
	
	void Start()
	{		
		
	}	
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	
	public void ReduceSpitRate(float spitReduction)
	{
		if(!shrinkAndDissapear)
			networkView.RPC ("SetFluidSpitRate", RPCMode.All, (fluidSpitRate - spitReduction));
	}
	
	[RPC]
	void SetFluidSpitRate(float newRate)
	{
		fluidSpitRate = newRate;
	}

	void Update()
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
			float xSpeed = Mathf.Cos(Time.timeSinceLevelLoad*0.8383f);
			float ySpeed = Mathf.Sin(Time.timeSinceLevelLoad*0.772383f);
			
			currentCosX = Time.timeSinceLevelLoad * xSpeed * Speed;
			currentSinY = Time.timeSinceLevelLoad * ySpeed * Speed;
			currentXRadius = Camera.main.GetScreenWidth() * 0.5f;
			currentYRadius = Camera.main.GetScreenHeight() * 0.5f;

			Vector3 curPos = transform.position;
			
			curPos.x = Mathf.Cos(currentCosX) * Mathf.Sin(currentSinY*0.4283f) * currentXRadius;
			curPos.y = Mathf.Sin(currentSinY) * Mathf.Cos(currentCosX*0.1531f) * currentYRadius;
			
			transform.position = curPos;
			fluidField.DropInkAt(curPos.x, curPos.y, (int)(fluidSpitRate*2.5f), fluidSpitRate);			
			
			if(framesOfInkDropped == 5)
			{
				fluidField.DropVelocityAt(curPos.x, curPos.y, 10, 100);
			}
			
			++framesOfInkDropped;
			
			if(fluidSpitRate < 0.25f)
			{
				fluidField.DropVelocityAt(curPos.x, curPos.y, 10, 100);
				shrinkAndDissapear = true;
			}
			//fluidField.UpdateBasedOnBlackHole(this);
			//RotationSpeed = Mathf.Min(1000, RotationSpeed);
			//this.transform.Rotate(Vector3.forward, Time.deltaTime * Mathf.Max(50, RotationSpeed));
			//if(RotationSpeed < 0.0f)
			//	shrinkAndDissapear = true;
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
