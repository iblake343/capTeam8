
cd ../zig
Write-Host "zig build"
zig build --prominent-compile-errors
If ($?) {
    # Write-Host "zig build ok"
} Else {
    # Write-Host "zig build error"
    cd ../dotnet
    exit
}

cp zig-out/bin/core.dll ../dotnet
cd ../dotnet
Write-Host "dotnet build"
dotnet build
