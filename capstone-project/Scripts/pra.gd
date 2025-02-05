extends Node2D

var hex_radius = 50  
var hex_width = hex_radius * 2
var hex_height = sqrt(3) * hex_radius
var is_dragging = false
var has_been_placed = false

var grid_origin = Vector2(0, 0)  # Offset for grid alignment

# Static variable to count the number of instances (shared among all instances of this script)
static var instance_count = 0
const MAX_INSTANCES = 7  # Limit to 8 tiles

func _input(event):
	if Input.is_action_just_pressed("Rotate") and not has_been_placed:
		rotate_tile()

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed and not has_been_placed:
			var click_position = event.position
			if position.distance_to(click_position) < 200:  
				is_dragging = true
				print("Dragging started!")
		elif is_dragging:
			is_dragging = false
			if not has_been_placed:
				has_been_placed = true
				snap_to_grid()  
				print("Piece placed!")
				spawn_new_instance()

func _process(delta):
	if is_dragging:
		position = get_global_mouse_position()

func rotate_tile():
	rotation_degrees += 60
	if rotation_degrees > 180:
		rotation_degrees -= 360
	print("Rotated to:", rotation_degrees)

func snap_to_grid():
	var snapped_hex = pixel_to_hex(get_global_mouse_position())
	position = grid_to_pixel(snapped_hex.x, snapped_hex.y)  

func pixel_to_hex(pos: Vector2) -> Vector2i:
	pos -= grid_origin  
	var q = (pos.x * 2/3) / hex_radius
	var r = (-pos.x / 3 + pos.y * sqrt(3)/3) / hex_radius
	return Vector2i(round(q), round(r))

func spawn_new_instance():
	if instance_count >= MAX_INSTANCES:
		print("Maximum tiles placed!")
		return
	
	var new_tile = load("res://pra.tscn").instantiate()
	new_tile.global_position = Vector2(152, 502)  
	new_tile.scale = Vector2(0.43, 0.43)
	get_parent().add_child(new_tile)  
	var new_script = load("res://pra.gd")  
	new_tile.set_script(new_script)  

	instance_count += 1  # Increase the counter
	print("New instance created! Total:", instance_count)

func grid_to_pixel(q: int, r: int) -> Vector2:
	var x = hex_radius * 3/2 * q
	var y = hex_radius * sqrt(3) * (r + q / 2.0)
	return Vector2(x, y) + grid_origin  
