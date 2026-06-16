#version 330 core

layout(location = 0) in vec3 i_pos;
layout(location = 1) in vec4 i_rgba;

uniform mat4 u_modelViewProjectionMatrix;

out vec4 t_rgba;

void main()
{
	t_rgba = i_rgba;
	gl_Position =  u_modelViewProjectionMatrix * vec4(i_pos, 1);
}
