@tool
extends Node3D

func _process(_delta: float) -> void:
	if OS.has_feature("editor"):
		RenderingServer.global_shader_parameter_set("PlayerVisionSource",global_position);
