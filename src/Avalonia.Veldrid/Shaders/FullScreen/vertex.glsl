#version 330

const vec4 _26[4] = vec4[](vec4(-1.0, 1.0, 0.0, 0.0), vec4(1.0, 1.0, 1.0, 0.0), vec4(-1.0, -1.0, 0.0, 1.0), vec4(1.0, -1.0, 1.0, 1.0));

out vec2 vdspv_fsin0;

void main()
{
    gl_Position = vec4(_26[gl_VertexID].xy, 0.0, 1.0);
    vdspv_fsin0 = _26[gl_VertexID].zw;
}

