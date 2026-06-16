#version 330 core

layout(location = 0) in vec3 i_pos;
layout(location = 1) in vec3 i_rgb;

out vec3 t_rgb;

uniform mat4 u_model;
uniform mat4 u_view;
uniform mat4 u_projection;

void main(void)
{
    t_rgb = i_rgb;
    gl_Position = vec4(i_pos, 1.0) * u_model * u_view * u_projection;
}