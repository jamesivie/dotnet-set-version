# dotnet-set-version
[dotnet-set-version](https://github.com/jamesivie/dotnet-set-version) sets version numbers into project files and AssemblyInfo files.

## Usage:
`dotnet-set-version <version_number> (alt_version_string)`

## Global Installation

`dotnet tool install --global dotnet-set-version --version <tool_version>`

## Local Installation

`dotnet new tool-manifest # if you are setting up this repo`
`dotnet tool install --local dotnet-set-version --version <tool_version>`

## GitHub Action Sample Usage
```
      - name: Install dotnet-set-version
        run: dotnet tool install --global dotnet-set-version
      - name: Run dotnet-set-version
        run: dotnet-set-version 0.1.${{ github.run_number }} ${{ github.sha }} -NoPackageOutputPath
```
