# BookTracker — Checkpoint c7 (RAG with a vector store)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c7 — retrieval-augmented generation.** *Day 2 · Section 3 — RAG Pipelines.* This is `c6` plus a
> full **RAG pipeline**: a new `BookTracker.VectorStore` project (chunking → embeddings → vector
> store), a `/api/recommend` endpoint that grounds Claude's recommendations in retrieved book
> content, and **Qdrant** wired up via Docker Compose.

## What's new in this checkpoint

### `BookTracker.VectorStore` project (new)

A self-contained RAG library:

```text
BookTracker.VectorStore/
├── Chunking/       ITextChunker + TextChunker          # split text into embeddable chunks
├── Embeddings/     IEmbeddingService + EmbeddingService # embed text (Ollama nomic-embed-text)
├── Stores/         IVectorStore                          # the store abstraction, with two impls:
│                   ├── InMemoryVectorStore              #   default — no infrastructure needed
│                   └── QdrantVectorStore                #   backed by Qdrant (docker compose up)
├── Ingestion/      CorpusIngestionService              # chunk → embed → upsert the book corpus
└── VectorStoreOptions.cs                                # Provider = InMemory | Qdrant, etc.
```

Built on `Microsoft.Extensions.AI`, `OllamaSharp` (embeddings), and `Qdrant.Client`.

### RAG endpoint & service (new in the API)

- **`RagService`** (`IRagService`) — retrieves relevant chunks for a query and asks Claude to
  recommend from them.
- **`RecommendEndpoints`** — `POST /api/recommend`.
- **`RecommendDtos`** — request/result shapes.
- EF Core migration **`AddBookDescription`** — books now carry a description, the text that gets
  chunked and embedded into the corpus.

### `docker-compose.yml` (new)

Runs **Qdrant** (ports `6333` REST/dashboard, `6334` gRPC). The in-memory store is the default, so
Docker is optional until you switch `VectorStore:Provider` to `Qdrant`.

## Prerequisites

- **Anthropic API key** (as before):
  `dotnet user-secrets set "Anthropic:ApiKey" "your-key-here" --project BookTracker.Api`
- **Ollama** for embeddings (local): `ollama pull nomic-embed-text && ollama serve`
- **Docker** *(optional)* — only if you want the Qdrant-backed store instead of in-memory.

## Getting started

```bash
cd src/c7/BookTracker

# optional: real vector DB instead of the in-memory store
docker compose up -d           # starts Qdrant on 6333/6334

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

To use Qdrant, set `VectorStore:Provider` to `Qdrant` in `appsettings`; otherwise the
`InMemoryVectorStore` is used and no infrastructure is required.

## Project layout

```text
BookTracker.sln
docker-compose.yml                      # ← NEW: Qdrant
BookTracker.VectorStore/                # ← NEW: chunking, embeddings, stores, ingestion
BookTracker.Core/                       # + RecommendDtos, IRagService
BookTracker.Data/                       # + AddBookDescription migration
BookTracker.Api/                        # + Services/RagService.cs, Endpoints/RecommendEndpoints.cs
```

## API endpoints

All prior endpoints still work. New in `c7`:

| Method | Route              | Description                                                     |
|--------|--------------------|-----------------------------------------------------------------|
| POST   | `/api/recommend`   | Retrieve relevant book content and have Claude recommend from it |

## Try it

```bash
curl -X POST http://localhost:5255/api/recommend \
  -H "Content-Type: application/json" \
  -d '{"query":"I liked Clean Code — what should I read next?"}'
```

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 3 (RAG Pipelines).
- **Deck:** `decks/Day 2/Day2-Section3-RAG-Pipelines.pptx`
- **Docs:** [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/) ·
  [Qdrant](https://qdrant.tech/documentation/) ·
  [Ollama embeddings](https://ollama.com/library/nomic-embed-text) ·
  [Embeddings (Anthropic)](https://docs.claude.com/en/docs/build-with-claude/embeddings)
- **Previous:** [`c6`](../../c6/BookTracker/README.md) — streaming, tools, and the agent loop.
- **Next:** [`c8`](../../c8/BookTracker/README.md) — write your own MCP server in C#.
