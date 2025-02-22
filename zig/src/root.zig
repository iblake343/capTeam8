pub const Turn = union(enum) {
    place_hexes: struct { Location, Direction, Orientation },
    place_tokens: Location,
    move_tokens: struct { Location, Direction, Count },

    pub const Kind = std.meta.FieldEnum(Turn);
    pub fn parse(kind: Kind, src: []const u8, poi: []Location) !Turn {
        var tokens = std.mem.tokenizeAny(u8, src, " \r\t");

        const src_loc = tokens.next() orelse return error.expected_location;
        if (src_loc.len != 1) return error.expected_location;
        const ix = indexFromPoiChar(src_loc[0]) catch return error.invalid_location;
        if (ix >= poi.len) return error.invalid_location;
        const loc = poi[ix];

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

fn indexFromPoiChar(char: u8) !usize {
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
    cells: [max_size][max_size]Cell = [1][max_size]Cell{.{.illegal} ** max_size} ** max_size,
    current_player: Player = @enumFromInt(0),
    hexes_count: usize = 0,
    initial_stack_count: usize = 0,
    done_resizing: bool = false,
    origin: Location = .{ 0, 0 },
    size: @Vector(2, u31) = .{ 1, 1 },
    out_of_moves: PArray = PArray.initFill(false),

    const PArray = std.EnumArray(Player, bool);
    pub const max_size = 30;
    pub const count_players = std.meta.fields(Player).len;
    pub const max_hexes = 16 * count_players;
    pub const poi_buf_len = max_hexes;

    fn splat(scalar: i32) Location {
        return @splat(scalar);
    }

    /// computes @min(b - a, 0) but adjusts the type
    fn delta(a: Location, b: Location) @Vector(2, u31) {
        return @intCast(@max(b - a, splat(0)));
    }

    pub fn location(b: *const Board, dx: usize, dy: usize) Location {
        const idx: i32 = @intCast(dx);
        const idy: i32 = @intCast(dy);
        return b.origin + Location{ idx, idy };
    }

    fn addHex(b: *Board, loc: Location) void {
        b.size += delta(loc, b.origin);
        b.origin -= delta(loc, b.origin);
        b.size += delta(b.origin + b.size, loc + splat(1));
        b.at(loc).* = .empty;
    }

    fn elem(as: []Location, a: Location) bool {
        for (as) |x| if (std.meta.eql(a, x)) return true;
        return false;
    }

    fn ptrCast(ixs: []Location) []u8 {
        const bytes: [*]u8 = @ptrCast(ixs.ptr);
        return bytes[0 .. ixs.len * @sizeOf(Location)];
    }

    pub fn pointsOfInterest(b: *const Board, buf: *[poi_buf_len]Location) []Location {
        var fba = std.heap.FixedBufferAllocator.init(ptrCast(buf));
        var list = std.ArrayList(Location).initCapacity(fba.allocator(), buf.len) catch unreachable;
        const next_move = b.nextExpectedMove() orelse return buf[0..0];
        switch (next_move) {
            .place_hexes => {
                // find all .empty cells adjacent to a .illegal cell;
                const w, const h = b.size;
                for (0..h) |dy| for (0..w) |dx| {
                    const loc = b.location(dx, dy);
                    if (b.get(loc) != .empty) continue;
                    if (b.get(loc + Direction.vector(.nw)) == .illegal or
                        b.get(loc + Direction.vector(.ne)) == .illegal or
                        b.get(loc + Direction.vector(.sw)) == .illegal or
                        b.get(loc + Direction.vector(.se)) == .illegal or
                        b.get(loc + Direction.vector(.w)) == .illegal or
                        b.get(loc + Direction.vector(.e)) == .illegal)
                    {
                        list.appendAssumeCapacity(loc);
                    }
                };
                // if no empty cells exist, return just 0, 0
                if (list.items.len == 0) list.appendAssumeCapacity(.{ 0, 0 });
            },
            .place_tokens => {
                // find all .empty cells on the outside;
                // find first .empty cell on board; circle island until back to start
                const w, const h = b.size;
                const start = b: for (0..h) |dy| (for (0..w) |dx| {
                    const loc = b.location(dx, dy);
                    if (b.get(loc) == .empty) break :b loc;
                }) else unreachable;
                list.appendAssumeCapacity(start);

                var dir: Direction = .ne;
                var loc: Location = start;
                while (!std.meta.eql(start, b: {
                    // find next cell
                    for (0..6) |_| {
                        dir = dir.right();
                        if (b.get(loc + dir.vector()) != .illegal) {
                            loc += dir.vector();
                            dir = dir.back();
                            break :b loc;
                        }
                    } else unreachable;
                })) {
                    if (b.get(loc) == .empty and
                        !elem(list.items, loc))
                        list.appendAssumeCapacity(loc);
                }
            },
            .move_tokens => {
                findPoiMoveTokens(b, &list, b.current_player);
            },
        }
        return list.items;
    }

    fn findPoiMoveTokens(b: *const Board, list: *std.ArrayList(Location), player: Player) void {
        // find all stacks of current_player with count > "1" (> 0)
        const w, const h = b.size;
        for (0..h) |dy| for (0..w) |dx| {
            const loc = b.location(dx, dy);

            if (b.get(loc) != .stack) continue;
            if (b.get(loc).stack.color != player) continue;
            if (b.get(loc).stack.count == 0) continue;

            if (b.get(loc + Direction.vector(.nw)) != .empty and
                b.get(loc + Direction.vector(.ne)) != .empty and
                b.get(loc + Direction.vector(.sw)) != .empty and
                b.get(loc + Direction.vector(.se)) != .empty and
                b.get(loc + Direction.vector(.w)) != .empty and
                b.get(loc + Direction.vector(.e)) != .empty)
                continue;

            list.appendAssumeCapacity(loc);
        };
    }

    pub fn winner(b: *const Board) ?Player {
        var scores: [count_players]Score = undefined;
        for (&scores, 0..) |*score, ix|
            score.* = b.scorePlayer(@enumFromInt(ix));
        return indexOfUniqueMaxPlayer(&scores);
    }

    fn indexOfUniqueMaxPlayer(xs: []const Score) ?Player {
        var max: Score = 0;
        var ix_max: ?usize = null;

        for (xs, 0..) |x, ix| {
            if (x > max) {
                max = x;
                ix_max = ix;
            } else if (x == max) {
                ix_max = null;
            }
        }

        if (ix_max) |ix| return @enumFromInt(ix);
        return null;
    }

    const Score = u16;
    fn scorePlayer(b: *const Board, player: Player) Score {
        var count_stacks: Score = 0;
        var max_count_contiguous: Score = 0;

        var breadcrumbs = breadcrumbsWithOrigin(b.origin);
        const w, const h = b.size;
        for (0..h) |dy| for (0..w) |dx| {
            const loc = b.location(dx, dy);
            if (breadcrumbs.guard(loc)) continue;
            if (b.get(loc) != .stack) continue;
            if (b.get(loc).stack.color != player) continue;

            // DFS the region, counting stacks
            // this DFS is modified to ensure that the stack cannot possibly be longer than 16 items
            var count_contiguous: Score = 0;

            var loc_buf: [16]Location = undefined;
            var fba = std.heap.FixedBufferAllocator.init(ptrCast(&loc_buf));
            var stack = std.ArrayList(Location).initCapacity(fba.allocator(), 16) catch unreachable;

            stack.appendAssumeCapacity(loc);
            while (stack.popOrNull()) |node| {
                count_contiguous += 1;
                for (0..std.meta.fields(Direction).len) |ix_dir| {
                    const next = node + Direction.vector(@enumFromInt(ix_dir));
                    if (breadcrumbs.guard(next)) continue;
                    if (b.get(next) != .stack) continue;
                    if (b.get(next).stack.color != player) continue;

                    stack.appendAssumeCapacity(next);
                }
            }

            count_stacks += count_contiguous;
            if (count_contiguous > max_count_contiguous)
                max_count_contiguous = count_contiguous;
        };

        return count_stacks * 16 + max_count_contiguous;
    }

    fn breadcrumbsWithOrigin(origin: Location) struct {
        const Breadcrumbs = @This();
        origin: Location,
        data: [max_size][max_size]bool = [1][max_size]bool{.{false} ** max_size} ** max_size,

        fn at(b: *Breadcrumbs, loc: Location) *bool {
            const ix, const iy = loc - b.origin;
            const x: usize = @intCast(ix);
            const y: usize = @intCast(iy);
            return &b.data[x][y];
        }
        fn get(b: *const Breadcrumbs, loc: Location) bool {
            return @constCast(b).at(loc).*;
        }
        fn set(b: *Breadcrumbs, loc: Location) void {
            b.at(loc).* = true;
        }
        fn guard(b: *Breadcrumbs, loc: Location) bool {
            if (@reduce(.Min, loc - b.origin) < 0) return false;
            defer b.set(loc);
            return b.get(loc);
        }
    } {
        return .{ .origin = origin };
    }

    pub fn nextExpectedMove(b: *const Board) ?std.meta.FieldEnum(Turn) {
        if (b.hexes_count < max_hexes) return .place_hexes;
        if (b.initial_stack_count < count_players) return .place_tokens;
        if (b.out_of_moves.get(b.current_player)) return null;
        return .move_tokens;
    }

    pub fn doTurn(b: *Board, turn: Turn) TurnError!void {
        switch (turn) {
            .place_hexes => |pl| try b.placeHexes(pl[0], pl[1], pl[2]),
            .place_tokens => |loc| try b.placeTokens(loc),
            .move_tokens => |pl| {
                try b.moveTokens(pl[0], pl[1], pl[2]);

                inline for (std.meta.fields(Player)) |f| {
                    const player: Player = @enumFromInt(f.value);
                    var poi: [poi_buf_len]Location = undefined;
                    var fba = std.heap.FixedBufferAllocator.init(ptrCast(&poi));
                    var list = std.ArrayList(Location).initCapacity(fba.allocator(), poi_buf_len) catch unreachable;

                    findPoiMoveTokens(b, &list, player);
                    if (list.items.len == 0) b.out_of_moves.set(player, true);
                }
            },
        }
        b.current_player = b.current_player.next();
        for (0..count_players) |_|
            if (b.out_of_moves.get(b.current_player)) {
                b.current_player = b.current_player.next();
            } else break;
    }

    fn placeHexes(
        board: *Board,
        cell: Location,
        dir_orient: Direction,
        orient: Orientation,
    ) PlaceHexesError!void {
        const orig, const dir = reifyTileOrientation(cell, dir_orient, orient);

        const v = dir.vector();
        const w = dir.right().vector();
        const loc1 = orig;
        const loc2 = orig + v;
        const loc3 = orig + w;
        const loc4 = loc2 + w;

        for ([4]Location{ loc1, loc2, loc3, loc4 }) |loc| {
            if (board.get(loc) != .illegal) return error.collision;
        }

        for ([4]Location{ loc1, loc2, loc3, loc4 }) |loc| {
            board.addHex(loc);
        }

        board.hexes_count += 4;
    }

    // TODO does this fn need to do validation?
    fn placeTokens(b: *Board, loc: Location) PlaceTokensError!void {
        b.at(loc).* = .{ .stack = .{ .color = b.current_player, .count = 15 } };
        b.initial_stack_count += 1;
    }

    // pub const MoveTokensError = error { oob, not_enough_tokens, collision, invalid_direction };
    fn moveTokens(b: *Board, start: Location, dir: Direction, count: Count) MoveTokensError!void {
        // ensure that there is a stack at `start`
        // that matches the current_player
        // and has at least count + 1 tokens
        {
            const cell = b.get(start);
            if (cell != .stack) return error.cell_is_not_stack;
            if (cell.stack.color != b.current_player) return error.wrong_color;
            if (cell.stack.count < count) return error.not_enough_tokens;
        }

        // find dest by moving start by dir until not .empty
        // ensure that dest != start
        const dest = b: for (1..max_size) |udist| {
            const dist: i32 = @intCast(udist);
            if (b.get(start + dir.vector() * splat(dist)) == .empty) continue;
            if (dist == 1) return error.invalid_direction;
            break :b start + dir.vector() * splat(dist - 1);
        } else unreachable;

        // do the moving
        b.at(start).stack.count -= count;
        b.at(dest).* = .{
            .stack = .{ .color = b.current_player, .count = count - 1 },
        };
    }

    fn at(b: *Board, loc: Location) *Cell {
        return &b.cells[@intCast(@mod(loc[0], max_size))][@intCast(@mod(loc[1], max_size))];
    }

    pub fn get(b: *const Board, loc: Location) Cell {
        return @constCast(b).at(loc).*;
    }
};

pub fn reifyTileOrientation(
    cell: Location,
    face: Direction,
    orientation: Orientation,
) struct { Location, Direction } {
    return switch (orientation) {
        .flat => .{ cell + face.vector() + face.left().vector(), face.right() },
        .ur => .{ cell + face.vector(), face },
        .ul => .{ cell + face.vector(), face.left() },
        .r => .{ cell + face.vector(), face.right() },
        .l => .{ cell + face.vector(), face.left().left() },
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

pub const Location = @Vector(2, i32);
const Loc = Location;

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
    pub fn vector(dir: Direction) Location {
        return switch (dir) {
            .e => .{ 1, 0 },
            .w => .{ -1, 0 },
            .se => .{ 0, 1 },
            .nw => .{ 0, -1 },
            .ne => .{ 1, -1 },
            .sw => .{ -1, 1 },
        };
    }
};

// fn inRange(a: anytype, x: anytype, b: anytype) bool {
//     return a <= x and x < b;
// }

export fn InitBoard(b: *Board) void {
    b.* = .{};
}

export fn BoardSize() i32 {
    return @truncate(@sizeOf(Board));
}

export fn xAt(b: *Board, x: i32, y: i32) u8 {
    return b.at(.{ x, y }).toByte();
}

const std = @import("std");
