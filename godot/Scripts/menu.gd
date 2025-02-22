extends CanvasLayer

@onready var pan = $Panel
@onready var btn = $"Panel/StartGameSubMenu/Single Player"
@onready var btn1 = $"Panel/StartGameSubMenu/Two Player"
@onready var btn2 = $Panel/StartGameSubMenu/Online
var popbox = true
var other = false
static var background_scene = null

func _ready():
	# Initially hide the panel
	pan.visible = false

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
	if !other and !popbox:
		pan.visible = false

# Function for showing the buttons when "Start Game" is pressed
func _on_start_game_btn_pressed() -> void:
	pan.visible = true
	other = true
	popbox = true

func _on_start_game_btn_mouse_entered() -> void:
	pan.visible = true
	other = true
	popbox = true

func _on_start_game_btn_mouse_exited() -> void:
	other = false
	
func _on_panel_mouse_exited() -> void:
	popbox = false


func _on_panel_mouse_entered() -> void:
	popbox = true


func _on_single_player_mouse_entered() -> void:
	popbox = true



func _on_two_player_mouse_entered() -> void:
	popbox = true


func _on_online_mouse_entered() -> void:
	popbox = true


func _on_single_player_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/difficulty.tscn")


func _on_settings_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/settings.tscn")
