#version 330

uniform sampler2D Input;

layout(location = 0) out vec4 fsout_Color0;
in vec2 vdspv_fsin0;

void main()
{
    fsout_Color0 = texture(Input, vdspv_fsin0);
}

