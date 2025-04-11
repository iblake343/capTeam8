extends Button

func _on_pressed() -> void:
	var node = $"../../../Help"
	node.set_visible(!node.is_visible())


func _on_visibility_changed() -> void:
	pass # Replace with function body.
