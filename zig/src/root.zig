fn parseFirstTurn(src: []const u8) !Turn {
    var tokens = std.mem.tokenizeAny(u8, src, " \r\t");
    const dir_src = tokens.next() orelse return error.expected_direction;

    const dir = std.meta.stringToEnum(Direction, dir_src) orelse return error.invalid_direction;

    return .{ .place_hexes = .{ .{ 0, 0 }, dir.right() } };
}

pub const Turn = union(enum) {
    place_hexes: struct { Location, Direction },
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

        if (kind == .place_tokens) {
            if (tokens.next()) |_| return error.unexpected_extra;
            return .{ .place_tokens = loc };
        }

        const DirectionNorthMajor = enum { sw, nw, n, ne, se, s };
        const src_dir = tokens.next() orelse return error.expected_direction;
        const dir_north_major = std.meta.stringToEnum(DirectionNorthMajor, src_dir) orelse return error.invalid_direction;
        const dir: Direction = @enumFromInt(@intFromEnum(dir_north_major));

        switch (kind) {
            .place_hexes => {
                const src_orient = tokens.next() orelse return error.expected_orientation;
                const orient = std.meta.stringToEnum(Orientation, src_orient) orelse return error.invalid_orientation;

                if (tokens.next()) |_| return error.unexpected_extra;

                return .{ .place_hexes = reifyTileOrientation(loc, dir, orient) };
            },
            .move_tokens => {
                const src_count = tokens.next() orelse return error.expected_count;
                const count = std.fmt.parseInt(Count, src_count, 10) catch return error.invalid_count;

                if (tokens.next()) |_| return error.unexpected_extra;
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

pub const PlaceHexesError = error{ collision, not_adjacent_to_land };
pub const PlaceTokensError = error{invalid_location};
pub const MoveTokensError = error{
    cell_is_not_stack,
    wrong_color,
    not_enough_tokens,
    collision,
    invalid_direction,
};
pub const TurnError = PlaceHexesError || PlaceTokensError || MoveTokensError;

pub const Board = struct {
    cells: ModMatrix(max_size, Cell) = .fill(.illegal),
    current_player: ?Player = @enumFromInt(0),

    /// Only written to by addHex()
    origin: Location = .{ 0, 0 },
    size: @Vector(2, u31) = .{ 0, 0 },

    fn parseLoc(src: []const u8) !Location {
        var it = std.mem.splitScalar(u8, src, ',');
        const x_src = it.next() orelse return error.missing_x_coordinate;
        const y_src = it.next() orelse return error.missing_y_coordinate;
        if (it.next() != null) return error.too_many_coordinates;
        const x = try std.fmt.parseInt(i32, x_src, 10);
        const y = try std.fmt.parseInt(i32, y_src, 10);
        return .{ x, y };
    }
    pub fn parse(src_r: []const u8) !Board {
        var board: Board = .{};
        if (src_r.len < 2) {
            // TODO possibly more validation, only allowed states are "h" and "t"
            return board;
        }

        board.current_player = @enumFromInt(@as(u1, switch (src_r[src_r.len - 1]) {
            'h' => 0,
            't' => 1,
            else => return error.invalid_ht,
        }));

        // remove unnecessary "|t" or "|h"
        const src = src_r[0 .. src_r.len - 2];
        var it = std.mem.splitScalar(u8, src, '|');

        // up to 32 hexes for the tiles
        for (0..32) |i| {
            if (it.next()) |loc_src| {
                const loc = try parseLoc(loc_src);
                board.addHex(loc);
            } else {
                if (i % 4 != 0) return error.incomplete_tile;
                // this is an assertion but i'm not writing it as one
                // TODO add validation that give ht matches logical ht
                board.current_player = @enumFromInt(@divFloor(i, 4) % 2);
                return board;
            }
        }

        // up to 32 stacks
        for (0..32) |_| {
            if (it.next()) |stack_src| {
                var it2 = std.mem.splitAny(u8, stack_src, "ht");
                const loc_src = it2.next() orelse return error.missing_loc;
                const ht_ix = (it2.index orelse return error.hissing_ht) - 1;
                const amount_src = it2.rest();
                const loc = try parseLoc(loc_src);
                const player: Player = @enumFromInt(@as(u1, switch (stack_src[ht_ix]) {
                    'h' => 0,
                    't' => 1,
                    else => return error.invalid_ht,
                }));
                const amount = try std.fmt.parseInt(Count, amount_src, 10) - 1;
                board.cells.at(loc).* = .{ .stack = .{ .color = player, .count = amount } };
            } else {
                return board;
            }
        }
        return board;
    }

    fn countHexes(board: *const Board) usize {
        var count: usize = 0;
        const w, const h = board.size;
        for (0..w) |dx| for (0..h) |dy| {
            if (board.cells.get(board.location(dx, dy)) != .illegal) count += 1;
        };
        return count;
    }

    fn countStacks(board: *const Board) usize {
        var count: usize = 0;
        const w, const h = board.size;
        for (0..w) |dx| for (0..h) |dy| {
            if (board.cells.get(board.location(dx, dy)) == .stack) count += 1;
        };
        return count;
    }

    const PArray = std.EnumArray(Player, bool);
    pub const max_size = 30;
    pub const count_players = std.meta.fields(Player).len;
    pub const max_hexes = 16 * count_players;
    pub const poi_buf_len = max_hexes;

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
        if (b.size[0] == 0) {
            b.cells.at(loc).* = .empty;
            b.origin = loc;
            b.size = .{ 1, 1 };
            return;
        }
        b.size += delta(loc, b.origin);
        b.origin -= delta(loc, b.origin);
        b.size += delta(b.origin + b.size, loc + splat(1));
        b.cells.at(loc).* = .empty;
    }

    fn elem(as: []Location, a: Location) bool {
        for (as) |x| if (std.meta.eql(a, x)) return true;
        return false;
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
                    if (b.cells.get(loc) != .empty) continue;
                    if (b.cells.get(loc + Direction.vector(.nw)) == .illegal or
                        b.cells.get(loc + Direction.vector(.ne)) == .illegal or
                        b.cells.get(loc + Direction.vector(.sw)) == .illegal or
                        b.cells.get(loc + Direction.vector(.se)) == .illegal or
                        b.cells.get(loc + Direction.vector(.w)) == .illegal or
                        b.cells.get(loc + Direction.vector(.e)) == .illegal)
                    {
                        list.appendAssumeCapacity(loc);
                    }
                };
            },
            .place_tokens => {
                // find all .empty cells on the outside;
                // find first .empty cell on board; circle island until back to start
                const w, const h = b.size;
                const start = b: for (0..h) |dy| (for (0..w) |dx| {
                    const loc = b.location(dx, dy);
                    if (b.cells.get(loc) == .empty) break :b loc;
                }) else unreachable;
                list.appendAssumeCapacity(start);

                var dir: Direction = .ne;
                var loc: Location = start;
                while (!std.meta.eql(start, b: {
                    // find next cell
                    for (0..6) |_| {
                        dir = dir.right();
                        if (b.cells.get(loc + dir.vector()) != .illegal) {
                            loc += dir.vector();
                            dir = dir.back();
                            break :b loc;
                        }
                    } else unreachable;
                })) {
                    if (b.cells.get(loc) == .empty and
                        !elem(list.items, loc))
                        list.appendAssumeCapacity(loc);
                }
            },
            .move_tokens => {
                if (b.current_player) |p|
                    findPoiMoveTokens(b, &list, p);
            },
        }
        return list.items;
    }

    fn findPoiMoveTokens(b: *const Board, list: *std.ArrayList(Location), player: Player) void {
        // find all stacks of current_player with count > "1" (> 0)
        const w, const h = b.size;
        for (0..h) |dy| for (0..w) |dx| {
            const loc = b.location(dx, dy);

            if (b.cells.get(loc) != .stack) continue;
            if (b.cells.get(loc).stack.color != player) continue;
            if (b.cells.get(loc).stack.count == 0) continue;

            if (b.cells.get(loc + Direction.vector(.nw)) != .empty and
                b.cells.get(loc + Direction.vector(.ne)) != .empty and
                b.cells.get(loc + Direction.vector(.sw)) != .empty and
                b.cells.get(loc + Direction.vector(.se)) != .empty and
                b.cells.get(loc + Direction.vector(.w)) != .empty and
                b.cells.get(loc + Direction.vector(.e)) != .empty)
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
            if (b.cells.get(loc) != .stack) continue;
            if (b.cells.get(loc).stack.color != player) continue;

            // DFS the region, counting stacks
            // this DFS is modified to ensure that the stack cannot possibly be longer than 16 items
            var count_contiguous: Score = 0;

            var loc_buf: [16]Location = undefined;
            var fba = std.heap.FixedBufferAllocator.init(ptrCast(&loc_buf));
            var stack = std.ArrayList(Location).initCapacity(fba.allocator(), 16) catch unreachable;

            stack.appendAssumeCapacity(loc);
            while (stack.pop()) |node| {
                count_contiguous += 1;
                for (0..std.meta.fields(Direction).len) |ix_dir| {
                    const next = node + Direction.vector(@enumFromInt(ix_dir));
                    if (breadcrumbs.guard(next)) continue;
                    if (b.cells.get(next) != .stack) continue;
                    if (b.cells.get(next).stack.color != player) continue;

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
        if (b.countHexes() < max_hexes) return .place_hexes;
        if (b.countStacks() < count_players) return .place_tokens;
        if (b.current_player == null) return null;
        return .move_tokens;
    }

    pub fn doTurn(b: *Board, turn: Turn) TurnError!void {
        var out_of_moves: PArray = PArray.initFill(false);
        switch (turn) {
            .place_hexes => |pl| {
                try b.placeHexes(pl[0], pl[1]);
            },
            .place_tokens => |loc| try b.placeTokens(loc),
            .move_tokens => |pl| {
                try b.moveTokens(pl[0], pl[1], pl[2]);

                inline for (std.meta.fields(Player)) |f| {
                    const player: Player = @enumFromInt(f.value);
                    var poi: [poi_buf_len]Location = undefined;
                    var fba = std.heap.FixedBufferAllocator.init(ptrCast(&poi));
                    var list = std.ArrayList(Location).initCapacity(fba.allocator(), poi_buf_len) catch unreachable;

                    findPoiMoveTokens(b, &list, player);
                    if (list.items.len == 0) out_of_moves.set(player, true);
                }
            },
        }

        if (b.current_player) |player| {
            var next: ?Player = player.next();
            defer b.current_player = next;

            for (0..count_players) |_| {
                if (!out_of_moves.get(next.?)) break;
                next = next.?.next();
            } else {
                next = null;
            }
        }
    }

    fn placeHexes(
        board: *Board,
        orig: Location,
        dir: Direction,
    ) PlaceHexesError!void {
        const v = dir.vector();
        const w = dir.right().vector();
        const loc1 = orig;
        const loc2 = orig + v;
        const loc3 = orig + w;
        const loc4 = loc2 + w;

        // detect plaing tile on top of other tile
        for ([4]Location{ loc1, loc2, loc3, loc4 }) |loc| {
            if (board.cells.get(loc) != .illegal) return error.collision;
        }

        // make sure that the tile is adjacent to land
        // this check is skipped if the tile is the first on the board
        if (board.countHexes() != 0) inline for (.{
            // all the spaces adjacent to the tile
            orig - w,                orig - v,
            orig + v - w,            orig + w - v,
            orig + splat(2) * v - w, orig + splat(2) * w - v,
            orig + splat(2) * v,     orig + splat(2) * w,
            orig + splat(2) * v + w, orig + splat(2) * w + v,
        }) |loc| {
            if (board.cells.get(loc) == .empty) break;
        } else return error.not_adjacent_to_land;

        for ([4]Location{ loc1, loc2, loc3, loc4 }) |loc| {
            board.addHex(loc);
        }
    }

    fn canPlaceHexes(
        board: *const Board,
        orig: Location,
        dir: Direction,
        ignore_floating: bool,
    ) bool {
        const v = dir.vector();
        const w = dir.right().vector();
        const loc1 = orig;
        const loc2 = orig + v;
        const loc3 = orig + w;
        const loc4 = loc2 + w;

        // detect plaing tile on top of other tile
        for ([4]Location{ loc1, loc2, loc3, loc4 }) |loc| {
            if (board.cells.get(loc) != .illegal) return false;
        }

        // make sure that the tile is adjacent to land
        // this check is skipped if the tile is the first on the board
        if (board.countHexes() != 0 or ignore_floating) inline for (.{
            // all the spaces adjacent to the tile
            orig - w,                orig - v,
            orig + v - w,            orig + w - v,
            orig + splat(2) * v - w, orig + splat(2) * w - v,
            orig + splat(2) * v,     orig + splat(2) * w,
            orig + splat(2) * v + w, orig + splat(2) * w + v,
        }) |loc| {
            if (board.cells.get(loc) == .empty) break;
        } else return false;

        return true;
    }

    fn placeTokens(b: *Board, tloc: Location) PlaceTokensError!void {
        if (b.current_player == null) return;
        const player = b.current_player.?;
        // validate loc
        block: {
            const w, const h = b.size;
            const start = b: for (0..h) |dy| (for (0..w) |dx| {
                const loc = b.location(dx, dy);
                if (b.cells.get(loc) == .empty) break :b loc;
            }) else unreachable;
            if (eql(start, tloc)) break :block;

            var dir: Direction = .ne;
            var loc: Location = start;
            while (!std.meta.eql(start, b: {
                // find next cell
                for (0..6) |_| {
                    dir = dir.right();
                    if (b.cells.get(loc + dir.vector()) != .illegal) {
                        loc += dir.vector();
                        dir = dir.back();
                        break :b loc;
                    }
                } else unreachable;
            })) {
                if (b.cells.get(loc) == .empty)
                    if (eql(loc, tloc)) break :block;
            }
            return error.invalid_location;
        }
        b.cells.at(tloc).* = .{ .stack = .{ .color = player, .count = 15 } };
    }

    // pub const MoveTokensError = error { oob, not_enough_tokens, collision, invalid_direction };
    fn moveTokens(b: *Board, start: Location, dir: Direction, count: Count) MoveTokensError!void {
        // ensure that there is a stack at `start`
        // that matches the current_player
        // and has at least count + 1 tokens
        if (b.current_player == null) return;
        const player = b.current_player.?;
        {
            const cell = b.cells.get(start);
            if (cell != .stack) return error.cell_is_not_stack;
            if (cell.stack.color != player) return error.wrong_color;
            if (cell.stack.count < count) return error.not_enough_tokens;
        }

        // find dest by moving start by dir until not .empty
        // ensure that dest != start
        const dest = b: for (1..max_size) |udist| {
            const dist: i32 = @intCast(udist);
            if (b.cells.get(start + dir.vector() * splat(dist)) == .empty) continue;
            if (dist == 1) return error.invalid_direction;
            break :b start + dir.vector() * splat(dist - 1);
        } else unreachable;

        // do the moving
        b.cells.at(start).stack.count -= count;
        b.cells.at(dest).* = .{
            .stack = .{ .color = player, .count = count - 1 },
        };
    }
};

fn ModMatrix(comptime n: comptime_int, comptime T: type) type {
    return struct {
        const Mat = @This();
        const size = n;
        data: [n][n]T,
        fn fill(value: T) Mat {
            return .{ .data = .{.{value} ** n} ** n };
        }

        fn at(b: *Mat, loc: Location) *T {
            return &b.data[@intCast(@mod(loc[0], n))][@intCast(@mod(loc[1], n))];
        }

        pub fn get(b: *const Mat, loc: Location) T {
            return @constCast(b).at(loc).*;
        }
    };
}

pub fn reifyTileOrientation(
    cell: Location,
    face: Direction,
    orientation: Orientation,
) struct { Location, Direction } {
    return switch (orientation) {
        .flat => .{
            cell + face.vector() + face.left().vector(),
            face.right(),
        },
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

    fn toInt(cell: Cell) int {
        return switch (cell) {
            .illegal => 1,
            .empty => 0,
            .stack => |s| 1 + @as(int, s.count) + @as(int, switch (s.color) {
                .red => 100,
                .blue => 200,
            }),
        };
    }

    fn fromInt(byte: int) error{illegalByte}!Cell {
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

pub fn toLoc(x: anytype, y: anytype) Location {
    return .{ @truncate(x), @truncate(y) };
}

pub const Location = @Vector(2, i32);
const Loc = Location;

fn splat(scalar: i32) Location {
    return @splat(scalar);
}

fn adjacentN(comptime N: comptime_int, orig: Location) [N * 6]Location {
    var locs: [N * 6]Location = undefined;
    inline for (0..6) |i| {
        const dir: Direction = @enumFromInt(i);
        const dir2 = dir.back().left();
        inline for (0..N) |x|
            locs[i * N + x] = orig + dir.vector() * splat(N) + dir2.vector() * splat(x);
    }
    return locs;
}

test adjacentN {
    try std.testing.expectEqual(adjacentN(3, .{ 0, 0 }), .{
        .{ 3, 0 },  .{ 2, 1 },   .{ 1, 2 },
        .{ 0, 3 },  .{ -1, 3 },  .{ -2, 3 },
        .{ -3, 3 }, .{ -3, -2 }, .{ -3, -1 },
        .{ -3, 0 }, .{ -2, -1 }, .{ -1, -2 },
        .{ 0, -3 }, .{ 1, -3 },  .{ 2, -3 },
        .{ 3, -3 }, .{ 3, -2 },  .{ 3, -1 },
    });
}

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

export fn InitBoard(b: *Board) callconv(.C) void {
    b.* = .{};
}

export fn BoardSize() callconv(.C) i32 {
    return @truncate(@sizeOf(Board));
}

export fn xAt(b: *Board, x: i32, y: i32) callconv(.C) int {
    return b.cells.at(.{ x, y }).toInt();
}

/// This function ignores errors
export fn xTuiDoTurn(board: *Board) callconv(.C) void {
    tuiDoTurn(board) catch unreachable;
}
fn tuiDoTurn(board: *Board) !void {
    const stdout = std.io.getStdOut().writer();
    const stdin = std.io.getStdIn().reader();
    var line_buf: [30]u8 = undefined;

    var poi_buf: [Board.poi_buf_len]Location = undefined;

    var maybe_err: ?anyerror = null;
    while (true) {
        const poi = board.pointsOfInterest(&poi_buf);
        if (maybe_err) |err| {
            try stdout.print("error: {s}\n", .{@errorName(err)});
            maybe_err = null;
        }

        const kind = board.nextExpectedMove() orelse unreachable;

        const hexes_count = board.countHexes();
        try stdout.print("{s}> ", .{switch (kind) {
            .place_hexes => if (hexes_count == 0)
                "first_tile_direction"
            else
                "anchor, face, orientation",
            .place_tokens => "start_location",
            .move_tokens => "stack, direction, count",
        }});
        const line = (try stdin.readUntilDelimiterOrEof(&line_buf, '\n')) orelse {
            break;
        };

        const turn = (if (hexes_count == 0)
            parseFirstTurn(line)
        else
            Turn.parse(kind, line, poi)) catch |err| {
            maybe_err = err;
            continue;
        };

        board.doTurn(turn) catch |err| {
            maybe_err = err;
            continue;
        };

        break;
    }
}

fn ixOf(as: []Location, a: Location) ?usize {
    for (as, 0..) |x, ix| if (std.meta.eql(x, a)) return ix;
    return null;
}

pub fn drawBoard(board: *const Board, poi_list: []Location, writer: anytype, color: std.io.tty.Config) !void {
    const w, const h = board.size;
    const max_row = 2 * w + h;
    for (0..max_row) |row| {
        var row_flag = false;
        for (0..(3 * h) + 1) |col| {
            const line = "/  \\__";
            const ix = ((max_row - row) * 3 + col) % 6;
            const loc = locFromTuiRowCol(board, row, col);
            switch (ix) {
                1 => {
                    if (ixOf(poi_list, loc)) |ix_poi| {
                        try color.setColor(writer, .dim);
                        try writer.writeByte(poiChar(ix_poi));
                        try color.setColor(writer, .reset);
                    } else if (board.cells.get(loc) == .empty)
                        try writer.writeByte('.')
                    else
                        try writer.writeByte(' ');
                },
                2 => {
                    switch (board.cells.get(loc)) {
                        .illegal => try writer.writeByte(' '),
                        .empty => try writer.writeByte('.'),
                        .stack => |stack| {
                            try color.setColor(writer, switch (stack.color) {
                                .red => .red,
                                .blue => .blue,
                            });
                            try writer.writeByte(switch (stack.count) {
                                0...8 => '1' + @as(u8, stack.count),
                                9...14 => 'A' + @as(u8, stack.count) - 9,
                                15 => 'X',
                            });
                            try color.setColor(writer, .reset);
                        },
                    }
                },
                0, 3...5 => {
                    const adj = switch (ix) {
                        0, 3 => locFromTuiRowCol(board, row, col -| 1),
                        4, 5 => locFromTuiRowCol(board, row + 1, col),
                        else => unreachable,
                    };

                    if (board.cells.get(loc) == .illegal and board.cells.get(adj) == .illegal) {
                        try writer.writeByte(' ');
                    } else {
                        row_flag = true;
                        if (eql(loc, .{ 0, 0 }) or eql(adj, .{ 0, 0 })) {
                            try color.setColor(writer, .yellow);
                            try writer.writeByte(line[ix]);
                            try color.setColor(writer, .reset);
                        } else {
                            try writer.writeByte(line[ix]);
                        }
                    }
                },
                else => unreachable,
            }
        }
        if (row_flag)
            try writer.writeByte('\n')
        else
            try writer.writeByte('\r');
    }
    try writer.writeByte('\n');
}

fn locFromTuiRowCol(
    board: *const Board,
    row: usize,
    col: usize,
) Location {
    const w, const h = board.size;
    const max_row = 2 * w + h - 1;
    const dy: i32 = @intCast(col / 3);
    const dx: i32 = @divFloor(@as(i32, @intCast(max_row -| row)) - dy, 2);
    return board.origin + Location{ dx, dy };
}

export fn xDrawBoard(board: *const Board) callconv(.C) void {
    const stdout = std.io.getStdOut().writer();
    const config = std.io.tty.detectConfig(std.io.getStdOut());
    var poi_buf: [Board.poi_buf_len]Location = undefined;
    const poi = board.pointsOfInterest(&poi_buf);
    drawBoard(board, poi, stdout, config) catch unreachable;
}

fn poiChar(ix: usize) u8 {
    return switch (ix) {
        0...25 => @truncate('a' + ix),
        26...51 => @truncate('A' + ix - 26),
        else => @panic("rendering poi, ix too large"),
    };
}

const int = i32;
export fn xCurrentPlayer(board: *const Board) callconv(.C) int {
    return @intCast(@intFromEnum(board.current_player orelse return -1));
}
export fn xCountTilesPlaced(board: *const Board) callconv(.C) int {
    return @intCast(@divFloor(board.countHexes(), 4));
}
export fn xWinner(b: *const Board) callconv(.C) int {
    return @intCast(@intFromEnum(b.winner() orelse return -1));
}
export fn xExpectedMoveKind(b: *const Board) callconv(.C) int {
    return @intCast(@intFromEnum(b.nextExpectedMove() orelse return -1));
}

const Check = enum { sea, coast, land };
fn maxWith(dest: *Check, src: Check) void {
    dest.* = @enumFromInt(@max(
        @intFromEnum(dest.*),
        @intFromEnum(src),
    ));
}

fn legalTileLocations(
    comptime action: enum { count, get },
    board: *const Board,
    dest: ?[*][2]int,
) switch (action) {
    .count => usize,
    .get => void,
} {
    const L = Location;
    var checks = ModMatrix(Board.max_size, Check).fill(.sea);
    const w, const h = board.size;
    for (0..h) |dy| for (0..w) |dx| {
        const loc = board.location(dx, dy);
        if (board.cells.get(loc) == .illegal) continue;

        maxWith(checks.at(loc), .land);
        for (adjacentN(1, loc)) |loc3| maxWith(checks.at(loc3), .coast);
        for (adjacentN(2, loc)) |loc2| maxWith(checks.at(loc2), .coast);
        for (adjacentN(3, loc)) |loc1| maxWith(checks.at(loc1), .coast);
    };

    var count: usize = 0;
    for (0..h + 6) |dy| for (0..w + 6) |dx| {
        const loc = board.location(dx, dy) - L{ 3, 3 };
        if (checks.get(loc) == .coast) {
            if (action == .get) {
                dest.?[count] = loc;
            }
            count += 1;
        }
    };

    return switch (action) {
        .count => count,
        .get => {},
    };
}

export fn xCountLegalTileLocations(board: *const Board) callconv(.C) int {
    return @intCast(legalTileLocations(.count, board, null));
}
export fn xGetLegalTileLocations(board: *const Board, options: [*][2]int) callconv(.C) void {
    legalTileLocations(.get, board, options);
}

export fn xPlaceTile(board: *Board, x: int, y: int, dir_i: int) callconv(.C) bool {
    const orig: Location = .{ x, y };
    const dir: Direction = @enumFromInt(dir_i);
    if (board.placeHexes(orig, dir)) |_| {} else |err| {
        return err catch false;
    }
    board.current_player = board.current_player.?.next();
    return true;
}
export fn xIsLegalTilePlacement(board: *Board, x: int, y: int, dir_i: int) callconv(.C) bool {
    const orig: Location = .{ x, y };
    const dir: Direction = @enumFromInt(dir_i);
    return board.canPlaceHexes(orig, dir, false);
}

export fn xCountLegalInitialStackLocations(board: *const Board) callconv(.C) int {
    var buf: [Board.poi_buf_len]Location = undefined;
    const locs = board.pointsOfInterest(&buf);
    return @intCast(locs.len);
}

export fn xGetLegalInitialStackLocations(board: *const Board, options: [*][2]int) callconv(.C) void {
    var buf: [Board.poi_buf_len]Location = undefined;
    const locs = board.pointsOfInterest(&buf);
    for (locs, options) |loc, *dest| {
        dest.* = .{ loc[0], loc[1] };
    }
}

export fn xPlaceInitialStack(board: *Board, x: int, y: int) callconv(.C) bool {
    return if (board.doTurn(.{
        .place_tokens = .{ x, y },
    })) |_| true else |err| err catch false;
}

fn legalTileArrangements(comptime action: enum { count, get }, b: *const Board, options: ?[*][3]int) switch (action) {
    .count => usize,
    .get => void,
} {
    var count: usize = 0;

    const w, const h = b.size;
    for (0..h + 6) |dy| for (0..w + 6) |dx| {
        const loc = b.location(dx, dy) - splat(3);
        if (b.cells.get(loc) != .illegal) continue;
        for (0..3) |diri| {
            const dir: Direction = @enumFromInt(diri);
            if (b.canPlaceHexes(loc, dir, true)) {
                if (action == .get)
                    options.?[count] = .{ loc[0], loc[1], @intCast(diri) };
                count += 1;
            }
        }
    };

    switch (action) {
        .count => return @max(count, 6),
        .get => {
            if (count == 0) for (0..6) |diri| {
                options.?[diri] = .{ 0, 0, @intCast(diri) };
            };
        },
    }
}

export fn xCountLegalTileArrangements(board: *const Board) callconv(.C) int {
    return @intCast(legalTileArrangements(.count, board, null));
}

export fn xGetLegalTileArrangements(board: *const Board, options: [*][3]int) callconv(.C) void {
    legalTileArrangements(.get, board, options);
}

export fn xCountLegalStartStacks(board: *Board) callconv(.C) int {
    var buf: [16]Location = undefined;
    var fba = std.heap.FixedBufferAllocator.init(ptrCast(&buf));
    var list = std.ArrayList(Location).initCapacity(fba.allocator(), 16) catch unreachable;
    board.findPoiMoveTokens(&list, board.current_player orelse return 0);
    return @intCast(list.items.len);
}
export fn xGetLegalStartStacks(board: *Board, coords: [*][2]int) callconv(.C) void {
    var buf: [16]Location = undefined;
    var fba = std.heap.FixedBufferAllocator.init(ptrCast(&buf));
    var list = std.ArrayList(Location).initCapacity(fba.allocator(), 16) catch unreachable;
    board.findPoiMoveTokens(&list, board.current_player orelse return);
    for (list.items, coords) |loc, *dest| {
        dest.* = .{ loc[0], loc[1] };
    }
}

export fn xCountLegalDestLocations(board: *const Board, x: int, y: int) callconv(.C) int {
    var count: int = 0;
    const start: Location = .{ x, y };
    for (0..6) |diri| {
        if (board.cells.get(start + Direction.vector(@enumFromInt(diri))) == .empty) count += 1;
    }
    return count;
}

export fn xGetLegalDestLocations(board: *const Board, x: int, y: int, coords: [*][2]int) callconv(.C) void {
    const start: Location = .{ x, y };
    var count: usize = 0;
    outer: for (0..6) |diri| {
        const dir: Direction = @enumFromInt(diri);
        const dest = b: for (1..Board.max_size) |udist| {
            const dist: i32 = @intCast(udist);
            if (board.cells.get(start + dir.vector() * splat(dist)) == .empty) continue;
            if (dist == 1) continue :outer;
            break :b start + dir.vector() * splat(dist - 1);
        } else unreachable;

        coords[count] = .{ dest[0], dest[1] };
        count += 1;
    }
}

export fn xMoveTokens(board: *Board, x1: int, y1: int, x2: int, y2: int, amt: int) callconv(.C) bool {
    return if (board.doTurn(.{
        .move_tokens = .{
            .{ x1, y1 },
            factorDirection(.{ x1, y1 }, .{ x2, y2 }) orelse return false,
            @intCast(amt),
        },
    })) |_| true else |err| err catch false;
}

export fn xGetFrame(board: *const Board, coords: *[4]int) callconv(.C) void {
    coords.* = .{
        board.origin[0],
        board.origin[1],
        (board.origin + board.size)[0],
        (board.origin + board.size)[1],
    };
}

export fn xParse(board: *Board, src: [*c]u8) callconv(.C) bool {
    board.* = Board.parse(std.mem.span(src)) catch return false;
    return true;
}

export fn xCountStacks(board: *const Board, player: int) callconv(.C) int {
    return @intCast(@min(@divFloor(board.scorePlayer(@enumFromInt((player))), 16), 16));
}

export fn xCountContiguousStacks(board: *const Board, player: int) callconv(.C) int {
    const i: u31 = @intCast(board.scorePlayer(@enumFromInt((player))));
    return switch (i % 16) {
        0 => if (i == 0) 0 else 16,
        else => |x| x,
    };
}

fn factorDirection(a: Location, b: Location) ?Direction {
    const dx = std.math.order(b[0], a[0]);
    const dy = std.math.order(b[1], a[1]);
    const ds = b - a;

    if (ds[0] != 0 and ds[1] != 0 and ds[0] != -ds[1]) return null;
    return switch (dx) {
        .eq => switch (dy) {
            .eq => null,
            .lt => .nw,
            .gt => .se,
        },
        .lt => switch (dy) {
            .eq => .w,
            .gt => .sw,
            .lt => null,
        },
        .gt => switch (dy) {
            .eq => .e,
            .gt => null,
            .lt => .ne,
        },
    };
}

fn ptrCast(ixs: []Location) []u8 {
    const bytes: [*]u8 = @ptrCast(ixs.ptr);
    return bytes[0 .. ixs.len * @sizeOf(Location)];
}

const std = @import("std");
const eql = std.meta.eql;
