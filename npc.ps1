<#
.SYNOPSIS
Create, find, validate and preview text conversations. Inky is not required.
.EXAMPLE
./npc.ps1 new -Id mechanic -Name "Mechanic" -X 180 -Y 235
.EXAMPLE
./npc.ps1 play -Id mechanic
#>
[CmdletBinding()]
param(
    [ValidateSet('help', 'new', 'list', 'check', 'play')][string]$Command = 'help',
    [string]$Id,
    [string]$Name,
    [string]$Conversation,
    [ValidateRange(80, 560)][int]$X = 180,
    [ValidateRange(170, 270)][int]$Y = 235
)
$ErrorActionPreference = 'Stop'
$contentDirectory = Join-Path $PSScriptRoot 'PawnShop.Game/Content/Npcs'

if ($Command -eq 'help') {
    @'
Text conversations: edit in VS Code, then build normally. No Inky/export step.

  ./npc.ps1 new -Id mechanic -Name "Mechanic" -X 180 -Y 235
      Creates a mechanic folder with NPC JSON, sprites, conversation JSON and Ink, then places it in the shop.
  ./npc.ps1 list
      Lists NPC IDs, display names, and source files.
  ./npc.ps1 check
      Builds and checks every registered conversation, including alternate branches.
  ./npc.ps1 play -Id mechanic
      Builds and previews that NPC's conversation in this terminal (no game window).

In game: click the NPC, or use Left/Right to select one and Tab to talk.
Start here: PawnShop.Game/Content/Npcs/README.md
'@
    return
}

if ($Command -eq 'new') {
    if (-not $Id) { $Id = Read-Host 'NPC ID (lowercase, e.g. mechanic)' }
    if ($Id -cnotmatch '^[a-z][a-z0-9_]*$') { throw 'ID must start with a lowercase letter and contain only lowercase letters, digits and underscores.' }
    if (-not $Name) { $Name = Read-Host 'Display name' }
    if ([string]::IsNullOrWhiteSpace($Name) -or $Name.Length -gt 24) { throw 'Display name must be 1-24 characters.' }
    foreach ($existing in Get-ChildItem -LiteralPath $contentDirectory -Filter 'npc.json' -Recurse) {
        $binding = Get-Content -Raw -LiteralPath $existing.FullName | ConvertFrom-Json
        if ($binding.id -ceq $Id) { throw "NPC ID '$Id' already exists in $($existing.Name). Edit its Ink file instead." }
    }
    $folder = Join-Path $contentDirectory $Id
    if (Test-Path -LiteralPath $folder) { throw "NPC folder '$Id' already exists; nothing was overwritten." }
    $placementPath = Join-Path $PSScriptRoot 'PawnShop.Game/Content/World/shop.npcs.json'
    $placementText = [IO.File]::ReadAllText($placementPath)
    $placement = $placementText | ConvertFrom-Json
    if ($placement.version -ne 1 -or $null -eq $placement.spawns) { throw 'Expected shop placement version 1 and spawns array.' }
    if (@($placement.spawns | Where-Object { $_.npcId -ceq $Id }).Count) { throw "NPC '$Id' is already placed." }
    $inkText = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'tools/dialogue/npc.ink.template')).Replace('__NPC_ID__', $Id)
    $npcText = [ordered]@{
        '$schema' = '../npc.schema.json'; version = 1; id = $Id; name = $Name
        sprite = @{ texture = 'sprites/body.png'; metadata = 'sprites/body.json'; idleAnimation = 'Idle' }
        interactionBounds = @{ x = -18; y = -40; width = 36; height = 58 }
        conversations = 'conversations.json'
    } | ConvertTo-Json -Depth 4
    $conversationText = [ordered]@{
        '$schema' = '../conversations.schema.json'; version = 1; default = 'greeting'
        entries = @{ greeting = 'dialogue/greeting.ink' }
    } | ConvertTo-Json -Depth 3
    # All inputs are read before creating files. Existing NPC folders are never overwritten.
    New-Item -ItemType Directory -Path $folder -ErrorAction Stop | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $folder 'dialogue'), (Join-Path $folder 'sprites') | Out-Null
    [IO.File]::WriteAllText((Join-Path $folder 'npc.json'), $npcText)
    [IO.File]::WriteAllText((Join-Path $folder 'conversations.json'), $conversationText)
    [IO.File]::WriteAllText((Join-Path $folder 'dialogue/greeting.ink'), $inkText)
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'tools/npcs/body.png'), (Join-Path $PSScriptRoot 'tools/npcs/body.json') -Destination (Join-Path $folder 'sprites')
    if ([IO.File]::ReadAllText($placementPath) -cne $placementText) { throw "Shop placement changed during creation. NPC files created; add '$Id' to shop.npcs.json manually." }
    $placement.spawns = @($placement.spawns) + @([ordered]@{ npcId = $Id; x = $X; y = $Y })
    $temporary = $placementPath + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
    try {
        [IO.File]::WriteAllText($temporary, ($placement | ConvertTo-Json -Depth 5))
        [IO.File]::Move($temporary, $placementPath, $true)
    } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary } }
    Write-Output "Created $folder and placed $Name in the shop."
    Write-Output "Edit npc.json, conversations.json, dialogue/greeting.ink and sprites/ in VS Code."
    Write-Output "Text preview: ./npc.ps1 play -Id $Id"
    return
}

if ($Command -eq 'play' -and -not $Id) { $Id = Read-Host 'NPC ID to preview (use list to find IDs)' }
Push-Location $PSScriptRoot
try {
    dotnet build PawnShop.Tests/PawnShop.Tests.csproj --nologo -v:quiet -m:1 -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Build failed. Fix the reported content errors before previewing.' }
    $arguments = @('run', '--project', 'PawnShop.Tests/PawnShop.Tests.csproj', '--no-build', '--', '--dialogue', $Command)
    if ($Command -eq 'play') { $arguments += $Id; if ($Conversation) { $arguments += $Conversation } }
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Dialogue command failed. See the diagnostic above.' }
} finally { Pop-Location }

