Shader "MyShader/dark_fluid_shader"
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
		    Blend OneMinusDstColor One
		    BlendOp RevSub
	        
	        Pass
	        {
	        	
	        }
	    }
    }
}