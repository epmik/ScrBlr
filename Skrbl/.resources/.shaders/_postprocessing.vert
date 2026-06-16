#version 330 core

layout(location = 0) in vec3 i_pos;
layout(location = 1) in vec2 i_uv0;

out vec4 t_xyzw;
out vec2 t_uv0;

uniform mat4 u_model;
uniform mat4 u_view;
uniform mat4 u_projection;

void main(void)
{
    t_xyzw = vec4(i_pos, 1.0) * u_model * u_view;
    t_uv0 = i_uv0;
    gl_Position = vec4(i_pos, 1.0) * u_model * u_view * u_projection;
}