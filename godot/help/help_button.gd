extends Button
var help_scene = preload("res://help/help.tscn")

func _on_pressed() -> void:
	var node = help_scene.instantiate()
	get_parent().add_child(node)
	node.position = get_window().size * 0.5;
