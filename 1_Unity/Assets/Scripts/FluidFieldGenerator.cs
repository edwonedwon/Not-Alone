using UnityEngine;
using System.Collections;

public class FluidFieldGenerator : MonoBehaviour
{
	public Texture2D heightMap;
	public Material material;	
	
	public int N = 64;
	public int KKount = 3;
	public float fluidFPS = 10.0f;
	
	int width = 0;
	int height = 0;
	float screenWidth = 0;
	float screenHeight = 0;
	int arraySize = 0;
	
	private QuickFluidSolver solver;
	Color32[] colors = null;
	Vector3[] vertices = null;
	
	void Start()
	{
		GenerateHeightmap();
		solver = new QuickFluidSolver(N, KKount);
		solver.InitFields();
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
		
		screenHeight = (2*Camera.main.orthographicSize);
  		screenWidth = (screenHeight*Camera.main.aspect);
		
		width = 255;//.Math.Min(heightMap.width, 255);
		height = 255;//System.Math.Min(heightMap.height, 255);
		arraySize = height*width;
		
		Vector2 screenAspectScale = new Vector2(screenWidth/(float)width, screenHeight/(float)height);
		
		
		vertices = new Vector3[arraySize];
		colors = new Color32[arraySize];
		Vector2[] uv = new Vector2[arraySize];
		Vector4[] tangents = new Vector4[arraySize];
		
		Vector2 uvScale = new Vector2 (1.0f / (width - 1), 1.0f / (height - 1));
		
		float halfSizeWidth = width * 0.5f;
		float halfSizeHeight = height * 0.5f;
		
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
		solver.Update(position, scale, width, screenWidth, screenHeight, height, dt);		
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		solver.Render(colors, vertices, width, height);
		
		//for (int y=0;y<height;y++)
		{
			//for (int x=0;x<width;x++)
			{				
				//int idx = y*width+x;
				//float differenceX = x - Input.mousePosition.x;
				//float differenceY = y - Input.mousePosition.y;
				//float pixelHeight = Mathf.Cos(differenceX * 0.1f);
				//vertices[idx] = new Vector3 (x, pixelHeight, y);
				//colors[idx] = new Color32(255, 255, 0, 255);
				//mesh.vertices[y*width + x] = Vector3.Scale(sizeScale, vertex);
			}
		}
				
		//mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.vertices = vertices;
		//mesh.RecalculateNormals();
	}
	
	
	public class QuickFluidSolver
	{
        private bool MouseIsDown = false;
		public Vector2 LastMousePos = new Vector2(0,0);
		public Vector2 mouseDir = new Vector2(0,0);
		
        float[] u;
        float[] u0;
        float[] v;
        float[] v0;

        float[] densityField;
        float[] prevDensityField;

        int N = 0;
		int KCount = 0;

        public QuickFluidSolver(int n, int k)
        {
			N = n;
			KCount = k;
            InitFields();
        }

        public void InitFields()
        {
            int size = (N + 2) * (N + 2);

            u = new float[size];
            u0 = new float[size];
            v = new float[size];
            v0 = new float[size];			

            densityField = new float[size];
            prevDensityField = new float[size];

            for (int i = 0; i < size; ++i)
            {
                densityField[i] = prevDensityField[i] = 0.0f;
                u[i] = u0[i] = 0.0f;
                v[i] = v0[i] = 0.0f;
            }
        }
				
		public void OnMouseDown(int finger, Vector2 pos)
		{
			MouseIsDown = true;
			mouseDir.x = pos.x - LastMousePos.x;
			mouseDir.y = pos.y - LastMousePos.y;
			
			LastMousePos.x = pos.x;
			LastMousePos.y = pos.y;
		}
		
		public void OnMouseUp(int finger, Vector2 pos)
		{
			MouseIsDown = false;
			LastMousePos.x = pos.x;
			LastMousePos.y = pos.y;
		}
		
        public void Render(Color32[] colorfield, Vector3[] vertices, int width, int height)
        {
            float sceneWidth = width;
            float sceneHeight = height;			
			
			for (int y=0;y<height; ++y)
			{
				float yperc = (float)y / sceneHeight;
				
				for (int x=0;x<width; ++x)
				{
					float xperc = (float)x / sceneWidth;
					
					float fxcord = xperc * N;
					float fycord = yperc * N;
					
					int xcoord = (int)(fxcord);
                    int ycoord = (int)(fycord);
					
					float xfrac = fxcord;
					float yfrac = fycord;

                    int idx0 = IX(xcoord, ycoord);
					
					float uval = u[idx0] * 10;
                    float vval = v[idx0] * 10;
					
					float d = System.Math.Abs(densityField[idx0]) * 0.1f;
					float r = System.Math.Abs(uval) * 0.1f;
                    float b = System.Math.Abs(vval) * 0.1f;
					
                    float alpha = (d + r + b) * 2;
					float red = (d+r)*1;
					float blue = (d+r) * 1;
					
					if (alpha > 1)
						alpha = 1;
					
					int colorIdx = y*width+x;
					byte colorIntensity = (byte)(alpha*255);
					colorfield[colorIdx] = new Color32(colorIntensity, colorIntensity, colorIntensity, 0);
					vertices[colorIdx].z = 0;//colorIntensity*0.8f;
				}
			}	
		}
		 
        public void Update(Vector3 position, Vector3 scale, float width, float height, float screenwidth, float screenheight, float dt)
        {
			float resDiffX = (LastMousePos.x / screenwidth);
			float resDiffY = (LastMousePos.y / screenheight);
			
            if (MouseIsDown)
            {
				//DebugStreamer.message = "pos.x = " + LastMousePos.x.ToString() + ", " + LastMousePos.y.ToString();
				
                int xCell =  (int)(resDiffX * N);
                int yCell =  (int)(resDiffY * N);
				
				//DebugStreamer.message = "cell index: " + xCell.ToString() + ", " + yCell.ToString();
				
				if(xCell<0)
					xCell=0;
				if(xCell>=N)
					xCell=N-1;
				if(yCell<0)
					yCell=0;
				if(yCell>=N)
					yCell=N-1;
				
                int idx = IX(xCell, yCell);
                u0[idx] += mouseDir.x * 100 * dt;
				v0[idx] += mouseDir.y * 100 * dt;
            }
			
			float viscosity = 0.00001f;
			float diff = 0.00001f;
            VelocityStep(viscosity, dt);
            DensityStep(diff, dt);
        }

        private void DensityStep(float diff, float dt)
        {
            AddSource(densityField, prevDensityField, dt);
            //SwapD();
            Diffuse(0, prevDensityField, densityField, diff, dt);
            //SwapD();
            Advect(0, densityField, prevDensityField, u, v, dt);
        }

        private void VelocityStep(float visc, float dt)
        {
            AddSource(u, u0, dt);
            AddSource(v, v0, dt);

            //SwapU();
            //SwapV();

            Diffuse(1, u0, u, visc, dt);
            Diffuse(2, v0, v, visc, dt);
           // Diffuse(1, u0, u, visc, dt);
            //Diffuse(2, v0, v, visc, dt);
            
            Project(u0, v0, u, v);
            //Project(u, v, u0, v0);
           // SwapU();
            //SwapV();

            Advect(1, u, u0, u0, v0, dt);
            Advect(2, v, v0, u0, v0, dt);
            Project(u, v, u0, v0);
        }

        private void Diffuse(int b, float[]x, float[] x0, float diff, float dt)
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
                        int idx = IX(i, j);
                        x[idx] = (x0[idx] + a * (x[IX(i - 1, j)] + x[IX(i + 1, j)] + x[IX(i, j - 1)] + x[IX(i, j + 1)])) / divisor;
                    }
                }
            }
            
            SetBoundary(b, x);            
        }

        private void Advect(int b, float[] d, float[] d0, float[] inu, float[] inv, float dt)
        {
            int i, j, j0, i1, j1;
            float x, y, s0, t0, s1, t1;

            float dt0 = dt*N;

            for ( i=1 ; i<=N ; i++ )
            {
                for ( j=1 ; j<=N ; j++ )
                {
                    int idx = IX(i, j);

                    x = i - dt0 * inu[idx];
                    y = j - dt0 * inv[idx];

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

                    int idxj1 = IX(i0, j1);
                    int idxj0 = IX(i1, j0);
                    int idx11 = IX(i1, j1);

                    d[idx] = s0 * (t0 * d0[idx] + t1 * d0[idxj1]) + s1 * (t0 * d0[idxj0] + t1 * d0[idx11]);
                }
            }

            SetBoundary(b, d);
        }

        private void Project(float[] inu, float[] inv, float[] p, float[] div)
        {
            int i, j, k;
            float h = 1.0f / N;
            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    int idx = IX(i, j);
                    div[idx] = -0.5f * h * (inu[IX(i + 1, j)] - inu[IX(i - 1, j)] + inv[IX(i, j + 1)] - inv[IX(i, j - 1)]);
                    p[idx] = 0;
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
                        int idx = IX(i, j);
                        p[idx] = (div[IX(i, j)] + p[IX(i - 1, j)] + p[IX(i + 1, j)] + p[IX(i, j - 1)] + p[IX(i, j + 1)]) / 4.0f;
                    }
                }
                SetBoundary(0, p);
            }

            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    inu[IX(i, j)] -= 0.5f * N * (p[IX(i + 1, j)] - p[IX(i - 1, j)]);
                    inv[IX(i, j)] -= 0.5f * N * (p[IX(i, j + 1)] - p[IX(i, j - 1)]);
                }
            }

            SetBoundary(1, inu);
            SetBoundary(2, inv);
        }

        private int IX(int i, int j)
        {
            return ((i) + (N + 2) * (j));
        }

        private void SwapU()
        {
            Swap(u0, u);
            //Swap(u, u0);
        }
        
		private void SwapV()
        {
            Swap(v0, v);
            //Swap(v, v0);
        }
        
		private void SwapD()
        {
            Swap(prevDensityField, densityField);
            //Swap(densityField, prevDensityField);
        }

        private void Swap(float[] x0, float[] x)
        {
            //float[] temp = x0;
            //int l = x0.Length;
            //float[] temp = new float[l];
            //for (int i = 0; i < l; ++i)
            //   temp[i] = x0[i];
			
			
            //for (int i = 0; i < l; ++i)
            //{
            //    x0[i] = x[i];
             //   x[i] = temp[i];
            //}            
        }

        private void AddSource(float[] x, float[] s, float dt)
        {
            int size = (N + 2) * (N + 2);
            for (int i = 0; i < size; ++i)
                x[i] += s[i] * dt;
        }

        private void SetBoundary(int b, float[] x)
        {            
            for (int i=1 ; i<=N ; i++ )
            {
                x[IX(0  ,i)] = b==1 ? -x[IX(1,i)] : x[IX(1,i)];
                x[IX(N+1,i)] = b==1 ? -x[IX(N,i)] : x[IX(N,i)];
                x[IX(i,0  )] = b==2 ? -x[IX(i,1)] : x[IX(i,1)];x[IX(i,N+1)] = b==2 ? -x[IX(i,N)] : x[IX(i,N)];
            }

            x[IX(0  ,0  )] = 0.5f*(x[IX(1,0  )]+x[IX(0  ,1)]);
            x[IX(0  ,N+1)] = 0.5f*(x[IX(1,N+1)]+x[IX(0  ,N )]);
            x[IX(N+1,0  )] = 0.5f*(x[IX(N,0  )]+x[IX(N+1,1)]);
            x[IX(N+1,N+1)] = 0.5f*(x[IX(N,N+1)]+x[IX(N+1,N )]);
        }
    }
}