pub const Turn = union(enum) {
    place_hexes: struct { Location, Direction, Orientation },
    place_tokens: Location,
    move_tokens: struct { Location, Direction, Count },

    pub const Kind = std.meta.FieldEnum(Turn);
    pub fn parse(kind: Kind, src: []const u8, poi: []Board.Index, b: *const Board) !Turn {
        var tokens = std.mem.tokenizeAny(u8, src, " \r\t");

        const src_loc = tokens.next() orelse return error.expected_location;
        if (src_loc.len != 1) return error.expected_location;
        const ix = indexFromPoiChar(src_loc[0]) catch return error.invalid_location;
        if (ix >= poi.len) return error.invalid_location;
        const loc = b.locFromIndex(poi[ix]);

        if (kind == .place_tokens) return .{ .place_tokens = loc };

        const src_dir = tokens.next() orelse return error.expected_direction;
        const dir = std.meta.stringToEnum(Direction, src_dir) orelse return error.invalid_direction;

        switch (kind) {
            .place_hexes => {
                const src_orient = tokens.next() orelse return error.expected_orientation;
                const orient = std.meta.stringToEnum(Orientation, src_orient) orelse return error.invalid_orientation;

                return .{ .place_hexes = .{ loc, dir, orient } };
            },
            .move_tokens => {
                const src_count = tokens.next() orelse return error.expected_count;
                const count = std.fmt.parseInt(Count, src_count, 10) catch return error.invalid_count;

                return .{ .move_tokens = .{ loc, dir, count } };
            },
            .place_tokens => unreachable,
        }
    }
};

fn indexFromPoiChar(char: u8) !Board.Index {
    return switch (char) {
        'a'...'z' => char - 'a',
        'A'...'Z' => char - 'A' + 26,
        '0'...'9' => char - '0' + 52,
        else => return error.bad_index,
    };
}

pub const Orientation = enum { ur, ul, flat, r, l };

pub const PlaceHexesError = error{ oob, collision };
pub const PlaceTokensError = error{ oob, invalid_location };
pub const MoveTokensError = error{
    oob,
    cell_is_not_stack,
    wrong_color,
    not_enough_tokens,
    collision,
    invalid_direction,
};
pub const TurnError = PlaceHexesError || PlaceTokensError || MoveTokensError;

pub const Board = struct {
    origin: struct { x: u31, y: u31 },
    cells: [size * size]Cell,
    current_player: Player,
    hexes_count: usize,
    initial_stack_count: usize = 0,

    pub const Index = std.math.IntFittingRange(0, size * size - 1);
    pub const size = 30;
    pub const count_players = std.meta.fields(Player).len;
    pub const max_hexes = 16 * count_players;
    // for 2 players, 32
    pub const poi_buf_len = max_hexes;

    pub fn locFromUnsignedCoords(b: *const Board, x: usize, y: usize) Location {
        const ix: i32 = @intCast(x);
        const iy: i32 = @intCast(y);
        return .{ .x = ix - b.origin.x, .y = iy - b.origin.y };
    }

    pub fn locFromIndex(b: *const Board, ix: usize) Location {
        const y = ix / size;
        const x = ix % size;
        return b.locFromUnsignedCoords(x, y);
    }

    pub const empty: Board = .{
        .origin = .{ .x = @divFloor(size, 2), .y = @divFloor(size, 2) },
        .cells = [1]Cell{.illegal} ** (size * size),
        .current_player = .blue,
        .hexes_count = 0,
    };

    fn ptrCast(ixs: []Index) []u8 {
        const bytes: [*]u8 = @ptrCast(ixs.ptr);
        return bytes[0 .. ixs.len * @sizeOf(Index)];
    }

    pub fn pointsOfInterest(b: *const Board, buf: *[poi_buf_len]Index) []Index {
        var fba = std.heap.FixedBufferAllocator.init(ptrCast(buf));
        var list = std.ArrayList(Index).initCapacity(fba.allocator(), buf.len) catch unreachable;
        const next_move = b.nextExpectedMove();
        switch (next_move) {
            .place_hexes => {
                // find all .empty cells adjacent to a .illegal cell;
                for (b.cells, 0..) |cell, ix| {
                    if (cell != .empty) continue;
                    const loc = b.locFromIndex(ix);
                    if ((b.get(loc.move(.nw)) catch .empty) == .illegal or
                        (b.get(loc.move(.ne)) catch .empty) == .illegal or
                        (b.get(loc.move(.sw)) catch .empty) == .illegal or
                        (b.get(loc.move(.se)) catch .empty) == .illegal or
                        (b.get(loc.move(.w)) catch .empty) == .illegal or
                        (b.get(loc.move(.e)) catch .empty) == .illegal)
                    {
                        list.appendAssumeCapacity(@truncate(ix));
                    }
                }
                // if no empty cells exist, return just 0, 0
                if (list.items.len == 0)
                    list.appendAssumeCapacity(
                        @truncate(b.indexOfLoc(.{ .x = 0, .y = 0 }) catch unreachable),
                    );
            },
            .place_tokens => {
                // find all .empty cells on the outside;
                // find first .empty cell on board; circle island until back to start
                const start = b: for (0..size) |y| (for (0..size) |x| {
                    const loc = b.locFromUnsignedCoords(x, y);
                    if (b.get(loc) catch unreachable == .empty)
                        break :b loc;
                }) else unreachable;
                list.appendAssumeCapacity(b.indexOfLoc(start) catch unreachable);

                std.debug.print("{any} start\n", .{start});

                var dir: Direction = .ne;
                var loc: Location = start;
                while (!std.meta.eql(start, b: {
                    // find next cell
                    for (0..6) |_| {
                        dir = dir.right();
                        if ((b.get(loc.move(dir)) catch .illegal) != .illegal) {
                            loc = loc.move(dir);
                            dir = dir.back();
                            break :b loc;
                        }
                    } else unreachable;
                })) {
                    const ix = b.indexOfLoc(loc) catch unreachable;
                    if (b.cells[ix] == .empty and std.mem.indexOfScalar(Index, list.items, ix) == null)
                        list.appendAssumeCapacity(ix);
                }
            },
            .move_tokens => {
                // find all stacks of current_player with count > "1" (> 0)
                for (0..size) |y| for (0..size) |x| {
                    const loc = b.locFromUnsignedCoords(x, y);
                    const ix = b.indexOfLoc(loc) catch unreachable;

                    if (b.cells[ix] != .stack) continue;
                    if (b.cells[ix].stack.color != b.current_player) continue;
                    if (b.cells[ix].stack.count == 0) continue;

                    list.appendAssumeCapacity(ix);
                };
            },
        }
        return list.items;
    }

    pub fn nextExpectedMove(b: *const Board) std.meta.FieldEnum(Turn) {
        if (b.hexes_count < max_hexes) return .place_hexes;
        if (b.initial_stack_count < count_players) return .place_tokens;
        return .move_tokens;
    }

    pub fn doTurn(b: *Board, turn: Turn) TurnError!void {
        switch (turn) {
            .place_hexes => |pl| try b.placeHexes(pl[0], pl[1], pl[2]),
            .place_tokens => |loc| try b.placeTokens(loc),
            .move_tokens => |pl| try b.moveTokens(pl[0], pl[1], pl[2]),
        }
        b.current_player = b.current_player.next();
    }

    fn placeHexes(
        board: *Board,
        cell: Location,
        dir_orient: Direction,
        orient: Orientation,
    ) PlaceHexesError!void {
        const orig, const dir = reifyTileOrientation(cell, dir_orient, orient);

        const rgt = dir.right();
        const loc2 = orig.move(dir);
        const xa = try board.at(orig);
        const xb = try board.at(loc2);
        const xc = try board.at(orig.move(rgt));
        const xd = try board.at(loc2.move(rgt));

        for ([4]*Cell{ xa, xb, xc, xd }) |ptr| {
            if (ptr.* != .illegal) return error.collision;
        }

        for ([4]*Cell{ xa, xb, xc, xd }) |ptr| {
            ptr.* = .empty;
        }

        board.hexes_count += 4;
    }

    /// ensures that the placing location is valid
    ///
    /// `valid` means that the piece is on an external edge of the board,
    /// i.e. there is a path from the piece to the bounds of our space
    /// that only crosses one non-illegal tile (the one at `loc`)
    fn placeTokens(b: *Board, loc: Location) PlaceTokensError!void {
        (try b.at(loc)).* = .{ .stack = .{ .color = b.current_player, .count = 15 } };
        b.initial_stack_count += 1;
    }

    // pub const MoveTokensError = error { oob, not_enough_tokens, collision, invalid_direction };
    fn moveTokens(b: *Board, start: Location, dir: Direction, count: Count) MoveTokensError!void {
        // ensure that there is a stack at `start`
        // that matches the current_player
        // and has at least count + 1 tokens
        {
            const cell = try b.get(start);
            if (cell != .stack) return error.cell_is_not_stack;
            if (cell.stack.color != b.current_player) return error.wrong_color;
            if (cell.stack.count < count) return error.not_enough_tokens;
        }

        // find dest by moving start by dir until not .empty
        // ensure that dest != start
        var x = start;
        const dest = b: while (b.get(x.move(dir))) |cell| {
            if (cell != .empty) break :b x;
            x = x.move(dir);
        } else |err| switch (err) {
            error.oob => x,
        };

        if (std.meta.eql(dest, start)) return error.invalid_direction;

        // do the moving
        (try b.at(start)).stack.count -= count;
        (try b.at(dest)).* = .{
            .stack = .{ .color = b.current_player, .count = count - 1 },
        };
    }

    pub fn indexOfLoc(b: *const Board, loc: Location) error{oob}!Index {
        if (!inRange(0, loc.x + b.origin.x, size) or !inRange(0, loc.y + b.origin.y, size))
            return error.oob;
        return @intCast(size * (loc.y + b.origin.y) + loc.x + b.origin.x);
    }

    fn at(b: *Board, loc: Location) error{oob}!*Cell {
        return &b.cells[try b.indexOfLoc(loc)];
    }

    pub fn get(b: *const Board, loc: Location) error{oob}!Cell {
        return (try @constCast(b).at(loc)).*;
    }
};

pub fn reifyTileOrientation(
    cell: Location,
    face: Direction,
    orientation: Orientation,
) struct { Location, Direction } {
    return switch (orientation) {
        .flat => .{ cell.move(face).move(face.left()), face.right() },
        .ur => .{ cell.move(face), face },
        .ul => .{ cell.move(face), face.left() },
        .r => .{ cell.move(face), face.right() },
        .l => .{ cell.move(face), face.left().left() },
    };
}

pub const Cell = union(enum) {
    illegal,
    empty,
    stack: Stack,

    fn toByte(cell: Cell) u8 {
        return switch (cell) {
            .illegal => 1,
            .empty => 0,
            .stack => |s| 1 + s.count + @as(u8, switch (s.color) {
                .red => 100,
                .blue => 200,
            }),
        };
    }

    fn fromByte(byte: u8) error{illegalByte}!Cell {
        return switch (byte) {
            0 => .empty,
            1 => .illegal,
            101...116 => .{ .stack = .{ .color = .red, .size = @truncate(byte - 101) } },
            201...216 => .{ .stack = .{ .color = .blue, .size = @truncate(byte - 201) } },
            else => error.illegalByte,
        };
    }
};

pub const Count = u4;
pub const Stack = struct {
    color: Player,
    count: Count,
};

pub const Location = struct {
    x: i32,
    y: i32,

    pub fn move(loc: Location, dir: Direction) Location {
        return loc.moveDist(dir, 1);
    }
    pub fn moveDist(loc: Location, dir: Direction, dist: i32) Location {
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

pub const Player = enum {
    red,
    blue,
    fn next(p: Player) Player {
        return @enumFromInt((@intFromEnum(p) +% 1) % std.meta.fields(Player).len);
    }
};

pub const Direction = enum(u8) {
    nw,
    ne,
    e,
    se,
    sw,
    w,
    pub fn right(dir: Direction) Direction {
        return @enumFromInt((@intFromEnum(dir) + 1) % 6);
    }
    pub fn left(dir: Direction) Direction {
        return @enumFromInt((@intFromEnum(dir) + 5) % 6);
    }
    pub fn back(dir: Direction) Direction {
        return @enumFromInt((@intFromEnum(dir) + 3) % 6);
    }
};

fn v(x: i32, y: i32) Location {
    return .{ .x = x, .y = y };
}

fn inRange(a: anytype, x: anytype, b: anytype) bool {
    return a <= x and x < b;
}

export fn InitBoard(b: *Board) void {
    b.* = Board.empty;
}

export fn BoardSize() i32 {
    return @truncate(@sizeOf(Board));
}

export fn xAt(b: *Board, x: i32, y: i32) u8 {
    return (b.at(v(x, y)) catch {
        return 2;
    }).toByte();
}

const std = @import("std");
