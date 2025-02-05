extends Node

var background_scene = null

func _ready():
	if background_scene == null:
		background_scene = load("res://Scenes/SharkAnimation2.tscn").instantiate()
		get_tree().root.add_child(background_scene)  # Keep it at root so it doesn't get freed
