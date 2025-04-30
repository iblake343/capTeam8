= Core

When first writing the game core, Andrew decided that the best way to iterate over prototypes would be to use Zig rather than C Sharp,
for several reasons:
- Zig is more familiar to him than C Sharp, so the development loop would be faster.
- Zig's support for Algebraic Data Types, including first-class tagged unions and optionals, made designing strong types for the core concepts easy. C Sharp does not have any support for general sum types except through inheritance.
- Zig's support for static analysis through strict type-checking and compile-time code execution gives Andrew more confidence in the robustness of Zig code than C Sharp code in general.
- Zig is a simpler language than C Sharp, and rewriting the core from Zig to C Sharp seemed unlikely to encounter any major design problems. Ultimately, rewriting the core to C Sharp turned out to be unnecessary anyway.

There are a number of design decisions that I, Andrew, had to make when first designing the core.
Firstly: in Battle Sheep, the board is dynamically sized, and according to the networking protocol, it should be indexable at negative indices.
I decided very quickly that I did not want runtime memory allocation in my core.
Although that may seem like a microoptimization to avoid an allocation and move,
it is general practice for Zig programmers to avoid unnecessary allocations at all costs,
and I was used to that pattern and convinced by the clean code I had yielded in the past from similar designs.
If there was to be no runtime allocation, then the size of the Board object had to be large enough to hold a full-sized board in any configuration. If we let $n$ represent the maximum width of a Battle Sheep board, then a naive design would place the initial tile at the center of a square matrix with size at least $2 n$. However, this would waste at least three fourths of the memory in practice.
Rather, I decided to use a square matrix of size $n$, and when indexing into it always use a custom index function that uses the modulus of the index.
This was so convenient that I extracted it into its own type, `ModMatrix`, the full source of which is to the right.
Then the bulk of the Board's state is wrapped up in a `cells: ModMatrix(max_size, Cell)` field.
I paired this with two vector fields, `origin` and `size`, that I together called the frame of the Board. These defined a rectangle in index space that covered all of the occupied tiles, so that I could easily iterate through the tiles of the board via

```zig
const w, const h = board.size;
for (0..w) |dx| for (0..h) |dy| {
  // Board.location(dx, dy) is just `board.origin + .{ dx, dy }`, modulo typecasting
  const loc = board.location(dx, dy);
};
```

```zig
pub fn ModMatrix(comptime n: comptime_int, comptime T: type) type {
    return struct {
        const Mat = @This();
        pub const size = n;
        data: [n][n]T,
        
        pub fn fill(value: T) Mat {
            return .{ .data = .{.{value} ** n} ** n };
        }

        pub fn at(b: *Mat, loc: Location) *T {
            return &b.data[@intCast(@mod(loc[0], n))][@intCast(@mod(loc[1], n))];
        }

        pub fn get(b: *const Mat, loc: Location) T {
            return @constCast(b).at(loc).*;
        }
    };
}
```

These `origin` and `size` vectors contain duplicate information about the board,
as it is theoretically possible to perform a search of the board and find its most extreme indices any time they are required.
However, by only changing the board shape through the `Board.addHex()` function and updating the `origin` and `size` there,
we can easily keep the duplicate information in sync with the board.
It should be noted that Zig does not have private fields or read/write accessors, so this pattern is only enforced by a comment.

The only other field of Board is `current_player`, which is also theoretically computable from the board cells.
In fact, my original prototype had a lot of duplicate information being stored in the Board type, including the total number of cells, the number of cells that had a player token in them, and a map of which players had legal moves remaining. When writing the `parse()` function for networking capability I realized that keeping track of all this information meant needing to make sure it was computed correctly during parsing, and that it was bad design in general to keep this duplicate information that could go out-of-sync with the board state. I turned most of them into functions (or eliminated them entirely) and only kept `origin`, `size`, and `current_player` because it seemed like the effort to recompute them every time I needed them was not worth the added cleanliness of removing them, and because modifications to them only happened in dedicated functions: `addHex()` for `origin` and `size`; `doTurn()` for `current_player`.


= Architecture

Board Class
Game Class
Player Interface
Display Interface

Modularity
Expandability


= Help

// not me:
= Team Assignments
= Theme
= UI Design
= AI
= Networking
= Installation
