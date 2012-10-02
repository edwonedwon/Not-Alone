Shader "MyShader/non_texture_shader"
{

    Category
    {
        BindChannels
        { 
            Bind "Color", color 
            Bind "Vertex", vertex
        }
        SubShader
        {
	        Pass
	        {
	        
	        }
	    }
    }
}