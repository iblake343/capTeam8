extends Node2D

func _on_exit_button_pressed() -> void:
	queue_free()

var button_down = false
func _on_toolbar_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			button_down = event.pressed
	if event is InputEventMouseMotion:
		if button_down:
			position = position + event.relative
		
