# Compatibility entry point; NPC folders now own conversations.
[CmdletBinding()]
param(
    [ValidateSet('help', 'new', 'list', 'check', 'play')][string]$Command = 'help',
    [string]$Id, [string]$Name, [string]$Conversation,
    [ValidateRange(80, 560)][int]$X = 180,
    [ValidateRange(170, 270)][int]$Y = 235
)
& (Join-Path $PSScriptRoot 'npc.ps1') @PSBoundParameters
