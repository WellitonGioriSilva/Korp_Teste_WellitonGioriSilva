# Korp Teste - Sistema de Emissão de Notas Fiscais

Aplicação full stack desenvolvida para o teste técnico da Korp. O sistema permite cadastrar produtos, criar notas fiscais com múltiplos itens e imprimir notas fiscais com baixa assíncrona de estoque.

O projeto foi estruturado com frontend em Angular, backend em C#/.NET e arquitetura de microsserviços, utilizando PostgreSQL para persistência e RabbitMQ para comunicação assíncrona entre os serviços.

## Visão Geral da Solução

A solução é composta por:

- `frontend`: aplicação Angular para cadastro, consulta e impressão de notas fiscais.
- `backend/estoque-api`: microsserviço responsável por produtos e saldo de estoque.
- `backend/faturamento-api`: microsserviço responsável por notas fiscais e pelo fluxo de impressão.
- `backend/Shared/Contracts`: biblioteca compartilhada com os contratos dos eventos trafegados pelo RabbitMQ.
- `postgres`: banco de dados relacional com bases separadas para estoque e faturamento.
- `rabbitmq`: broker de mensagens usado para processamento assíncrono da baixa de estoque.

## Funcionalidades Implementadas

### Produtos

- Cadastro de produtos com descrição e saldo.
- Edição de produtos.
- Consulta de produto por identificador.
- Listagem de produtos com filtro por descrição.
- Validação de saldo não negativo.

No software atual, o código do produto é representado pelo `Id` gerado pela base de dados.

No backend, os produtos são persistidos na base `estoque_db`. No frontend, as telas ficam disponíveis em:

- `/produtos`
- `/produtos/novo`
- `/produtos/:id`
- `/produtos/:id/editar`

### Notas Fiscais

- Cadastro de nota fiscal com múltiplos produtos.
- Numeração sequencial gerada no banco de dados.
- Status inicial `Aberta`.
- Listagem e visualização de notas fiscais.
- Cálculo de total por item e total da nota.
- Validação de saldo antes da criação da nota fiscal.
- Impressão da nota fiscal com atualização do status para `Processando`, `Fechada` ou `Erro`.

No backend, as notas fiscais são persistidas na base `faturamento_db`. No frontend, as telas ficam disponíveis em:

- `/notas-fiscais`
- `/notas-fiscais/nova`
- `/notas-fiscais/:id`

## Fluxo de Impressão e Baixa de Estoque

O fluxo de impressão foi implementado de forma assíncrona para separar a responsabilidade de faturamento da responsabilidade de estoque.

1. O usuário clica em imprimir uma nota fiscal com status `Aberta`.
2. O frontend abre uma conexão SSE com a `faturamento-api` para receber o resultado do processamento.
3. A `faturamento-api` altera a nota para `Processando`.
4. A `faturamento-api` publica o evento `BaixaEstoqueSolicitadaEvent` no RabbitMQ.
5. A `estoque-api` consome o evento, valida os itens e baixa o saldo dos produtos.
6. Em caso de sucesso, a `estoque-api` publica `BaixaEstoqueRealizadaEvent`.
7. Em caso de falha, a `estoque-api` publica `BaixaEstoqueFalhouEvent` com o motivo.
8. A `faturamento-api` consome o resultado, atualiza a nota para `Fechada` ou `Erro` e envia o feedback ao frontend via SSE.
9. O frontend atualiza a tela e exibe uma mensagem ao usuário.

## Arquitetura de Microsserviços

O sistema possui dois microsserviços principais:

- Serviço de Estoque: expõe endpoints de produtos e executa a baixa de estoque.
- Serviço de Faturamento: expõe endpoints de notas fiscais, cria notas, solicita impressão e atualiza o status conforme o resultado da baixa.

A comunicação síncrona ocorre apenas quando a `faturamento-api` consulta a `estoque-api` para validar produtos e saldo antes da criação da nota fiscal. A baixa definitiva do estoque ocorre por mensageria, via RabbitMQ.

## Tratamento de Falhas

O tratamento de falhas foi implementado em diferentes pontos:

- Erros de domínio e validação são centralizados por `ErrorServiceException`, que converte exceções em respostas HTTP padronizadas.
- Falhas ao consultar a `estoque-api` durante a criação da nota retornam HTTP 503 com mensagem apropriada.
- Se a publicação no RabbitMQ falhar, a nota fiscal é marcada como `Erro`.
- Se a baixa de estoque falhar no processamento assíncrono, a `estoque-api` publica um evento de falha.
- A `faturamento-api` consome o evento de falha, atualiza a nota para `Erro` e envia feedback ao usuário via SSE.
- O frontend exibe mensagens de erro por meio do serviço de toast.

Exemplo de cenário de falha atendido: se o estoque não tiver saldo suficiente no momento da baixa, o evento de falha é publicado, a nota fiscal fica com status `Erro` e o usuário recebe a mensagem de erro na interface.

## Idempotência

A `estoque-api` possui controle de eventos processados por meio da entidade `EventoProcessado`.

Antes de executar a baixa, o serviço verifica se o par `EventoId` e `EventoType` já foi processado. Caso já exista, a operação não baixa o estoque novamente. Isso evita efeitos colaterais em caso de reentrega da mensagem pelo broker.

## Banco de Dados

O projeto usa PostgreSQL com duas bases:

- `estoque_db`: produtos e eventos processados.
- `faturamento_db`: notas fiscais e itens da nota fiscal.

As bases são criadas pelo script:

```text
docker/postgres/init/01-create-databases.sql
```

As migrations do Entity Framework Core são aplicadas automaticamente na inicialização de cada API por meio de `Database.Migrate()`.

## Backend

### Tecnologias e Frameworks

O backend foi implementado em C# com:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Npgsql Entity Framework Core Provider
- AutoMapper
- RabbitMQ.Client
- Swashbuckle/Swagger

Não foi utilizado Golang neste projeto; portanto, não há gerenciamento de dependências Go aplicável.

### Uso de LINQ

Foi utilizado LINQ em diversos pontos do backend, principalmente para transformação, agregação e consulta de dados:

- Filtro de produtos por descrição com `Where`.
- Agrupamento de itens de baixa por produto com `GroupBy`.
- Soma de quantidades com `Sum`.
- Projeção de itens para eventos com `Select`.
- Validação de existência de evento processado com `AnyAsync`.
- Carregamento de notas fiscais com itens usando `Include`.

Exemplo no fluxo de baixa: os itens recebidos no evento são agrupados por `ProdutoId` e suas quantidades são somadas antes da atualização do saldo.

### Endpoints Principais

Estoque API - `http://localhost:8080`

- `GET /api/Produto`
- `GET /api/Produto?descricao={termo}`
- `GET /api/Produto/{id}`
- `POST /api/Produto`
- `PUT /api/Produto/{id}`

Faturamento API - `http://localhost:8081`

- `GET /api/NotaFiscal`
- `GET /api/NotaFiscal/{id}`
- `POST /api/NotaFiscal`
- `POST /api/NotaFiscal/{id}/Impressao`
- `GET /api/NotaFiscal/{id}/stream`

Swagger:

- Estoque API: `http://localhost:8080/swagger`
- Faturamento API: `http://localhost:8081/swagger`

## Frontend Angular

### Tecnologias Utilizadas

O frontend foi implementado com:

- Angular 22
- Angular Router
- Angular Reactive Forms
- Angular HttpClient
- RxJS
- Tailwind CSS
- Flowbite

### Ciclos de Vida Angular Utilizados

Foram utilizados os ciclos:

- `ngOnInit`: usado para carregar dados iniciais das telas, como produtos, notas fiscais e detalhes por id.
- `ngOnDestroy`: usado nas telas de impressão de nota fiscal para encerrar a inscrição SSE e evitar conexões abertas quando o componente é destruído.

Componentes que usam `ngOnInit`:

- `ProdutoList`
- `ProdutoForm`
- `ProdutoView`
- `NotaFiscalList`
- `NotaFiscalForm`
- `NotaFiscalView`

Componentes que usam `ngOnDestroy`:

- `NotaFiscalList`
- `NotaFiscalView`

### Uso de RxJS

Sim, o projeto usa RxJS de forma central no frontend.

Principais usos:

- `Observable` para chamadas HTTP e conexão SSE.
- `BehaviorSubject` para estado local de tela, como carregamento, salvamento, produtos, notas e nota em impressão.
- `combineLatest` para combinar estado da lista com filtro de busca.
- `map` para transformar dados de rota, busca e totais.
- `startWith` para inicializar filtros e prévias de total.
- `debounceTime` e `distinctUntilChanged` para busca de produtos.
- `switchMap` para encadear leitura de parâmetros da rota com chamadas HTTP.
- `finalize` para desligar indicadores de carregamento/salvamento.
- `catchError` para tratar falhas mantendo a tela estável.
- `filter` e `take(1)` para aguardar apenas o resultado final da impressão.
- `Subscription` e `unsubscribe` para controlar a conexão SSE.

### Componentes Visuais

Foram utilizadas classes utilitárias do Tailwind CSS para layout, cores, espaçamento e estados visuais. A biblioteca Flowbite está instalada para suporte a componentes visuais baseados em Tailwind.

O feedback ao usuário é exibido por um serviço de toast próprio, com componente compartilhado para renderizar mensagens de sucesso e erro.

## Como Executar

Pré-requisitos:

- Docker
- Docker Compose

Subir toda a aplicação:

```bash
docker compose up --build
```

URLs:

- Frontend: `http://localhost:4200`
- Estoque API: `http://localhost:8080`
- Faturamento API: `http://localhost:8081`
- RabbitMQ Management: `http://localhost:15672`

Credenciais do RabbitMQ:

- Usuário: `admin`
- Senha: `admin123`

Para parar os containers:

```bash
docker compose down
```

Para remover também os volumes de banco e broker:

```bash
docker compose down -v
```

## Estrutura de Pastas

```text
.
├── backend
│   ├── estoque-api
│   ├── faturamento-api
│   └── Shared
│       └── Contracts
├── docker
│   └── postgres
│       └── init
├── frontend
└── docker-compose.yml
```
