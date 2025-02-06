extends CanvasLayer

# Make background_scene static so it persists across scene changes
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
	get_tree().root.add_child(background_scene)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	var animation_player = background_scene.get_node_or_null("AnimationPlayer")
	if animation_player and not animation_player.is_playing():
			animation_player.play("Animation", 0, true)


func _on_start_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/select_net_ai.tscn")


func _on_settings_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/settings.tscn")
