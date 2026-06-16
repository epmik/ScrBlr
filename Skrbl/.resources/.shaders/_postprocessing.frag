#version 330

in vec2 t_uv0;

out vec4 o_col;

void main()
{
    float r = t_uv0.x;

    o_col = vec4(r, r, r, 1.0);

    //o_col = vec4(250.0 / 255.0, 87.0 / 255.0, 5.0 / 255.0, 1.0);
}