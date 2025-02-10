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

pub const Board = struct {
    origin: struct { x: u31, y: u31 },
    cells: [size * size]Cell,

    pub const size = 30;

    pub const empty: Board = .{
        .origin = .{ .x = @divFloor(size, 2), .y = @divFloor(size, 2) },
        .cells = [1]Cell{.illegal} ** (size * size),
    };

    /// this function does not verify that the placing location is valid
    pub fn setTile(b: *Board, orig: Location, dir: Direction) void {
        b.at(orig).* = .empty;
        b.at(orig.move(dir, 1)).* = .empty;
        b.at(orig.move(dir.right(), 1)).* = .empty;
        b.at(orig.move(dir, 1).move(dir.right(), 1)).* = .empty;
    }

    pub fn at(b: *Board, loc: Location) *Cell {
        return &b.cells[@intCast(size * (loc.y + b.origin.y) + loc.x + b.origin.x)];
    }

    pub fn get(b: *const Board, loc: Location) Cell {
        return b.cells[@intCast(size * (loc.y + b.origin.y) + loc.x + b.origin.x)];
    }

    pub const Cell = union(enum) {
        illegal,
        empty,
        stack: Stack,
    };

    pub const Stack = struct {
        color: Color,
        size: Size,

        pub const Size = u4;
    };

    pub const Location = struct {
        x: i32,
        y: i32,

        pub fn move(loc: Location, dir: Direction, dist: i32) Location {
            var loc2 = loc;
            switch (dir) {
                .e => loc2.x += dist,
                .w => loc2.x -= dist,
                .se => loc2.y += dist,
                .nw => loc2.y -= dist,
                .sw => {
                    loc2.x -= dist;
                    loc2.y += dist;
                },
                .ne => {
                    loc2.x += dist;
                    loc2.y -= dist;
                },
            }
            return loc2;
        }
    };
};

pub const Color = enum { red, blue };

pub const Direction = enum {
    nw,
    ne,
    e,
    se,
    sw,
    w,
    pub fn right(dir: Direction) Direction {
        return @enumFromInt((@intFromEnum(dir) + 1) % 6);
    }
};

const std = @import("std");
const lib = @import("prototype_lib");
