#version 330 core

in vec2 UV;

out vec3 o_color;

uniform sampler2D u_source;

void main()
{
	o_color = texture(u_source, UV).xyz;
}