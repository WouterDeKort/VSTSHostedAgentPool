[CmdletBinding()]
Param(
    [parameter(Mandatory=$true)]
    [String[]]
    $VSTSToken,
    [parameter(Mandatory=$true)]
    [String[]]
    $VSTSUrl,
    $windowsLogonAccount,
    $windowsLogonPassword,
    $poolName = "Default",
    $vstsAgentPackageUri = "https://vstsagentpackage.azureedge.net/agent/2.140.2/vsts-agent-win-x64-2.140.2.zip",
    $prepareDataDisks = $true
)

$ErrorActionPreference="Stop";

If(-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator"))
{
     throw "Run command in Administrator PowerShell Prompt"
};

if(-NOT (Test-Path $env:SystemDrive\'vstsagent'))
{
    mkdir $env:SystemDrive\'vstsagent'
};

Set-Location $env:SystemDrive\'vstsagent';

for($i=1; $i -lt 100; $i++)
{
    $destFolder="A"+$i.ToString();
    if(-NOT (Test-Path ($destFolder)))
    {
        mkdir $destFolder;
        Set-Location $destFolder;
        break;
    }
};

$agentZip="$PWD\agent.zip";

$DefaultProxy=[System.Net.WebRequest]::DefaultWebProxy;
$WebClient=New-Object Net.WebClient;
$Uri=$vstsAgentPackageUri;

if($DefaultProxy -and (-not $DefaultProxy.IsBypassed($Uri)))
{
    $WebClient.Proxy = New-Object Net.WebProxy($DefaultProxy.GetProxy($Uri).OriginalString, $True);
};

$WebClient.DownloadFile($Uri, $agentZip);
Add-Type -AssemblyName System.IO.Compression.FileSystem;[System.IO.Compression.ZipFile]::ExtractToDirectory($agentZip, "$PWD");

#will default to directly attached disk, if data disk is not there
$agentWorkFolder = "D:\w"

if ($prepareDataDisks) {
    $disks = Get-Disk | Where-Object partitionstyle -eq 'raw' | Sort-Object number

    $letters = 70..89 | ForEach-Object { [char]$_ }
    $count = 0
    $label = "datadisk"

    foreach ($disk in $disks) {
        $driveLetter = $letters[$count].ToString()
        $disk | 
        Initialize-Disk -PartitionStyle MBR -PassThru |
        New-Partition -UseMaximumSize -DriveLetter $driveLetter |
        Format-Volume -FileSystem NTFS -NewFileSystemLabel $label.$count -Confirm:$false -Force
        #we have a data disk - so, we will use it :)
        $agentWorkFolder = $driveLetter + ":\w";
    $count++
    }
}

if(-NOT (Test-Path ($agentWorkFolder))) {
    mkdir $agentWorkFolder;
}

.\config.cmd --unattended `
             --url $VSTSUrl `
             --auth PAT `
             --token $VSTSToken `
             --pool $poolName `
             --agent $env:COMPUTERNAME `
             --replace `
             --runasservice `
             --work $agentWorkFolder `
             --windowsLogonAccount $windowsLogonAccount `
             --windowsLogonPassword $windowsLogonPassword

Remove-Item $agentZip;

