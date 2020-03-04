static const float4 _25[4] = { float4(-1.0f, 1.0f, 0.0f, 0.0f), float4(1.0f, 1.0f, 1.0f, 0.0f), float4(-1.0f, -1.0f, 0.0f, 1.0f), float4(1.0f, -1.0f, 1.0f, 1.0f) };

static float4 gl_Position;
static int gl_VertexIndex;
static float2 _42;

struct SPIRV_Cross_Input
{
    uint gl_VertexIndex : SV_VertexID;
};

struct SPIRV_Cross_Output
{
    float2 _42 : TEXCOORD0;
    float4 gl_Position : SV_Position;
};

void vert_main()
{
    gl_Position = float4(_25[gl_VertexIndex].xy, 0.0f, 1.0f);
    _42 = _25[gl_VertexIndex].zw;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    gl_VertexIndex = int(stage_input.gl_VertexIndex);
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output._42 = _42;
    return stage_output;
}
