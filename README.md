# Virt-A-Mate Absorb

![Build](https://github.com/acidbubbles/vam-varbsorb/workflows/Build/badge.svg) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/1b8474c95a0b4910a731c80f527d25da)](https://app.codacy.com/manual/acidbubbles/vam-varbsorb?utm_source=github.com&utm_medium=referral&utm_content=acidbubbles/vam-varbsorb&utm_campaign=Badge_Grade_Dashboard) [![codecov](https://codecov.io/gh/acidbubbles/vam-varbsorb/branch/master/graph/badge.svg)](https://codecov.io/gh/acidbubbles/vam-varbsorb) [![lgtm](https://img.shields.io/lgtm/alerts/g/acidbubbles/vam-varbsorb.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/acidbubbles/vam-varbsorb/alerts/)

Get rid of your files that are available in a .var file.

## Usage

Download from the [Releases](https://github.com/acidbubbles/vam-varbsorb/releases), and extract somewhere on your machine.

In a command line:

```bash
> varbsorb --vam C:\Vam
```

Arguments:

- `--vam`: The root path of Virt-A-Mate
- `--include`: Limit affected files to this path prefix, relative to vam (affects files that would be deleted or updated)
- `--exclude`: Paths to skip, relative to vam (affects files that would be deleted or updated)
- `--verbose`: Print more information
- `--warnings`: Print missing references found while scanning (usually broken scenes)
- `--noop`: Does not actually delete or update anything

## Gotchas

This will delete files on your system. Varbsorb tries very hard to ensure it's as safe as possible, but if you have symbolic links in your Saves or Custom folder, Varbsorb will follow them. You might want to make a backup of your Virt-A-Mate Saves and Custom folders, just in case (or add them to Git).

Folders starting with a `.` such as `.git` will not be scanned nor cleaned.

## Contributing

Pull requests welcome!

You'll need the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1). You can run with:

```bash
> dotnet run --project .\src\ -- --vam :\Vam
```

Launch settings are configured for [vscode](https://code.visualstudio.com/).

## License

[MIT](./LICENSE.md)
