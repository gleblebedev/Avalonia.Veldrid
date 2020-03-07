Texture2D<float4> _12 : register(t0);
SamplerState _16 : register(s0);

static float4 _9;
static float2 _22;

struct SPIRV_Cross_Input
{
    float2 _22 : TEXCOORD0;
};

struct SPIRV_Cross_Output
{
    float4 _9 : SV_Target0;
};

void frag_main()
{
    _9 = _12.Sample(_16, _22);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    _22 = stage_input._22;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output._9 = _9;
    return stage_output;
}
