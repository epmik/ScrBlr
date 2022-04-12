#version 330 core

out vec4 oColor;

in vec3 ioColor;

void main()
{
    oColor = vec4(ioColor, 1.0);
}