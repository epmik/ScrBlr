#version 330

out vec4 o_col;

in vec3 t_rgb;
in vec2 t_uv0;

uniform sampler2D u_tex0;
uniform sampler2D u_tex1;

void main()
{
    o_col = mix(vec4(t_rgb, 1.0), mix(texture(u_tex0, t_uv0), texture(u_tex1, t_uv0), 0.5), 0.5);
}