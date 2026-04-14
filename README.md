# Project ATLAS
Project ATLAS is an Atmospheric, Temperature, and Liquid tile-based simulation with high level of customization, that can be used both for educational purposes and video games.

> [!WARNING]
>
> WIP
>
> This project is still under development! Not recommended for serious use. Check [Roadmap](##roadmap) to see the progress.

## Overview

Project ATLAS runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), an engine primarily developed for [Space Station 14](https://github.com/space-wizards/space-station-14). The project was inspired by atmospheric and puddle systems from a said game, but they've been rewritten from scratch to be as customizable as possible.

### Main features of this project:
- **Real physical laws** - Realistic heat conduction and gas diffusion are already implemented.
- **8-directional interaction** - tiles interact in all directions instead of just 4, preventing it from forming "star" shapes.
- **Customizable grid** - the main subgrid which stores smallest elements of the simulation can be easily scaled to be as precise as you want.
- **Multithreading for chunks** - each chunk is processed in parallel, providing improved performance.

## Quick start

1. Clone this repo:
```shell
git clone https://github.com/space-wizards/space-station-14.git
```
2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:
```shell
cd space-station-14
python RUN_THIS.py
```
3. Compile the solution:

Depending on your system, launch `Scripts/bat/buildAllRelease.bat` or `Scripts/sh/buildAllRelease.sh`.

4. Launch the compiled solution:

Depending on your system, launch `Scripts/bat/runQuickAll.bat`. or `Scripts/sh/runQuickAll.sh`.

## Roadmap

TODO

## Code & assets
Most of the code unrelated to the project's goals and all graphical assets were taken from [Space Station 14](https://github.com/space-wizards/space-station-14).

## License

All code for the content repository is licensed under the [MIT license](https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT).

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and copyright specified in the metadata file.
