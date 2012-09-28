using UnityEngine;
using System.Collections;
using System;

public class FluidFieldGenerator : MonoBehaviour
{
	public Texture2D heightMap;
	public Material material;	
	
	public int N = 64;
	public int KKount = 3;
	public float fluidFPS = 10.0f;
	public float viscocity = 10;
	public float density = 10;
	
	public float particleGridOffsets = 0.5f;
	public float initialParticleGridSize = 25.0f;
	
	public float mousePower = 100;
	public float mouseVectorPower = 10;
	
	
	
	private int width = 0;
	private int height = 0;
	private Vector2 screenAspectScale;
	private float screenWidth = 0;
	private float screenHeight = 0;
	private int arraySize = 0;
	
	
	private QuickFluidSolver solver;
	private Color32[] colors = null;
	private Vector3[] vertices = null;
	
	private ParticleSystem ps = null;
	private UnityEngine.ParticleSystem.Particle[] particles = null;
	
	
	
	
	void Start()
	{
		GenerateHeightmap();
		solver = new QuickFluidSolver(N, KKount, screenAspectScale, initialParticleGridSize);
	}
	
	private void GenerateHeightmap()
	{
		gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent("MeshRenderer");
		
		if (material)
			renderer.material = material;
		else
			renderer.material.color = Color.white;
		Mesh mesh = GetComponent<MeshFilter>().mesh;		
		
		width = 255;//.Math.Min(heightMap.width, 255);
		height = 255;//System.Math.Min(heightMap.height, 255);
		arraySize = height*width;
		
		screenHeight = (2*Camera.main.orthographicSize);
  		screenWidth = (screenHeight*Camera.main.aspect);
		screenAspectScale = new Vector2(screenWidth/(float)width, screenHeight/(float)height);
		
		ps = GameObject.Find("fluid particles").GetComponent<UnityEngine.ParticleSystem>();
		particles = new UnityEngine.ParticleSystem.Particle[arraySize];
		
		vertices = new Vector3[arraySize];
		colors = new Color32[arraySize];
		Vector2[] uv = new Vector2[arraySize];
		Vector4[] tangents = new Vector4[arraySize];
		
		//Vector2 uvScale = new Vector2 (1.0f / (width - 1), 1.0f / (height - 1));
		
		float halfSizeWidth = width * 0.5f;
		float halfSizeHeight = height * 0.5f;
		
		System.Random rand = new System.Random();
		
		for (int y=0;y<height;y++)
		{
			for (int x=0;x<width;x++)
			{
				float xpos = x - halfSizeWidth;
				float ypos = y - halfSizeHeight;				
				
				int idx = y*width+x;
				float pixelHeight = 0;//heightMap.GetPixel(x, y).grayscale;
				Vector3 vertex = new Vector3 (xpos, ypos, pixelHeight);
				vertices[idx] = Vector3.Scale(vertex, screenAspectScale);
				uv[idx] = new Vector2 (x, y);
				colors[idx] = new Color32(0, 255, 0, 255);
				
				float randomX = (float)rand.NextDouble() * particleGridOffsets;
				float randomY = (float)rand.NextDouble() * particleGridOffsets;
				
				particles[idx].position = new Vector3(vertices[idx].x+randomX, vertices[idx].y+randomY, vertices[idx].z-10);
				particles[idx].color = new Color32(173, 194, 72, 32);
				particles[idx].lifetime = 10000.0f;
				particles[idx].startLifetime = 0.0f;				
			}
		}
		
		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uv;
	
		//indices
		int[] triangles = new int[(height - 1) * (width - 1) * 6];
		int index = 0;
		for (int y=0;y<height-1;y++)
		{
			for (int x=0;x<width-1;x++)
			{
				// For each grid cell output two triangles
				triangles[index++] = (y     * width) + x;
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = (y     * width) + x + 1;
	
				triangles[index++] = ((y+1) * width) + x;
				triangles[index++] = ((y+1) * width) + x + 1;
				triangles[index++] = (y     * width) + x + 1;
			}
		}
		
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		//mesh.tangents = tangents;
	}
	
	public void OnMouseDown(int finger, Vector2 pos)
	{
		solver.OnMouseDown(finger, pos);
	}
	
	public void OnMouseUp(int finger, Vector2 pos)
	{
		solver.OnMouseUp(finger, pos);
	}
	
	void Update()
	{
		Vector3 position = GetComponent<Transform>().position;
		Vector3 scale = GetComponent<Transform>().localScale;
		
		float dt = 1.0f / fluidFPS;
		
		solver.Update(position, scale, Camera.main, viscocity, density, mousePower, mouseVectorPower, dt);
		solver.Render(colors, vertices, width, height, Camera.main, particles, dt);
		
		ps.SetParticles(particles, particles.Length);
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.colors32 = colors;
		mesh.vertices = vertices;
		//mesh.RecalculateNormals();
	}
	
	
	public class QuickFluidSolver
	{
        private int MouseFingerDown = -1;
		public Vector2 LastMousePos = new Vector2(0,0);
		public Vector2 mouseDir = new Vector2(0,0);
		private Vector3 previousScreenPos = new Vector3(0,0,0);
		private Vector2 screenAspectScale;
		
		private float initialParticleGridSize = 1.0f;
		
        private float[,] u;
        private float[,] u0;
        private float[,] v;
        private float[,] v0;
        private float[,] densityField;
        private float[,] prevDensityField;

        private int N = 0;
		private int N1 = 0;
		
		int KCount = 0;
				
        public QuickFluidSolver(int n, int k, Vector2 aspectRatio, float particleGridSize)
        {
			N = n;
			KCount = k;
			initialParticleGridSize = particleGridSize;
            InitFields(aspectRatio);
        }

        private void InitFields(Vector2 aspect)
        {
			screenAspectScale = aspect;
            
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
				
		public void OnMouseDown(int finger, Vector2 pos)
		{
			if(MouseFingerDown != finger)
			{
				LastMousePos.x = pos.x;
				LastMousePos.y = pos.y;
				
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(LastMousePos.x, LastMousePos.y, 0));
				Vector3 screenPos = Camera.main.WorldToViewportPoint(worldPos);
				previousScreenPos = screenPos;	
			}
			
			MouseFingerDown = finger;
			mouseDir.x = pos.x - LastMousePos.x;
			mouseDir.y = pos.y - LastMousePos.y;
			
			LastMousePos.x = pos.x;
			LastMousePos.y = pos.y;
		}
		
		public void OnMouseUp(int finger, Vector2 pos)
		{
			MouseFingerDown = -1;
			LastMousePos.x = pos.x;
			LastMousePos.y = pos.y;
		}
		
       	public void Render(Color32[] colorfield, Vector3[] vertices, int width, int height, Camera camcam, UnityEngine.ParticleSystem.Particle[] particles, float dt)
        {
            float sw = width-1;
            float sh = height-1;
			
			for (int y=0;y<height; ++y)
			{				
				for (int x=0;x<width; ++x)
				{				
					float xperc = (float)x / sw;
					float yperc = (float)y / sh;
					
					float fxcord = xperc * N;
					float fycord = yperc * N;					
					
					float uval = Math.Min (1, Math.Abs (SampleField(u, fxcord, fycord))*1);
                    float vval = Math.Min (1, Math.Abs (SampleField(v, fxcord, fycord))*1);
					float dval = SampleField(densityField, fxcord, fycord) * 0.2f;
					
					byte horizontalVelocity = (byte)(255*(uval+vval));
					byte verticalVelocity = (byte)(255*vval);
					byte densityColor = (byte)(255*dval);
					
					int colorIdx = y*width+x;
					
					particles[colorIdx].color = new Color32(72, 194, 178, densityColor);
					particles[colorIdx].size = 16;
					//particles[colorIdx].lifetime += dt;
					//particles[colorIdx].velocity = new Vector3(uval*100,-vval*100,0);
					//particles[colorIdx].position += particles[colorIdx].velocity*dt;					
					
					colorfield[colorIdx] = new Color32(horizontalVelocity, 0, 0, 255);
					//vertices[colorIdx].z = densityColor*0.1f;
				}
			}
		}
		 
        public void Update(Vector3 position, Vector3 scale, Camera camcam, float visc, float diffus, float mousePower, float mouseVectorPower, float dt)
        {			
			Vector3 worldPos = camcam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
			Vector3 screenPos = camcam.WorldToViewportPoint(worldPos);
			
			float mouseChangeX = previousScreenPos.x-screenPos.x;
			float mouseChangeY = previousScreenPos.y-screenPos.y;
			previousScreenPos = screenPos;
			
			int mouseIterations = 10;
			for(int i = 0; i < mouseIterations; ++i)
			{
				float p = (float)i / (float)(mouseIterations-1);
				
				float curScreenPosx = previousScreenPos.x + (mouseChangeX*p);
				float curScreenPosy = previousScreenPos.y + (mouseChangeY*p);
				
				int xCell =  (int)(curScreenPosx * N);
				int yCell =  (int)(curScreenPosy * N);
				
	            if (MouseFingerDown == 0)
				{
					u0[xCell, yCell] += mouseDir.x * mouseVectorPower * dt;
					v0[xCell, yCell] += mouseDir.y * mouseVectorPower * dt;
					
	                //int mouseRadius = 10;
					//float velPower = 50.0f;
					///float goalVal = 0.0f;
					//UpdateBlackHole(curScreenPosx, curScreenPosy, mouseRadius, velPower, mouseVectorPower, goalVal, dt, false);
	            }
				else if(MouseFingerDown == 1)
				{				
					//creates the mouse power!
					int mouseRadius = 20;
					float velPower = 0.0f;
					float goalVal = 1.0f;
					UpdateBlackHole(curScreenPosx, curScreenPosy, mouseRadius, velPower, mousePower, goalVal, dt, true);
				}
			}
			
			//ink-hole!
			{
				 
				float inkHoleX = 0.5f;
				float inkHoleY = 0.5f;		//right in the middle!
				int blackHoleRadius = 40;
				float velocityPower = 900.0f;
				float holePower = 1000.0f;
				float goalValue = 0.0f;
				UpdateBlackHole(inkHoleX, inkHoleY, blackHoleRadius, velocityPower, holePower, goalValue, dt, true);
			}
			
			float viscosity = 0.000001f*visc;
			float diff = 0.000001f*diffus;
			
            VelocityStep(viscosity, dt);
            DensityStep(diff, dt);
        }
		
		private void UpdateBlackHole(float x, float y, int radius, float velocitypower, float holePower, float goalValue, float dt, bool affectDensity)
		{			
			float centerXCell =  x * N;
	        float centerYCell =  y * N;
			
			float invAspectX = (1.0f/screenAspectScale.x);
			float invAspectY = (1.0f/screenAspectScale.y);
			
			float hrX = radius * 0.5f*invAspectX;
			float hrY = radius * 0.5f*invAspectY;
			
			for(float fy = -hrY; fy < hrY; fy+=1)
			{
				for(float fx = -hrX; fx < hrX; fx+=1)
				{
					float fxCell = centerXCell+fx;
					float fyCell = centerYCell+fy;
					
					int xCell = (int)(fxCell);
					int yCell = (int)(fyCell);					
					
					if(xCell >= 0 && xCell < N && yCell >= 0 && yCell < N)
					{
						float xFrac = 1-(fxCell-xCell);
						float yFrac = 1-(fyCell-yCell);

						float directionX = centerXCell - xCell;
						float directionY = centerYCell - yCell;
							
						int diffx = (int)(directionX);
						int diffy = (int)(directionY);		
						
						if(diffx != 0)
						{
							directionX = 1.0f/(float)diffx;
							
						}
						if(diffy != 0)
						{
							directionY = 1.0f/(float)diffy;
							
						}						
						
						u0[xCell,yCell] += directionX*velocitypower*dt;
						v0[xCell,yCell] += directionY*velocitypower*dt;
						
						if(affectDensity)
						{
							float a = densityField[xCell, yCell];//SampleField(densityField, fxCell, fyCell);
							float b = goalValue;
							float difference = b-a;
							float change = difference * (holePower*dt*dt);				
							densityField[xCell, yCell] = goalValue;
						}
						
						//DebugStreamer.message = "field density: [" + xCell.ToString() + ", " + yCell.ToString() + "] : " + this.densityField[xCell, yCell].ToString();
					}
				}
			}
		}
		
        private void DensityStep(float diff, float dt)
        {
            AddDensitySource(0.1f, dt);
            Diffuse(0, prevDensityField, densityField, diff, dt);
            AdvectDensity(0, dt);
        }
		
		float SampleField(float[,] field, float xcoord, float ycoord)
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
		
        private void VelocityStep(float visc, float dt)
        {
            AddSource(u, u0, dt);
            AddSource(v, v0, dt);

            Diffuse(1, u0, u, visc, dt);
            Diffuse(2, v0, v, visc, dt);
            
            Project(u0, v0, u, v);

            Advect(1, u, u0, u0, v0, dt);
            Advect(2, v, v0, u0, v0, dt);
            Project(u, v, u0, v0);
        }

        private void Diffuse(int b, float[,]x, float[,] x0, float diff, float dt)
        {
            int i, j, k;
            float a=dt*diff*N*N;
            float divisor = (1.0f + 4.0f * a);

            for (k = 0; k < KCount; k++)
            {
                for (i = 1; i <= N; i++)
                {
                    for (j = 1; j <= N; j++)
                    {
                        x[i, j] = (x0[i, j] + a * (x[i - 1, j] + x[i + 1, j] + x[i, j - 1] + x[i, j + 1])) / divisor;
                    }
                }
            }
            
            SetBoundary(b, x);            
        }

        private void Advect(int b, float[,] d, float[,] d0, float[,] inu, float[,] inv, float dt)
        {
            int i, j, j0, i1, j1;
            float x, y, s0, t0, s1, t1;

            float dt0 = dt*N;

            for ( i=1 ; i<=N ; ++i )
            {
                for ( j=1 ; j<=N ; ++j )
                {
                    x = i - dt0 * inu[i, j];
                    y = j - dt0 * inv[i, j];

                    if (x<0.5) 
                        x=0.5f; 
                    if (x>N+0.5) 
                        x=N+ 0.5f;                    
                    
                    if (y<0.5) 
                        y=0.5f; 
                    if (y>N+0.5) 
                        y=N+ 0.5f;

                    int i0 = (int)x;
                    i1 = i0 + 1;

                    j0=(int)y; 
                    j1=j0+1;
                    s1 = x-i0; 
                    s0 = 1-s1; 
                    t1 = y-j0; 
                    t0 = 1-t1;

                    d[i, j] = s0 * (t0 * d0[i, j] + t1 * d0[i0, j1]) + s1 * (t0 * d0[i1, j0] + t1 * d0[i1, j1]);
                }
            }

            SetBoundary(b, d);
        }
		
		private void AdvectDensity(int b, float dt)
        {
            int i, j, j0, i1, j1;
            float x, y, s0, t0, s1, t1;

            float dt0 = dt*N;

            for (i=1 ; i<=N ; ++i )
            {
                for ( j=1 ; j<=N ; ++j )
                {
                    x = i - dt0 * u[i, j];
                    y = j - dt0 * v[i, j];

                    if (x<0.5) 
                        x=0.5f; 
                    if (x>N+0.5) 
                        x=N+ 0.5f;                    
                    
                    if (y<0.5) 
                        y=0.5f; 
                    if (y>N+0.5) 
                        y=N+ 0.5f;

                    int i0 = (int)x;
                    i1 = i0 + 1;

                    j0=(int)y; 
                    j1=j0+1;
                    s1 = x-i0; 
                    s0 = 1-s1; 
                    t1 = y-j0; 
                    t0 = 1-t1;

                    float f = s0 * (t0 * prevDensityField[i, j] + t1 * prevDensityField[i0, j1]) + s1 * (t0 * prevDensityField[i1, j0] + t1 * prevDensityField[i1, j1]);
					if(f>1)
						f=1;
					else if(f<0)
						f=0;
					densityField[i, j] = f;
                }
            }

            SetBoundary(b, densityField);
        }

        private void Project(float[,] inu, float[,] inv, float[,] p, float[,] div)
        {
            int i, j, k;
            float h = 1.0f / N;
            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    div[i, j] = -0.5f * h * (inu[i + 1, j] - inu[i - 1, j] + inv[i, j + 1] - inv[i, j - 1]);
                    p[i, j] = 0;
                }
            }

            SetBoundary(0, div);
            SetBoundary(0, p);

            for (k = 0; k < 20; k++)
            {
                for (i = 1; i <= N; i++)
                {
                    for (j = 1; j <= N; j++)
                    {
                        p[i, j] = (div[i, j] + p[i - 1, j] + p[i + 1, j] + p[i, j - 1] + p[i, j + 1]) / 4.0f;
                    }
                }
                SetBoundary(0, p);
            }

            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    inu[i, j] -= 0.5f * N * (p[i + 1, j] - p[i - 1, j]);
                    inv[i, j] -= 0.5f * N * (p[i, j + 1] - p[i, j - 1]);
                }
            }

            SetBoundary(1, inu);
            SetBoundary(2, inv);
        }

        private void AddSource(float[,] x, float[,] s, float dt)
        {
            int size = (N + 2) * (N + 2);
            for(int i = 0; i < N1; ++i)
			{
				for(int j = 0; j < N1; ++j)
				{
					x[i,j] += s[i,j] * dt;
				}
			}
        }
		
		private void AddVelocitySources(float dt)
        {	
			for(int i = 0; i < N1; ++i)
			{
				for(int j = 0; j < N1; ++j)
				{
					u[i,j] += u0[i,j] * dt;
					v[i,j] += v0[i,j] * dt;
				}
			}
        }
		
		private void AddDensitySource(float removalRate, float dt)
        {	
			float mult = 1.0f - removalRate;
			for(int i = 0; i < N1; ++i)
			{
				for(int j = 0; j < N1; ++j)
				{
					densityField[i,j] += prevDensityField[i,j] * dt;
					densityField[i,j] *= mult;					
				}
			}
        }
		
        private void SetBoundary(int b, float[,] x)
        {            
            for (int i=1 ; i<=N ; i++ )
            {
                x[0  ,i] = b==1 ? -x[1,i] : x[1,i];
                x[N+1,i] = b==1 ? -x[N,i] : x[N,i];
                x[i,0 ] = b==2 ? -x[i,1] : x[i,1];x[i,N+1] = b==2 ? -x[i,N] : x[i,N];
            }

            x[0  ,0 ] = 0.5f*(x[1,0  ]+x[0  ,1]);
            x[0  ,N+1] = 0.5f*(x[1,N+1]+x[0  ,N]);
            x[N+1,0] = 0.5f*(x[N,0  ]+x[N+1,1]);
            x[N+1,N+1] = 0.5f*(x[N,N+1]+x[N+1,N ]);
        }
    }
}