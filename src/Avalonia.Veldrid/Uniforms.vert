#version 450

layout (set=0, binding=0) uniform WindowUniforms
{
    mat4 Model;
    mat4 View;
    mat4 Projection;
    vec2 Viewport;
};

void main()
{
    gl_Position = Projection*View*vec4(0,0,1,1);
}