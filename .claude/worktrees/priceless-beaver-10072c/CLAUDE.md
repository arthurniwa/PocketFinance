# PocketFinance — Documentação do Projeto

## Visão Geral

PocketFinance é uma aplicação web de controle financeiro pessoal desenvolvida em ASP.NET Core MVC (.NET 10). Permite que usuários registrem receitas e despesas, visualizem gráficos de gastos por categoria e gerenciem metas financeiras com acompanhamento de progresso.

O projeto foi iniciado com auxílio do Gemini e está em processo de revisão e evolução com Claude Code.

---

## Estrutura da Solução

```
PocketFinance/
├── PocketFinance.Core/          # Camada de dados: modelos, DbContext, migrations
├── PocketFinance.Web/           # Aplicação Web ASP.NET Core MVC + Razor Pages (Identity)
├── PocketFinance.API/           # Console app legada (não utilizada na Web, pode ser removida)
└── PocketFinance.slnx           # Arquivo de solução
```

---

## Stack Tecnológica

| Componente         | Tecnologia                              |
|--------------------|-----------------------------------------|
| Framework          | .NET 10 / ASP.NET Core MVC              |
| Banco de Dados     | SQLite (`financas.db`)                  |
| ORM                | Entity Framework Core 10.0.2            |
| Autenticação       | ASP.NET Core Identity                   |
| Frontend           | Bootstrap 5.3, Chart.js, jQuery         |
| Ícones             | Bootstrap Icons 1.10                    |
| Localização        | pt-BR, moeda R$                         |

---

## Banco de Dados

**Arquivo:** `PocketFinance.Web/financas.db`  
**DbContext:** `PocketFinance.Core.AppDbContext` (herda de `IdentityDbContext<IdentityUser>`)

### Entidades

#### `Transacao`
| Campo        | Tipo      | Notas                                              |
|--------------|-----------|----------------------------------------------------|
| `Id`         | int (PK)  |                                                    |
| `Descricao`  | string    | Obrigatório                                        |
| `Categoria`  | string    | Hardcoded: Alimentação, Transporte, Moradia, etc.  |
| `Valor`      | decimal   | Positivo = receita, Negativo = despesa             |
| `Data`       | DateTime  | Default: `DateTime.Now`                            |
| `Tipo`       | int       | 0 = Despesa, 1 = Receita                           |
| `UsuarioId`  | string    | FK para `AspNetUsers.Id`                           |

#### `Meta`
| Campo                | Tipo      | Notas                                    |
|----------------------|-----------|------------------------------------------|
| `Id`                 | int (PK)  |                                          |
| `Nome`               | string    | Obrigatório                              |
| `ValorMeta`          | decimal   | Valor alvo da meta                       |
| `ValorAtual`         | decimal   | Valor já depositado                      |
| `DataAlvo`           | DateTime  | Prazo para atingir a meta                |
| `UsuarioId`          | string    | FK para `AspNetUsers.Id`                 |
| `PorcentagemConcluida` | int (computed) | `(ValorAtual/ValorMeta)*100`, máx 100 |

### Migrations (9 aplicadas)
De `20260129` até `20260217`: criação inicial, categorias, Identity, isolamento por usuário, metas.

---

## Controllers & Funcionalidades

### `HomeController`
- `GET /` — Landing page (não autenticado: marketing; autenticado: hub de navegação)
- `GET /Home/Privacy`

### `TransacaoController` `[Authorize]`
- `GET  /Transacao` — Lista transações do usuário com totais (Entradas, Saídas, Saldo) e gráfico de pizza por categoria
- `GET  /Transacao/Criar` — Formulário de criação
- `POST /Transacao/Criar` — Salva nova transação
- `GET  /Transacao/Editar/{id}` — Carrega transação para edição
- `POST /Transacao/Editar/{id}` — Salva edição
- `GET  /Transacao/Deletar/{id}` — Remove transação (**⚠️ deveria ser POST/DELETE**)

### `MetaController` `[Authorize]`
- `GET  /Meta` — Lista metas do usuário com barra de progresso colorida
- `GET  /Meta/Criar` — Formulário de criação
- `POST /Meta/Criar` — Salva nova meta
- `GET  /Meta/Editar/{id}` — Carrega meta para edição
- `POST /Meta/Editar/{id}` — Salva edição
- `POST /Meta/Depositar/{id}` — Adiciona valor à meta (ação rápida via modal)
- `GET  /Meta/Deletar/{id}` — Remove meta (**⚠️ deveria ser POST/DELETE**)

---

## Identity / Autenticação

- Email + senha com hashing (ASP.NET Core Identity)
- Confirmação de e-mail **desabilitada** (`RequireConfirmedAccount = false`)
- Claim customizada: `"NomeCompleto"` adicionada no registro
- Two-factor: código existente mas desabilitado
- Páginas customizadas: Login, Register, Manage (Index, Email, ChangePassword)

---

## Frontend

### Layouts
- `_Layout.cshtml` — Sidebar offcanvas, navbar, Bootstrap CDN, formatador de moeda inline
- `_LayoutExterno.cshtml` — Layout simplificado para páginas de login/registro

### Formatação de Moeda
Script inline em `_Layout.cshtml` — classe `.money` dispara auto-formatação no estilo `1.234,56`.

### Categorias (hardcoded nas views)
`Alimentação`, `Transporte`, `Moradia`, `Lazer`, `Contas`, `Saúde`, `Educação`, `Salário`, `Extra`

---

## Configuração

**`appsettings.json`** — Contém connection string para SQL Server (morta/não usada).  
**Connection string real** — Hardcoded em `AppDbContext.OnConfiguring`: `Data Source=../PocketFinance.Web/financas.db`

**Portas locais:**
- HTTP: `localhost:5284`
- HTTPS: `localhost:7046`

---

## Problemas Conhecidos & Dívida Técnica

> Esta seção documenta os problemas identificados na revisão senior. Ver seção "Roadmap de Melhorias" para o plano de correção.

### Crítico (Segurança)
1. **IDOR em `Transacao/Editar POST`** — Não há verificação se o `Id` da transação pertence ao usuário antes de chamar `Update()`. Um usuário malicioso pode sobrescrever transações de outros usuários enviando um `Id` arbitrário.
2. **Idem em `Meta/Editar POST`** — Mesmo problema.
3. **Deletar via GET** — `Deletar` é uma ação destrutiva exposta em `GET`, vulnerável a CSRF e acesso por link/bot.

### Alto (Arquitetura)
4. **`AppDbContext` sem construtor de DI** — `OnConfiguring` hardcoda a connection string, ignorando completamente o sistema de opções do DI. O `AddDbContext` no `Program.cs` não tem efeito real.
5. **`new AppDbContext()` nos controllers** — Instanciação manual em vez de injeção via construtor. Impede testabilidade e viola DI.
6. **Connection string lida mas nunca usada** — `Program.cs` lê `AppDbContextConnection` do `appsettings.json` (que aponta para SQL Server) e nunca a utiliza.

### Médio (Qualidade de Código)
7. **Lógica de parsing de moeda duplicada** — Idêntica em `Criar` e `Editar` do `TransacaoController`. Deve ser um método privado.
8. **`Tipo` deveria ser `enum`** — `int` sem semântica explícita. Um `enum TipoTransacao { Despesa = 0, Receita = 1 }` torna o código autoexplicativo.
9. **Cálculo de entradas/saídas inconsistente** — `Index` calcula entradas por `Valor > 0` (sinal) mas o campo `Tipo` existe exatamente para isso. Pode divergir se houver bug de inserção.
10. **JS inline no layout** — O formatador de moeda deveria estar em `site.js`, não no `_Layout.cshtml`.

### Baixo (Manutenção)
11. **`appsettings.json` com connection string morta** — Gera confusão.
12. **`PocketFinance.API` legado** — Console app não utilizada. Pode ser removida ou preservada como `samples/`.
13. **Comentários desnecessários nos controllers** — `// Traz as metas ordenadas...`, `// Garante que não perde o dono...`. Bons nomes de variável eliminam a necessidade.

---

## Roadmap de Melhorias

### Fase 1 — Correções de Segurança e Arquitetura (prioridade máxima)
- [ ] Corrigir IDOR: verificar ownership antes de `Update()` em `Editar POST`
- [ ] Converter `Deletar` de GET para POST (anti-forgery token)
- [ ] Corrigir `AppDbContext`: adicionar construtor com `DbContextOptions`, mover connection string para `appsettings.json`
- [ ] Converter controllers para usar injeção de construtor (`private readonly AppDbContext _db`)
- [ ] Extrair parsing de moeda para método privado em `TransacaoController`
- [ ] Converter `Tipo` para `enum TipoTransacao`

### Fase 2 — Funcionalidades Novas
- [ ] Filtro de transações por período (mês/ano)
- [ ] Relatório mensal com comparativo mês anterior
- [ ] Exportação de extrato (CSV ou PDF)
- [ ] Limites de gastos por categoria (orçamento mensal)
- [ ] Transações recorrentes

### Fase 3 — Evolução da Arquitetura
- [ ] Adicionar camada de serviço (`ITransacaoService`, `IMetaService`)
- [ ] Mover categorias para tabela no banco ou enum centralizado
- [ ] Paginação na listagem de transações
- [ ] Testes unitários nos serviços

---

## Comandos Úteis

```bash
# Rodar a aplicação
cd PocketFinance.Web
dotnet run

# Adicionar nova migration
dotnet ef migrations add NomeDaMigration --project ../PocketFinance.Core --startup-project .

# Aplicar migrations
dotnet ef database update --project ../PocketFinance.Core --startup-project .
```
