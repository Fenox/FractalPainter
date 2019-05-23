float4x4 World;
float4x4 View;
float4x4 Projection;

uniform float2 c = float2(-.22f, .75f);

uniform int maxIterations = 100;
uniform float step = 3 / 1000.0f;

uniform float thresh2 = 4;

uniform float minX = -1.5f;
uniform float maxX = 1.5f;
uniform float minY = -1.5f;
uniform float maxY = 1.5f;

float4 getColor(int Iterations)
{
	float val = step * Iterations;				
	float4 newColor;	

	val*=10;
	val%=3;
	val *= 2;
	float fade = val % 1;
			
	if(val < 1)
		newColor = float4(1,fade,0,1);
	else if(val < 2)
		newColor = float4(1-fade,1,0,1);
	else if(val < 3)
		newColor = float4(0,1,fade,1);
	else if(val < 4)
		newColor = float4(0,1-fade,1,1);
	else if(val < 5)
		newColor = float4(fade,0,1,1);
	else 
		newColor = float4(1,0,1-fade,1);

	return newColor;		
}

float4 getMandelColor(float2 Position)
{	
	float c_re = Position.x, c_im = Position.y;
	float Z_re = Position.x + c.x, Z_im = Position.y + c.y;

	for(int n=0; n<maxIterations; ++n)
	{
		float Z_re2 = Z_re*Z_re, Z_im2 = Z_im*Z_im; 
		if(Z_re2 + Z_im2 > thresh2)
		{
			return getColor(n);
		}
		Z_im = 2*Z_re*Z_im + c_im;
		Z_re = Z_re2 - Z_im2 + c_re;
	}
	return float4(1,1,1,1);
}

float4 getJuliaColor(float2 z)
{		
	float ding;	
	for(int i = 0; i < maxIterations; i++)
	{
		if(z.x * z.x + z.y * z.y > thresh2)
		{			
			return getColor(i);			
		}
		ding = z.x;
		z.x = z.x * z.x - z.y * z.y;
		z.y *= 2 * ding;
		z += c;
	}
	return float4(1,1,1,1);
} 

struct VertexShaderIO
{
    float4 Position : POSITION0;
    float4 Tex : TEXCOORD0;
};

VertexShaderIO VertexShaderFunction(VertexShaderIO input)
{
    VertexShaderIO output;
	
	output.Tex = input.Tex;
    //float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);

    float4 viewPosition = mul(input.Position, View);
    output.Position = mul(viewPosition, Projection);
    return output;
}

float4 PixelShaderFunctionJulia(float2 Position : TEXCOORD0) : COLOR0
{	
	return getJuliaColor(Position);
}

float4 PixelShaderFunctionMandel(float2 Position : TEXCOORD0) : COLOR0
{
	return getMandelColor(Position);
}

technique JuliaFraktal
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionJulia();
    }

	pass Pass2
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionMandel();
	}
}


