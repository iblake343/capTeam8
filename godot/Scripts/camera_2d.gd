extends Camera2D

@export var zoom_step: float = 0.1  # Zoom step for scrolling
@export var min_zoom: float = 0.15  # Minimum zoom
@export var max_zoom: float = 2  # Maximum zoom
@export var drag_speed: float = 2  # Speed of camera movement during drag

var dragging = false
var last_mouse_pos = Vector2.ZERO

func _ready():
	pass

func _input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT:
			if event.pressed:
				dragging = true
				last_mouse_pos = event.position
			else:
				dragging = false

	if event is InputEventMouseMotion and dragging:
		# Calculate the offset by the mouse drag and move the camera accordingly
		var delta = event.position - last_mouse_pos
		position -= delta * drag_speed  # Move the camera based on drag
		last_mouse_pos = event.position  # Update the last mouse position

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
		# Zoom in and out using the scroll wheel
		var zoom_factor = 1.0
		zoom_factor = 1 - zoom_step  # Zoom out

		# Apply zoom and clamp each component (x and y) separately
		zoom.x = clamp(zoom.x * zoom_factor, min_zoom, max_zoom)
		zoom.y = clamp(zoom.y * zoom_factor, min_zoom, max_zoom)
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_WHEEL_UP:
		# Zoom in and out using the scroll wheel
		var zoom_factor = 1.0
		zoom_factor = 1 + zoom_step  # Zoom out

		# Apply zoom and clamp each component (x and y) separately
		zoom.x = clamp(zoom.x * zoom_factor, min_zoom, max_zoom)
		zoom.y = clamp(zoom.y * zoom_factor, min_zoom, max_zoom)
