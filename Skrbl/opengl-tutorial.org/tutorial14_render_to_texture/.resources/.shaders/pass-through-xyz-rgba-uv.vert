#version 330 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec4 i_rgba;
layout(location = 2) in vec2 i_uv;

out vec3 t_position;
out vec4 t_rgba;
out vec2 t_uv;

void main()
{
	t_position = i_position;
	t_rgba = i_rgba;
	t_uv = i_uv;
	gl_Position =  vec4(i_position, 1);
}
