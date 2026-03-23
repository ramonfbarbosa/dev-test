> **⚠️ OBS:** As credenciais do SendGrid (usadas no envio de e-mail de confirmação) foram removidas do código por segurança. Caso precisem testar essa funcionalidade e não possuam uma API Key própria do SendGrid, por favor me peçam pelo LinkedIn ou solicitem à Gisele que entre em contato comigo para que eu forneça as credenciais.

# Teste Prático para Desenvolvedores Fullstack

## Como rodar com Docker

### Pré-requisitos

- [Docker Engine](https://docs.docker.com/engine/install/) + [Docker Compose](https://docs.docker.com/compose/install/) via WSL 2, ou [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Subindo a aplicação

```bash
# Primeira vez (constrói as imagens):
docker-compose up --build

# Próximas vezes:
docker-compose up

# Para rodar em background (modo detached):
docker-compose up -d --build
```

> Se o seu Docker for recente (V2), também pode usar `docker compose` (sem hífen).

### Serviços

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Frontend** | http://localhost:3000 | Vite dev server (React) |
| **Backend** | http://localhost:8081/swagger | .NET 8 WebApi + Swagger |
| **MySQL** | localhost:3306 | Banco de dados |

> O backend espera o MySQL ficar saudável (healthcheck) antes de iniciar e executa as migrations automaticamente.

**Login:** `admin` / `admin`

### Comandos úteis

```bash
# Ver status dos containers:
docker-compose ps

# Acompanhar logs em tempo real:
docker-compose logs -f

# Logs de um serviço específico:
docker-compose logs -f backend

# Parar tudo:
docker-compose down

# Parar e remover volumes (reset do banco):
docker-compose down -v

# Rebuild de um serviço específico:
docker-compose up --build backend
```

---

## Tarefa 10 — Funcionalidades Extras Implementadas

### 📊 Dashboard Analítico (Fullstack)

Página `/dashboard` com visão geral do sistema em tempo real.

**Backend — `GET /api/dashboard`**
- Query CQRS dedicada que agrega dados via LINQ direto no `DbContext`.

**KPIs:** Total de Clientes, Clientes este Mês, Estados Distintos, Total de Usuários, Usuários Ativos, Emails Confirmados.

**Gráficos (ApexCharts):** Novos Clientes por Mês (área), Usuários por Perfil (donut), Clientes por Estado (barras horizontais).

**Tabelas:** Últimos 10 clientes e últimos 10 usuários cadastrados com dados formatados.

---

### 📄 Paginação Server-Side (Fullstack)

- `PagedList<T>` genérico no backend com `Skip`/`Take` e metadados (`TotalPages`, `TotalCount`, etc.).
- Componente `<Pagination />` no frontend com navegação numérica, seletor de tamanho de página e informação contextual.

---

### 🔀 Ordenação Dinâmica (Fullstack)

- Backend aceita `sortBy` e `sortDirection` nas queries de listagem.
- Frontend: colunas clicáveis no `DataTable` com indicador visual de direção.

---

### 📤 Exportação de Clientes em CSV (Fullstack)

- `GET /api/client/export` — Query CQRS que gera CSV com todos os campos do cliente.
- Frontend: botão "Exportar CSV" na listagem com download automático via Axios blob.
- Encoding UTF-8 com BOM para compatibilidade com Excel.

---

### 🎨 Tema Dark/Light (Frontend)

- Toggle de tema no navbar (ícone lua/sol) com `useTheme()` hook.
- Duas paletas completas (`THEME_PALETTE_LIGHT` / `THEME_PALETTE_DARK`).
- Gráficos, inputs, sidebar e background adaptam automaticamente ao tema via `usePalette()`.

---

### 🧪 Cobertura de Testes (Backend)

**48 testes unitários** cobrindo validators, handlers, services e domain de clientes e usuários.

Infraestrutura: `ClientControlContextFactory` (DbContext in-memory), `TestUserFactory`/`TestClientFactory`, `RecordingEmailProvider` (mock de emails).

---

---

### 📥 Importação de Clientes em Lote com Tracking (Fullstack)

- `POST /api/client/import` — Upload de CSV processado em background via fila (`ClientImportQueue` + `ClientImportProcessor`).
- `GET /api/client/imports` — Listagem paginada de todas as importações com status, usuário que enviou, contadores e datas.
- `GET /api/client/imports/{id}/errors` — Detalhamento dos erros linha a linha (lê `.result.json` do processamento).

**Entidade `ClientImport`:** Rastreia cada upload com status (`Pendente`, `Processando`, `Concluído`, `Concluído com erros`, `Falhou`), contadores (`TotalRows`, `ImportedRows`, `FailureCount`) e timestamps. Relacionamento FK com `User` para integridade referencial.

**Frontend:**
- Tela `/clients/imports` com DataTable paginado, ordenação por colunas e badges de status coloridos.
- Modal de erros: ao clicar nos erros de uma importação, abre modal com tabela linha × mensagem.
- Sub-item "Importações" no menu lateral sob "Clientes".

---

### 📧 Reenvio de Email de Confirmação (Fullstack)

- `POST /api/user/{id}/resend-confirmation-email` — Command CQRS que regenera o token e reenvia o email de confirmação.
- Validação: verifica se o usuário existe e se o email ainda não foi confirmado.
- Frontend: botão amarelo "Reenviar Email" na coluna de ações da listagem de usuários, visível apenas para usuários com email não confirmado.
- Diálogo de confirmação antes do envio.

---

### 🐳 Docker Compose (Infraestrutura)

- Orquestração completa com `docker-compose.yml`: MySQL 8.0, backend .NET 8 e frontend Vite/React.
- **MySQL:** Healthcheck integrado, volume persistente, script de inicialização (`init-user.sql`) para criação automática do usuário de aplicação.
- **Backend:** Multi-stage build (SDK → Runtime), migrations automáticas na inicialização, volume dedicado para importações de clientes.
- **Frontend:** Build otimizado com Dockerfile próprio, variáveis de ambiente configuráveis.
- Dependências entre serviços garantem ordem de inicialização correta (MySQL → Backend → Frontend).
- Um único `docker-compose up --build` sobe o ambiente completo pronto para uso.

---

> **⚠️ OBS:**As credenciais do SendGrid (usadas no envio de e-mail de confirmação) foram removidas do código por segurança. Caso precisem testar essa funcionalidade e não possuam uma API Key própria do SendGrid, por favor me peçam pelo LinkedIn ou solicitem à Gisele que entre em contato comigo para que eu forneça as credenciais.