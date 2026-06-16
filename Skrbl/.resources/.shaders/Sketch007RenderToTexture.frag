#version 330

out vec4 o_col;

in vec3 t_rgb;

void main()
{
    o_col = vec4(t_rgb, 1.0);
}