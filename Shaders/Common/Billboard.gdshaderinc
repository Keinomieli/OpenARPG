mat4 sphericalBillboard(mat4 viewMatrix, mat4 invViewMatrix, mat4 modelMatrix) {
    return viewMatrix * mat4(
        invViewMatrix[0],
        invViewMatrix[1],
        invViewMatrix[2],
        modelMatrix[3]);
}

mat4 cylindricalBillboard(mat4 viewMatrix, mat4 invViewMatrix, mat4 modelMatrix) {
	vec3 local_up = modelMatrix[1].xyz;
	vec4 ax = vec4(normalize(cross(local_up, invViewMatrix[2].xyz)), 0.0);
	vec4 ay = vec4(local_up.xyz, 0.0);
	vec4 az = vec4(normalize(cross(invViewMatrix[0].xyz, local_up)), 0.0);
	return viewMatrix * mat4(ax, ay, az, modelMatrix[3]);
}