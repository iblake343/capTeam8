extends Node2D

func _on_exit_button_pressed() -> void:
	Music.playButtonPress()
	set_visible(!is_visible())

var button_down = false
func _on_toolbar_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		print("mousedown on " + name)
		if event.button_index == MOUSE_BUTTON_LEFT:
			button_down = event.pressed
	if event is InputEventMouseMotion:
		if button_down:
			position = position + event.relative

func _ready() -> void:
	position = get_parent().size * 0.5;
	
func _on_visibility_changed() -> void:
	position = get_parent().size * 0.5;

func _on_main_menu_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().change_scene_to_file.bind("res://Scenes/menu.tscn").call_deferred()

func _on_show_board_btn_pressed() -> void:
	Music.playButtonPress()
	$"../Win Dialogue".hide()

func _on_exit_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().quit()
