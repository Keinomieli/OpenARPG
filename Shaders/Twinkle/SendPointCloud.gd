@tool
extends MeshInstance3D

const maxPointsCount = 100
var worldPoints : Array[Vector4] = []

@export var points : Array[Node3D] = []

@export var SendToShader: bool:
	set(value):
		sendToShader()

func sendToShader():
	worldPoints.clear()
	
	var refPointsCount = points.size()
	for n in maxPointsCount:
		if n >= refPointsCount:
			worldPoints.append(Vector4.ZERO)
		else:
			var p = points[n].global_position
			worldPoints.append(Vector4(p.x,p.y,p.z,1.0))
	
	var mat = get_surface_override_material(0)
	mat.set_shader_parameter("worldPoints", worldPoints)

func _ready():
	sendToShader()
