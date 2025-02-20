extends Node2D

var hex_radius = 50
var hex_width = hex_radius * 2
var hex_height = sqrt(3) * hex_radius
var rows = 50
var columns = 50

func _ready():
	queue_redraw()  # Requests the _draw() function to be called

func _draw():
	draw_hex_grid()

func draw_hex_grid():
	for row in range(rows):
		for col in range(columns):
			var x_offset = col * hex_width * 0.75
			var y_offset = row * hex_height
			if col % 2 == 1:
				y_offset += hex_height / 2
			draw_hexagon(Vector2(x_offset, y_offset))

func draw_hexagon(center: Vector2):
	var points = []
	for i in range(6):
		var angle = i * PI / 3
		var x = center.x + hex_radius * cos(angle)
		var y = center.y + hex_radius * sin(angle)
		points.append(Vector2(x, y))

	# Fill the hexagon (white)
	draw_polygon(PackedVector2Array(points), PackedColorArray([Color(1, 1, 1, 1)]))

	# Draw black border
	points.append(points[0])  # Close the loop for the outline
	draw_polyline(PackedVector2Array(points), Color(0, 0, 0), 2.0)  # Black border with width 2.0
