pub fn main() !void {
    const stdout = std.io.getStdOut().writer();
    const config = std.io.tty.detectConfig(std.io.getStdOut());
    const stdin = std.io.getStdIn().reader();
    var line_buf: [30]u8 = undefined;

    // the most poi we could have are the border tiles when placing
    var poi_buf: [Board.poi_buf_len]Location = undefined;

    var board: Board = .{};

    var maybe_err: ?anyerror = null;
    while (true) {
        const poi = board.pointsOfInterest(&poi_buf);
        try drawBoard(board, poi, stdout, config);

        if (maybe_err) |err| {
            try stdout.print("error: {s}\n", .{@errorName(err)});
            maybe_err = null;
        }

        const kind = board.nextExpectedMove() orelse {
            if (board.winner()) |player| {
                try stdout.print("player {s} wins!\n", .{@tagName(player)});
            } else {
                try stdout.print("It's a tie game!\n", .{});
            }
            break;
        };
        const player = board.current_player;

        try stdout.print("{s} {s}> ", .{ @tagName(player), @tagName(kind) });
        const line = (try stdin.readUntilDelimiterOrEof(&line_buf, '\n')) orelse {
            break;
        };

        var tokens = std.mem.tokenizeAny(u8, line, " \t\r");
        if (std.ascii.eqlIgnoreCase(tokens.next() orelse "", "quit")) break;

        const turn = Turn.parse(kind, line, poi) catch |err| {
            maybe_err = err;
            continue;
        };

        if (turn != kind) {
            maybe_err = error.unexpected_move_kind;
            continue;
        }

        board.doTurn(turn) catch |err| {
            maybe_err = err;
            continue;
        };
    }
}

fn ixOf(as: []Location, a: Location) ?usize {
    for (as, 0..) |x, ix| if (std.meta.eql(x, a)) return ix;
    return null;
}

pub fn drawBoard(board: Board, poi_list: []Location, writer: anytype, color: std.io.tty.Config) !void {
    const w, const h = board.size;
    for (0..h) |dy| {
        try writer.writeByteNTimes(' ', dy);
        for (0..w) |dx| {
            const loc = board.location(dx, dy);

            switch (board.get(loc)) {
                .illegal => {
                    try color.setColor(writer, .dim);
                    if (ixOf(poi_list, loc)) |ix| {
                        try writer.writeByte(' ');
                        try writer.writeByte(poiChar(ix));
                    } else {
                        try writer.writeAll(" .");
                    }
                },
                .empty => {
                    if (ixOf(poi_list, loc)) |ix| {
                        try writer.writeByte(' ');
                        try writer.writeByte(poiChar(ix));
                    } else {
                        try writer.writeAll(" O");
                    }
                },
                .stack => |stack| {
                    if (ixOf(poi_list, loc)) |ix| {
                        try color.setColor(writer, .dim);
                        try writer.writeByte(poiChar(ix));
                    } else {
                        try writer.writeByte(' ');
                    }
                    try color.setColor(writer, switch (stack.color) {
                        .red => .red,
                        .blue => .blue,
                    });
                    try writer.writeByte(switch (stack.count) {
                        0...8 => '1' + @as(u8, stack.count),
                        9...14 => 'A' + @as(u8, stack.count) - 9,
                        15 => 'X',
                    });
                },
            }
            try color.setColor(writer, .reset);
        }
        try writer.writeByte('\n');
    }
}

fn poiChar(ix: usize) u8 {
    return switch (ix) {
        0...25 => @truncate('a' + ix),
        26...51 => @truncate('A' + ix),
        else => @panic("rendering poi, ix too large"),
    };
}

fn SliceIterator(T: type) type {
    return struct {
        ix: usize,
        slice: []T,

        fn init(slice: []T) SliceIterator(T) {
            return .{ .ix = 0, .slice = slice };
        }

        fn peek(slit: *const SliceIterator(T)) ?T {
            if (slit.ix == slit.slice.len) return null;
            return slit.slice[slit.ix];
        }

        fn next(slit: *SliceIterator(T)) ?T {
            if (slit.ix == slit.slice.len) return null;
            defer slit.ix += 1;
            return slit.slice[slit.ix];
        }

        fn advance(slit: *SliceIterator(T)) void {
            slit.ix += 1;
        }
    };
}

const std = @import("std");
const lib = @import("prototype_lib");
const Board = lib.Board;
const Location = lib.Location;
const Cell = lib.Cell;
const Turn = lib.Turn;
