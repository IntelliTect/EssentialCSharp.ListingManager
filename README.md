# EssentialCSharp.ListingManager [![NuGet](https://img.shields.io/nuget/v/IntelliTect.EssentialCSharp.ListingManager.svg)](https://www.nuget.org/packages/IntelliTect.EssentialCSharp.ListingManager/)
Tool used to expose useful functionality to IntelliTect/EssentialCSharp collaborators

# Installation

## Global Tool Installation

Run `dotnet tool install IntelliTect.EssentialCSharp.ListingManager -g`. This will install the package as a dotnet global tool.

You can verify the installation by running:
```bash
ListingManager --version
```

## Local Development Installation

For local development, you can install from the local build:
```bash
dotnet pack
dotnet tool install -g IntelliTect.EssentialCSharp.ListingManager --add-source ./ListingManager/bin/Release/
```

# Update

Run `dotnet tool update -g IntelliTect.EssentialCSharp.ListingManager`. This will update the package for use globally.

# Verification

After installation, verify that the tool is working correctly:

```bash
# Check version
ListingManager --version

# View help and available commands
ListingManager --help
```

# Usage

## Available Commands

The ListingManager provides two main commands:

1. `update` - Updates namespaces and filenames for all listings and accompanying tests within a chapter
2. `scan` - Scans for various issues (mismatched listings, missing tests)

## Common Options

Any command can be run with these optional parameters:

- `--verbose` - Provides more detailed messages in the log
- `--help` or `-h` - Shows help and usage information

## Update Command Options

The `update` command supports these additional options:

- `--preview` - Displays the changes that will be made without actually making them
- `--by-folder` - Changes a listing's chapter based on the chapter number in the chapter's path
- `--single-dir` - All listings are in a single directory and not separated into chapter and chapter test directories
- `--all-chapters` - The passed in path is the parent directory to many chapter directories rather than a single chapter directory

## Updating Listings

To update listings at a path, provide the chapter's directory path:
```bash
ListingManager update "user/EssentialCSharp/src/Chapter03/"
```

**NOTE:** It is highly recommended that you commit and push your changes before running this command. Additionally, you should 
run this command with `--preview` and `--verbose` specified to ensure there are no adverse effects. Once you are confident
that the proposed changes are what you want, you can run the command without the `--preview` modifier.

```bash
ListingManager update "user/EssentialCSharp/src/Chapter03/" --preview --verbose
```

## Scanning for Issues

To find potentially mismatched listings in a chapter:
```bash
ListingManager scan listings "user/EssentialCSharp/src/Chapter03/"
```

To find missing tests:
```bash
ListingManager scan tests "user/EssentialCSharp/src/Chapter03/"
```

## Batch Operations

To run all chapters in PowerShell:
```powershell
Get-ChildItem -Path 'insert_src_path_here' -Directory | Where-Object {
    !$_.name.EndsWith("Tests")
} | ForEach-Object {
    ListingManager update $_.FullName --preview --verbose
} 
```

# Pushing new versions

The easiest and best way is to create a new release with a tag number and version number that are identical in the format of vx.x.x and a nuget release will be created and uploaded with that same number.

If you want the more difficult method:
To push a new version from the command line you must first pack the changes as a nupkg by running `dotnet pack` at 
the solution level. If the pack is successfully navigate to the directory where the nupkg was created. Run 
`dotnet nuget push IntelliTect.EssentialCSharp.<version>.nupkg -k <ApiKey> -s https://api.nuget.org/v3/index.json`. For
more detailed instructions and to get an API key visit. https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package and
scroll down to the "Create API Keys" section. Replace `<version>` with the proper version number. In other words, provide the
full name of the newly generated nupkg. NOTE: The version must be higher than the version present on nuget. To change this
modify the `<version>` tag in `ListingManager/ListingManager.csproj`
