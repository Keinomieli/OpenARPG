shader_type spatial;
render_mode unshaded, depth_test_disabled, blend_add;

varying vec3 world_position;

uniform float intensity : hint_range(0, 20) = 5;
uniform float minDist : hint_range(0, 10) = 2;
uniform float maxDist : hint_range(0, 20) = 5;
uniform float distFalloff : hint_range(0.01,10.0) = 1.5;
uniform vec3 color : source_color = vec3(1,1,1);

uniform float normalScale : hint_range(0.01, 20) = 2.0;
uniform float normalNegationRangeMin : hint_range(0, 10) = .1;
uniform float normalNegationRangeMax : hint_range(0, 10) = .5;

#include "res://Shaders/Twinkle/TwinkleLib.gdshaderinc"

void vertex() {
	world_position = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
}

uniform sampler2D depth_texture : source_color, hint_depth_texture, repeat_disable;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear, repeat_disable;
uniform sampler2D normal_texture : hint_normal_roughness_texture, filter_linear, repeat_disable;

uniform sampler2D attenuation_curve : repeat_disable;
uniform sampler2D lambert_curve : repeat_disable;

void fragment() {
	vec3 pixel_world_position;
	float linear_depth = linearDepth(depth_texture, SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX, pixel_world_position);

	vec3 pixel_normal = texture(normal_texture, SCREEN_UV).xyz * 2.0 - 1.0;
	vec4 scene_color = texture(screen_texture, SCREEN_UV);
	
  	vec3 dif = world_position - pixel_world_position;
	float dis = max(length(dif), 0);
	vec3 dir = normalize(dif);

	float lambert;
	float falloff = normalDistanceFalloff(INV_VIEW_MATRIX, pixel_normal, dir, dis, normalScale, normalNegationRangeMin, normalNegationRangeMax, minDist, maxDist, distFalloff, lambert);
	float attenuation = texture(attenuation_curve, vec2(falloff, 0)).r;
	float lambert_modified = texture(lambert_curve, vec2(lambert, 0)).r;
	
	ALBEDO = scene_color.rgb * color * attenuation * intensity * lambert_modified;
	ALPHA = scene_color.a;
}