pub fn main() !void {
    const stdout = std.io.getStdOut().writer();

    var board = Board.empty;

    board.setTile(.{ .x = 0, .y = 0 }, .nw);
    board.setTile(.{ .x = 0, .y = -2 }, .w);

    for (0..16) |i| {
        board.at(.{ .x = -4 + @as(i32, @intCast(i)), .y = 1 }).* = .{ .stack = .{ .color = .red, .size = @intCast(i) } };
        board.at(.{ .x = -4 + @as(i32, @intCast(i)), .y = 2 }).* = .{ .stack = .{ .color = .blue, .size = @intCast(i) } };
    }

    try drawBoard(board, stdout, std.io.tty.detectConfig(std.io.getStdOut()));
}

pub fn drawBoard(b: Board, writer: anytype, color: std.io.tty.Config) !void {
    for (0..Board.size) |y| {
        try writer.writeByteNTimes(' ', y);
        for (0..Board.size) |x| {
            if (b.origin.x == x and b.origin.y == y) {
                try color.setColor(writer, .dim);
            }

            const char: u8 = switch (b.cells[Board.size * y + x]) {
                .illegal => '.',
                .empty => 'O',
                .stack => |stack| b: {
                    try color.setColor(writer, switch (stack.color) {
                        .red => .red,
                        .blue => .blue,
                    });
                    break :b @as(u8, switch (stack.size) {
                        0...8 => '1' + @as(u8, stack.size),
                        9...14 => 'A' + @as(u8, stack.size) - 9,
                        15 => 'X',
                    });
                },
            };

            try writer.print("{c} ", .{char});

            try color.setColor(writer, .reset);
        }
        try writer.writeByte('\n');
    }
}

const std = @import("std");
const lib = @import("prototype_lib");
const Board = lib.Board;
