extends CanvasLayer
@onready var animation_player = $Background/SharkObject/Sketchfab_Scene/AnimationPlayer

# Called when the node enters the scene tree for the first time.
func _ready():
	animation_player.play("Animation", -1, 1.0)  # Play the animation
	animation_player.animation_finished.connect(_on_animation_finished)

func _on_animation_finished(anim_name):
	if anim_name == "Animation":
		animation_player.play("Animation")  # Restart when finished


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_start_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://select_net_ai.tscn")
