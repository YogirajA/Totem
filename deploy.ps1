param($target="default",$majorminor, [int]$buildNumber)

. .\build-helpers

help {
    write-help "deploy" "updates databases instead of rebuilding them."  
}

function publish($project) {
    task "Publish $project" {
        dotnet publish --configuration $configuration --no-restore --output $publish/$project /nologo
    } src/$project
}

main {
    validate-target "default"

    $targetFramework = "netcoreapp3.1"
    $configuration = 'Release'
    $product = "Totem"
    $yearInitiated = (Get-Date).Year
    $owner = "Headspring"
    $publish = "$(resolve-path .)/publish"

    $connectionStrings = @{
        DEV = connection-string EmployeeDirectory src/Totem/appsettings.json;
    }

    task ".NET Environment" { dotnet --info }
    task "Project Properties" { project-properties $majorminor $buildNumber } src
    task "Clean" { dotnet clean --configuration $configuration /nologo } src
    task "Restore (Database Migration)" { dotnet restore --packages ./packages/ } src/Totem.DatabaseMigration
    task "Restore (Solution)" { dotnet restore } src
    task "Switch directory for NPM install" {Set-Location .\src\Totem }
    task "Npm clean install" { 
        Remove-Item  node_modules -Recurse -ErrorAction Ignore

        npm install
    }
    task "Switch directlry back to root" {Set-Location ..\.. }
    task "Build" { dotnet build --configuration $configuration --no-restore /nologo } src
 
    task "Update DEV Databases" { update-database DEV } src/Totem.DatabaseMigration

    delete-directory $publish
    publish Totem
    publish Totem.DatabaseMigration
}
