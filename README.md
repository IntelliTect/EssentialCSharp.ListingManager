# ListingManager
Tool used to expose useful functionality to IntelliTect/EssentialCSharp collaborators

# Installation

Run `dotnet tool install IntelliTect.EssentialCSharp.ListingManager -g`. This will install the Nupkg as a dotnet global tool.

# Update

Run `dotnet tool update -g IntelliTect.EssentialCSharp.ListingManager`. This will update the Nupkg for use globally.

# Usage

Any command can be run with these optional parameters.

- `verbose` -> provides more detail into what the command is doing

`ListingUpdating` can be run with the following additional optional parameters.

- `preview` -> leave files in place but still print actions that would take place to console

Run `ListingManager` from the command line. 

For available commands run `ListingManager -h`. This will display all the commands available to you.

To update Listings at a path provide the Chapter's path and specify the `ListingUpdating` mode.
`ListingManager -path "user/EssentialCSharp/src/Chapter03/" -mode ListingUpdating` or 
`ListingManager -path "user/EssentialCSharp/src/Chapter03/"`
NOTE: It is highly recommended that you commit and push your changes before running this command. Additionally you should 
run this command with `--preview` and `--verbose` specified to ensure there are no adverse affects. Once you are confident
that the proposed changes are what you want, you can run the command without the `--preview` modifier.

For help with the creation of missing unit tests in a chapter run 
`ListingManager -path "user/EssentialCSharp/src/Chapter03/" -mode TestGeneration`. This will find missing tests and
generate a unit test with the correct imports, filename, and namespace so you can just focus on proper assertions within
the test. From the interactive prompt `INTL>` you can provide any of the following options followed by enter.
- `d` -> deletes the previously generated test and continues
- `q` -> leaves the previously generated test as is and exits
- enter -> leaves previously genertaed test as is and continues

To find potentially mismatched listings in a chapter run, 
`ListingManager -path "user/EssentialCSharp/src/Chapter03/" -mode ScanForMismatchedListings`. Potentially mismatched listings
will be printed to the console.

# Pushing new versions


To push a new version from the command line you must first pack the changes as a nupkg by running `dotnet pack` at 
the solution level. If the pack is successfully navigate to the directory where the nupkg was created. Run 
`dotnet nuget push IntelliTect.EssentialCSharp.<version>.nupkg -k <ApiKey> -s https://api.nuget.org/v3/index.json`. For
more detailed instructions and to get an API key visit. https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package and
scroll down to the "Create API Keys" section. Replace `<version>` with the proper version number. In other words, provide the
full name of the newly generated nupkg. NOTE: The version must be higher than the version present on nuget. To change this
modify the `<version>` tag in `ListingManager/ListingManager.csproj`
