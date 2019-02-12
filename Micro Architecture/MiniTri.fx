float2 viewPos;
float2 viewScale;


struct VS_IN
{
	float4 pos : POSITION;
	uint input : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = input.pos;
	output.pos.xy += viewPos;
	output.pos.xy *= viewScale;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return 0.8f;
}

technique11 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}
}