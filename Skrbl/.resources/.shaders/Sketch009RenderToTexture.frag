#version 330

out vec4 o_col;

in vec2 t_uv0;

uniform sampler2D u_tex0;

void main()
{
    o_col = texture(u_tex0, t_uv0);
}