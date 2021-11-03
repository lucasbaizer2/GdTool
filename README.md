# GdTool
GdTool is a standalone commannd line tool for reverse engineering games made with the Godot engine.
It has both a compiler and a decompiler for all release GdScript versions (before GdScript 2.0, which is currently unreleased).
GdTool has no dependencies on the Godot engine and works as a lightweight toolset written with .NET 5.0.

# Installing
Get the newest version of GdTool from the [Releases](https://github.com/lucasbaizer2/GdTool/releases) page.

# Usage

Godot games are packaged as an executable file alongside a proprietary "PCK" file.
The executable contains the engine, and the PCK file contains all of the game data. including GdScript code.

To start reverse engineering a game, first find the GdScript version the game was compiled with:
```
> GdTool detect -i "path-to-game-executable"
```

The output will contain a GdScript bytecode version (below is an example for an arbitrary game):
```
Bytecode version hash: 5565f55 (5565f5591f1096870327d893f8539eff22d17e68)
3.2.0 release (5565f55 / 2019-08-26 / Bytecode version: 13) - added `ord` function
```

Now that the bytecode version is known, the PCK file can be extracted and the code decompiled with:
```
> GdTool decode -i "path-to-game-pck-file" -b 5565f55 -d
                                            # ^ hash of bytecode version
```
The bytecode version flag can be ommitted if there is no interest in decompiling (the raw bytecode will be extracted instead).

To rebuild a decoded PCK directory:
```
> GdTool build -i "path-to-decoded-directory" -b 5565f55
```
If there is no interest in recompiling code for the rebuild PCK (i.e. when it was decoded the bytecode information was ommitted), the bytecode version can similarly be left out when recompiling as well. 

# License
GdTool is licensed under the [MIT License](https://github.com/lucasbaizer2/GdTool/blob/master/LICENSE).

# Special Thanks
The large majority of information about how the Godot engine works came from [gdsdecomp](https://github.com/bruvzg/gdsdecomp).
The work done for this repository could not be done without the work done from gdsdecomp, and a special thanks goes out to all its contributors.
