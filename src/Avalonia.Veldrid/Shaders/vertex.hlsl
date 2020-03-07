static const float4 _41[4] = { float4(-1.0f, 1.0f, 0.0f, 0.0f), float4(1.0f, 1.0f, 1.0f, 0.0f), float4(-1.0f, -1.0f, 0.0f, 1.0f), float4(1.0f, -1.0f, 1.0f, 1.0f) };

cbuffer _18_20 : register(b0)
{
    row_major float4x4 _20_m0 : packoffset(c0);
    row_major float4x4 _20_m1 : packoffset(c4);
    row_major float4x4 _20_m2 : packoffset(c8);
    float2 _20_m3 : packoffset(c12);
};


static float4 gl_Position;
static int gl_VertexIndex;
static float2 _58;

struct SPIRV_Cross_Input
{
    uint gl_VertexIndex : SV_VertexID;
};

struct SPIRV_Cross_Output
{
    float2 _58 : TEXCOORD0;
    float4 gl_Position : SV_Position;
};

void vert_main()
{
    gl_Position = mul(float4(_41[gl_VertexIndex].xy, 0.0f, 1.0f), mul(_20_m0, mul(_20_m1, _20_m2)));
    _58 = _41[gl_VertexIndex].zw * _20_m3;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    gl_VertexIndex = int(stage_input.gl_VertexIndex);
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output._58 = _58;
    return stage_output;
}
