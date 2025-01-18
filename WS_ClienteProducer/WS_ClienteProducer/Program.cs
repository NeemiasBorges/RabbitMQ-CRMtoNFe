using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using WS_ClienteProducer.Services.RabbitMQ;
using WS_ClienteProducer.Services.RabbitMQ.Interface;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .CreateLogger();

builder.Services.AddSingleton<IClientFactory, ClientFactory>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "WS_ClienteProducer",
            serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"))
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("WS_ClienteProducer"));

builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/metrics-dashboard", async context =>
{
    var html = @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Dashboard de Metricas</title>
            <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap' rel='stylesheet'>
            <style>
                body { 
                    font-family: 'Inter', sans-serif;
                    margin: 0;
                    padding: 20px;
                    background-color: #f5f5f5;
                }
                .dashboard-container {
                    max-width: 1200px;
                    margin: 0 auto;
                }
                h1 {
                    color: #2d3748;
                    font-size: 24px;
                    margin-bottom: 30px;
                }
                .metrics-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
                    gap: 20px;
                    margin-bottom: 30px;
                }
                .metric-card {
                    background: white;
                    border-radius: 10px;
                    padding: 20px;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }
                .metric-card h2 {
                    color: #4a5568;
                    font-size: 18px;
                    margin-top: 0;
                    margin-bottom: 20px;
                }
                .metric-value {
                    font-size: 24px;
                    font-weight: 600;
                    color: #2d3748;
                    margin-bottom: 10px;
                }
                .charts-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
                    gap: 20px;
                }
                .chart-container {
                    background: white;
                    border-radius: 10px;
                    padding: 20px;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                    height: 300px;
                    position: relative;
                }
                .chart-container h2 {
                    color: #4a5568;
                    font-size: 18px;
                    margin-top: 0;
                    margin-bottom: 20px;
                }
                @media (max-width: 768px) {
                    .charts-grid {
                        grid-template-columns: 1fr;
                    }
                }
            </style>
        </head>
        <body>
            <div class='dashboard-container'>
                <h1>Dashboard de Metricas</h1>
                
                <div class='metrics-grid'>
                    <div class='metric-card'>
                        <h2>Uso de Memoria</h2>
                        <div id='memoryValue' class='metric-value'>--</div>
                    </div>
                    <div class='metric-card'>
                        <h2>Mensagens Processadas</h2>
                        <div id='messagesValue' class='metric-value'>--</div>
                    </div>
                    <div class='metric-card'>
                        <h2>Tempo Online</h2>
                        <div id='uptimeValue' class='metric-value'>--</div>
                    </div>
                </div>

                <div class='charts-grid'>
                    <div class='chart-container'>
                        <h2>Uso de Memoria (ultimos 5 minutos)</h2>
                        <canvas id='memoryChart'></canvas>
                    </div>
                    <div class='chart-container'>
                        <h2>Mensagens por Minuto</h2>
                        <canvas id='messagesChart'></canvas>
                    </div>
                </div>
            </div>

            <script>
                let memoryChart, messagesChart;
                const maxDataPoints = 30;
                const memoryData = Array(maxDataPoints).fill(null);
                const messagesData = Array(maxDataPoints).fill(null);
                const timeLabels = Array(maxDataPoints).fill('');
                let updateInterval;

                function throttle(func, limit) {
                    let inThrottle;
                    return function(...args) {
                        if (!inThrottle) {
                            func.apply(this, args);
                            inThrottle = true;
                            setTimeout(() => inThrottle = false, limit);
                        }
                    }
                }

                function initCharts() {
                    const chartConfig = {
                        type: 'line',
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                y: {
                                    beginAtZero: true,
                                    ticks: {
                                        callback: function(value) {
                                            return value.toFixed(1);
                                        }
                                    }
                                },
                                x: {
                                    ticks: {
                                        maxRotation: 45,
                                        minRotation: 45
                                    }
                                }
                            },
                            animation: {
                                duration: 0
                            },
                            plugins: {
                                legend: {
                                    display: false
                                }
                            }
                        }
                    };

                    memoryChart = new Chart(
                        document.getElementById('memoryChart'),
                        {
                            ...chartConfig,
                            data: {
                                labels: timeLabels,
                                datasets: [{
                                    data: memoryData,
                                    borderColor: '#4299e1',
                                    borderWidth: 2,
                                    fill: false,
                                    tension: 0.1
                                }]
                            }
                        }
                    );

                    messagesChart = new Chart(
                        document.getElementById('messagesChart'),
                        {
                            ...chartConfig,
                            data: {
                                labels: timeLabels,
                                datasets: [{
                                    data: messagesData,
                                    borderColor: '#48bb78',
                                    borderWidth: 2,
                                    fill: false,
                                    tension: 0.1
                                }]
                            }
                        }
                    );
                }

                function updateCharts(data) {
                    try {
                        const now = new Date().toLocaleTimeString();
                        
                        document.getElementById('memoryValue').textContent = `${data.memoryUsage.toFixed(2)} MB`;
                        document.getElementById('messagesValue').textContent = `${data.messagesProcessed} msgs`;
                        document.getElementById('uptimeValue').textContent = `${Math.floor(data.uptime)} min`;

                        timeLabels.shift();
                        memoryData.shift();
                        messagesData.shift();

                        timeLabels.push(now);
                        memoryData.push(data.memoryUsage);
                        messagesData.push(data.messagesProcessed);

                        if (memoryChart && messagesChart) {
                            memoryChart.update('none');
                            messagesChart.update('none');
                        }
                    } catch (error) {
                        console.error('Erro ao atualizar gráficos:', error);
                        clearInterval(updateInterval);
                    }
                }

                const throttledUpdate = throttle(updateCharts, 1000);

                window.addEventListener('load', () => {
                    initCharts();
                    
                    updateInterval = setInterval(async () => {
                        try {
                            const response = await fetch('/metrics-api');
                            if (!response.ok) throw new Error('Falha na resposta da API');
                            const data = await response.json();
                            throttledUpdate(data);
                        } catch (error) {
                            console.error('Erro ao atualizar metricas:', error);
                            clearInterval(updateInterval);
                        }
                    }, 10000);

                    // Primeira atualização imediata
                    fetch('/metrics-api')
                        .then(response => {
                            if (!response.ok) throw new Error('Falha na resposta da API');
                            return response.json();
                        })
                        .then(data => throttledUpdate(data))
                        .catch(error => console.error('Erro na primeira atualização:', error));
                });

                window.addEventListener('beforeunload', () => {
                    if (updateInterval) {
                        clearInterval(updateInterval);
                    }
                });
            </script>
        </body>
        </html>";

    context.Response.Headers.Add("Content-Type", "text/html");
    await context.Response.WriteAsync(html);
});

app.MapGet("/metrics-api", () =>
{
    var process = Process.GetCurrentProcess();
    return Results.Json(new
    {
        memoryUsage = process.WorkingSet64 / 1024.0 / 1024.0, 
        messagesProcessed = 23,
        uptime = (DateTime.Now - process.StartTime).TotalMinutes,
        timestamp = DateTime.Now
    });
});

app.MapHealthChecks("/health");

try
{
    Log.Information("Iniciando o WS_ClienteProducer");
    app.Run("http://localhost:5000");
}
catch (Exception ex)
{
    Log.Fatal(ex, "O WS_ClienteProducer falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}