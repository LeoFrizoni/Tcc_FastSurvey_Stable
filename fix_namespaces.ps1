# Script para corrigir todos os namespaces de FastSurvey para FASTSURVEY
$projectPath = "projeto/FastSurvey"

# Função para corrigir namespaces em um arquivo
function Fix-Namespaces {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $originalContent = $content
    
    # Corrigir namespace declarations
    $content = $content -replace 'namespace FastSurvey\.', 'namespace FASTSURVEY.'
    
    # Corrigir using statements
    $content = $content -replace 'using FastSurvey\.', 'using FASTSURVEY.'
    
    # Corrigir referências de classe base
    $content = $content -replace 'FastSurvey\.Dtos\.', 'FASTSURVEY.Dtos.'
    $content = $content -replace 'FastSurvey\.Services\.', 'FASTSURVEY.Services.'
    $content = $content -replace 'FastSurvey\.Controllers\.', 'FASTSURVEY.Controllers.'
    
    # Se o conteúdo mudou, salvar o arquivo
    if ($content -ne $originalContent) {
        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Fixed: $filePath"
    }
}

# Encontrar todos os arquivos .cs e corrigir namespaces
Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | ForEach-Object {
    Fix-Namespaces $_.FullName
}

Write-Host "Namespace correction completed!"
