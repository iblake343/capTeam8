extends CanvasLayer
static var background_scene = null

func _ready():
	# Check if background_scene is already in the scene tree
	if not is_instance_valid(background_scene):
		background_scene = load("res://Scenes/PersistentBackground.tscn").instantiate()
		call_deferred("add_background_scene")
	else:
		# If background exists, check the animation state
		var animation_player = background_scene.get_node_or_null("AnimationPlayer")
		if animation_player and not animation_player.is_playing():
			animation_player.play("Animation", 0, true)  # Resume if needed

func add_background_scene():
	# Add background to the root
	get_tree().root.add_child(background_scene)

func _process(delta: float) -> void:
	# Ensure background animation continues if not playing
	var animation_player = background_scene.get_node_or_null("AnimationPlayer")
	if animation_player and not animation_player.is_playing():
		animation_player.play("Animation", 0, true)

func _on_single_player_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/difficulty.tscn")
	

func _on_two_player_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/characterPlayer1.tscn")

func _on_online_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/character.tscn")

func _on_settings_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/settings.tscn")

func _on_quit_btn_pressed() -> void:
	get_tree().quit()
