$artifactLocation = 'artifacts\bin\Nito.Mvvm.Core\Debug\net45'
$testProjectLocation = 'test/UnitTests'
$outputLocation = 'testResults'
# iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/9052f57a54d89656df7f63571f525d0f9a22a2ff/Coverage.ps1'))

# Expected variables:
#   $artifactLocation - the relative path to the artifact output directory containing pdb files.
#   $testProjectLocation - the relative path to the project that can run "dnx test".
#   $outputLocation - the relative path where test results should be stored. This path does not have to exist.

$ErrorActionPreference = "Stop"

$libPath = (Resolve-Path $artifactLocation).Path
md -Force $outputLocation | Out-Null
$outputPath = (Resolve-Path $outputLocation).Path
$outputFile = Join-Path $outputPath -childpath 'coverage.xml'

Write-Output $libPath
Write-Output $outputPath
Write-Output $outputFile

Function Verify-OnlyOnePackage
{
	param ($name)

	$location = $env:USERPROFILE + '\.dnx\packages\' + $name
	If ((Get-ChildItem $location).Count -ne 1)
	{
		throw 'Invalid number of packages installed at ' + $location
	}
}

Verify-OnlyOnePackage 'OpenCover'
Verify-OnlyOnePackage 'coveralls.io'
Verify-OnlyOnePackage 'ReportGenerator'

pushd
Try
{
	cd $testProjectLocation

	# Execute OpenCover with a target of "dnx test"
	$command = (Get-ChildItem ($env:USERPROFILE + '\.dnx\packages\OpenCover'))[0].FullName + '\tools\OpenCover.Console.exe' + ' -register:user -target:dnx.exe "-targetargs:--lib ' + $libPath + ' test" "-output:' + $outputFile + '" -skipautoprops -returntargetcode "-excludebyattribute:System.Diagnostics.DebuggerNonUserCodeAttribute" "-filter:+[Nito*]*"'
	Write-Output $command
	iex $command

	# Either display or publish the results
	If ($env:CI -eq 'True')
	{
	    $command = (Get-ChildItem ($env:USERPROFILE + '\.dnx\packages\coveralls.io'))[0].FullName + '\tools\coveralls.net.exe' + ' --opencover "' + $outputFile + '" --full-sources'
		Write-Output $command
		iex $command
	}
	Else
	{
		$command = (Get-ChildItem ($env:USERPROFILE + '\.dnx\packages\ReportGenerator'))[0].FullName + '\tools\ReportGenerator.exe -reports:"' + $outputFile + '" -targetdir:"' + $outputPath + '"'
		Write-Output $command
		iex $command
		cd $outputPath
		./index.htm
	}
}
Finally
{
	popd
}

