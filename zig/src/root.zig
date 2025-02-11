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

        fn toByte(cell: Cell) u8 {
            return switch (cell) {
                .illegal => 1,
                .empty => 0,
                .stack => |s| 1 + s.size + @as(u8, switch (s.color) {
                    .red => 100,
                    .blue => 200,
                }),
            };
        }
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

fn v(x: i32, y: i32) Board.Location {
    return .{ .x = x, .y = y };
}

export fn InitBoard(b: *Board) void {
    b.* = Board.empty;
}

export fn BoardSize() i32 {
    return @truncate(@sizeOf(Board));
}

export fn xAt(b: *Board, x: i32, y: i32) u8 {
    return b.at(v(x, y)).toByte();
}

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
