$artifactLocation = 'artifacts\bin\Nito.Mvvm.Core\Debug\net45'
$testProjectLocation = 'test/UnitTests'
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/6e9b87eabc076c4cd68840d74a10bf9a9579cefb/Coverage.ps1'))
