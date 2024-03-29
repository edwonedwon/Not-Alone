// unlit, vertex colour, alpha blended
// cull off

Shader "tk2d/CutoutVertexColor" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags {"LightMode" = "Vertex" "IgnoreProjector"="True" }
		LOD 100

		AlphaTest Greater 0
		Blend Off		
		Cull Off

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}
