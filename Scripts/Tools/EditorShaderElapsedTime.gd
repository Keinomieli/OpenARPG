@tool
extends Node

var time : float = -600.0

func _process(_delta: float) -> void:
	if OS.has_feature("editor"):
		time += _delta
		if time > 600.0:
			time = -600.0
		RenderingServer.global_shader_parameter_set("ElapsedTime", time);
