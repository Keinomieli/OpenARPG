float rand_1D(float n) {
	return fract(sin(n) * 58.5453123);
}

float rand_2D(vec2 n) {
	return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 58.5453);
}

float noise_1D(float p) {
	float fl = floor(p);
	float fc = fract(p);
	return mix(rand_1D(fl), rand_1D(fl + 1.0), fc);
}

float noise_2D(vec2 n, float smoothness) {
	vec2 d = vec2(0.0, smoothness);
	vec2 s = floor(n), f = smoothstep(vec2(0.0), vec2(1.0), fract(n));
	float a = mix(rand_2D(s), rand_2D(s + d.yx), f.x);
	float b = mix(rand_2D(s + d.xy), rand_2D(s + d.yy), f.x);
	return mix(a,b,f.y);
}

vec2 voronoi_noise_vector(vec2 xy, float offset)
{
    mat2 m = mat2(vec2(15.27, 27.63), vec2(14.41, 16.98));
    xy = fract(sin(xy * m) * 16.32);
    return vec2(sin(xy.y*+offset)*0.5+0.5, cos(xy.x*offset)*0.5+0.5);
}

vec2 voronoi_noise(vec2 xy, float AngleOffset, float CellDensity)
{
    vec2 g = floor(xy * CellDensity);
    vec2 f = fract(xy * CellDensity);
    float t = 8.0;
    vec3 res = vec3(8.0, 0.0, 0.0);

	vec2 result = vec2(0.0,0.0);

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            vec2 lattice = vec2(float(x),float(y));
            vec2 offset = voronoi_noise_vector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = vec3(d, offset.x, offset.y);
                result.x = res.x;
                result.y = res.y;
            }
        }
    }

	return result;
}