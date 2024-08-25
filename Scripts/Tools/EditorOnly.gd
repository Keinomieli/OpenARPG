extends Node

@export var delete_self : bool = true
@export var enable_nodes : Array[Node] = []
@export var disable_nodes : Array[Node] = []

func _ready() -> void:
	if delete_self:
		queue_free()
	else:
		for node in enable_nodes:
			node.visible = true
			node.set_process(true)
			node.set_physics_process(true)
		for node in disable_nodes:
			node.visible = false
			node.set_process(false)
			node.set_physics_process(false)
