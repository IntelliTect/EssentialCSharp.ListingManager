# ListingManager [![NuGet](https://img.shields.io/nuget/v/IntelliTect.EssentialCSharp.ListingManager.svg)](https://www.nuget.org/packages/IntelliTect.EssentialCSharp.ListingManager/)
Tool used to expose useful functionality to IntelliTect/EssentialCSharp collaborators

# Installation

Run `dotnet tool install IntelliTect.EssentialCSharp.ListingManager -g`. This will install the Nupkg as a dotnet global tool.

# Update

Run `dotnet tool update -g IntelliTect.EssentialCSharp.ListingManager`. This will update the Nupkg for use globally.

# Current State

**Currently there are bugs in this program and the main usage should be the following: `ListingManager.exe --path <DirectoryOfTheChapter> --mode ListingUpdating`**

The rest will hopefully be fixed better in the future. It can be used, but may break

# Usage

Any command can be run with these optional parameters.

- `verbose` -> provides more detail into what the command is doing

`ListingUpdating` can be run with the following additional optional parameters.

- `preview` -> leave files in place but still print actions that would take place to console
- `by-folder` -> changes a listing's chapter based on the chapter number in the chapter's path
- `chapter-only` -> changes only the chapter of the listing, leaving the listing number unchanged. Use with `byfolder` 

Run `ListingManager` from the command line. 

For available commands run `ListingManager -h`. This will display all the commands available to you.

To update Listings at a path provide the Chapter's path and specify the `ListingUpdating` mode.
`ListingManager -path "user/EssentialCSharp/src/Chapter03/" -mode ListingUpdating` or 
`ListingManager -path "user/EssentialCSharp/src/Chapter03/"`
NOTE: It is highly recommended that you commit and push your changes before running this command. Additionally you should 
run this command with `--preview` and `--verbose` specified to ensure there are no adverse affects. Once you are confident
that the proposed changes are what you want, you can run the command without the `--preview` modifier.

To find potentially mismatched listings in a chapter run, 
`ListingManager -path "user/EssentialCSharp/src/Chapter03/" -mode ScanForMismatchedListings`. Potentially mismatched listings
will be printed to the console.

To run all chapters in powershell from ListingManager directory,
```
Get-ChildItem -Path 'insert.srcPathNameHere' -Directory | Where-Object {
!$_.name.EndsWith("Tests")
} | ForEach-Object {
listingmanager --path $_.FullName --preview --verbose
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
