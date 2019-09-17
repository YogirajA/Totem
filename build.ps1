param($target="default", [int]$buildNumber)

. .\build-helpers

help {
    write-help "build (default)" "Optimized for local development: updates databases instead of rebuilding them."
    write-help "build rebuild" "Builds a clean local copy, rebuilding databases instead of updating them."
    write-help "build ci 1234" "Continuous integration build, applying build counter to assembly versions."
    write-help "build migratetest" "Updates test database with test environment scripts instead of rebuilding"
}

function publish($project) {
    task "Publish $project" {
        dotnet publish --configuration $configuration --no-restore --output $publish/$project /nologo
    } src/$project
}

main {
    validate-target "default" "rebuild" "ci" "migratetest"

    $targetFramework = "netcoreapp2.1"
    $configuration = 'Release'
    $product = "Totem"
    $yearInitiated = 2019
    $owner = "Headspring"
    $publish = "$(resolve-path .)/publish"

    $connectionStrings = @{
        DEV = connection-string EmployeeDirectory src/Totem/appsettings.json;
        TEST = connection-string EmployeeDirectory src/Totem.Tests/appsettings.json
    }

    task ".NET Environment" { dotnet --info }
    task "Project Properties" { project-properties "1.0" $buildNumber } src
    task "Clean" { dotnet clean --configuration $configuration /nologo } src
    task "Restore (Database Migration)" { dotnet restore --packages ./packages/ } src/Totem.DatabaseMigration
    task "Restore (Solution)" { dotnet restore } src
    task "Build" { dotnet build --configuration $configuration --no-restore /nologo } src

    if ($target -eq "default") {
        task "Update DEV/TEST Databases" { update-database DEV TEST } src/Totem.DatabaseMigration
    } elseif ($target -eq "rebuild") {
        task "Rebuild DEV/TEST Databases" { rebuild-database DEV TEST } src/Totem.DatabaseMigration
    } elseif ($target -eq "ci") {
        task "Rebuild TEST Database" { rebuild-database TEST } src/Totem.DatabaseMigration
    } elseif ($target -eq "migratetest") {
      task "Update TEST Database" { update-database TEST } src/Totem.DatabaseMigration
    }

    task "Test" { dotnet test --configuration $configuration --no-build } src/Totem.Tests

    if ($target -eq "ci") {
        delete-directory $publish
        publish Totem
        publish Totem.DatabaseMigration
    }
}