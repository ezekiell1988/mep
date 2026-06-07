#Requires -Version 7.0
<#
.SYNOPSIS
    Ejemplo — gpt-5.5 : LLM General (Chat Completion)
.NOTES
    Credenciales: leer de src/demo/credentials/ai-foundry.json
    Estructura: { "azureOpenAIEndpoint": "https://{resource}.openai.azure.com/openai/v1",
                  "apiKey": "...",
                  "models": { "llm": "gpt-5.5" } }
#>

# ── Cargar credenciales (ajustar ruta según contexto) ───────────────────────
# $creds = Get-Content 'credentials\ai-foundry.json' -Raw | ConvertFrom-Json

$model   = '<gpt-5.5>'
$uri     = '<https://{resource}.openai.azure.com/openai/v1>/chat/completions'
$headers = @{ 'api-key' = '<API_KEY>'; 'Content-Type' = 'application/json' }

$body = @{
    model    = $model
    messages = @(
        @{ role = 'system'; content = 'Eres un asistente experto en certificaciones Microsoft.' }
        @{ role = 'user';   content = '¿Cuáles son los 3 temas más importantes del examen AZ-900?' }
    )
    # IMPORTANTE — gpt-5.5 es modelo de razonamiento:
    #   - Usar max_completion_tokens, NO max_tokens
    #   - NO enviar temperature
    #   - Mínimo 2000 tokens o la respuesta puede truncarse
    max_completion_tokens = 2000
} | ConvertTo-Json -Depth 10

$result = Invoke-RestMethod -Uri $uri -Method POST -Headers $headers -Body $body

Write-Host $result.choices[0].message.content
Write-Host "Tokens usados: $($result.usage.total_tokens)"
