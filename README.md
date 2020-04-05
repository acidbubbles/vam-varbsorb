# Virt-A-Mate Absorb

![Build](https://github.com/acidbubbles/vam-varbsorb/workflows/Build/badge.svg) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/1b8474c95a0b4910a731c80f527d25da)](https://app.codacy.com/manual/acidbubbles/vam-varbsorb?utm_source=github.com&utm_medium=referral&utm_content=acidbubbles/vam-varbsorb&utm_campaign=Badge_Grade_Dashboard) [![codecov](https://codecov.io/gh/acidbubbles/vam-varbsorb/branch/master/graph/badge.svg)](https://codecov.io/gh/acidbubbles/vam-varbsorb) [![lgtm](https://img.shields.io/lgtm/alerts/g/acidbubbles/vam-varbsorb.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/acidbubbles/vam-varbsorb/alerts/)

Get rid of Virt-A-Mate `Custom` and `Saves` files that have been made available in a .var file.

## Usage

Download from the [Releases](https://github.com/acidbubbles/vam-varbsorb/releases), and extract somewhere on your machine.

In a command line:

```bash
> varbsorb --vam C:\Vam
```

This will scan your Virt-A-Mate `Saves` and `Custom` folders as well as the `.var` packages in your `AddonPackages` folder. When it finds a file that exists in both the AddonPackages var files and your Saves/Custom folder, it will update the scene and delete the duplicated files.

Arguments:

- `--vam`: The root path of Virt-A-Mate.
- `--include`: Limit affected files to this path prefix, relative to vam (affects files that would be deleted or updated).
- `--exclude`: Paths to skip, relative to vam (affects files that would be deleted or updated).
- `--verbose`: Print more information.
- `--warnings`: Print missing references found while scanning (usually broken scenes).
- `--noop`: Does not actually delete or update anything.
- `--log`: Log file operations to a file.

Note that if you don't specify the `--vam` argument, it will first check if it's in the same folder as `VaM.exe`. So if you have no idea how to run a command line tool, you can drop it in the Virt-A-Mate install folder and doube-click on it.

## Gotchas

This will delete files on your system. Varbsorb tries very hard to ensure it's as safe as possible, but if you have symbolic links in your Saves or Custom folder, Varbsorb will follow them. You might want to make a backup of your Virt-A-Mate Saves and Custom folders, just in case (or add them to Git).

Folders starting with a `.` such as `.git` will not be cleaned.

When multiple var version matches are found, the highest version will be selected. If multiple var packages are found, the one with the least files will be selected. The reasoning is if someone creates a var file with a look that contains morphs, textures and other things, but there's also a package that exists that only contains the textures, the latter is usually a better choice.

## Roadmap

- It would be fairly simple to find duplicates in the Saves and Custom folder and clean them up too.
- It could find any scripts, morphs or textures in the Saves folder and migrate them to the Custom folder automatically.
- Using parallelism this could be a few times faster.
- Once Virt-A-Mate provides a service for var packages, we could potentially automatically download them.

## Contributing

Pull requests welcome!

You'll need the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1). You can run with:

```bash
> dotnet run --project .\src\ -- --vam :\Vam
```

Launch settings are configured for [vscode](https://code.visualstudio.com/).

## License

[MIT](./LICENSE.md)
