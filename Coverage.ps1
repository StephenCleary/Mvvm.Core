$artifactLocation = 'artifacts\bin\Nito.Mvvm.Core\Debug\net45'
$testProjectLocation = 'test/UnitTests'
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/9052f57a54d89656df7f63571f525d0f9a22a2ff/Coverage.ps1'))
