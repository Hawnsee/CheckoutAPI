$url = "http://localhost:8080/api/checkout"
$totalRequests = 25

Write-Host "Iniciando bombardeo CONCURRENTE de $totalRequests peticiones a $url..."
Write-Host "==========================================================="

# En Windows PowerShell 5.1 es necesario cargar el ensamblado explícitamente
Add-Type -AssemblyName System.Net.Http

$httpClient = [System.Net.Http.HttpClient]::new()
$tasks = @()

# Disparamos todas las peticiones de golpe sin esperar respuesta
for ($i = 1; $i -le $totalRequests; $i++) {
    $guid = [guid]::NewGuid().ToString()
    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post, $url)
    $request.Headers.Add("Idempotency-Key", $guid)
    
    # SendAsync devuelve una Promesa (Task) y pasa a la siguiente línea inmediatamente
    $tasks += $httpClient.SendAsync($request)
}

Write-Host "¡Las $totalRequests peticiones han salido al mismo tiempo!"
Write-Host "Esperando a que el servidor devuelva los códigos de respuesta..."

# Ahora sí, bloqueamos hasta que todas hayan vuelto
[System.Threading.Tasks.Task]::WaitAll($tasks)

# Evaluamos los resultados
for ($i = 0; $i -lt $tasks.Count; $i++) {
    $task = $tasks[$i]
    $num = $i + 1
    if ($task.Status -eq 'RanToCompletion') {
        $statusCode = $task.Result.StatusCode
        Write-Host "[$num] Respuesta recibida: $statusCode"
    } else {
        Write-Host "[$num] Petición fallida (TimeOut o error de red)" -ForegroundColor Red
    }
}

$httpClient.Dispose()
Write-Host "==========================================================="
Write-Host "Bombardeo asíncrono finalizado. ¡Revisa tu Worker!"
