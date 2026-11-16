# Registro de Uso de IA — AquaSense (Global Solution 2026.2)

Este arquivo registra a conversa com a IA (Claude) usada como apoio no desenvolvimento da API. A IA foi usada para sugerir estrutura, revisar código e gerar testes. As decisões de escopo, a revisão do código e a execução (`dotnet run` e `dotnet test`) foram feitas pelo grupo.

- **Ferramenta:** Claude (Anthropic)
- **Disciplinas:** SOA e C#
- **Projeto:** AquaSense — API REST em C# / .NET 8 com SQLite

---

## Conversa 1 — Contexto e estrutura do projeto

**👤 Usuário**

> Estou fazendo a Global Solution da FIAP (SOA e C#). Nosso grupo criou uma "CitizenTech" chamada AquaSense: um sensor de nível de água (ESP32) envia leituras para uma API, e um app Expo consulta. Eu sou responsável pela API em C#.
>
> Requisitos do professor: POO com interface, injeção de dependência, Web API REST com DTOs e validação, EF Core + LINQ, async/await e testes unitários.
>
> Já criei a solution com `dotnet new sln` e dois projetos: `src/AquaSense.Api` (webapi .NET 8) e `tests/AquaSense.Tests` (xUnit). Ainda não escrevi nada de regra de negócio. Me sugere uma estrutura de pastas e quais entidades eu preciso, sem exagerar. É projeto acadêmico, quero algo simples e que eu consiga explicar na apresentação.

**🤖 Claude**

> Sugestão enxuta, suficiente para cobrir todos os requisitos:
>
> **Entidades**
> - `Local`: ponto monitorado (`Id`, `Nome`, `Latitude`, `Longitude`) com lista de leituras.
> - `Leitura`: medição (`Id`, `LocalId`, `NivelAgua` em cm, `DataHora` em UTC).
> - `NivelRisco`: enum `Normal`, `Atencao`, `Critico`.
>
> **Estrutura**
> ```
> src/AquaSense.Api
>   Controllers/  → endpoints REST (finos)
>   Services/     → regras de aplicação (async + LINQ)
>   Models/       → entidades, enum, interface e classificador (POO)
>   Dtos/         → entrada e saída, com validação
>   Data/         → DbContext (EF Core) e seed
> ```
>
> **Como cada requisito é atendido**
> - *Interface + POO:* `IClassificadorNivel` com a implementação `ClassificadorPorLimiar`. Assim, a regra pode ser trocada depois (por exemplo, por um modelo de ML) sem mexer no resto.
> - *Injeção de dependência:* registrar classificador e services no `Program.cs`.
> - *LINQ:* um endpoint de resumo por hora com `GroupBy`, `Average`, `Max` e `Count`.
> - *Async:* todos os métodos de service e controller com `CancellationToken`.
>
> Um cuidado de privacidade: como o `Local` é um ponto de monitoramento, guarde só coordenadas aproximadas e nunca um endereço residencial. Vale deixar isso explícito no README.

---

## Conversa 2 — Modelos, classificador e banco

**👤 Usuário**

> Fechado, vou seguir essa estrutura. Não precisa escrever todas as classes: me dá o exemplo de uma, a `Local`, bem comentada, pra eu ver o padrão e fazer as outras (`Leitura`, o enum `NivelRisco`, a interface `IClassificadorNivel` e o `ClassificadorPorLimiar`) por conta própria.

**🤖 Claude**

> Segue a `Local` como modelo.
>
> ```csharp
> namespace AquaSense.Api.Models;
>
> /// <summary>Ponto monitorado (ex.: um trecho de rua com histórico de alagamento).</summary>
> public class Local
> {
>     public int Id { get; set; }
>     public string Nome { get; set; } = string.Empty;
>
>     // Coordenadas aproximadas do ponto monitorado (nunca endereço residencial).
>     public double Latitude { get; set; }
>     public double Longitude { get; set; }
>
>     // Lado "1" da relação 1:N: um local tem várias leituras.
>     public List<Leitura> Leituras { get; set; } = new();
> }
> ```
>
> O padrão para as demais:
> - `Id` como `int`, que o EF Core reconhece como chave primária por convenção.
> - Texto não anulável inicia com `string.Empty`, para não gerar warning com `Nullable` habilitado.
> - Coleções iniciam vazias (`new()`), evitando `NullReferenceException`.
> - Na `Leitura`, o lado "N" leva `LocalId` e a navegação `Local?`, que é anulável porque nem sempre é carregada.
> - Comentário `///` em cada classe e nos campos com unidade. O `NivelAgua` fica em centímetros, e o `DataHora` em UTC.
>
> Dicas para o que você vai escrever: o enum são só os três valores (`Normal`, `Atencao`, `Critico`), sem acento, para evitar problemas de serialização. A interface precisa de um único método, `NivelRisco Classificar(double nivelAguaCm)`. No classificador, use constantes públicas para os limites, para os testes reaproveitarem.

**👤 Usuário**

> Fiz o resto seguindo o exemplo. No classificador usei: abaixo de 10 cm = Normal, de 10 a 24,99 = Atenção, 25 ou mais = Crítico, e lancei `ArgumentOutOfRangeException` para valor negativo. No `Leitura` coloquei um método `Classificar` que recebe o classificador por parâmetro. Pode revisar?

**🤖 Claude**

> A lógica está correta. Pontos da revisão:
>
> - A ordem das condições (checar `>= 25` primeiro, depois `>= 10`) está certa. Se invertesse, todo valor crítico cairia em Atenção.
> - Validar o valor negativo **antes** da classificação foi uma boa decisão, porque falha cedo e com mensagem clara.
> - O método `Leitura.Classificar(IClassificadorNivel)` aplica o padrão Strategy. A leitura não conhece a regra, só usa a que recebe. Assim, no futuro, dá para trocar por um classificador de ML sem alterar a `Leitura`.
> - Sugestão: registre o classificador como `Singleton` no `Program.cs`, já que ele não guarda estado.
>
> Nos testes, vale cobrir exatamente os limites 10 e 25, que é onde costuma ocorrer o erro de `>` versus `>=`.

**👤 Usuário**

> Agora o `AppDbContext` com SQLite. Quero relação 1:N (um local tem várias leituras), nome obrigatório com no máximo 100 caracteres, e um seeder que crie o banco sozinho na primeira execução com 1 local e umas 10 leituras SIMULADAS, só pra eu mostrar no Swagger. Ainda não quero mexer com migrations.

**🤖 Claude**

> - `AppDbContext` com `DbSet<Local>` e `DbSet<Leitura>`, e no `OnModelCreating`: `Nome` com `IsRequired().HasMaxLength(100)` e `HasMany(...).WithOne(...).HasForeignKey(l => l.LocalId)` com `Cascade`.
> - `DbSeeder.InicializarAsync` chama `EnsureCreatedAsync()` (ou `MigrateAsync()` caso existam migrations no futuro) e só insere dados se a tabela `Locais` estiver vazia, para não duplicar a cada execução.
> - Para o seed, usei um local provisório em São Paulo ("Ponto Piloto (provisório)"), já que o ponto real ainda não foi definido, e 10 leituras de 20 em 20 minutos, com os valores `2, 4, 6, 9, 12, 18, 27, 22, 14, 8`. Assim, a demonstração passa pelos três níveis (Normal, Atenção e Crítico).
> - Marque no comentário que são dados **simulados**, para ninguém confundir com dado real.
>
> Connection string no `appsettings.json`: `Data Source=aquasense.db`.

---

## Conversa 3 — DTOs, services e controllers

**👤 Usuário**

> Agora a parte de API. Preciso destes endpoints:
> - `GET /locais`, `GET /locais/{id}`, `POST /locais`
> - `GET /locais/{id}/leituras` (mais recentes primeiro)
> - `GET /locais/{id}/resumo-por-hora`
> - `POST /leituras` (é o que o ESP32 vai chamar)
>
> Não quero devolver a entidade direto. Cria DTOs de entrada e saída com validação por Data Annotations. No `POST /leituras` o `DataHora` tem que ser opcional (se o ESP32 não mandar, usa a hora do servidor). E na resposta da leitura quero um campo `nivel` já com a classificação.

**🤖 Claude**

> **DTOs**
> - `CriarLocalRequest`: `Nome` `[Required, StringLength(100, MinimumLength = 3)]`, `Latitude` `[Range(-90, 90)]`, `Longitude` `[Range(-180, 180)]`.
> - `CriarLeituraRequest`: `LocalId` `[Range(1, int.MaxValue)]`, `NivelAgua` `[Range(0, 1000)]`, `DataHora` como `DateTime?`.
> - `LocalResponse`, `LeituraResponse` (com `Nivel` como `string`) e `ResumoHoraResponse` (`Hora`, `NivelMedio`, `NivelMaximo`, `Quantidade`).
>
> **Services** (`ILocalService`/`LocalService` e `ILeituraService`/`LeituraService`), todos assíncronos e com `CancellationToken`:
> - Leituras usam `AsNoTracking()`.
> - `RegistrarAsync` devolve `null` quando o local não existe, e o controller converte isso em `404`.
> - `DataHora = request.DataHora?.ToUniversalTime() ?? DateTime.UtcNow`.
>
> **Controllers** finos, com `[ApiController]`, `[ProducesResponseType]` para `200/201/400/404` e comentários XML que aparecem no Swagger. `POST /locais` usa `CreatedAtAction(nameof(Obter), ...)`.
>
> Detalhe: com `[ApiController]`, a validação de Data Annotations já devolve `400` automaticamente, sem `if (!ModelState.IsValid)`.

**👤 Usuário**

> Rodei e o Swagger mostrou o `nivel` como número (0, 1, 2) em alguns lugares. Eu queria que aparecesse o nome (Normal, Atencao, Critico).

**🤖 Claude**

> No `LeituraResponse`, o `Nivel` já é `string` (`l.Classificar(_classificador).ToString()`), então ali sai o nome. Se o número apareceu, é em algum ponto em que o enum `NivelRisco` é serializado direto. Para padronizar, registre o conversor no `Program.cs`:
>
> ```csharp
> builder.Services
>     .AddControllers()
>     .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
> ```
>
> Assim, qualquer enum exposto pela API sai como texto.

---

## Conversa 4 — Resumo por hora (LINQ)

**👤 Usuário**

> Preciso explicar o LINQ na apresentação. Como eu faço o resumo por hora? Quero agrupar as leituras de um local por hora cheia e mostrar média, máximo e quantidade, ordenado por hora.

**🤖 Claude**

> Busque as leituras do local (`Where` + `AsNoTracking` + `ToListAsync`) e agrupe truncando a data para a hora cheia:
>
> ```csharp
> return leituras
>     .GroupBy(l => new DateTime(l.DataHora.Year, l.DataHora.Month, l.DataHora.Day, l.DataHora.Hour, 0, 0))
>     .OrderBy(g => g.Key)
>     .Select(g => new ResumoHoraResponse
>     {
>         Hora = g.Key,
>         NivelMedio = Math.Round(g.Average(l => l.NivelAgua), 2),
>         NivelMaximo = g.Max(l => l.NivelAgua),
>         Quantidade = g.Count()
>     })
>     .ToList();
> ```
>
> Fiz o agrupamento **em memória** (depois do `ToListAsync`) porque o SQLite com EF Core não traduz bem esse truncamento de data para SQL, e para o volume de uma entrega acadêmica isso não é problema. Vale registrar essa limitação no README: em produção, com muitas leituras, o ideal seria agrupar no banco ou pré-agregar.

**👤 Usuário**

> Ok, vou anotar isso nas limitações. Valeu.

---

## Conversa 5 — Testes unitários

**👤 Usuário**

> Agora os testes com xUnit. Quero:
> 1. Testar o classificador nos limites (0, 9.9, 10, 24.9, 25, 80) e o erro com valor negativo.
> 2. Testar que `Leitura.Classificar` usa o classificador recebido.
> 3. Testar o `LeituraService` com banco em memória: local inexistente retorna `null`; local existente salva e classifica; e o resumo por hora (média, máximo, quantidade e agrupamento).

**🤖 Claude**

> Criei dois arquivos.
>
> **`ClassificadorPorLimiarTests`**: um `[Theory]` com `[InlineData]` para os valores de fronteira (é nos limites 10 e 25 que costuma haver erro de `>` versus `>=`), um `[Fact]` com `Assert.Throws<ArgumentOutOfRangeException>` para `-1` e outro para `Leitura.Classificar`.
>
> **`LeituraServiceTests`**: um `DbContext` novo por teste, com `UseInMemoryDatabase(Guid.NewGuid().ToString())` para isolar os dados. No teste do resumo, são 3 leituras (10 e 20 na mesma hora, 40 na hora seguinte), e a verificação é: 2 grupos, média 15, máximo 20 e quantidade 2 no primeiro, máximo 40 no segundo.
>
> Pacotes necessários no projeto de testes: `Microsoft.EntityFrameworkCore.InMemory` (8.0.11) e a referência ao projeto da API.
>
> Com isso, são 11 casos no total (6 do `Theory`, mais os `Fact`s).

**👤 Usuário**

> Rodei `dotnet test`: 11 testes, 0 falhas. Tirei o print para as evidências.

---

## Conversa 6 — Revisão final e README

**👤 Usuário**

> Revisa o projeto como se fosse o professor. O que ainda está fraco ou faltando? Estou usando o ESP32 e o app Expo do grupo, que vão chamar essa API.

**🤖 Claude**

> O projeto cobre os requisitos (interface, DI, REST com DTOs, EF Core + LINQ, async e testes). Pontos que eu registraria como **limitações**, em vez de tentar resolver agora:
>
> 1. Sem autenticação: qualquer cliente pode chamar `POST /leituras`. Para uma entrega acadêmica é aceitável, mas deixe escrito.
> 2. Limiares fixos e didáticos. A evolução natural é receber a classificação do modelo de ML da disciplina de IA, e a interface `IClassificadorNivel` já deixa isso preparado.
> 3. Resumo por hora em memória (conversa 4).
> 4. O ESP32 só consegue enviar leituras se a API estiver acessível por uma URL pública.
> 5. Dados de demonstração simulados, e o ponto piloto real ainda não foi definido.
>
> Sugiro também um aviso no README de que o AquaSense **não substitui os alertas oficiais** da Defesa Civil e do CGE.

**👤 Usuário**

> Monta o README seguindo esse padrão da Global Solution: ficha da CitizenTech, introdução, conceitos aplicados, tabela de classificação, estrutura, endpoints, como executar, evidências e limitações.

**🤖 Claude**

> Gerei o README com essas seções. Antes de entregar, confira estes pontos, pois eu não tenho como verificá-los por você:
> - o link do repositório na ficha;
> - a porta do Swagger (`launchSettings.json`);
> - o número de testes e os prints em `evidencias/`;
> - os RMs e os nomes do grupo.

---

## Como a IA foi usada

| Etapa | Uso da IA | O que o grupo fez |
|---|---|---|
| Estrutura do projeto | Sugestão de pastas e entidades | Escolheu o escopo e adaptou ao problema do alagamento |
| Modelos e classificador | Exemplo de uma classe (`Local`) e revisão do restante | Escreveu `Leitura`, o enum, a interface e o classificador, e definiu os limiares (10 e 25 cm) |
| DTOs, services e controllers | Geração e explicação | Executou, testou no Swagger e pediu o ajuste do enum |
| Resumo por hora (LINQ) | Sugestão da consulta | Entendeu o agrupamento para explicar na apresentação |
| Testes | Geração dos casos | Rodou `dotnet test` e registrou o resultado |
| README | Estrutura do documento | Preencheu os dados do grupo e as evidências |

Todo o código foi lido, executado e testado pelo grupo antes da entrega.
