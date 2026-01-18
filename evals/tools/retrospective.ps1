param(
    [Parameter(Mandatory=$true)][string]$LogPath,
    [int]$Limit = 20,
    [double]$WeightFEA = 0.35,
    [double]$WeightSC = 0.30,
    [double]$WeightRC = 0.25,
    [double]$WeightOE = 0.10
)

Write-Host "Retrospective eval on: $LogPath (limit=$Limit)"

# Collect files (JSON or NDJSON). Take most recent first.
$files = Get-ChildItem -Path $LogPath -Include *.json,*.ndjson -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First $Limit
if (-not $files) {
    Write-Error "No log files found under $LogPath"
    exit 1
}

$results = @()

function Get-PropValue {
    param(
        [psobject]$Object,
        [string]$Name,
        $Default
    )

    $prop = $Object.PSObject.Properties[$Name]
    if ($null -ne $prop) {
        return $prop.Value
    }

    return $Default
}

function Get-JsonLines([string]$path) {
    $content = Get-Content -Raw -Path $path
    if ($path.ToLower().EndsWith('.ndjson')) {
        return $content -split "`n" | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json }
    } else {
        $obj = $content | ConvertFrom-Json
        if ($obj -is [System.Collections.IEnumerable]) { return $obj } else { return @($obj) }
    }
}

foreach ($f in $files) {
    try {
        $entries = Get-JsonLines $f.FullName
    } catch {
        Write-Warning "Skip invalid JSON: $($f.FullName)"
        continue
    }

    foreach ($e in $entries) {
        # Expected fields (fallbacks to neutral if missing)
        $reqTotal   = [double](Get-PropValue -Object $e -Name 'required_fields_total'   -Default 0)
        $reqCorrect = [double](Get-PropValue -Object $e -Name 'required_fields_correct' -Default 0)
        $schemaPass = [bool]  (Get-PropValue -Object $e -Name 'schema_pass'             -Default $false)
        $schemaMinor= [bool]  (Get-PropValue -Object $e -Name 'schema_minor_violation'  -Default $false)
        $sections   = Get-PropValue -Object $e -Name 'sections_present' -Default $null  # e.g., @{ summary=$true; environment=$false; next_steps=$true }
        $closedInSlo= [bool]  (Get-PropValue -Object $e -Name 'closed_in_slo'           -Default $false)
        $reopened   = [bool]  (Get-PropValue -Object $e -Name 'reopened'                -Default $false)

        # FEA
        $fea = 0.0
        if ($reqTotal -gt 0) { $fea = [math]::Min(1.0, ($reqCorrect / $reqTotal)) }

        # SC
        $sc = if ($schemaPass) { 1.0 } elseif ($schemaMinor) { 0.5 } else { 0.0 }

        # RC (weighted coverage of sections)
        $rc = 0.0
        $wSummary = 0.3; $wEnv = 0.4; $wNext = 0.3
        if ($sections) {
            $rc = ($wSummary * ([bool]$sections.summary)) +
                  ($wEnv     * ([bool]$sections.environment)) +
                  ($wNext    * ([bool]$sections.next_steps))
        }

        # OE (outcome effectiveness)
        $oe = if ($closedInSlo -and -not $reopened) { 1.0 } elseif ($closedInSlo -or -not $reopened) { 0.5 } else { 0.0 }

        $overall = $WeightFEA*$fea + $WeightSC*$sc + $WeightRC*$rc + $WeightOE*$oe

        $results += [pscustomobject]@{
            file         = $f.Name
            fea          = [math]::Round($fea, 3)
            sc           = [math]::Round($sc, 3)
            rc           = [math]::Round($rc, 3)
            oe           = [math]::Round($oe, 3)
            overall      = [math]::Round($overall, 3)
        }
    }
}

if (-not $results) {
    Write-Error "No entries parsed from logs"
    exit 1
}

$avg = [pscustomobject]@{
    count   = $results.Count
    feaAvg  = [math]::Round((($results | Measure-Object -Property fea      -Average).Average), 3)
    scAvg   = [math]::Round((($results | Measure-Object -Property sc       -Average).Average), 3)
    rcAvg   = [math]::Round((($results | Measure-Object -Property rc       -Average).Average), 3)
    oeAvg   = [math]::Round((($results | Measure-Object -Property oe       -Average).Average), 3)
    overall = [math]::Round((($results | Measure-Object -Property overall  -Average).Average), 3)
}

Write-Host "\nPer-Issue Scores (top 10):"
$results | Select-Object -First 10 | Format-Table -AutoSize

Write-Host "\nAggregate:" -ForegroundColor Cyan
$avg | Format-List

# Emit compact JSON for pipelines
$report = [pscustomobject]@{ generatedAt = (Get-Date).ToUniversalTime().ToString("o"); limit=$Limit; weights=@{fea=$WeightFEA; sc=$WeightSC; rc=$WeightRC; oe=$WeightOE}; aggregate=$avg; sample=$results | Select-Object -First 10 }
$report | ConvertTo-Json -Depth 5
