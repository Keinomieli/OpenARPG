@tool
extends Node

@export_dir var input_folder = "res://input_meshes/"
@export_dir var output_folder = "res://output_scenes/"

@export var process_meshes: bool:
	set(value):
		process_dir()

func process_dir():
	var dir = DirAccess.open(input_folder)
	if dir:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			if file_name.ends_with(".res") or file_name.ends_with(".tres"):
				process_mesh_file(file_name)
			file_name = dir.get_next()
		dir.list_dir_end()
		print("Process complete.")
	else:
		printerr("An error occurred when trying to access the path.")

func process_mesh_file(file_name):
	var mesh_resource = load(input_folder + "/" + file_name)
	if mesh_resource is Mesh:
		var mesh_instance = MeshInstance3D.new()
		mesh_instance.name = file_name.get_basename()
		mesh_instance.mesh = mesh_resource
		
		#var scene_root = Node3D.new()
		#scene_root.add_child(mesh_instance)
		#mesh_instance.owner = scene_root
		
		var output_file_name = file_name.get_basename() + ".tscn"
		var packed_scene = PackedScene.new()
		packed_scene.pack(mesh_instance)
		var error = ResourceSaver.save(packed_scene, output_folder + "/" + output_file_name)
		if error == OK:
			print("Scene saved successfully: " + output_file_name)
		else:
			printerr("Error saving scene: " + output_file_name)
	else:
		printerr("File is not a Mesh resource: " + file_name)
