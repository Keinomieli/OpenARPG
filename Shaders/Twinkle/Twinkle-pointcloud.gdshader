shader_type spatial;
render_mode unshaded, depth_test_disabled, blend_add;

const int maxPointsCount = 100;
uniform vec4 worldPoints[maxPointsCount];

uniform float intensity = 20;
uniform float minDist : hint_range(0, 10) = 2;
uniform float maxDist : hint_range(0, 10) = 5;
uniform float distFalloff : hint_range(0.01,6.0) = 2.0;
uniform vec3 color : source_color = vec3(1,1,1);

uniform float normalScale : hint_range(0.01, 6) = 2.0;
uniform float normalNegationRangeMin : hint_range(0, 10) = 0;
uniform float normalNegationRangeMax : hint_range(0, 10) = 0;

#include "res://Shaders/Twinkle/TwinkleLib.gdshaderinc"

void vertex() {
}

uniform sampler2D depth_texture : source_color, hint_depth_texture;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D normal_texture : hint_normal_roughness_texture, filter_linear, repeat_disable;

void fragment() {
	vec3 pixel_world_position;
	float linear_depth = linearDepth(depth_texture, SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX, pixel_world_position);
	
	vec3 pixel_normal = texture(normal_texture, SCREEN_UV).xyz * 2.0 - 1.0;
	vec4 scene_color = texture(screen_texture, SCREEN_UV);

	float light = 0.0;
	
	for(int i = 0; i < maxPointsCount; i++) {
		vec4 worldPoint = worldPoints[i];
	  	vec3 dif = worldPoint.xyz - pixel_world_position;
		float dis = max(length(dif), 0);
		vec3 dir = normalize(dif);
		
		float lambert;
		float falloff = normalDistanceFalloff(INV_VIEW_MATRIX, pixel_normal, dir, dis, normalScale, normalNegationRangeMin, normalNegationRangeMax, minDist, maxDist, distFalloff, lambert);

		light += falloff * intensity * lambert * worldPoint.w;		
	}

	ALBEDO = scene_color.rgb * color * light;
	ALPHA = scene_color.a;
}