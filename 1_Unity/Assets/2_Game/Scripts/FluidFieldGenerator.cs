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

	private struct PlayerMouseDownInfo
	{
		public GameObject player;
		public PlayerScript playerScript;
		
		public Vector3 previousScreenPos;
		public PlayerScript.FingerState previousMouseState;
		
		
		public float mouseRadius;
		public float inkFlow;
		public float velocityFlow;
		public bool dropInk;
		public bool changeVelocity;
	}
	
	public float InkClearRate = 0.5f;
	
	public float Player_1_MouseRadius = 50;
	public float Player_1_InkFlow = 1;
	public float Player_1_VelocityFlow = 1;
	public bool Player_1_DropInk = true;
	public bool Player_1_ChangeVelocity = true;
	
	public float Player_2_MouseRadius = 50;
	public float Player_2_InkFlow = 0;
	public float Player_2_VelocityFlow = 1;
	public bool Player_2_DropInk = true;
	public bool Player_2_ChangeVelocity = true;
	
	
	public float Color_Fluid_R = 0.0f;
	public float Color_Fluid_G = 0.0f;
	public float Color_Fluid_B = 1.0f;
	
	public float Color_Ink_R = 0.0f;
	public float Color_Ink_G = 1.0f;
	public float Color_Ink_B = 0.0f;
	
	private PlayerMouseDownInfo[] ownerPlayerMouseInfo = new PlayerMouseDownInfo[2];
	
    public float[,] u;
    private float[,] u0;
    public float[,] v;
    private float[,] v0;
    public float[,] densityField;
    private float[,] prevDensityField;
	
	
    private FieldChunk[,] chunks = null;
	
	
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
		if(ownerPlayerMouseInfo[0].player == null)
		{
			ownerPlayerMouseInfo[0].player = GameObject.FindGameObjectWithTag("PLAYER1");
			if(ownerPlayerMouseInfo[0].player != null)
				ownerPlayerMouseInfo[0].playerScript = ownerPlayerMouseInfo[0].player.GetComponent<PlayerScript>();
			
			
			ownerPlayerMouseInfo[0].mouseRadius = Player_1_MouseRadius;
			ownerPlayerMouseInfo[0].inkFlow = Player_1_InkFlow;
			ownerPlayerMouseInfo[0].velocityFlow = Player_1_VelocityFlow;
			ownerPlayerMouseInfo[0].dropInk = Player_1_DropInk;
			ownerPlayerMouseInfo[0].changeVelocity = Player_1_ChangeVelocity;
		}
		
		if(ownerPlayerMouseInfo[1].player == null)
		{
			ownerPlayerMouseInfo[1].player = GameObject.FindGameObjectWithTag("PLAYER2");
			if(ownerPlayerMouseInfo[1].player != null)
				ownerPlayerMouseInfo[1].playerScript = ownerPlayerMouseInfo[1].player.GetComponent<PlayerScript>();
			
			ownerPlayerMouseInfo[1].mouseRadius = Player_2_MouseRadius;
			ownerPlayerMouseInfo[1].inkFlow = Player_2_InkFlow;
			ownerPlayerMouseInfo[1].velocityFlow = Player_2_VelocityFlow;
			ownerPlayerMouseInfo[1].dropInk = Player_2_DropInk;
			ownerPlayerMouseInfo[1].changeVelocity = Player_2_ChangeVelocity;
		}
		
		
		
		Vector3 position = GetComponent<Transform>().position;
		Vector3 scale = GetComponent<Transform>().localScale;
		
		float dt = 1.0f / fluidFPS;
		
		UpdateFluids(position, scale, Camera.main, viscocity, density, dt);
		
		for(int i = 0; i < VisualizerGridSize; ++i)
		{
			for(int  j = 0; j < VisualizerGridSize; ++j)
			{
				fieldVisualizers[i, j].GetComponent<FieldVisualizer>().UpdateLookBasedOnFluid(this, N, VisualizerGridSize, VisualizerGridSize, Color_Fluid_R, Color_Fluid_G, Color_Fluid_B, Color_Ink_R, Color_Ink_G, Color_Ink_B);
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
	
	public void UpdateMouses(Camera camcam, float dt)
	{
		System.Random rand = new System.Random();
		
		Vector2[] playerScreenPos = new Vector2[2] { Vector2.zero, Vector2.zero};
		bool doLinkInk = false;
		
		for(int m = 0; m < 2; ++m)
		{
			if(ownerPlayerMouseInfo[m].player == null)
				continue;
			if(ownerPlayerMouseInfo[m].playerScript.DoLinkInk())
				doLinkInk = true;
			
			Vector3 worldPos = ownerPlayerMouseInfo[m].player.transform.position;
			Vector3 screenPos = camcam.WorldToScreenPoint(worldPos);
			screenPos = camcam.ScreenToViewportPoint(screenPos);
			
			playerScreenPos[m].x = screenPos.x;
			playerScreenPos[m].y = screenPos.y;
			
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
		
		if(doLinkInk)
			InkAlongLine(playerScreenPos[0].x, playerScreenPos[0].y, playerScreenPos[1].x, playerScreenPos[1].y);
	}
	
    public void UpdateFluids(Vector3 position, Vector3 scale, Camera camcam, float visc, float diffus, float dt)
    {			
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
	
	private void InkAlongLine(float posx1, float posy1, float posx2, float posy2)
	{
		int x1 =  (int)(posx1 * N);
		int y1 =  (int)(posy1 * N);
		
		int x2 =  (int)(posx2 * N);
		int y2 =  (int)(posy2 * N);
		
		int dx = (int)Mathf.Abs(x2 - x1);
        int dy = (int)Mathf.Abs(y2 - y1);
            
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
				densityField[x1, y1] = 1;
			
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
		float d0 = field[aX, aY];
		float d1 = field[aX+1, aY];
		float d2 = field[aX+1, aY+1];
		float d3 = field[aX, aY+1];		
		float xFrac = xcoord - aX;
		float yFrac = ycoord - aY;
		return LERP(yFrac, LERP(xFrac, d0, d1), LERP(xFrac, d2, d3));		
	}		
	
	private float LERP(float f, float v0, float v1)
	{
		return ((1.0f-(f))*(v0)+(f)*(v1));
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
	
	//private ParticleSystem ps = null;
	//private UnityEngine.ParticleSystem.Particle[] particles = null;
	
	private int N = 0;
	private int beginXIdx = 0;
	private int beginYIdx = 0;
	
	public FieldVisualizer()
	{
		
	}
	
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
		for (int y=0;y<height-1;y++)
		{
			for (int x=0;x<width-1;x++)
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
		mesh.RecalculateNormals();
		//mesh.tangents = tangents;
		
		
		//Find particle system..
		//ps = GameObject.Find("fluid particles").GetComponent<UnityEngine.ParticleSystem>();
		//particles = new UnityEngine.ParticleSystem.Particle[arraySize];
	}
		
	public void UpdateLookBasedOnFluid(FluidFieldGenerator fluidField, float N, float vertDivsX, float vertDivsY, float fluidR, float fluidG, float fluidB, float Color_Ink_R, float Color_Ink_G, float Color_Ink_B)
	{
        float sw = width-1;
        float sh = height-1;
		
		for (int y = 0; y < height; ++y)
		{
			float yperc = (float)y / sh;
			float fycord = beginYIdx + (yperc * N) / vertDivsY;
			int colorIdx = y*width;
			
			for (int x = 0; x < width; ++x)
			{
				float xperc = (float)x / sw;
				float fxcord = beginXIdx + (xperc * N) / vertDivsX;
				
				float vval = Math.Abs (fluidField.SampleField(fluidField.u, fxcord, fycord));
                vval += Math.Abs (fluidField.SampleField(fluidField.v, fxcord, fycord));
				float dval = fluidField.SampleField(fluidField.densityField, fxcord, fycord);
				
				float fr = vval * fluidR;
				float fg = vval * fluidG;
				float fb = vval * fluidB;
				
				float ir = dval * Color_Ink_R;
				float ig = dval * Color_Ink_G;
				float ib = dval * Color_Ink_B;
				
				byte vertColorR = (byte)Math.Min(255, (fr+ir)*0.5f*255);
				byte vertColorG = (byte)Math.Min(255, (fg+ig)*0.5f*255);
				byte vertColorB = (byte)Math.Min(255, (fb+ib)*0.5f*255);				
				
				//byte horizontalVelocity = (byte)Math.Max(0, Math.Min(255, 255*vval));
				//byte verticalVelocity = (byte)Math.Max(0, Math.Min(255, 255*uval));
				//byte densityColor = (byte)Math.Max(0, Math.Min(255, 255*dval));
				
				//particles[colorIdx].color = new Color32(72, 194, 178, 0);
				//particles[colorIdx].size = this.initialParticleGridSize;
				//particles[colorIdx].lifetime += dt;
				//particles[colorIdx].velocity = new Vector3(uval*100,-vval*100,0);
				//particles[colorIdx].position += particles[colorIdx].velocity*dt;					
				
				int vidx = colorIdx+x;
				vertColors[vidx].r = vertColorR;
				vertColors[vidx].g = vertColorG;
				vertColors[vidx].b = vertColorB;				
				//vertices[colorIdx].z = densityColor*0.1f;
			}
		}
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.colors32 = vertColors;
		mesh.vertices = vertPositions;
		//mesh.RecalculateNormals();		
	}
}