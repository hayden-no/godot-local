param([Parameter(Mandatory = $true)][string]$PlatformName, [Parameter(Mandatory = $true)][string]$Architecture, [bool]$DevMode = $true, [bool]$Mono = $true, [bool]$RealIsDouble = $true, [string]$Postfix)



class Results {
    [System.Collections.Generic.List[Ignored]] $Ignored
    [System.Collections.Generic.List[Warning]] $Warnings
    [System.Collections.Generic.List[string]] $Generated
    [bool] $Success

    Results(){
        $this.Ignored = [System.Collections.Generic.List[Ignored]]::new()
        $this.Warnings = [System.Collections.Generic.List[Warning]]::new()
        $this.Generated = [System.Collections.Generic.List[String]]::new()
        $this.Success = $false
    }

    Results([array]$arr){
        $this.Ignored = [System.Collections.Generic.List[Ignored]]::new()
        $this.Warnings = [System.Collections.Generic.List[Warning]]::new()
        $this.Generated = [System.Collections.Generic.List[String]]::new()
        $this.Success = $false

        $genRegex = [Regex]"Generating (.*\.cs)"
        $warnRegex = [Regex]"WARNING\: (\w+?) '(\w+?)' (.*)\."
        $ignoreRegex = [Regex]"Ignoring type '(.*)' because (.*)"

        $successRegex = [Regex]"The Godot API sources were successfully generated"

        foreach ($match in $genRegex.Matches($arr)){
            $this.Generated.Add($match.Groups[0].Value)
        }

        foreach ($match in $warnRegex.Matches($arr)){
            $toAdd = [Warning]::new()
            $toAdd.Kind = $match.Groups[0].Value
            $toAdd.Name = $match.Groups[1].Value
            $toAdd.Reason = $match.Groups[2].Value
            $this.Warnings.Add($toAdd)
        }

        foreach ($match in $ignoreRegex.Matches($arr)){
            $toAdd = [Ignored]::new()
            $toAdd.Type = $match.Groups[0].Value
            $toAdd.Reason = $match.Groups[1].Value
            $this.Ignored.Add($toAdd)
        }

        $this.Success = $successRegex.Matches($arr).Count -gt 0
    }

    [string] ToString(){
        return "Generated: {0:N0} | Ignored: {1:N0} | Warnings: {2:N0} | Success: {3}" -f $this.Generated.Count, $this.Ignored.Count, $this.Warnings.Count, $this.Success
    }
}

class Warning {
    [string] $Kind
    [string] $Name
    [string] $Reason
}

class Ignored {
    [string] $Type
    [string] $Reason
}

if ($Mono -eq $false){
    throw "Mono is disabled.  Mono glue cannot be generated!"
}

[string] $GodotPath = [string]::Join(".", "bin/godot", "$PlatformName", "editor", $DevMode ? "dev" : "", $RealIsDouble ? "double" : "", "$Architecture", "$Postfix", $Mono ? "mono" : "", "exe");

if ([System.Io.File]::Exists("$GodotPath") -eq $false){
    throw "FileNotFound: No file found at '$GodotPath'."
}

Write-Host "Starting mono glue generation..." -ForegroundColor Cyan
Write-Host "    using '$($GodotPath.TrimStart("bin/"))'" -ForegroundColor Blue
$sw = [System.Diagnostics.Stopwatch]::StartNew();

$output = .$GodotPath --headless --generate-mono-glue modules/mono/glue 2>&1 | Out-String
$result = [Results]::new($output)

$sw.Stop();
$time = "{0:00}:{1:00}.{2:000}" -f $sw.Elapsed.Minutes, $sw.Elapsed.Seconds, $sw.Elapsed.Milliseconds
Write-Host "Finished in $time" -ForegroundColor Cyan
Write-Host "$result" -ForegroundColor Magenta
return $result
