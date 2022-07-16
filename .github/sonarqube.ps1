param (
  [Parameter(Mandatory=$true)]
  [string] $projectId,

  [Parameter(Mandatory=$false)]
  [string] $sqUrl = "http://localhost:9000",

  [Parameter(Mandatory=$true)]
  [string] $sqToken
)

# dotnet tool install --global dotnet-sonarscanner
# https://www.oracle.com/java/technologies/downloads/#jdk18-windows
# Add to Path :: C:\Program Files\Java\jdk-18.0.1.1\bin
# Add JAVA_HOME :: C:\Program Files\Java\jdk-18.0.1.1
$rootDir        = $PSScriptRoot;
$artifactsDir 	= [IO.Path]::GetFullPath((Join-Path $rootDir "../artifacts/"));
$coverageDir	= [IO.Path]::GetFullPath((Join-Path $artifactsDir "t1-coverage/"));
$sqReportPaths  = ($coverageDir + "**/coverage.opencover.xml");

dotnet sonarscanner begin /k:$projectId /d:sonar.host.url=$sqUrl  /d:sonar.login=$sqToken /d:sonar.cs.opencover.reportsPaths=$sqReportPaths
./ci-test.ps1
dotnet sonarscanner end /d:sonar.login=$sqToken
