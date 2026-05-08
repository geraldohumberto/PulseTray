# PulseTray

PulseTray e uma aplicacao leve para Windows que monitora consultas SQL em tempo real diretamente na bandeja do sistema.

## Funcionalidades

- Aplicativo Windows Forms com icone na System Tray.
- Configuracao de banco MySQL: host, porta, usuario, senha e database.
- Ate 5 queries `SELECT COUNT(*)`, cada uma com nome, status ativo/inativo e limite de alerta.
- Atualizacao automatica a cada 10 minutos por padrao.
- Icone muda para vermelho, toca alerta sonoro e envia notificacao do Windows quando uma query passa do limite.
- Configuracoes salvas em `%AppData%\PulseTray\settings.json`.

## Estrutura

```text
src/
  PulseTray.Core
  PulseTray.Configuration
  PulseTray.Data
  PulseTray.Notifications
  PulseTray.UI
```

## Build

Localmente, com o SDK .NET 8 instalado:

```powershell
dotnet restore PulseTray.sln
dotnet build PulseTray.sln --configuration Release
dotnet publish src/PulseTray.UI/PulseTray.UI.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

## GitHub Actions

O workflow `.github/workflows/build.yml` compila o projeto em `windows-latest`, publica o app WinForms e envia o artefato `PulseTray-win-x64`.
