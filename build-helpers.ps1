function validate-target([Parameter(ValueFromRemainingArguments)]$expectedTargets) {
    if ( ($expectedTargets -eq $null) -or (!$expectedTargets.Contains($target)) ) {

        if ($global:helpBlock) {
            &$global:helpBlock
        }

        throw "Invalid Target: $target`r`nValid Targets: $expectedTargets"
    }
}

function write-help($example, $description) {
    write-host
    write-host "$example" -foregroundcolor GREEN
    write-host "    $description"
}

function project-properties($majorMinorVersion, $buildNumber) {

    $versionPrefix = "$majorMinorVersion.$buildNumber"
    $versionSuffix = if ($buildNumber -eq "") { "dev" } else { "" }
    $copyright = $(get-copyright)

    if ($versionSuffix) {
        write-host "$product $versionPrefix-$versionSuffix"
    } else {
        write-host "$product $versionPrefix"
    }

    write-host $copyright

    regenerate-file (resolve-path "Directory.build.props") @"
<Project>
    <PropertyGroup>
        <Product>$product</Product>
        <VersionPrefix>$versionPrefix</VersionPrefix>
        <VersionSuffix>$versionSuffix</VersionSuffix>
        <Copyright>$copyright</Copyright>
        <LangVersion>latest</LangVersion>
    </PropertyGroup>
</Project>
"@
}

function get-copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $yearInitiated) { $year } else { "$yearInitiated-$year" }
    return "© $copyrightSpan $owner"
}

function regenerate-file($path, $newContent) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($newContent -ne $oldContent) {
        write-host "Generating $path"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}

function delete-directory($path) {
    if (test-path $path) {
        write-host "Deleting $path"
        rd $path -recurse -force -ErrorAction SilentlyContinue | out-null
    }
 }
 
function find-dependency($name) {
    $exes = @(gci packages -rec -filter $name)

    if ($exes.Length -ne 1) {
        throw "Expected to find 1 $name, but found $($exes.Length)."
    }

    return $exes[0].FullName
}

function connection-string($environmentVariablePrefix, $appsettingsPath) {
    $environmentVariableName = "$($environmentVariablePrefix):ConnectionStrings:Database"

    if (test-path env:$environmentVariableName) {
        return (get-item env:$environmentVariableName).Value
    }

    return (get-content $appsettingsPath | out-string | convertfrom-json).ConnectionStrings.Database
}

function update-database([Parameter(ValueFromRemainingArguments)]$environments) {
    $migrationsProject =  (get-item -path .).Name
    $roundhouseExePath = find-dependency rh.exe
    $roundhouseOutputDir = [System.IO.Path]::GetDirectoryName($roundhouseExePath) + "\output"

    $migrationScriptsPath ="Scripts"
    $roundhouseVersionFile = "bin\$configuration\$targetFramework\$migrationsProject.dll"

    foreach ($environment in $environments) {
        $connectionString = $connectionStrings[$environment]

        write-host "Executing RoundhousE for environment:" $environment

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --sqlfilesdirectory $migrationScriptsPath `
                                       --versionfile $roundhouseVersionFile `
                                       --transaction `
                                       --silent }
    }
}

function rebuild-database([Parameter(ValueFromRemainingArguments)]$environments) {
    $migrationsProject = (get-item -path .).Name
    $roundhouseExePath = find-dependency rh.exe
    $roundhouseOutputDir = [System.IO.Path]::GetDirectoryName($roundhouseExePath) + "\output"

    $migrationScriptsPath ="Scripts"
    $roundhouseVersionFile = "bin\$configuration\$targetFramework\$migrationsProject.dll"

    foreach ($environment in $environments) {
        $connectionString = $connectionStrings[$environment]

        write-host "Executing RoundhousE for environment:" $environment

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --silent `
                                       --drop }

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --sqlfilesdirectory $migrationScriptsPath `
                                       --versionfile $roundhouseVersionFile `
                                       --transaction `
                                       --silent `
                                       --simple }
    }
}

function seed-database-for-examples($environment) {
    $migrationsProject = (get-item -path .).Name
    $roundhouseExePath = find-dependency rh.exe
    $roundhouseOutputDir = [System.IO.Path]::GetDirectoryName($roundhouseExePath) + "\output"

    $migrationScriptsPath = "..\..\examples\Messaging"
	
    $roundhouseVersionFile = "bin\$configuration\$targetFramework\$migrationsProject.dll"
	
    $connectionString = $connectionStrings[$environment]
	
    execute { & $roundhouseExePath --connectionstring $connectionString `
            --commandtimeout 300 `
            --env $environment `
            --output $roundhouseOutputDir `
            --sqlfilesdirectory $migrationScriptsPath `
            --versionfile $roundhouseVersionFile `
            --transaction `
            --silent }
}

function task($heading, $command, $path) {
    write-host
    write-host $heading -fore CYAN
    execute $command $path
}

function execute($command, $path) {
    if ($path -eq $null) {
        $global:lastexitcode = 0
        & $command
    } else {
        Push-Location $path
        $global:lastexitcode = 0
        & $command
        Pop-Location
    }

    if ($lastexitcode -ne 0) {
        throw "Error executing command:$command"
    }
}

function help($helpBlock) {
    $global:helpBlock = $helpBlock
}

function main($mainBlock) {
    if ($target -eq "help") {
         if ($global:helpBlock) {
            &$global:helpBlock
            return
         }
    }

    try {
        &$mainBlock
        write-host
        write-host "Build Succeeded" -fore GREEN
        exit 0
    } catch [Exception] {
        write-host
        write-host $_.Exception.Message -fore DARKRED
        write-host
        write-host "Build Failed" -fore DARKRED
        exit 1
    }
}