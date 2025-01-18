# 🐰 RabbitMQ-CRM-Worker

Um serviço Worker em .NET que gerencia o processamento assíncrono de mensagens para integração com CRM e NFe, utilizando RabbitMQ como message broker. Implementa padrões de resiliência, telemetria e monitoramento.

## 📋 Descrição

Sistema de processamento assíncrono que consome mensagens de filas RabbitMQ para integração com CRM e NFe. Utiliza padrões modernos de desenvolvimento como Circuit Breaker, OpenTelemetry e Health Checks.

## 🚀 Começando

### Pré-requisitos

- .NET 9.0
- RabbitMQ Server 3.12 ou superior
- Docker
- Visual Studio 2022 ou VS Code

### 🔧 Instalação

1. Clone o repositório

```bash
git clone https://github.com/NeemiasBorges/RabbitMQ-CRMtoNFe.git
```

2. Restaure os pacotes NuGet

```bash
dotnet restore
```

3. Instalar Docker

- Se ainda não tiver o Docker instalado, você pode baixá-lo e instalá-lo através do site oficial: Docker Download.

4. Rodar RabbitMQ com Docker

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

5. Acessar o Painel de Administração do RabbitMQ

- URL: http://localhost:15672
- Usuário: guest
- Senha: guest

6. Configure os appsettings.json

6.1 Producer:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "RabbitMQ": {
    "QueueCrm": "fila.crm",
    "QueueNfe": "fila.nfe",
    "RoutingKeyCrm": "rk.crm",
    "RoutingKeyNfe": "rk.nfe"
  }
}
```

6.2 Consumer CRM:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "RabbitMQ": {
    "Queue": "fila.crm",
    "RoutingKey": "crm"
  }
}
```

6.3 Consumer CRM:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "RabbitMQ": {
    "Queue": "fila.nfe",
    "RoutingKey": "nfe"
  }
}
```

## ⚙️ Configurações

### Dependências NuGet

```xml
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.7.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="7.0.0" />
```

### Estrutura do Projeto

```
├── WS_ClienteProducer/
│   ├── Connected Services/
│   ├── Dependências/
│   ├── Properties/
│   ├── DTO/
│   │   └── CostumerDTO.cs
│   ├── logs/
│   ├── Services/
│   │   └── RabbitMQ/
│   ├── appsettings.json
│   ├── Program.cs
│   └── Worker.cs
├── WS_Consumer_CRM/
│   ├── Connected Services/
│   ├── Dependências/
│   ├── Properties/
│   ├── DTO/
│   │   └── CostumerDTO.cs
│   ├── appsettings.json
│   ├── Program.cs
│   └── Worker.cs
└── WS_Consumer_NFE/
    ├── Connected Services/
    ├── Dependências/
    ├── Properties/
    ├── DTO/
    │   └── CostumerDTO.cs
    ├── appsettings.json
    ├── Program.cs
    └── Worker.cs
```

### 🔄 Fluxo de Mensagens

![Fluxo de Mensagens](https://i.imgur.com/r4NW0X5.gif)

O diagrama acima ilustra o fluxo de mensagens em nossa arquitetura:

1. O Producer publica mensagens em filas específicas
2. O Consumer CRM processa mensagens relacionadas ao CRM
3. O Consumer NFE processa mensagens relacionadas a Notas Fiscais

### Estrutura de Filas

- `fila.crm`: Processa mensagens relacionadas ao CRM
- `fila.nfe`: Processa mensagens relacionadas a Notas Fiscais

![Filas](https://i.imgur.com/40WIfcv.png)

## 📊 Monitoramento e Telemetria

### Logs e Métricas

O serviço utiliza uma combinação de Serilog e OpenTelemetry para fornecer visibilidade operacional:

#### Logs (Serilog)

- Localização: `/logs/worker-{data}.log`
- Formato: Logs estruturados com informações de:
  - Ambiente
  - Nome da máquina
  - Nível de log (Information, Warning, Error, etc.)
  - Timestamp
  - Mensagem detalhada

#### Métricas (OpenTelemetry)

Métricas básicas do runtime são coletadas e disponibilizadas, incluindo:

- Uso de memória
- Tempo de execução
- Eventos do runtime .NET
- Taxa de processamento de mensagens
- Latência de processamento
- Erros por minuto

### Endpoints de Monitoramento

1. **Dashboard de Métricas**

   ```
   http://localhost:5000/metrics-dashboard
   ```

   ![Filas](https://i.imgur.com/UaaXsXh.png)

   - Interface visual com gráficos em tempo real
   - Métricas de uso de memória
   - Contadores de mensagens processadas
   - Tempo de atividade do serviço

2. **API de Métricas**

   ```
   http://localhost:5000/metrics-api
   ```

   ![Filas](https://i.imgur.com/FMvwE3X.png)

   - Dados brutos em formato JSON
   - Ideal para integração com outras ferramentas

3. **Documentação API**

   ```
   http://localhost:5000/swagger
   ```

   - Documentação interativa Swagger
   - Teste de endpoints
   - Descrição dos recursos disponíveis

4. **Health Check**

   ```
   http://localhost:5000/health
   ```

   ![Filas](https://i.imgur.com/jKQyHVl.png)

   - Status de saúde do serviço
   - Verificações de dependências

### Resiliência

Implementa:

- Circuit Breaker
- Retry Policies
- Tratamento de falhas

## 🛠️ Desenvolvimento

### Build

```bash
dotnet build
```

## 📦 Deploy

### Como Serviço Windows

```powershell
sc.exe create "RabbitMQ CRM Worker" binpath="caminho-do-exe"
```

### Como Serviço Linux

```bash
sudo systemctl enable rabbitmq-crm-worker
sudo systemctl start rabbitmq-crm-worker
```

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👥 Autor

<div align="center">
  <a href="https://github.com/NeemiasBorges" target="_blank">
    <img src="https://avatars.githubusercontent.com/u/51499704?v=4" width="115" alt="Foto de perfil de Neemias Borges">
  </a>
  <br>
  <a href="https://github.com/NeemiasBorges">
    <img src="https://img.shields.io/badge/Neemias%20Borges-F6C953?style=for-the-badge&logo=phoenixframework&logoColor=%23FD4F00" alt="Badge Neemias Borges">
  </a>
  <a href="https://www.linkedin.com/in/neemias-borges/">
    <img src="https://img.shields.io/badge/LinkedIn-Neemias%20Borges-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn Neemias Borges">
  </a>
</div>

## Licença 📄

Licença MIT

## Contato 📧

neemiasb.dev@gmail.com
