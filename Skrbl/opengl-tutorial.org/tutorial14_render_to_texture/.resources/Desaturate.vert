#version 330 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec2 i_uv;

out vec2 t_uv;

void main()
{
	t_uv = i_uv;
	gl_Position =  vec4(i_position, 1);
}
