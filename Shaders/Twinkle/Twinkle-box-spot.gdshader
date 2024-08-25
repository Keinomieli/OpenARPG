shader_type spatial;
render_mode unshaded, depth_test_disabled, blend_add;

varying vec3 world_position;
varying vec3 world_forward;
varying vec3 world_right;
varying vec3 world_up;

uniform float intensity = 20;
uniform float minDist : hint_range(0, 10) = 2;
uniform float maxDist : hint_range(0, 10) = 5;
uniform float distFalloff : hint_range(0.01,6.0) = 2.0;
uniform vec3 color : source_color = vec3(1,1,1);

uniform float maxWidth : hint_range(0, 10) = 2;
uniform float maxLength : hint_range(0, 10) = 2;

uniform float normalScale : hint_range(0.01, 6) = 2.0;
uniform float normalNegationRangeMin : hint_range(0, 10) = 0;
uniform float normalNegationRangeMax : hint_range(0, 10) = 0;

uniform float minAngle : hint_range(0, 180) = 0;
uniform float maxAngle : hint_range(0, 180) = 30;
uniform float anglePower : hint_range(0.01, 6.0) = 2.0;

#include "res://Shaders/Twinkle/TwinkleLib.gdshaderinc"

void vertex() {
	world_position = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
	world_right = (MODEL_MATRIX * vec4(1.0, 0.0, 0.0, 0.0)).xyz;
	world_up = (MODEL_MATRIX * vec4(0.0, 1.0, 0.0, 0.0)).xyz;
	world_forward = (MODEL_MATRIX * vec4(0.0, 0.0, -1.0, 0.0)).xyz;
}

uniform sampler2D depth_texture : source_color, hint_depth_texture;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D normal_texture : hint_normal_roughness_texture, filter_linear, repeat_disable;

void fragment() {
	vec3 pixel_world_position;
	float linear_depth = linearDepth(depth_texture, SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX, pixel_world_position);
	
	vec3 pixel_normal = texture(normal_texture, SCREEN_UV).xyz * 2.0 - 1.0;
	vec4 scene_color = texture(screen_texture, SCREEN_UV);

   	vec3 closestPoint = closestPointBox(pixel_world_position, world_position, world_forward, world_right, world_up, maxWidth, maxLength);

	vec3 dif = closestPoint - pixel_world_position;
	float dis = max(length(dif), 0);
	vec3 dir = normalize(dif);
	
	float lambert;
	float falloff = normalDistanceFalloff(INV_VIEW_MATRIX, pixel_normal, dir, dis, normalScale, normalNegationRangeMin, normalNegationRangeMax, minDist, maxDist, distFalloff, lambert);
	
	falloff *= spotFalloff(dir, world_forward, minAngle, maxAngle, anglePower);
	
	float light = falloff * intensity;
	ALBEDO = scene_color.rgb * color * light * lambert;
	ALPHA = scene_color.a;
}