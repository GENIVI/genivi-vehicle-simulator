
# BuildConfig should contain variables for:
#    $unity: path to unity editor executable
#    $username: username used to access remote share on IC machine
#    $password: password for the $username account
#    $ComputerName: remote IC machine name
#    $ICPATH: remote path to folder where IC should be deployed e.g. "\\MACHINE\sharename\deploy\"
#    $MainPath: local path to folder where main sim app should be deployed: e.g. "C:\deploy"
#    $ICLaunchService: ip and port where the IC launcher service is running
. "$PSScriptRoot\BuildConfig.ps1"

$MainExe = $MainPath + "\JLR_MAIN.exe"
$ICexe = $ICPath + "\JLR_CONSOLE.exe"
$ICStart = $ICLaunchService + "/start"
$ICStop = $ICLaunchService + "/stop"

function BuildAndDeploy() {

	#create a dir to build in
	New-Item TempDeploy -type directory

	Write-Host "Bulding Main App.."
	&$unity -quit -batchmode -executeMethod BuildType.BuildInternalMain | Write-Output

	Write-Host "Main App Built, Packaging.."

	#clean up any old versions
	Remove-Item $MainPath\* -recurse

	Copy-Item TempDeploy\* $MainPath -recurse

	Write-Host "Building Console.."
	&$unity -quit -batchmode -executeMethod BuildType.BuildInternalConsole | Write-Output

	Write-Host "Copying Console to remote machine.."

	#set up PSDrive for remote access
	$SecurePassword = ConvertTo-SecureString -AsPlainText $Password -Force
	$cred = New-Object -TypeName "System.Management.Automation.PSCredential" -ArgumentList $username, $SecurePassword
	New-PSDrive -Name Y -PSProvider filesystem -Root $ICPath -Credential $cred

	#clean up any old versions
	Remove-Item Y:\* -recurse

	Copy-Item TempDeploy\* Y:\ -recurse
		
	#clean up temp build dir
	Remove-Item TempDeploy/* -recurse
	Remove-Item TempDeploy
}

function RunBoth() {
	Write-Host "Starting Main Application"

	&$MainExe

	Start-Sleep -s 3

	Write-Host "Starting Console Application"
	Invoke-WebRequest -Uri $ICStart
}

function KillBoth() {
	Stop-Process -processname JLR_MAIN
	
	Invoke-WebRequest -Uri $ICStop
}

function MakeAndRun() {
	BuildAndDeploy
	RunBoth
}