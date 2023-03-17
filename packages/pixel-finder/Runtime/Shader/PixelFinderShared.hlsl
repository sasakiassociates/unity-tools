
static const uint MAX_VALUE = 16384;
static const uint TEXTURE_SIZE = 512;
static const float PIXEL_FRACTION = 1 / (float)TEXTURE_SIZE;

bool colorMatch(float4 c, float4 d)
{
    return abs(c.r - d.r) < 0.01 && abs(c.g - d.g) < 0.01 && abs(c.b - d.b) < 0.01;
}


float calcFrac(const float pos, const float r)
{
    const float theta = atan2(pos + PIXEL_FRACTION, r) - atan2(pos, r);
    return theta * r / PIXEL_FRACTION;
}
