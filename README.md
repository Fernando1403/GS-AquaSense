# AquaSense — API (C# / .NET 8) — Global Solution 2026.2
 
- Fernando Gonzales Alexandre - RM: 555045
- Lucas Dias - RM: 555450
- Vitor Teixeira - RM: 555012
- Gabriel Guerreiro - RM: 554973
- Luiz Felipe Coelho - RM: 555074
---
 
## Ficha da CitizenTech
 
| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | AquaSense |
| Cidade e bairro | São Paulo (SP) — ponto piloto a definir |
| Cidadão beneficiado | Moradores, trabalhadores e comerciantes de áreas com recorrência de alagamentos |
| Problema, em uma frase | Pessoas expostas a alagamentos nem sempre têm informações locais, organizadas e compreensíveis para planejar seus deslocamentos |
| Como funciona | O sensor (ESP32) envia leituras de nível de água para esta API; o aplicativo consulta as leituras e a classificação do nível |
| O que cada parte faz | API (C# e SOA): guarda locais e leituras e classifica o nível; IA/ML: padrões históricos; ESP32/Wokwi: sensor; App Expo: consulta; demais disciplinas conforme a proposta |
| Link do repositório | `<colar o link do GitHub>` |
| Indicador de impacto | Taxa de leituras recebidas e classificadas corretamente |
| ODS conectados | 11, 13 e 3 |
 
> O AquaSense não substitui alertas oficiais da Defesa Civil ou do CGE. Os limiares de classificação são didáticos e os dados de demonstração são simulados.
 
---
 
## 1. Introdução e Contextualização
 
**Problema:** moradores e trabalhadores de áreas sujeitas a alagamento não têm uma forma simples de saber, em tempo quase real, qual é o nível de água em um ponto específico da cidade.
 
**Objetivo:** oferecer um backend que receba leituras de um sensor de nível de água, armazene o histórico por local e classifique cada leitura em **Normal**, **Atenção** ou **Crítico**, para que um aplicativo consulte essas informações.
 
**Premissas:**
- cada local monitorado é um ponto aproximado (latitude e longitude), nunca um endereço residencial;
- o nível de água é medido em centímetros;
- a classificação usa limiares didáticos, não oficiais;
- se o local informado não existir, a API responde `404`.
**Natureza:** API REST em C#/.NET 8 com banco de dados SQLite, que integra a CitizenTech do grupo (sensor ESP32 e aplicativo mobile consomem esta API).
 
---
 
## 2. Conceitos Aplicados
 
- **POO e interfaces:** as entidades `Local` e `Leitura` concentram dados e comportamento. A interface `IClassificadorNivel` permite trocar a regra de classificação sem alterar o resto do código; `ClassificadorPorLimiar` é a implementação atual.
- **Injeção de dependência:** serviços e classificador são registrados no `Program.cs` e recebidos pelos construtores.
- **Web API REST:** controllers, DTOs de entrada e saída, validação com Data Annotations e códigos HTTP corretos (`200`, `201`, `400`, `404`).
- **EF Core + LINQ:** SQLite, relacionamento 1:N (um local tem várias leituras) e consulta de resumo por hora com `GroupBy`, `Average`, `Max` e `Count`.
- **Async/Await:** serviços e controllers assíncronos, com `CancellationToken`.
- **Testes unitários:** xUnit para o classificador e para o serviço de leituras (banco em memória).
---
 
## 3. Classificação do Nível de Água
 
| Nível (cm) | Classificação |
|---|---|
| menor que 10 | Normal |
| de 10 a menos de 25 | Atencao |
| 25 ou mais | Critico |
 
Valor negativo é inválido (lança `ArgumentOutOfRangeException`; na API, o `POST /leituras` rejeita com `400`).
 
---
 
## 4. Estrutura do Projeto
 
```
src/AquaSense.Api
  Controllers/   endpoints REST
  Services/      regras de aplicação (async, LINQ)
  Models/        entidades, enum, interface e classificador (POO)
  Dtos/          objetos de entrada e saída, com validação
  Data/          DbContext (EF Core) e seed de demonstração
tests/AquaSense.Tests   testes xUnit
evidencias/             prints do Swagger e dos testes
```
 
---
 
## 5. Endpoints
 
| Método | Rota | Descrição |
|---|---|---|
| GET | `/locais` | Lista os locais monitorados |
| GET | `/locais/{id}` | Obtém um local pelo id |
| POST | `/locais` | Cadastra um local |
| GET | `/locais/{id}/leituras` | Leituras do sensor (mais recentes primeiro) |
| GET | `/locais/{id}/resumo-por-hora` | Média, máximo e quantidade por hora (LINQ) |
| POST | `/leituras` | Registra uma leitura (usado pelo ESP32) |
 
Exemplo de `POST /leituras`:
 
```json
{ "localId": 1, "nivelAgua": 30 }
```
 
Resposta (`201`):
 
```json
{ "id": 11, "localId": 1, "nivelAgua": 30, "dataHora": "2026-11-16T17:00:00Z", "nivel": "Critico" }
```
 
---
 
## 6. Como Executar
 
Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
 
```bash
dotnet restore
dotnet run --project src/AquaSense.Api
```
 
O Swagger abre em **http://localhost:5221/swagger**. O banco SQLite (`aquasense.db`) é criado automaticamente na primeira execução, com 1 local e 10 leituras de demonstração (simuladas).
 
**Testes:**
 
```bash
dotnet test
```
 
Resultado obtido: **11 testes, 0 falhas, 11 bem-sucedidos**.
 
---
 
## 7. Demonstração / Evidências
 
### 7.1 Visão geral do Swagger
<img width="1911" height="1022" alt="image" src="https://github.com/user-attachments/assets/0dc5245c-43ee-42dc-a339-d55726a0cd73" />

 
### 7.2 Listar locais — `GET /locais`
<img width="1897" height="1028" alt="image" src="https://github.com/user-attachments/assets/7188ba78-5b59-41d5-a279-ff75897b0acc" />

### 7.3 Listar locais por id — `GET /locais/id`
<img width="1912" height="1027" alt="image" src="https://github.com/user-attachments/assets/85d953d4-cf62-4b69-b223-8b996a73c7e6" />

### 7.4 Registrar leitura — `POST /leituras` (nível crítico)
<img width="1907" height="1030" alt="image" src="https://github.com/user-attachments/assets/bd05d9bd-1042-4e19-9f1e-0341c98b76e4" />

 
### 7.5 Listar leituras de um local — `GET /locais/1/leituras`
<img width="1907" height="986" alt="image" src="https://github.com/user-attachments/assets/e2724318-b73a-491f-b105-357511c6a8ac" />

 
### 7.6 Resumo por hora — `GET /locais/1/resumo-por-hora`
<img width="1905" height="1006" alt="image" src="https://github.com/user-attachments/assets/dc8f140d-9429-437c-bf85-2ad93ebc8ae1" />

 
### 7.7 Testes automatizados — `dotnet test`
<img width="1907" height="1017" alt="image" src="https://github.com/user-attachments/assets/9deb26cb-f6cb-4701-b07a-a74a76e64fa2" />

 
---
 
## 8. Limitações e Próximos Passos
 
- Os dados de demonstração são simulados; o ponto piloto real ainda será definido pelo grupo.
- A classificação usa limiares fixos e didáticos; uma evolução seria receber a classificação do modelo de ML da disciplina de IA.
- O projeto não tem autenticação, por ser uma entrega acadêmica.
- A integração com o ESP32 depende de a API estar acessível por uma URL pública.
---
 
## 9. Uso de IA
 
Os prompts utilizados estão registrados em [`prompts.md`](prompts.md).
 
