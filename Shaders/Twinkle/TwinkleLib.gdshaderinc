float invLerp(float a, float b, float v) {
	return (v-a)/(b-a);
}

vec3 closestPointBox(vec3 point, vec3 planePosition, vec3 planeNormal, vec3 planeRight, vec3 planeUp, float boxWidth, float boxLength) {
    float distance_to_plane = dot(planeNormal, point - planePosition);
    vec3 closestPoint = point - distance_to_plane * planeNormal;

	float rightDistance = dot(planeRight, closestPoint - planePosition);
	float upDistance = dot(planeUp, closestPoint - planePosition);

	float right = min(abs(rightDistance), boxWidth) * sign(rightDistance);
	float up = min(abs(upDistance), boxLength) * sign(upDistance);

   	return planePosition + planeRight * right + planeUp * up;
}

vec3 closestPointOnBoxSurface_WIP(vec3 point, vec3 boxCenter, vec3 boxSize, mat4 boxRotation) {
    // Transform point to box's local space
    vec3 localPoint = (boxRotation * vec4(point - boxCenter, 1.0)).xyz;

    // Initialize the closest point as the point itself
    vec3 closestPoint = localPoint;

    // Clamp the point to the box's half size in all dimensions
    closestPoint = clamp(closestPoint, -boxSize * 0.5, boxSize * 0.5);

    // Find the closest face by comparing distances in each dimension
    for (int i = 0; i < 3; ++i) {
        if (abs(closestPoint[i] + boxSize[i] * 0.5) < abs(closestPoint[i] - boxSize[i] * 0.5)) {
            closestPoint[i] = boxSize[i] * 0.5;
        } else {
            closestPoint[i] = -boxSize[i] * 0.5;
        }
    }

    // Transform the closest point back to world space
    return (boxRotation * vec4(closestPoint, 1.0)).xyz + boxCenter;
}

float spotFalloff(vec3 lightToSurface, vec3 lightDir, float min_angle, float max_angle, float falloff_power) {
	float angle = dot(lightToSurface, lightDir);
	float angle_falloff = smoothstep(cos(radians(max_angle)), cos(radians(min_angle)), angle);
	return pow(angle_falloff, falloff_power);
}

float linearDepth(sampler2D depth_texture, vec2 screen_uv, mat4 inv_proj_mat, mat4 inv_view_mat, out vec3 pixel_world_position) {
	float depth = texture(depth_texture, screen_uv).x;
	vec3 encoded_screen_pos = vec3(screen_uv * 2.0 - 1.0, depth);

	vec4 view = inv_proj_mat * vec4(encoded_screen_pos, 1.0);
	view.xyz /= view.w;

	vec4 world = inv_view_mat * inv_proj_mat * vec4(encoded_screen_pos, 1.0);
	pixel_world_position = world.xyz / world.w;

	return -view.z;
}

float normalDistanceFalloff(mat4 inv_view_matrix, vec3 tangent_normal, vec3 light_dir, float dist, float normal_scale, float normal_neg_min, float normal_neg_max, float min_dist, float max_dist, float dist_falloff, out float lambert) {
	mat3 view_to_world_normal_mat = mat3(
        inv_view_matrix[0].xyz,
        inv_view_matrix[1].xyz,
        inv_view_matrix[2].xyz
	);
	vec3 world_normal = view_to_world_normal_mat * tangent_normal;

	float normalNegation = 1.0-clamp(invLerp(normal_neg_min, normal_neg_max, dist),0.0,1.0);
	float normalStrength = pow(max(dot(world_normal, light_dir),0),normal_scale);
	lambert = mix(normalStrength, 1, normalNegation);
	float falloff = max(1.0-max(invLerp(min_dist, max_dist, dist),0.0),0.0);

	return pow(falloff, dist_falloff);
}