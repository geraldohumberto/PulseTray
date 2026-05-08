# PulseTray

## Objetivo do Projeto

Criar um aplicativo leve para Windows que fique na bandeja do sistema (System Tray), próximo aos ícones de:

- rede
- volume
- apps em segundo plano
- bateria

O aplicativo deve monitorar consultas SQL em tempo real e exibir pequenos números diretamente na bandeja do Windows.

---

# Funcionalidades

## Monitoramento em Tempo Real

- Atualização automática a cada 10 minutos
- Consulta diretamente o banco de dados
- Exibição de números em tempo real
- Aplicação extremamente leve

---

# Regras de Exibição

Cada query retorna apenas:

```sql
SELECT COUNT(*)
```

O resultado será exibido na bandeja do Windows.

---

# Alertas

Quando um número ultrapassar:

```text
15
```

O sistema deve:

- mudar a cor para vermelho
- emitir alerta sonoro
- emitir notificação do Windows

---

# Estrutura da Configuração

## Aba 1 — Banco de Dados

Tela de configuração contendo:

- Host
- Porta
- Usuário
- Senha
- Database
- Botão Testar Conexão

---

## Aba 2 — Queries

Tela contendo até 5 queries.

Cada query possui:

- checkbox habilitar/desabilitar
- nome da query
- campo SQL
- limite de alerta personalizado

Exemplo:

| Ativa | Nome | Query | Limite |
|---|---|---|---|
| ✅ | Novos | SELECT COUNT(*) ... | 15 |

---

# Regras das Queries

- Se somente 1 query estiver ativa:
  - mostra apenas 1 número

- Se 5 queries estiverem ativas:
  - mostra 5 números

Cada query deve possuir:

- cor independente
- alerta independente
- notificação independente

---

# Tecnologia Escolhida

## C# + .NET Windows Forms

Motivos:

- integração nativa com Windows
- suporte ideal para System Tray
- menor consumo de memória
- notificações nativas
- alerta sonoro simples
- fácil gerar `.exe`
- melhor estabilidade que Python para este cenário

---

# Nome do Projeto

## Nome principal escolhido

# PulseTray

Significado:

- Pulse = monitoramento em tempo real
- Tray = bandeja do sistema do Windows

Ou seja:

```text
Monitoramento em tempo real na bandeja do Windows
```

---

# Possíveis nomes alternativos

## Profissionais

- QueryPulse
- MonitorTray
- TrayMetrics
- SignalTray
- OpsPulse
- DBPulse

---

# Estrutura futura do projeto

```text
PulseTray/
│
├── PulseTray.Core
├── PulseTray.UI
├── PulseTray.Data
├── PulseTray.Notifications
├── PulseTray.Configuration
└── PulseTray.sln
```

---

# Ideias Futuras

## Futuras melhorias

- múltiplos bancos
- histórico de métricas
- gráfico rápido ao clicar
- exportar logs
- modo dark/light
- websocket em tempo real
- integração Discord/Slack
- monitoramento de filas
- múltiplas bandejas
- ícones customizados

---

# Query atual identificada

A lógica encontrada no dashboard original foi:

```sql
SELECT COUNT(*) AS total_novos
FROM main_score
WHERE empresa = 'ATN'
  AND LOWER(status_atual) = 'novo'
  AND (
        DATE(data_recebimento) = CURDATE()
        OR DATE(data_update) = CURDATE()
      );
```

---

# Regra encontrada no código original

O dashboard original:

- remove `ATN_BOT`
- filtra `SITUACAO = "novo"`
- calcula total de registros novos

Arquivos analisados:

- db_funcs.py
- app.py
- visao_geral.py

---

# Objetivo Visual

Aplicação mínima e discreta.

Poucos centímetros de tamanho.

Sem dashboard pesado.

Somente:

- números
- alertas
- bandeja
- configuração simples

---

# Continuação no VSCode

Objetivo:

Continuar desenvolvimento diretamente pelo VSCode com Codex/ChatGPT integrado.

