param($target="default", [int]$buildNumber)

. .\build-helpers

help {
    write-help "build (default)" "Optimized for local development: updates databases instead of rebuilding them."
    write-help "build rebuild" "Builds a clean local copy, rebuilding databases instead of updating them."
    write-help "build ci 1234" "Continuous integration build, applying build counter to assembly versions."
    write-help "build migratetest" "Updates test database with test environment scripts instead of rebuilding"
    write-help "build testexample" "Tests an example app with Totem"
}

function publish($project) {
    task "Publish $project" {
        dotnet publish --configuration $configuration --no-restore --output $publish/$project /nologo
    } src/$project
}

main {
    validate-target "default" "rebuild" "ci" "migratetest" "testexample"

    $targetFramework = "netcoreapp3.1"
    $configuration = 'Release'
    $product = "Totem"
    $yearInitiated = 2020
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
    task "Switch directory for NPM install" {Set-Location .\src\Totem }
    task "Npm clean install" { 
        Remove-Item  node_modules -Recurse -ErrorAction Ignore

        npm install
    }
    task "Switch directlry back to root" {Set-Location ..\.. }
    task "Build" { dotnet build --configuration $configuration --no-restore /nologo } src
    task  "SQLLocalDB" {SqlLocalDB.exe start}

    if ($target -eq "default") {
        task "Update DEV/TEST Databases" { update-database DEV } src/Totem.DatabaseMigration
    }
    elseif ($target -eq "rebuild") {
        task "Rebuild DEV/TEST Databases" { rebuild-database DEV } src/Totem.DatabaseMigration
    }
    elseif ($target -eq "ci") {
        task "Rebuild TEST Database" { rebuild-database } src/Totem.DatabaseMigration
    }
    elseif ($target -eq "migratetest") {
        task "Update TEST Database" { update-database } src/Totem.DatabaseMigration
    }

    <# task "Test" { dotnet test --configuration $configuration --no-build } src/Totem.Tests
	
    if ($target -eq "testexample") {
        task "Seed DB with example data" { seed-database-for-examples DEV } src/Totem.DatabaseMigration
        write-host "Running Totem" -ForegroundColor Cyan
        $procId = (start 'dotnet' -verb runas -argumentlist "run -p src/Totem/Totem.csproj" -passthru).ID
        try {
            task "Run Sales Order example app" { dotnet test examples/SalesOrderApp/SalesOrderApp.csproj }
        }
        finally {
            task "Closing Totem" { start 'taskkill' -verb runas -argumentlist "/F /PID $procId /T" }
        } 
    }
    
    task "Removing variables" { gci Env: | where Name -clike 'NPM_CONFIG_*' | remove-item}

    task "Javascript Test" { npm run test --prefix src/Totem/ }
    #>

    if ($target -eq "ci") {
        delete-directory $publish
        publish Totem
        publish Totem.DatabaseMigration
    }
}
