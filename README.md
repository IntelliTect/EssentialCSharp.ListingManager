# EssentialCSharp.ListingManager [![NuGet](https://img.shields.io/nuget/v/IntelliTect.EssentialCSharp.ListingManager.svg)](https://www.nuget.org/packages/IntelliTect.EssentialCSharp.ListingManager/)
Tool used to expose useful functionality to IntelliTect/EssentialCSharp collaborators

# Installation

Run `dotnet tool install IntelliTect.EssentialCSharp.ListingManager -g`. This will install the Nupkg as a dotnet global tool.

# Update

Run `dotnet tool update -g IntelliTect.EssentialCSharp.ListingManager`. This will update the Nupkg for use globally.

# Usage

Run `ListingManager` from the command line. 

For available commands run `ListingManager -h`. This will display all the commands available to you.

## Update Command

The `update` command updates namespaces and filenames for all listings and accompanying tests within a chapter.

### Basic Usage

```bash
ListingManager update <directoryIn>
```

### Optional Parameters

- `--verbose` -> Displays more detailed messages in the log
- `--preview` -> Displays the changes that will be made without actually making them
- `--by-folder` -> Updates namespaces and filenames for all listings and accompanying tests within a folder
- `--single-dir` -> All listings are in a single directory and not separated into chapter and chapter test directories
- `--all-chapters` -> The passed in path is the parent directory to many chapter directories rather than a single chapter directory
- `--git` -> Use git mv for moving files instead of OS file operations. Requires the directory to be in a git repository. This preserves git history for renamed files.

### Examples

```bash
# Update listings in a chapter with preview
ListingManager update "user/EssentialCSharp/src/Chapter03/" --preview --verbose

# Update listings using git mv (preserves git history)
ListingManager update "user/EssentialCSharp/src/Chapter03/" --git --preview --verbose

# Update all chapters
ListingManager update "user/EssentialCSharp/src/" --all-chapters --preview --verbose
```

**NOTE:** It is highly recommended that you commit and push your changes before running this command. Additionally you should 
run this command with `--preview` and `--verbose` specified to ensure there are no adverse effects. Once you are confident
that the proposed changes are what you want, you can run the command without the `--preview` modifier.

## Scan Commands

### Scan for Mismatched Listings

To find potentially mismatched listings in a chapter run:
```bash
ListingManager scan listings <directoryIn>
```

### Scan for Missing Tests

To find missing tests:
```bash
ListingManager scan tests <directoryIn>
```

### Running All Chapters in PowerShell

From the ListingManager directory:
```powershell
Get-ChildItem -Path 'insert.srcPathNameHere' -Directory | Where-Object {
!$_.name.EndsWith("Tests")
} | ForEach-Object {
listingmanager update $_.FullName --preview --verbose
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
