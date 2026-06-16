
#include <hash-chunk.frag>

float random(vec2 uv)
{
    uint bit = uint(8.0 * uv.x) + 8u * uint(4.0 * uv.y);

    vec2 seed = fragCoord;
    
    uvec3 hash = hash(seed);
    
    return float(hash) * (1.0/float(0xffffffffu));
}