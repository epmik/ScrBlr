#version 330 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec2 i_uv;

out vec2 t_uv;
out vec3 t_position;

void main()
{
	t_uv = i_uv;
	t_position = i_position;
	gl_Position =  vec4(i_position, 1);
}
