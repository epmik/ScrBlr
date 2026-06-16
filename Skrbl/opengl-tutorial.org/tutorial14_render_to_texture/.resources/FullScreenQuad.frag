#version 330 core

in vec2 t_uv;

out vec3 o_color;

uniform sampler2D u_source;

void main()
{
	o_color = texture(u_source, t_uv).xyz;
}