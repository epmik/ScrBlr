#version 330

out vec4 o_col;

in vec3 t_rgb;
in vec2 t_uv0;

uniform sampler2D u_tex0;

void main()
{
    o_col = mix(texture(u_tex0, t_uv0), vec4(t_rgb, 1.0), 0.5);
}