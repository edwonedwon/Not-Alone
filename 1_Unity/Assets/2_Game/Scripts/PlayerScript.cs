using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	public int zOffset;
	
	public enum FingerState
	{
		None = -1,
		Single = 0,
		Both = 1,
	};
	
	private FingerState currentFingerState = FingerState.None;//neither!
	private FingerState previousFingerState = FingerState.None;		//no difference
	
	private bool mouseIsMovingWhileDown = false;
	private Vector2 prevTouchPos = Vector2.zero;
	private ArrayList currentMousePoints = new ArrayList();
	private float previousMangle = 0.0f;
	
	public float totalSadness = 0;
	public float totalHappiness = 0;
	public float totalAngriness = 0;
	
	private FluidFieldGenerator fluidField = null;
	public GameObject PinchCreateObjectPrefab = null;
	public GameObject RippleObjectPrefab = null;
	public GameObject VortexObjectPrefab = null;
	
	int currentPlaySndIdx = 0;
	public AudioClip[] fingerDownSounds = new AudioClip[3];
	
	public enum PlayerFeeling
	{
		Happy,
		Sad,
		Angry,
	};
	
	private class PlayerMovements
	{
		public int NumTouchDowns = 0;
		
		public int QuadrantDensity = 64;
		public struct Quad
		{
			public bool touched;
			public float mouseLoopsAround;
			public int timer;
			public Quad(bool t, int ti)
			{
				touched = t;
				timer = ti;
				mouseLoopsAround = 0;
			}
		};
		public Quad[,] QuadrantsTouched;
		
		public float TimeSinceLastFeelingChange = 0.0f;
		public float TimeSinceLastMouseMove = 0.0f;
		public float TimeSinceLastTouchDown = 0.0f;
		
		public float DistanceTraveled = 0.0f;
		public float TotalAngleChanges = 0.0f;
		public int LineIntersects = 0;
		
		public Vector2 minBounds = Vector2.zero;
		public Vector2 maxBounds = Vector2.zero;
		
		public float timeBetweenDifferentQuadrants = 0.0f;
		public float currentHappiness = 0.0f;
		public float currentSadness = 0.0f;
		public float currentAngriness = 0.0f;
		
		private float calculateLoopsTimer = 1.0f;
		
		public PlayerMovements()
		{
			QuadrantsTouched = new Quad[QuadrantDensity, QuadrantDensity];
			for(int i = 0; i < QuadrantDensity; ++i)
				for(int j = 0; j < QuadrantDensity; ++j)
					QuadrantsTouched[i,j] = new Quad(false, 0);					
				
			Reset();
		}
		
		public void Reset()
		{
			NumTouchDowns = 0;
			TimeSinceLastFeelingChange = 0.0f;
			minBounds = maxBounds = Vector2.zero;			
		}
		
		public bool ResetHitQuadrants()
		{
			TotalAngleChanges = 0.0f;
			LineIntersects = 0;
			DistanceTraveled = 0.0f;
			
			int borderSquaresCovered = 0;
			int innerSquresCovered = 0;
			int borderSize = 10;
			for(int i = 0; i < QuadrantDensity; ++i)
			{
				for(int j = 0; j < QuadrantDensity; ++j)
				{
					if(QuadrantsTouched[i,j].touched)
					{
						if(i < borderSize || i > QuadrantDensity-borderSize || j < borderSize || j > QuadrantDensity-borderSize)
							++borderSquaresCovered;
						else
							++innerSquresCovered;	
					}
					
					QuadrantsTouched[i,j].touched = false;
					QuadrantsTouched[i,j].timer = 0;
					QuadrantsTouched[i,j].mouseLoopsAround = 0;
				}
			}
			
			
			//string msg = "innerSquresCovered: " + innerSquresCovered.ToString() +
			//"\nborderSquaresCovered: " + borderSquaresCovered.ToString() +
			//		"\nQuadrantDensity x 4: " + (QuadrantDensity*4).ToString();
			//DebugStreamer.message = msg;
			
			if(innerSquresCovered < 40 && borderSquaresCovered > 60)
				return true;
			
			return false;
		}
			
		public void CalculateMovementBounds(Vector2 mousePos)
		{
			if(mousePos.x < minBounds.x)
				minBounds.x = mousePos.x;
			if(mousePos.x > maxBounds.x)
				maxBounds.x = mousePos.x;
			
			if(mousePos.y < minBounds.y)
				minBounds.y = mousePos.y;
			if(mousePos.y > maxBounds.y)
				maxBounds.y = mousePos.y;
		}
		
		
		public void CalculateCurrentQuadrant(PlayerScript playa, Vector2 mousePos, ArrayList mousepoints)
		{
			float xp = mousePos.x / Camera.main.pixelWidth;
			float yp = mousePos.y / Camera.main.pixelHeight;
			
			if(xp > 1)
				xp = 1;
			else if(xp < 0)
				xp = 0;
			if(yp > 1)
				yp = 1;
			else if(yp < 0)
				yp = 0;
						
			int xQuadrant = (int)(xp * (QuadrantDensity-1));
			int yQuadrant = (int)(yp * (QuadrantDensity-1));
			
			if(!QuadrantsTouched[xQuadrant, yQuadrant].touched)
				timeBetweenDifferentQuadrants = 0.0f;	//reset quandrant timer
			
			QuadrantsTouched[xQuadrant, yQuadrant].touched = true;
			QuadrantsTouched[xQuadrant, yQuadrant].timer = 100;
			
			calculateLoopsTimer -= Time.fixedDeltaTime;
			if(calculateLoopsTimer < 0.0f)
			{
				calculateLoopsTimer = 1.0f;
				Camera camcam = Camera.main;
				for(int i = 0; i < QuadrantDensity; ++i)
				{
					if(UnityEngine.Random.Range(0, 100) > 50)
						continue;
					
					float deltai = (float)i / (float)(QuadrantDensity-1);
					for(int j = 0; j < QuadrantDensity; ++j)
					{	
						float deltaj = (float)j / (float)(QuadrantDensity-1);
						float compundedMangle = 0.0f;
	            		float totalmangle = 0.0f;
						
						float screenx = Camera.main.pixelWidth * deltai;
						float screeny = Camera.main.pixelHeight * deltaj;
	
			            for (int mp = 1; mp < mousepoints.Count; ++mp)
			            {
			                Vector2 old = (Vector2)mousepoints[mp - 1];
			                Vector2 cur = (Vector2)mousepoints[mp];
			
			                float yDistCur = cur.y - screeny;
			                float xDistCur = cur.x - screenx;
			
			                float yDistOld = old.y - screeny;
			                float xDistOld = old.x - screenx;
			
			                float maxRadius = 55;
			                float minRadius = 5;
			
			                float curLen = (float)System.Math.Sqrt((xDistCur * xDistCur) + (yDistCur * yDistCur));
			                //float oldLen = (float)System.Math.Sqrt((xDistOld * xDistOld) + (yDistOld * yDistOld));
			
			                if (curLen > minRadius && curLen < maxRadius)
			                {
			                    float degsCur = ToDegrees(Mathf.Atan2(yDistCur, xDistCur));
			                    float degresOld = ToDegrees(Mathf.Atan2(yDistOld, xDistOld));
			
			                    if (System.Math.Sign(degresOld) == System.Math.Sign(degsCur))
			                    {
			                        compundedMangle += System.Math.Abs(System.Math.Abs(degsCur) - System.Math.Abs(degresOld));
			                        totalmangle += degsCur - degresOld;
			                    }
			                }
			            }
						
						QuadrantsTouched[i, j].mouseLoopsAround = Mathf.Max(0, totalmangle / 360.0f);
						
						if(QuadrantsTouched[i, j].mouseLoopsAround > 1.0f)	//two circles to make one appear!
						{
							if(this.TimeSinceLastTouchDown > 0.5f)
							{
								playa.SpawnVortex(camcam.ScreenToWorldPoint(new Vector2(screenx, screeny)));
								QuadrantsTouched[i, j].mouseLoopsAround = 0;
								return;
							}
						}
					}
				}
			}
		}
				
		public PlayerFeeling CalculateCurrentFeeling(PlayerScript player, PlayerFeeling curFeeling)
		{
			int quadsTouched = 0;			
			for(int i = 0; i < QuadrantDensity; ++i)
			{
				for(int j = 0; j < QuadrantDensity; ++j)
				{
					QuadrantsTouched[i,j].timer -= 1;
					if(QuadrantsTouched[i,j].timer < 0)
						QuadrantsTouched[i,j].timer = 0;
					
					if(QuadrantsTouched[i,j].timer > 0)
						++quadsTouched;
				}
			}			
			
			float dt = Time.deltaTime;
			float quadrantsPerSecond = quadsTouched / TimeSinceLastTouchDown;
			float anglesPerSecond = TotalAngleChanges / TimeSinceLastTouchDown;
			float distancePerSecond = DistanceTraveled / TimeSinceLastTouchDown;
			
			TotalAngleChanges -= 1000.0f * dt;
			//DistanceTraveled -= 1.0f*dt;
			if(TotalAngleChanges < 0)
				TotalAngleChanges = 0;
			if(DistanceTraveled < 0)
				DistanceTraveled= 0;
			
			FuzzyRule isSadQuadrants = new FuzzyRule(0, 2, 5, 15, quadsTouched);
			float amountOfSadQuads = isSadQuadrants.Fuzzify();
			FuzzyRule isHappyQuadrants = new FuzzyRule(5, 20, 35, 50, quadsTouched);
			float amountOfHappyQuads = isHappyQuadrants.Fuzzify();
			FuzzyRule isAngryQuadrants = new FuzzyRule(25, 40, 60, 255, quadsTouched);
			float amountOfAngryQuads = isAngryQuadrants.Fuzzify();
			
			float aps = anglesPerSecond * 0.1f;
			FuzzyRule isSadAngles = new FuzzyRule(0, 15, 45, 110, aps);
			float amountOfSadAngles = isSadAngles.Fuzzify();
			FuzzyRule isHappyAngles = new FuzzyRule(15, 50, 250, 360, aps);
			float amountOfHappyAngles = isHappyAngles.Fuzzify();
			FuzzyRule isAngryAngles = new FuzzyRule(200, 320, 400, 1500, aps);
			float amountOfAngryAngles = isAngryAngles.Fuzzify();			
						
			FuzzyRule isSadDistance = new FuzzyRule(0, 0.25f, 0.85f, 1.25f, distancePerSecond);
			float amountOfSadDistance = isSadDistance.Fuzzify();
			FuzzyRule isHappyDistance = new FuzzyRule(0.5f, 1.15f, 2.0f, 4.0f, distancePerSecond);
			float amountOfHappyDistance = isHappyDistance.Fuzzify();
			FuzzyRule isAngryDistance = new FuzzyRule(1.75f, 3.0f, 5.0f, 10.0f, distancePerSecond);
			float amountOfAngryDistance = isAngryDistance.Fuzzify();

			player.totalSadness = (amountOfSadQuads +  amountOfSadDistance)/2.0f;
			player.totalHappiness = (amountOfHappyQuads +  amountOfHappyDistance)/2.0f;
			player.totalAngriness = (amountOfAngryQuads +  amountOfAngryDistance)/2.0f;
				
			string debugMsg = "\ntotalSadness: " + player.totalSadness.ToString("f2");
			debugMsg += "\ntotalHappiness: " + player.totalHappiness.ToString("f2");
			debugMsg += "\ntotalAngriness: " + player.totalAngriness.ToString("f2");
			
			//string debugMsg = "LineIntersects: " + LineIntersects.ToString();

			//string debugMsg = "quadsTouched: " + quadsTouched.ToString("f2");
			//debugMsg += "\naps: " + aps.ToString("f2");
			//debugMsg += "\ndistancePerSecond: " + distancePerSecond.ToString("f2");
			debugMsg += "\ncurrent feeling: " + curFeeling.ToString();
			//DebugStreamer.message = debugMsg;
			
			if(TimeSinceLastFeelingChange < 3.50f)
				return curFeeling;
			
			if(player.totalSadness > player.totalHappiness && player.totalSadness > player.totalAngriness)
				return PlayerFeeling.Sad;
			else if(player.totalHappiness > player.totalSadness && player.totalHappiness > player.totalAngriness)
				return PlayerFeeling.Happy;
			else if(player.totalAngriness > player.totalSadness && player.totalAngriness > player.totalHappiness)
				return PlayerFeeling.Angry;
			
			return curFeeling;
		}
	};
	
	
	private PlayerFeeling currentFeeling = PlayerFeeling.Happy;
	private PlayerMovements currentMovements = new PlayerMovements();
	
	
	private int numberOfTapsInSameSpot = 0;
	private Vector2 lastMouseDownPos = Vector2.zero;

	public tk2dAnimatedSprite touchAnim;
	
	private bool doLinkInk = false;
	private bool isLocalPlayer = true;
	private bool isPlayer1 = false;
	public bool doInkBurst = false;
	
	private string[] touchAnimations = new string[3];
	
	public FingerState MouseFingerDown()
	{
		return currentFingerState;
	}
	
	public bool DoLinkInk()
	{
		return doLinkInk;
	}
	
	public void SetDoLinkInk(bool link)
	{
		doLinkInk = link;
	}
	
	
	void Start()
	{
		fluidField = GameObject.FindGameObjectWithTag("fluidField").GetComponent<FluidFieldGenerator>();
		
		isPlayer1 = gameObject.tag == "PLAYER1";
		
		if(Network.isClient && isPlayer1)
			isLocalPlayer = false;
		else if(Network.isServer && !isPlayer1)
			isLocalPlayer = false;
		
		if(!isLocalPlayer)
		{
			touchAnimations[0] = "fingerprintBeginAnim";
			touchAnimations[1] = "fingerprintLoopAnim";
			touchAnimations[2] = "fingerprintEndAnim";
			
			//set custom scale for the fingahprint
			touchAnim.scale = new Vector3(150, 150, 150);
		}
		else
		{
			touchAnimations[0] = "touchBeginAnim";
			touchAnimations[1] = "touchLoopAnim";
			touchAnimations[2] = "touchEndAnim";
			
			//set scale	for the regular glowball		
			touchAnim.scale = new Vector3(200, 200, 200);
		}
	
		touchAnim.animationCompleteDelegate += AnimationComplete;
		DontDestroyOnLoad(this);
	}
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	
	void FixedUpdate()
	{
		if(previousFingerState != currentFingerState)
		{
			if(currentFingerState != FingerState.None)
				touchAnim.Play(touchAnimations[0]);	//play the begin animation
			else if(currentFingerState == FingerState.None)
				touchAnim.Play(touchAnimations[2]);	//play end animation
		}
		
		float centerx = 0.5f;
		float centery = 0.5f;
		
		float xdiffCur = centerx - prevTouchPos.x;
		float ydiffCur = centery - prevTouchPos.y;
		float xdiffOld = centerx - transform.position.x;
		float ydiffOld = centery - transform.position.y;
		
		float angleOld = ToDegrees(Mathf.Atan2(ydiffOld, xdiffOld));
		float angleCur = ToDegrees(Mathf.Atan2(ydiffCur, xdiffCur));
		
		touchAnim.transform.Rotate(touchAnim.transform.forward, (angleOld-angleCur));
		prevTouchPos.x = transform.position.x;
		prevTouchPos.y = transform.position.y;

		previousFingerState = currentFingerState;
		
		UpdatePlayerMovementsAndFeelings();
		UpdatePlayerGestures();
	}
	
	
	public void SpawnVortex(Vector3 worldpos)
	{
		return;
		if(BlackHoleScript.WorldBlackHoles.Count > 4)
			return;
		
		Vector2 spawnPos = new Vector2(worldpos.x, worldpos.y);
		
		foreach(BlackHoleScript bh in BlackHoleScript.WorldBlackHoles)
		{
			Vector2 bhpos = bh.transform.position;
			if((spawnPos-bhpos).magnitude < 100)
				return;
		}
		
		Network.Instantiate(VortexObjectPrefab, spawnPos, Quaternion.identity, 0);
	}
	
	public void UpdatePlayerGestures()
	{
								
	}
	
	public void UpdatePlayerMovementsAndFeelings()
	{
		float dt = Time.deltaTime;
		currentMovements.TimeSinceLastTouchDown += dt;
		currentMovements.TimeSinceLastFeelingChange += dt;
		currentMovements.TimeSinceLastMouseMove += dt;
		currentMovements.timeBetweenDifferentQuadrants += dt;
		SetCurrentFeeling(currentMovements.CalculateCurrentFeeling(this, currentFeeling));
	}
			
			
	private void SetCurrentFeeling(PlayerFeeling newFeeling)
	{
		if(currentFeeling != newFeeling)
		{
			currentFeeling = newFeeling;
			currentMovements.Reset();
			//doInkBurst = true;
		}
	}
	
	// plays the looping animation after the begin animation ends
	public void AnimationComplete (tk2dAnimatedSprite touchAnim, int clipId)
	{
		switch (clipId)
		{
		case 0:
			touchAnim.Play(touchAnimations[1]);	break;
		}
	}
	
	public void UpdateAgainstBlackHole(BlackHoleScript blackHole)
	{
		if(mouseIsMovingWhileDown && blackHole != null)
		{
			Camera camcam = Camera.main;
			
			Vector3 blackHoleCenter = camcam.WorldToScreenPoint(blackHole.transform.position);
			
			float compundedMangle = 0.0f;
            float totalmangle = 0.0f;

            for (int i = 1; i < currentMousePoints.Count; ++i)
            {
                Vector2 old = (Vector2)currentMousePoints[i - 1];
                Vector2 cur = (Vector2)currentMousePoints[i];

                float yDistCur = cur.y - blackHoleCenter.y;
                float xDistCur = cur.x - blackHoleCenter.x;

                float yDistOld = old.y - blackHoleCenter.y;
                float xDistOld = old.x - blackHoleCenter.x;

                float maxRadius = 150;
                float minRadius = 20;

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
			
			int playerNm = isPlayer1 ? 1 : 2;			
			blackHole.AddToRotationSpeed((totalmangle-previousMangle) * 0.1f, playerNm);
			previousMangle = totalmangle;
		}		
	}
	
	public static float ToDegrees(float radians)
	{
		return radians * (180.0f / 3.14159265359f);
	}
	
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		int mouseState = (int)currentFingerState;
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
		
		currentFingerState = (FingerState)mouseState;
		transform.position = pos;
	}
	
	
	#region Finger Gestures
	
	
	public void PlayOneShotAudio(int idx)
	{
		networkView.RPC ("PlayAudio", RPCMode.AllBuffered, idx);
	}
	
	[RPC]
	void PlayAudio(int idx)
	{
		audio.PlayOneShot(fingerDownSounds[idx]);
	}
	
	
	public void OnPlayerFingerDown (int finger, Vector2 pos)
	{		
		PlayOneShotAudio(currentPlaySndIdx);
		if(++currentPlaySndIdx>2)
			currentPlaySndIdx = 0;
		
		
		
		currentMousePoints.Add(pos);
		currentMovements.CalculateMovementBounds(pos);
		currentMovements.CalculateCurrentQuadrant(this, pos, currentMousePoints);
		
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, zOffset));
		GameObject.Instantiate(RippleObjectPrefab, transform.position, Quaternion.identity);
		
		mouseIsMovingWhileDown = true;
		
		currentMovements.NumTouchDowns += 1;
		currentMovements.TimeSinceLastTouchDown = 0.0f;		

		//game-mode: tapping in same spot calling out plankton
		if(currentFingerState != (FingerState)finger)
		{
			Vector2 v2D = transform.position;
			
			currentFingerState = (FingerState)finger;
			if(currentFingerState == FingerState.Single)
			{
				//current semi-hack to place pinched-prefab object. must also be handled in the OnPinchEnd()
				if(PinchCreateObjectPrefab != null)
				{
					//Network.Instantiate(PinchCreateObjectPrefab, v2D, Quaternion.identity, 0);
				}
				
				Vector2 diff = lastMouseDownPos - pos;
				
				if(diff.magnitude < 30.0f)
				{
					if(++numberOfTapsInSameSpot > 1)
					{
						//if(UnityEngine.Random.Range (0, 100) > 25.0f)						
						fluidField.IncreaseSpiritParticles(1, isPlayer1 ? 0 : 1);
						numberOfTapsInSameSpot = 0;
					}
				}
				//else
				{
				//	numberOfTapsInSameSpot = 0;
				}
				
				//DebugStreamer.message = "numberOfTapsInSameSpot: " + numberOfTapsInSameSpot.ToString();
			}
		}
		
		lastMouseDownPos = pos;
	}
	
	
	
	
	public void OnPlayerFingerMove (int finger, Vector2 pos)
	{		
		currentMovements.TimeSinceLastMouseMove = 0.0f;
		currentMovements.CalculateMovementBounds(pos);
		currentMovements.CalculateCurrentQuadrant(this, pos, currentMousePoints);
		
		int currentMousePointCount = currentMousePoints.Count;
		if(currentMousePointCount > 1)
		{
			Vector2 prev = (Vector2)currentMousePoints[currentMousePointCount-1];
			Vector2 diff = new Vector2(prev.x-pos.x, prev.y-pos.y);
			
			Vector2 world1 = Camera.main.ScreenToViewportPoint(pos);
			Vector2 world2 = Camera.main.ScreenToViewportPoint(prev);
			float deltaX = Mathf.Abs(world2.x-world1.x);
			float deltaY = Mathf.Abs(world2.y-world1.y);
			
			float degsCur = ToDegrees(Mathf.Atan2(deltaY, deltaX));
			currentMovements.TotalAngleChanges += Mathf.Abs(degsCur);
			currentMovements.DistanceTraveled += Mathf.Sqrt((deltaX*deltaX) +(deltaY*deltaY));
		}
		
		/*
		//go thru lines and find intersects		
		currentMousePointCount -= 5;
		currentMovements.LineIntersects = 0;
		int startJ = 2;
		for(int i = 1; i < currentMousePointCount; i += 5)
		{
			Vector2 x1 = (Vector2)currentMousePoints[i-1];
			Vector2 x2 = (Vector2)currentMousePoints[i+3];
			
			for(int j = startJ; j < currentMousePointCount; j += 5)
			{
				Vector2 y1 = (Vector2)currentMousePoints[j-1];
				Vector2 y2 = (Vector2)currentMousePoints[j+3];
				
				if(LinesIntersect(x1, x2, y1, y2))
					currentMovements.LineIntersects += 1;				
			}			
			
			startJ+=2;			
		}
		*/
		
		
		if(currentMousePoints.Count > 60)
			currentMousePoints.RemoveAt(0);
		currentMousePoints.Add(pos);
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, zOffset));
		currentFingerState = (FingerState)finger;
	}
	
	
	public bool LinesIntersect(Vector2 x1, Vector2 x2, Vector2 y1, Vector2 y2)
    {
        float A1 = x1.y - x2.y;
        float B1 = x2.x - x1.x;
        float C1 = A1*x1.x+B1*x1.y;

        float A2 = y1.y - y2.y;
        float B2 = y2.x - y1.x;
        float C2 = A2 * y1.x + B2 * y1.y;

        float det = A1*B2 - A2*B1;

        if(det != 0.00f)
        {
            float x = (B2 * C1 - B1 * C2) / det;
            float y = (A1 * C2 - A2 * C1) / det;

            float minx1 = Mathf.Min(x1.x, x2.x);
            float maxx1 = Mathf.Max(x1.x, x2.x);

            float miny1 = Mathf.Min(x1.y, x2.y);
            float maxy1 = Mathf.Max(x1.y, x2.y);
			
            if((minx1 < x && x < maxx1) && (miny1 < y && y < maxy1))
			{
                return true;
			}

        }
        return false;   //parallellllll
    }
	
	public void OnPlayerFingerUp (int finger, Vector2 pos, float timeHeldDown)
	{
		mouseIsMovingWhileDown = false;
		currentFingerState = FingerState.None;
		previousMangle = 0;
		currentMousePoints.Clear();
		
		if(currentMovements.ResetHitQuadrants())
		{
			fluidField.ChangeColors();
			//change the level colah!
		}
	}
	
	private float pinchSizeAtBegin = 0;
	public void OnPinchBegin (Vector2 fingerPos1, Vector2 fingerPos2)		
	{
		
		if(RippleObjectPrefab != null)
		{
			pinchSizeAtBegin = (fingerPos2-fingerPos1).magnitude;
			//GameObject.Instantiate(RippleObjectPrefab, fingerPos1, Quaternion.identity);
			//GameObject.Instantiate(RippleObjectPrefab, fingerPos2, Quaternion.identity);
		}
	}
	
	public void OnPinchEnd (Vector2 fingerPos1, Vector2 fingerPos2)		
	{
		
		if(PinchCreateObjectPrefab != null)
		{
			Vector2 diff = fingerPos2-fingerPos1;
			float mag = diff.magnitude;
			if(mag < 132.0f && diff.magnitude < pinchSizeAtBegin)
			{
				Vector2 worldPos = Camera.main.ScreenToWorldPoint(fingerPos1+(diff*0.5f));
				Network.Instantiate(PinchCreateObjectPrefab, worldPos, Quaternion.identity, 0);
			}
		}
	}
	
	#endregion
}
