Shader "MyShader/fluid_particles"
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
	        Tags {Queue = Transparent}
		    Ztest Off
		    Zwrite Off
		    Blend SrcAlpha One
	        Pass
	        {
	        
	        }
	    }
    }
}