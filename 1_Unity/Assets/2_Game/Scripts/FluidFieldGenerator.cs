using UnityEngine;
using System.Collections;
using System;


public class FluidFieldGenerator : MonoBehaviour
{
	public Material material;
	
	public int N = 96;
	private int N1 = 0;
	public int KCount = 3;
	public int PCount = 3;
	public float fluidFPS = 10.0f;
	public float viscocity = 10;
	public float density = 10;
	 
	public int ChunkFieldN = 8;
	public int VisualizerGridSize = 2;
	public int VisualizerGridFieldGridDensity = 64;	
	public bool UseThreading = false;
	
	private Vector2 gridAspectScale = new Vector2(1,1);
	private GameObject[,] fieldVisualizers = null;	    
	
	public ParticleSystem spiritParticleSystem = null;
	
	public float SpiritGravity = 2.0f;
	public float SpiritToPlayerAttraction = 3.0f;
	public float SpritParticleSeperationDistance = 50;
	private bool spiritParticlesAllConnected = false;
	public float SpirtParticlesEffectedByGridVelocity = 8.0f;
	
	public class SpiritParticle
	{
		public Vector2 velocity = Vector2.zero;
		public Vector2 position = Vector2.zero;
		public float mass = 1.0f;
		public float initTime = 1.0f;
		
		public SpiritParticle(float m)
		{
			mass = m;
		}
	};
	
	private int maxSpiritParticles = 0;
	SpiritParticle[] spiritParticles;
	
	private struct PlayerMouseDownInfo
	{
		public GameObject player;
		public PlayerScript playerScript;
		
		public Vector3 previousScreenPos;
		public PlayerScript.FingerState previousMouseState;
		
		public float mouseRadius;
		public float inkFlow;
		public float velocityFlow;
	}
	
	public float InkClearRate = 0.5f;
	
	public float Player_1_MouseRadius = 50;
	public float Player_1_InkFlow = 1;
	public float Player_1_VelocityFlow = 1;
	
	public float Player_2_MouseRadius = 50;
	public float Player_2_InkFlow = 0;
	public float Player_2_VelocityFlow = 1;	
	
	private PlayerMouseDownInfo[] ownerPlayerMouseInfo = new PlayerMouseDownInfo[2];
	
    public float[,] u;
    private float[,] u0;
    public float[,] v;
    private float[,] v0;
    public float[,] densityField;
    private float[,] prevDensityField;
	
	
    private FieldChunk[,] chunks = null;
	
	
	public bool SpiritParticlesAllConnected()
	{
		return spiritParticlesAllConnected;
	}
	
	public class FieldChunk
    {
        public int fieldStartX;
        public int fieldStartY;
        public int fieldSize;
        public int mouseTime = 0;

        public FieldChunk(int xstart, int ystart, int size)
        {
            fieldStartX = xstart;
            fieldStartY = ystart;
            fieldSize = size;
            mouseTime = 0;
        }
    }	
	
	void Start()
	{
		InitFields();
		InitVisualizerField();
		InitChunkFields();
		
		spiritParticles = new SpiritParticle[150];
		for(int i = 0; i < 150; ++i)	
		{
			spiritParticles[i] = new SpiritParticle(UnityEngine.Random.Range(3.0f, 30.0f));
		}
	}
	
	public Color FluidColor = Color.red;
	public Color InkColor = Color.cyan;
	
	void OnEnable()
	{
		networkView.enabled = true;
	}
	
	public void ChangeColors()
	{
		networkView.RPC ("RotateColors", RPCMode.All);
	}
	
	[RPC]	
	void RotateColors()
	{
		float h1, s1, v1;
		float h2, s2, v2;
		
		RGBToHSV(FluidColor, out h1, out s1, out v1);
		RGBToHSV(InkColor, out h2, out s2, out v2);
			
		h1 += 95.0f;//UnityEngine.Random.Range(-270.0f, 270.0f);
		h2 = h1 + 180.0f;
		
		if(h1 > 360.0f)
			h1 -= 360.0f;
		if(h2 > 360.0f)
			h2 -= 360.0f;
		if(h1 < 0.0f)
			h1 += 360.0f;
		if(h2 < 0.0f)
			h2 += 360.0f;
		
		FluidColor = HSVToRGB(h1, s1, v1, 1.0f);
		InkColor = HSVToRGB(h2, s2, v2, 1.0f);
	}
	
	public Color HSVToRGB(float h, float s, float v, float alpha)
	{
	    int V = (int)(v * 255);
	    if (s > 0.0f)
	    {
	        float H = (h / 360.0f) * 6;
	
	        int HFloor = (int)Math.Floor(H);    //split the hue up into 6 segments. Like a pizza!
	        float F = H - HFloor;
	        float v255 = 255 * v;
	
	        //our RGABC's!
	        int R = V;
	        int G = V;
	        int A = (int)(v255 * (1 - s));
	        int B = (int)(v255 * (1 - (s * F)));
	        int C = (int)(v255 * (1 - (s * (1 - F))));
	        
	        switch (HFloor)
	        {
	            case 0: R = V; G = C; B = A; break;
	            case 1: R = B; G = V; B = A; break;
	            case 2: R = A; G = V; B = C; break;
	            case 3: R = A; G = B; B = V; break;
	            case 4: R = C; G = A; B = V; break;
	            case 5: R = V; G = A; break;
	            default: B = V; break;  //because RG are defaulted to V
	        }
	
	        if (R < 0) R = 0;
	        else if (R > 255) R = 255;
	
	        if (G < 0) G = 0;
	        else if (G > 255) G = 255;
	
	        if (B < 0) B = 0;
	        else if (B > 255) B = 255;
	
	        return new Color(R/255.0f, G/255.0f, B/255.0f, alpha);
	    }
	
	    return new Color(V/255.0f, V/255.0f, V/255.0f, alpha);
	}	
	private void RGBToHSV(Color c, out float h, out float s, out float v)
    {
		int r = (int)(c.r * 255.0f);
		int g = (int)(c.g * 255.0f);
		int b = (int)(c.b * 255.0f);
		
        int Max, Min, Diff, Sum;
        // Of our RGB values, assign the highest value to Max, and the Smallest to Min
        if (r > g)
		{
			Max = r;
			Min = g;
		}
        else
		{
			Max = g;
			Min = r;
		}
        if (b > Max) Max = b;
        else if (b < Min) Min = b;
        Diff = Max - Min;
        Sum = Max + Min;
        v = (float)Max / 255.0f;
        if (Max == 0) s = 0;
        else s = (float)Diff / Max; 
        float q = 0;
        if (Diff != 0)
            q = 60.0f / Diff;
        if (Max == r)
        {
            if (g < b) h = (float)(360.0f + q * (g - b)) / 360.0f;
            else h = (float)(q * (g - b)) / 360.0f;
        }
        else if (Max == g) h = (float)(120.0f + q * (b - r)) / 360.0f;
        else if (Max == b) h = (float)(240.0f + q * (r - g)) / 360.0f;
        else h = 0.0f;
        s -= 0.0005f;    //just round down a bit...
    }

	
	public void IncreaseSpiritParticles(int spiritChange, int playerNm)
	{
		networkView.RPC ("ChangeAmountOfSpirtParticles", RPCMode.All, spiritChange, playerNm);
	}
	
	[RPC]
	void ChangeAmountOfSpirtParticles(int spiritChange, int playerNm)
	{
		if(spiritChange == -1)
		{
			maxSpiritParticles = 0;
		}
		else
		{
			maxSpiritParticles += 1;
			Vector3 playerpos = ownerPlayerMouseInfo[playerNm].player.transform.position;
			playerpos.x += UnityEngine.Random.Range (-25,25);
			playerpos.z += UnityEngine.Random.Range (-25,25);
			spiritParticles[maxSpiritParticles-1].position = playerpos;
		}
	}
	
	public void ExplosionFromTheCentre()
	{
		DoVelocityBurst(0.5f, 0.5f, 30, -20.0f);
		DoVelocityBurst(0.5f, 0.5f, 20, -4.0f);
		//DoInkBurst(0.5f, 0.5f, 30, 1000);
	}
	
	private void InitVisualizerField()
	{
		gridAspectScale = new Vector2(1.0f/(float)VisualizerGridSize, 1.0f/(float)VisualizerGridSize);
		fieldVisualizers = new GameObject[VisualizerGridSize,VisualizerGridSize];

		for(int i = 0; i < VisualizerGridSize; ++i)
		{
			for(int  j = 0; j < VisualizerGridSize; ++j)
			{
				int beginX = (int)((float)i * (float)N * gridAspectScale.x);
				int beginY = (int)((float)j * (float)N * gridAspectScale.y);
			
				fieldVisualizers[i, j] = new GameObject();
				fieldVisualizers[i, j].AddComponent(typeof(FieldVisualizer));
			
				FieldVisualizer fVis = fieldVisualizers[i, j].GetComponent<FieldVisualizer>();
				fVis.BuildFieldVisualizerVertices(this, VisualizerGridFieldGridDensity, N, beginX, beginY, gridAspectScale);
			}
		}
	}
	
	public void InitChunkFields()
    {
        chunks = new FieldChunk[ChunkFieldN, ChunkFieldN];

        float chunkAspectX = (1.0f/(float)ChunkFieldN);
        float chunkAspectY = (1.0f/(float)ChunkFieldN);

        int chunkSize = N / ChunkFieldN;;
        for (int i = 0; i < ChunkFieldN; ++i)
        {
            for (int j = 0; j < ChunkFieldN; ++j)
            {
                int beginX = (int)((float)i * (float)N * chunkAspectX);
			    int beginY = (int)((float)j * (float)N * chunkAspectY);
                chunks[i,j] = new FieldChunk(beginX, beginY, chunkSize);
            }
        }
    }
	
	void Update()
	{
	

	}

	void FixedUpdate()
	{	
		if(ownerPlayerMouseInfo[0].player == null)
		{
			ownerPlayerMouseInfo[0].player = GameObject.FindGameObjectWithTag("PLAYER1");
			if(ownerPlayerMouseInfo[0].player != null)
				ownerPlayerMouseInfo[0].playerScript = ownerPlayerMouseInfo[0].player.GetComponent<PlayerScript>();
			
			ownerPlayerMouseInfo[0].mouseRadius = Player_1_MouseRadius;
			ownerPlayerMouseInfo[0].inkFlow = Player_1_InkFlow;
			ownerPlayerMouseInfo[0].velocityFlow = Player_1_VelocityFlow;
		}
		
		if(ownerPlayerMouseInfo[1].player == null)
		{
			ownerPlayerMouseInfo[1].player = GameObject.FindGameObjectWithTag("PLAYER2");
			if(ownerPlayerMouseInfo[1].player != null)
				ownerPlayerMouseInfo[1].playerScript = ownerPlayerMouseInfo[1].player.GetComponent<PlayerScript>();
			
			ownerPlayerMouseInfo[1].mouseRadius = Player_2_MouseRadius;
			ownerPlayerMouseInfo[1].inkFlow = Player_2_InkFlow;
			ownerPlayerMouseInfo[1].velocityFlow = Player_2_VelocityFlow;
		}
		
		Vector3 position = transform.position;
		Vector3 scale = transform.localScale;
		
		float dt = 1.0f / fluidFPS;
		
		UpdateFluids(position, scale, Camera.main, viscocity, density, dt);
		
		for(int i = 0; i < VisualizerGridSize; ++i)
		{
			for(int  j = 0; j < VisualizerGridSize; ++j)
			{
				fieldVisualizers[i, j].GetComponent<FieldVisualizer>().UpdateLookBasedOnFluid(this, N, VisualizerGridSize, VisualizerGridSize, FluidColor, InkColor);
			}
		}
	}	
	
    private void InitFields()
    {
		N1 = N+2;
        u = new float[N1, N1];
		u0 = new float[N1,N1];
        v = new float[N1,N1];
        v0 = new float[N1,N1];		

        densityField = new float[N1,N1];
        prevDensityField = new float[N1,N1];

        for(int i = 0; i < N1; ++i)
		{
			for(int j =0; j < N1; ++j)
            {
				densityField[i,j] = 0;
                prevDensityField[i,j] = 0.0f;
                u[i,j] = u0[i,j] = 0.0f;
                v[i,j] = v0[i,j] = 0.0f;
            }
		}
    }	 
	
	public float amountOfSadness = 0;
	public float amountOfHappiness = 0.0f;
	public float amountOfAngriness = 0.0f;
	
	public void UpdateMouses(Camera camcam, float dt)
	{
		System.Random rand = new System.Random();
		
		Vector2[] playerScreenPos = new Vector2[2] { Vector2.zero, Vector2.zero};
		Vector2[] playerWorldPos = new Vector2[2] { Vector2.zero, Vector2.zero};
		
		int inkLinks = 0;
		
		float prevSad = amountOfSadness;
		float prevHap = amountOfHappiness;
		float prevAng = amountOfAngriness;
		
		amountOfSadness = 0;
		amountOfHappiness = 0;
		amountOfAngriness = 0;
		
		for(int m = 0; m < 2; ++m)
		{
			if(ownerPlayerMouseInfo[m].player == null)
				continue;
			if(ownerPlayerMouseInfo[m].playerScript.DoLinkInk())
				++inkLinks;
			
			playerWorldPos[m] = ownerPlayerMouseInfo[m].player.transform.position;
			Vector3 screenPos = camcam.WorldToViewportPoint(playerWorldPos[m]);
			
			playerScreenPos[m].x = screenPos.x;
			playerScreenPos[m].y = screenPos.y;
			if(ownerPlayerMouseInfo[m].playerScript.doInkBurst)
			{
				DoVelocityBurst(screenPos.x, screenPos.y, 10, 100);
				ownerPlayerMouseInfo[m].playerScript.doInkBurst = false;
			}
			
			amountOfSadness += ownerPlayerMouseInfo[m].playerScript.totalSadness;
			amountOfHappiness += ownerPlayerMouseInfo[m].playerScript.totalHappiness;
			amountOfAngriness += ownerPlayerMouseInfo[m].playerScript.totalAngriness;
						
			float mouseChangeX = ownerPlayerMouseInfo[m].previousScreenPos.x - screenPos.x;
			float mouseChangeY = ownerPlayerMouseInfo[m].previousScreenPos.y - screenPos.y;
			
			PlayerScript.FingerState curMouseState = ownerPlayerMouseInfo[m].playerScript.MouseFingerDown();
			
			if(ownerPlayerMouseInfo[m].previousMouseState != curMouseState)
			{
				mouseChangeX = mouseChangeY = 0;
				ownerPlayerMouseInfo[m].previousMouseState = curMouseState;
			}
			
			ownerPlayerMouseInfo[m].previousScreenPos = screenPos;
			
			if(ownerPlayerMouseInfo[m].playerScript.MouseFingerDown() == PlayerScript.FingerState.None)
				continue;
			
			int chunkSize = N / ChunkFieldN;
			int mouseIterations = 10;
			float singlePass = 1.0f / (float)(mouseIterations-1);
			for(int i = 0; i < mouseIterations; ++i)
			{
				float p = (float)i / (float)(mouseIterations-1);
				
				float curScreenPosx = screenPos.x + (mouseChangeX*p);
				float curScreenPosy = screenPos.y + (mouseChangeY*p);
				
				int xCell =  (int)(curScreenPosx * N);
				int yCell =  (int)(curScreenPosy * N);
				
				int xChunk = (int)(xCell / chunkSize);
				int yChunk = (int)(yCell / chunkSize);
				if(xChunk >= 0 && xChunk < ChunkFieldN && yChunk >= 0 && yChunk < ChunkFieldN)
	            	chunks[xChunk, yChunk].mouseTime += 2;
				
				float mouseRadius = ownerPlayerMouseInfo[m].mouseRadius;
				float dx = mouseChangeX*-100*singlePass;
				float dy = mouseChangeY*-100*singlePass;
				float mousePower = 1.0f;
				float velPower = ownerPlayerMouseInfo[m].velocityFlow;
				float inkflow = ownerPlayerMouseInfo[m].inkFlow;
				UpdateBlackHole(curScreenPosx, curScreenPosy, dx, dy, mouseRadius, velPower, mousePower, inkflow, dt);
			}
		}
		
		
		
		if(amountOfSadness > 0.5f)
			amountOfSadness *= 10;
		if(amountOfHappiness > 0.5f)
			amountOfHappiness *= 10;
		if(amountOfAngriness > 0.5f)
			amountOfAngriness *= 10;
		
		amountOfSadness = Mathf.Lerp(prevSad, amountOfSadness, dt * 0.33f);
		amountOfHappiness = Mathf.Lerp(prevHap, amountOfHappiness, dt * 0.33f);
		amountOfAngriness = Mathf.Lerp(prevAng, amountOfAngriness, dt * 0.33f);
			
		if(inkLinks == 2)
		{
			float offset = 15;
			for(int i = 0; i < 5; ++i)
			{
				float frx1 = UnityEngine.Random.Range(-offset, offset);
				float fry1 = UnityEngine.Random.Range(-offset, offset);
				
				float frx2 = UnityEngine.Random.Range(-offset, offset);
				float fry2 = UnityEngine.Random.Range(-offset, offset);
				InkAlongLine(playerWorldPos[0].x+frx1, playerWorldPos[0].y+fry1, playerWorldPos[1].x+frx2, playerWorldPos[1].y+fry2);
			}
		}
	}
	
    public void UpdateFluids(Vector3 position, Vector3 scale, Camera camcam, float visc, float diffus, float dt)
    {
		linesToDraw.Clear();
		
		UpdateMouses(camcam, dt);

		float viscosity = 0.000001f*visc;
		float diff = 0.000001f*diffus;
		
		float widthPerCell = camcam.GetScreenWidth() / (float)N;
		float heightPerCell = camcam.GetScreenHeight() / (float)N;
		
		for (int i = 0; i < ChunkFieldN; ++i)
        {
            for (int j = 0; j < ChunkFieldN; ++j)
            {
                float chunkSizex = widthPerCell * chunks[i, j].fieldSize;
                float chunkSizey = heightPerCell * chunks[i, j].fieldSize;

                float xpos = chunks[i, j].fieldStartX * widthPerCell;
                float ypos = chunks[i, j].fieldStartY * heightPerCell;
				
                chunks[i, j].mouseTime = 5;
			}
		}
		
		VelocityStep(viscosity, dt);
		DensityStep(diff, dt);
		
		UpdateSpritParticles(camcam);
    }
	
	public void UpdateSpritParticles(Camera camcam)
	{
		if(spiritParticleSystem == null)
			return;
		
		int pcount = maxSpiritParticles;//spiritParticleSystem.particleCount;
		UnityEngine.ParticleSystem.Particle[] particles = new UnityEngine.ParticleSystem.Particle[pcount];
		//spiritParticleSystem.GetParticles(particles);
		
		Vector3 centerOfScreen = camcam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
		float minLineDist = 75.0f;
		
		int pIdx = 1;		
		int numConnectedParticles = 0;
		
		float dt = Time.deltaTime;
		
		for(int i = 0; i < pcount; ++i)
		{
			SpiritParticle sp = spiritParticles[i];
			Vector3 ppos = sp.position;
			Vector3 screenPos = camcam.WorldToViewportPoint(ppos);
			int xCell = (int)(screenPos.x * N);
			int yCell = (int)(screenPos.y * N);
			
			sp.initTime -= dt;
			if(sp.initTime > 0.0f)
			{
				sp.mass = 1.0f - sp.initTime;
			}
			else
			{			
				sp.velocity *= 1.0f - (0.5f*dt);
				
				if(xCell >= 1 && xCell < N-1 && yCell >= 1 && yCell < N-1)
				{
					ppos.x += u[xCell,yCell] * SpirtParticlesEffectedByGridVelocity;
					ppos.y += v[xCell,yCell] * SpirtParticlesEffectedByGridVelocity;
					//this.densityField[xCell,yCell] += 0.01f;
				}
				else //bring em back towards the center of the screen
				{
					Vector3 sub = centerOfScreen-ppos;
					float len = sub.magnitude;
					sub.Normalize();
					sub *= ((SpiritGravity * (sp.mass * 100) / len));
					sp.velocity.x += sub.x;
					sp.velocity.y += sub.y;
				}			
				
				//gravy towards player-finger-downs
				for(int m = 0; m < 2; ++m)
				{
					if(ownerPlayerMouseInfo[m].player == null)
						continue;
					if(ownerPlayerMouseInfo[m].playerScript.MouseFingerDown() != PlayerScript.FingerState.None)
					{
						Vector3 sub = ownerPlayerMouseInfo[m].player.transform.position-ppos;
						float len = sub.magnitude;
						sub.Normalize();
						sub *= ((SpiritToPlayerAttraction * (sp.mass * 400) / len));
						sp.velocity.x += sub.x;
						sp.velocity.y += sub.y;
					}				
				}
				
				
				//Do N-Body gravitations!
				for(int j = pIdx; j < pcount; ++j)
				{
					SpiritParticle sp2 = spiritParticles[j];				
					Vector3 ppos2 = sp2.position;
					Vector3 sub = ppos2-ppos;
					float len = sub.magnitude;
					
					if(len < minLineDist)
					{
						//Add a line between all these guys!
						float alpha = 1-(len / minLineDist);
						Color col = new Color(1.0f, 1.0f, 1.0f, alpha);
						linesToDraw.Add(new LineToDraw(ppos, ppos2, col, col));
						++numConnectedParticles;
					}
					
					sub.Normalize();
					sub *= ((SpiritGravity * (sp.mass * sp2.mass) / len)) * 0.25f;
						
					if(len < SpritParticleSeperationDistance)
					{
						sp.velocity.x -= sub.x;
						sp.velocity.y -= sub.y;
					}
					else
					{
						sp.velocity.x += sub.x;
						sp.velocity.y += sub.y;
					}				
				}
			}
			
			++pIdx;			
			
			ppos.x += sp.velocity.x * dt;
			ppos.y += sp.velocity.y * dt;
			
			sp.position = ppos;
			particles[i].startLifetime = 2.5f;
			particles[i].lifetime = 5.0f;
			particles[i].position = ppos;
			particles[i].size = sp.mass * 20;
			particles[i].color = new Color32(248, 168, 255, 255);
		}
		
		//DebugStreamer.message = "maxSpiritParticles: " + maxSpiritParticles.ToString();
		spiritParticlesAllConnected = numConnectedParticles == maxSpiritParticles;
		spiritParticleSystem.SetParticles(particles, maxSpiritParticles);
	}
		
	private struct LineToDraw
	{
		public Vector2 pos1;
		public Vector2 pos2;
		public Color color1;
		public Color color2;
		public LineToDraw(Vector2 v1, Vector2 v2, Color col1, Color col2)
		{
			pos1 = v1;
			pos2 = v2;
			color1 = col1;
			color2 = col2;
		}
	};
	private ArrayList linesToDraw = new ArrayList();
	
	public void PostRenderParticles() 
	{
		GL.PushMatrix();
		material.SetPass(0);
		
		int cnt = linesToDraw.Count;
		for(int i = 0; i < cnt; ++i)
		{
			LineToDraw ltd = (LineToDraw)linesToDraw[i];			
			GL.Begin(GL.LINES);
			GL.Color(ltd.color1);
			GL.Vertex(ltd.pos1);
			GL.Color (ltd.color2);
			GL.Vertex(ltd.pos2);
			GL.End();
		}
		
		GL.PopMatrix();
	}
	
	public void UpdateBasedOnBlackHole(BlackHoleScript bhole)
	{
		Camera camcam = Camera.main;
		float screenWidth = camcam.GetScreenWidth()-1;
		float screenHeight = camcam.GetScreenHeight()-1;
		
		float dt = 1.0f / fluidFPS;
		
		Vector3 bholePos = bhole.transform.position;		
		Vector3 screenPos = camcam.WorldToScreenPoint(bholePos);
		screenPos = camcam.ScreenToViewportPoint(screenPos);
		
		int radius = bhole.radius;		
		float velocityPower = bhole.velocityPower;
		float holePower = bhole.holePower;
		float goalValue = bhole.inkSpit;		
		
		float dx = bhole.spewingDirection.x;
		float dy = bhole.spewingDirection.y;
		
		UpdateBlackHole(screenPos.x, screenPos.y, dx, dy, radius, velocityPower, holePower, goalValue, dt);
	}
	
	
	public void DropInkAt(float worldPosx, float worldPosy, int radius, float burstStrength)
	{
		Vector3 screenPos = Camera.main.WorldToViewportPoint(new Vector3(worldPosx, worldPosy, 0));
		DoInkBurst(screenPos.x, screenPos.y, radius, burstStrength);		
	}
	
	public void DropVelocityAt(float worldPosx, float worldPosy, int radius, float burstStrength)
	{
		Vector3 screenPos = Camera.main.WorldToViewportPoint(new Vector3(worldPosx, worldPosy, 0));
		DoVelocityBurst(screenPos.x, screenPos.y, radius, burstStrength);		
	}
	
	public void DropVelocityInDirection(float worldPosx, float worldPosy, float dirx, float diry, float burstStrength)
	{
		Vector2 screenPos1 = Camera.main.WorldToViewportPoint(new Vector2(worldPosx, worldPosy));
		DoVelocityInDirection(screenPos1.x, screenPos1.y, dirx, diry, burstStrength);		
	}
	
	private void DoVelocityInDirection(float posx1, float posy1, float dirx, float diry, float burstStrength)
	{
		int x1 = (int)(posx1 * N);
		int y1 = (int)(posy1 * N);
		
		if(x1 >= 0 && x1 < N && y1 >= 0 && y1 < N)
		{
			u0[x1, y1] += dirx*burstStrength;
			v0[x1, y1] += diry*burstStrength;
		}
	}
	
	private void DoVelocityBurst(float posx1, float posy1, int radius, float burstStrength)
	{
		int x1 = (int)(posx1 * N);
		int y1 = (int)(posy1 * N);
		
		int hr = radius / 2;
		
		for(int i = x1-hr; i < x1+hr; ++i)
		{
			for(int j = y1-hr; j < y1+hr; ++j)
			{
				int xDelta = x1-i;
				int yDelta = y1-j;
				
				if(i >= 0 && i < N && j >= 0 && j < N)
				{
					u[i, j] += xDelta*burstStrength;
					v[i, j] += yDelta*burstStrength;
				}
			}
		}		
	}
	
	private void DoInkBurst(float posx1, float posy1, int radius, float burstStrength)
	{
		int x1 = (int)(posx1 * N);
		int y1 = (int)(posy1 * N);
		
		int hr = radius / 2;
		
		for(int i = x1-hr; i < x1+hr; ++i)
		{
			for(int j = y1-hr; j < y1+hr; ++j)
			{
				int xDelta = x1-i;
				int yDelta = y1-j;
				
				if(i >= 0 && i < N && j >= 0 && j < N)
				{
					prevDensityField[i, j] += burstStrength;
				}
			}
		}		
	}
	
	public void InkAlongLine(float posx1, float posy1, float posx2, float posy2)
	{
		
		Vector2 v1 = new Vector2(posx1, posy1);
		Vector2 v2 = new Vector2(posx2, posy2);
		
		Vector2 half = v1 + ((v2-v1)*0.5f);
		
		
		Color col1 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		Color col2 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		
		
		linesToDraw.Add(new LineToDraw(v1, half, col2, col1));
		linesToDraw.Add(new LineToDraw(half, v2, col1, col2));
		
		
		
		
		
		
		return;
		
		int x1 =  (int)(posx1 * N);
		int y1 =  (int)(posy1 * N);
		
		int x2 =  (int)(posx2 * N);
		int y2 =  (int)(posy2 * N);
		
		Vector3 line = new Vector3(x2-x1, y2-y1, 0);
		int dx = (int)Mathf.Abs(line.x);
        int dy = (int)Mathf.Abs(line.y);
            
		
		line = Quaternion.Euler(0, 0, 90) * line;
		
		int sx = 1;
		int sy = 1;
            
        if (x1 > x2)
			sx = -1;
        if (y1 > y2)
			sy = -1;

		int err = dx - dy;

        while (true)
        {
			if(x1 >= 0 && x1 < N && y1 >= 0 && y1 < N)
			{
				u[x1,y1] = line.x*0.05f;
				v[x1,y1] = line.y*0.05f;
			//	densityField[x1, y1] += 20.1f;
			}
            if (x1 == x2 && y2 == y1)
                break;
			
			int e2 = 2 * err;
            if (e2 > -dy)
            {
                err = err - dy;
                x1 = x1 + sx;
            }
            if (e2 < dx)
            {
                err = err + dx;
                y1 = y1 + sy;
            }
		}
	}
	
	private void UpdateBlackHole(float x, float y, float dx, float dy, float radius, float velocitypower, float holePower, float inkValue, float dt)
	{			
		if(radius < 1)
			return;
		
		float centerXCell =  x * N;
        float centerYCell =  y * N;
		
		float pixelsPerSquareX = Camera.main.GetScreenWidth() / (float)N;
		float pixelsPerSquareY = Camera.main.GetScreenHeight() / (float)N;
		
		float hrX = (radius / pixelsPerSquareX) * 0.5f;
		float hrY = (radius / pixelsPerSquareY) * 0.5f;
		float maxRadius = (float)Math.Sqrt((hrX*hrX)+(hrY*hrY))*0.5f;
		
		for(float fy = -hrY; fy <= hrY; fy += 1)
		{
			for(float fx = -hrX; fx <= hrX; fx += 1)
			{
				float fxCell = centerXCell+fx;
				float fyCell = centerYCell+fy;
				
				float distanceX = Math.Abs(fxCell-centerXCell);
				float distanceY = Math.Abs(fyCell-centerYCell);
				float totalD = (float)Math.Sqrt((distanceX*distanceX)+(distanceY*distanceY));
				if(totalD > maxRadius)
					continue;
				
				int xCell = (int)(fxCell);
				int yCell = (int)(fyCell);					
				
				if(xCell >= 0 && xCell < N && yCell >= 0 && yCell < N)
				{
					float xFrac = 1-(fxCell-xCell);
					float yFrac = 1-(fyCell-yCell);

					float directionX = dx;
					float directionY = dy;
					
					u0[xCell,yCell] += directionX*velocitypower*dt;
					v0[xCell,yCell] += directionY*velocitypower*dt;
									
					//float a = SampleField(densityField, fxCell, fyCell);
					//float b = goalValue;
					//float difference = b-a;
					//float change = difference * (holePower*dt*dt);				
					densityField[xCell, yCell] += inkValue;
				}
			}
		}
	}
	
	public void UpdateDensityStep(float diff, float dt, int chunkX, int chunkY, int NCOUNT, int startX, int startY)
	{
		AddDensitySource(startX, startY, NCOUNT, InkClearRate*0.1f, dt);
		Diffuse(startX, startY, NCOUNT, 0, prevDensityField, densityField, diff, dt);
		AdvectDensity(startX, startY, NCOUNT, 0, dt);
	}
	
    private void DensityStep(float diff, float dt)
    {
		for (int i = 0; i < ChunkFieldN; ++i)
        {
            for (int j = 0; j < ChunkFieldN; ++j)
            {
				int startX = chunks[i,j].fieldStartX;
				int startY = chunks[i,j].fieldStartY;
				int NCOUNT = chunks[i,j].fieldSize;
				
				if(UseThreading)
					System.Threading.ThreadPool.QueueUserWorkItem(new FluidUpdateWorker(this, diff, dt, i, j, startX, startY, NCOUNT).ThreadDiffusionStep);	
				else
					UpdateDensityStep(diff, dt, i, j, NCOUNT, startX, startY);
			}
		}
		//FluidUpdateWorker.AllWorkersCompleted.WaitOne();
    }

	public float SampleField(float[,] field, float xcoord, float ycoord)
	{
		int aX = (int)xcoord;
		int aY = (int)ycoord;
		
		float d0 = field[aX,   aY];
		float d1 = field[aX+1, aY];
		float d2 = field[aX+1, aY+1];
		float d3 = field[aX,   aY+1];
		
		float xFrac = xcoord - aX;
		float yFrac = ycoord - aY;
		
		return FloatLerp(yFrac, FloatLerp(xFrac, d0, d1), FloatLerp(xFrac, d2, d3));
	}		
	
	public static float FloatLerp(float t, float a, float b)
    {
        return (a + ((b - a) * t));
    }
	
	public void UpdateVelocityFieldStep_1(float visc, float dt, int chunkX, int chunkY, int NCOUNT, int startX, int startY)
	{
		AddVelocitySources(startX, startY, NCOUNT, dt);
        Diffuse(startX, startY, NCOUNT, 1, u0, u, visc, dt);
        Diffuse(startX, startY, NCOUNT, 2, v0, v, visc, dt);		
	}
	
	public void UpdateVelocityFieldStep_2(float visc, float dt, int chunkX, int chunkY, int NCOUNT, int startX, int startY)
	{
		Advect(startX, startY, NCOUNT, 1, u, u0, u0, v0, dt);
		Advect(startX, startY, NCOUNT, 2, v, v0, u0, v0, dt);			
	}
	
    private void VelocityStep(float visc, float dt)
    {			
		for (int i = 0; i < ChunkFieldN; ++i)
        {
            for (int j = 0; j < ChunkFieldN; ++j)
            {
				int startX = chunks[i,j].fieldStartX;
				int startY = chunks[i,j].fieldStartY;
				int NCOUNT = chunks[i,j].fieldSize;
				UpdateVelocityFieldStep_1(visc, dt, i, j, NCOUNT, startX, startY);
				//System.Threading.ThreadPool.QueueUserWorkItem(new FluidUpdateWorker(this, visc, dt, i, j, startX, startY, NCOUNT).VelocityStep1);			
			}
		}
		
		//FluidUpdateWorker.AllWorkersCompleted.WaitOne();
		Project(0, 0, N, u0, v0, u, v);		
		
		for (int i = 0; i < ChunkFieldN; ++i)
        {
            for (int j = 0; j < ChunkFieldN; ++j)
            {
				int startX = chunks[i,j].fieldStartX;
				int startY = chunks[i,j].fieldStartY;
				int NCOUNT = chunks[i,j].fieldSize;
				
				UpdateVelocityFieldStep_2(visc, dt, i, j, NCOUNT, startX, startY);
				//System.Threading.ThreadPool.QueueUserWorkItem(new FluidUpdateWorker(this, visc, dt, i, j, startX, startY, NCOUNT).VelocityStep2);	
			}
		}
		
		//FluidUpdateWorker.AllWorkersCompleted.WaitOne();
		Project(0, 0, N, u, v, u0, v0);			
    }
	
	public class FluidUpdateWorker
    {
        FluidFieldGenerator fluid;
		
		int chunkX;
		int chunkY;
		
		int startIdxX;
		int startIdxY;
		
		float frame_dt;		
		int NCount = 0;		
		float viscoscity = 0.0f;
		
        internal static readonly System.Threading.EventWaitHandle AllWorkersCompleted = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        private static int numberOfWorkers = 0;		
		
        public FluidUpdateWorker(FluidFieldGenerator f, float visc, float dt, int chnkX, int chnkY, int idxx, int idxy, int NCOUNT)
        {
            fluid = f;
			viscoscity = visc;
			frame_dt = dt;
			chunkX = chnkX;
			chunkY = chnkY;
			startIdxX = idxx;
			startIdxY = idxy;
			NCount = NCOUNT;
            System.Threading.Interlocked.Increment(ref numberOfWorkers);
        }
		
        public void ThreadDiffusionStep(object o)
        {
            fluid.UpdateDensityStep(viscoscity, frame_dt, chunkX, chunkY, NCount, startIdxX, startIdxY);
            
			if (System.Threading.Interlocked.Decrement(ref numberOfWorkers) == 0)
            {
                AllWorkersCompleted.Set();
            }
		}
		
        public void ThreadVelocityStep1(object o)
        {
            fluid.UpdateVelocityFieldStep_1(viscoscity, frame_dt, chunkX, chunkY, NCount, startIdxX, startIdxY);
            
			if (System.Threading.Interlocked.Decrement(ref numberOfWorkers) == 0)
            {
                AllWorkersCompleted.Set();
            }
        }
		
		public void ThreadVelocityStep2(object o)
        {
            fluid.UpdateVelocityFieldStep_2(viscoscity, frame_dt, chunkX, chunkY, NCount, startIdxX, startIdxY);
            
			if (System.Threading.Interlocked.Decrement(ref numberOfWorkers) == 0)
            {
                AllWorkersCompleted.Set();
            }
        }
	}
	
    private void Diffuse(int startX, int startY, int NCOUNT, int b, float[,]x, float[,] x0, float diff, float dt)
    {
        int i, j, k;
        float a=dt*diff*N*N;
        float divisor = (1.0f + 4.0f * a);

        for (k = 0; k < KCount; k++)
        {
            for (i = 1; i <= NCOUNT; i++)
            {
                for (j = 1; j <= NCOUNT; j++)
                {
					int idxI = startX+i;
					int idxJ = startY+j;
					
                    x[idxI, idxJ] = (x0[idxI, idxJ] + a * (x[idxI - 1, idxJ] + x[idxI + 1, idxJ] + x[idxI, idxJ - 1] + x[idxI, idxJ + 1])) / divisor;
                }
            }
        }
        
        SetBoundary(b, x);            
    }
	
    private void Advect(int startX, int startY, int NCOUNT, int b, float[,] d, float[,] d0, float[,] inu, float[,] inv, float dt)
    {
        int i, j, j0, i1, j1;
        float x, y, s0, t0, s1, t1;

        float dt0 = dt*N;

        for (i = 1; i <= NCOUNT; ++i)
        {
            for (j = 1; j <= NCOUNT; ++j)
            {
				int idxI = startX+i;
				int idxJ = startY+j;
				
                x = idxI - dt0 * inu[idxI, idxJ];
                y = idxJ - dt0 * inv[idxI, idxJ];

                if (x<0.5)
                    x=0.5f;
                if (x>N+0.5)
                    x=N+ 0.5f;
                
                if (y<0.5f)
                    y=0.5f;
                if (y>N+0.5f)
                    y=N+0.5f;

                int i0 = (int)x;
                i1 = i0 + 1;

                j0 = (int)y;
                j1 = j0+1;
                s1 = x-i0;
                s0 = 1-s1;
                t1 = y-j0;
                t0 = 1-t1;

                d[idxI, idxJ] = s0 * (t0 * d0[idxI, idxJ] + t1 * d0[i0, j1]) + s1 * (t0 * d0[i1, j0] + t1 * d0[i1, j1]);
            }
        }

        SetBoundary(b, d);
    }
	
	private void AdvectDensity(int startX, int startY, int NCOUNT, int b, float dt)
    {
        int i, j, j0, i1, j1;
        float x, y, s0, t0, s1, t1;

        float dt0 = dt*N;

        for (i = 1; i <= NCOUNT; ++i)
        {
            for (j = 1; j <= NCOUNT; ++j)
            {
				int idxI = startX+i;
				int idxJ = startY+j;
				
                x = idxI - dt0 * u[idxI, idxJ];
                y = idxJ - dt0 * v[idxI, idxJ];

                if (x<0.5)
                    x=0.5f;
                if (x>N+0.5f)
                    x=N+0.5f;
                if (y<0.5f)
                    y=0.5f;
                if (y>N+0.5)
                    y=N+0.5f;

                int i0 = (int)x;
                i1 = i0 + 1;

                j0 = (int)y;
                j1 = j0+1;
                s1 = x-i0;
                s0 = 1-s1;
                t1 = y-j0;
                t0 = 1-t1;

                float f = s0 * (t0 * prevDensityField[idxI, idxJ] + t1 * prevDensityField[i0, j1]) + s1 * (t0 * prevDensityField[i1, j0] + t1 * prevDensityField[i1, j1]);
				//if(f>1)
				//	f=1;
				//else if(f<0)
				//	f=0;
				densityField[idxI, idxJ] = f;
            }
        }

        SetBoundary(b, densityField);
    }	
	
    private void Project(int startX, int startY, int NCOUNT, float[,] inu, float[,] inv, float[,] p, float[,] div)
    {
        int i, j, k;
        float h = 1.0f / NCOUNT;
			
        for (i = 1; i <= NCOUNT; i++)
        {
            for (j = 1; j <= NCOUNT; j++)
            {
				int idxi = startX+j;
				int idxj = startY+i;				
                div[idxi, idxj] = -0.5f * h * (inu[idxi + 1, idxj] - inu[idxi - 1, idxj] + inv[idxi, idxj + 1] - inv[idxi, idxj - 1]);
                p[idxi, idxj] = 0;
            }
        }

        SetBoundary(0, div);
        SetBoundary(0, p);

        for (k = 0; k < PCount; k++)
        {
            for (i = 1; i <= NCOUNT; i++)
            {
                for (j = 1; j <= NCOUNT; j++)
                {
					int idxi = startX+i;
					int idxj = startY+j;
                    p[idxi, idxj] = (div[idxi, idxj] + p[idxi - 1, idxj] + p[idxi + 1, idxj] + p[idxi, idxj - 1] + p[idxi, idxj + 1]) / 4.0f;
                }
            }
            SetBoundary(0, p);
        }

        for (i = 1; i <= NCOUNT; i++)
        {
            for (j = 1; j <= NCOUNT; j++)
            {
				int idxi = startX+i;
				int idxj = startY+j;
                inu[idxi, idxj] -= 0.5f * NCOUNT * (p[idxi + 1, idxj] - p[idxi - 1, idxj]);
                inv[idxi, idxj] -= 0.5f * NCOUNT * (p[idxi, idxj + 1] - p[idxi, idxj - 1]);
            }
        }

        SetBoundary(1, inu);
        SetBoundary(2, inv);
    }
	
	private void AddVelocitySources(int startX, int startY, int NCOUNT, float dt)
    {	
		for(int i = 0; i < NCOUNT; ++i)
		{
			for(int j = 0; j < NCOUNT; ++j)
			{
				int idxI = startX+i;
				int idxJ = startY+j;				
				
				u[idxI, idxJ] += u0[idxI, idxJ] * dt;
				v[idxI, idxJ] += v0[idxI, idxJ] * dt;
			}
		}
    }
	
	private void AddDensitySource(int startX, int startY, int NCOUNT, float removalRate, float dt)
    {	
		float mult = 1.0f - removalRate;
		for(int i = 0; i < NCOUNT; ++i)
		{
			for(int j = 0; j < NCOUNT; ++j)
			{
				int idxI = startX+i;
				int idxJ = startY+j;
				
				densityField[idxI, idxJ] += prevDensityField[idxI, idxJ] * dt;
				densityField[idxI, idxJ] *= mult;					
			}
		}
    }
	
    private void SetBoundary(int b, float[,] x)
    {
        for (int i=1 ; i<=N ; ++i )
        {
            x[0  , i] 	= b == 1 ? -x[1, i] : x[1, i];
            x[N+1, i] 	= b == 1 ? -x[N, i] : x[N, i];
            
			x[i, 0] 	= b == 2 ? -x[i, 1] : x[i, 1];
			x[i, N+1] 	= b == 2 ? -x[i, N] : x[i, N];
        }

        x[0, 0] 	= 0.5f * (x[1, 0  ] + x[0, 1]);
        x[0, N+1] 	= 0.5f * (x[1, N+1] + x[0, N]);
        x[N,0] 		= 0.5f * (x[N, 0  ] + x[N+1, 1]);
        x[N,N+1] 	= 0.5f * (x[N, N+1] + x[N+1, N]);
    }
}






public class FieldVisualizer : MonoBehaviour
{
	private int width = 32;
	private int height = 32;
	private int arraySize = 0;	
	
	private Color32[] vertColors = null;
	private Vector3[] vertPositions = null;

	private int N = 0;
	private int beginXIdx = 0;
	private int beginYIdx = 0;
	
	public void BuildFieldVisualizerVertices(FluidFieldGenerator fluidField, int gridSize, int n, int bXIdx, int bYIdx, Vector2 gridAspectScale)
	{		
		N = n;
		width = height = gridSize;
		arraySize = height*width;
		beginXIdx = bXIdx;
		beginYIdx = bYIdx;
		
		gameObject.transform.parent = fluidField.transform;
		gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent("MeshRenderer");
		
		renderer.material = fluidField.material;		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		vertPositions = new Vector3[arraySize];
		vertColors = new Color32[arraySize];
		Vector2[] uv = new Vector2[arraySize];
		
		Camera camcam = Camera.main;
		float screenWidth = camcam.GetScreenWidth()-1;
		float screenHeight = camcam.GetScreenHeight()-1;
			
		float screenWidthPerBlockX = screenWidth * gridAspectScale.x;
		float screenWidthPerBlockY = screenHeight * gridAspectScale.y;
		
		float initialXPosition = (beginXIdx / (float)(N) / gridAspectScale.x) * screenWidthPerBlockX;
		float initialYPosition = (beginYIdx / (float)(N) / gridAspectScale.y) * screenWidthPerBlockY;
		
		initialXPosition -= screenWidth * 0.5f;
		initialYPosition -= screenHeight * 0.5f;
		
		float cellWidth = screenWidthPerBlockX / (float)(width-1);
		float cellHeight = screenWidthPerBlockY / (float)(height-1);
			
		System.Random rand = new System.Random();
		
		for (int y=0;y<height;y++)
		{
			for (int x=0;x<width;x++)
			{
				float xpos = initialXPosition + (x*cellWidth);
				float ypos = initialYPosition + (y*cellHeight);
				
				Vector3 vertPos = new Vector3(xpos, ypos, 0);
				int idx = y*width+x;
				
				vertPositions[idx] = vertPos;
				uv[idx] = new Vector2 (xpos, ypos);
				vertColors[idx] = new Color32(0, 255, 0, 255);
				
				//float randomX = (float)rand.NextDouble() * particleGridOffsets;
				//float randomY = (float)rand.NextDouble() * particleGridOffsets;
				
				//particles[idx].position = new Vector3(vertPositions[idx].x+randomX, vertPositions[idx].y+randomY, vertPositions[idx].z-10);
				//particles[idx].color = new Color32(173, 194, 72, 32);
				//particles[idx].lifetime = 10000.0f;
				//particles[idx].startLifetime = 0.0f;				
			}
		}
		
		mesh.vertices = vertPositions;
		mesh.colors32 = vertColors;
		mesh.uv = uv;
		
		//indices
		int[] triangles = new int[(height - 1) * (width - 1) * 6];
		int index = 0;
		for (int y = 0; y < height-1; ++y)
		{
			for (int x = 0; x < width-1; ++x)
			{
				// For each grid cell output two triangles
				triangles[index++] = (y * width) + x;
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = (y * width) + x + 1;
	
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = ((y+1) * width) + x + 1;
				triangles[index++] = (y * width) + x + 1;
			}
		}
		
		mesh.triangles = triangles;
		//mesh.RecalculateNormals();
		mesh.Optimize();
	}
		
	public void UpdateLookBasedOnFluid(FluidFieldGenerator fluidField, float N, float vertDivsX, float vertDivsY, Color fluidColor, Color inkColor)
	{
        float sw = width-1;
        float sh = height-1;
		
		float sadColorRating = fluidField.amountOfSadness*0.5f;
		float happyColorRating = fluidField.amountOfHappiness*0.5f;
		float angryColorRating = fluidField.amountOfAngriness*0.5f;
		
		for (int y = 0; y < height; ++y)
		{
			float yperc = (float)y / sh;
			float fycord = beginYIdx + ((yperc * N) / vertDivsY);
			int colorIdx = y*width;
			
			for (int x = 0; x < width; ++x)
			{
				float xperc = (float)x / sw;
				float fxcord = beginXIdx + ((xperc * N) / vertDivsX);
				
				float vval = Math.Abs (fluidField.SampleField(fluidField.u, fxcord, fycord));
                vval += Math.Abs(fluidField.SampleField(fluidField.v, fxcord, fycord));
				vval *= 512.0f;
				float dval = fluidField.SampleField(fluidField.densityField, fxcord, fycord) * 5;
				
				float fr = vval * fluidColor.r;
				float fg = vval * fluidColor.g;
				float fb = vval * fluidColor.b;
				
				float ir = dval * inkColor.r;
				float ig = dval * inkColor.g;
				float ib = dval * inkColor.b;
				
				byte vertColorR = (byte)Math.Min(255, (fr+ir));
				byte vertColorG = (byte)Math.Min(255, (fg+ig));
				byte vertColorB = (byte)Math.Min(255, (fb+ib));
				
				int vidx = colorIdx + x;
				vertColors[vidx].r = vertColorR;
				vertColors[vidx].g = vertColorG;
				vertColors[vidx].b = vertColorB;
				//vertPositions[vidx].z = vertColorR;
			}
		}
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.colors32 = vertColors;
		//mesh.vertices = vertPositions;
		//mesh.RecalculateNormals();		
	}
}