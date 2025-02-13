pub fn main() !void {
    const stdout = std.io.getStdOut().writer();
    const config = std.io.tty.detectConfig(std.io.getStdOut());
    const stdin = std.io.getStdIn().reader();
    var line_buf: [30]u8 = undefined;

    // the most poi we could have are the border tiles when placing
    var poi_buf: [Board.poi_buf_len]Index = undefined;

    var board = Board.empty;

    var maybe_err: ?anyerror = null;
    while (true) {
        const poi = board.pointsOfInterest(&poi_buf);
        try drawBoard(board, poi, stdout, config);

        if (maybe_err) |err| {
            try stdout.print("error: {s}\n", .{@errorName(err)});
            maybe_err = null;
        }

        const kind = board.nextExpectedMove();
        const player = board.current_player;

        try stdout.print("{s} {s}> ", .{ @tagName(player), @tagName(kind) });
        const line = (try stdin.readUntilDelimiterOrEof(&line_buf, '\n')) orelse {
            break;
        };

        var tokens = std.mem.tokenizeAny(u8, line, " \t\r");
        if (std.ascii.eqlIgnoreCase(tokens.next() orelse "", "quit")) break;

        const turn = Turn.parse(kind, line, poi, &board) catch |err| {
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

pub fn drawBoard(board: Board, poi_list: []Index, writer: anytype, color: std.io.tty.Config) !void {
    for (0..Board.size) |y| {
        try writer.writeByteNTimes(' ', y);
        for (0..Board.size) |x| {
            const loc = board.locFromUnsignedCoords(x, y);
            const ix = board.indexOfLoc(loc) catch unreachable;

            switch (try board.get(loc)) {
                .illegal => {
                    try color.setColor(writer, .dim);
                    if (std.mem.indexOfScalar(Index, poi_list, ix)) |pix| {
                        try writer.writeByte(' ');
                        try writer.writeByte(poiChar(pix));
                    } else {
                        try writer.writeAll(" .");
                    }
                },
                .empty => {
                    if (std.mem.indexOfScalar(Index, poi_list, ix)) |pix| {
                        try writer.writeByte(' ');
                        try writer.writeByte(poiChar(pix));
                    } else {
                        try writer.writeAll(" O");
                    }
                },
                .stack => |stack| {
                    if (std.mem.indexOfScalar(Index, poi_list, ix)) |pix| {
                        try color.setColor(writer, .dim);
                        try writer.writeByte(poiChar(pix));
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
const Index = Board.Index;
const Location = lib.Location;
const Cell = lib.Cell;
const Turn = lib.Turn;
