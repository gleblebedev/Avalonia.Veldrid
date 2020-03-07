#version 330

const vec4 _42[4] = vec4[](vec4(-1.0, 1.0, 0.0, 0.0), vec4(1.0, 1.0, 1.0, 0.0), vec4(-1.0, -1.0, 0.0, 1.0), vec4(1.0, -1.0, 1.0, 1.0));

layout(std140) uniform WindowUniforms
{
    mat4 Model;
    mat4 View;
    mat4 Projection;
    vec2 Viewport;
} _21;

out vec2 vdspv_fsin0;

void main()
{
    gl_Position = ((_21.Projection * _21.View) * _21.Model) * vec4(_42[gl_VertexID].xy, 0.0, 1.0);
    vdspv_fsin0 = _42[gl_VertexID].zw * _21.Viewport;
}

