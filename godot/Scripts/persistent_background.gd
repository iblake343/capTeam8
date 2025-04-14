extends Node3D
@onready var animation_player = $AnimationPlayer
# Called when the node enters the scene tree for the first time.
func _ready():
	var a = animation_player.get_node_or_null("AnimationPlayer")
	if a:
		a.play("Animation", 0, true)  # Play animation with looping enabled


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass
