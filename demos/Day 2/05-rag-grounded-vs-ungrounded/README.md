# Day 2 Demo 5: RAG — grounded vs ungrounded answer

| | |
|---|---|
| **Slide deck** | `decks/Day 2/Day2-Section3-RAG-Pipelines.pptx` (slide 11, "Live Demo") |
| **Scheduled** | 1:50 PM (Section 3 · RAG Pipelines in .NET) |
| **Duration** | ~10 min |
| **Risk** | Medium (Docker + Ollama + live API call) |
| **Internet** | Docker + API (Anthropic API for the answers; Qdrant and Ollama run locally) |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/20> |

## What this demo teaches

- **Why RAG exists:** an LLM asked about *your* data without retrieval will confidently invent
  plausible-sounding answers. Retrieval grounding eliminates the guessing.
- **The full pipeline in C#:** ingest → chunk → embed (Ollama `nomic-embed-text`, 768-dim) →
  store (Qdrant via gRPC) → retrieve top-K by cosine similarity → augmented prompt → grounded answer.
- **The contrast is the lesson:** the *same query* hits `/api/recommend` twice — `grounded=false`
  sends it straight to Claude (it fabricates catalog titles); `grounded=true` retrieves real
  catalog chunks first and Claude cites actual books.
- **Quality comes from chunking, not the LLM** — the chunker (`ChunkSize`/`ChunkOverlap` in
  `appsettings.json`) is the lever Lab 3 Part C tunes.
- **Prompt caching applies here too:** the retrieved context sits in the system block with
  `CacheControlEphemeral`, tying back to Section 1's caching demo.

The runnable solution is in [`src/`](src/) — a single self-contained Minimal API project
(`Demo5.Rag`, net10.0) with an embedded 18-book catalog (`src/Demo5.Rag/Data/books.json`), so it
needs no database and no BookTracker checkpoint. It mirrors the code attendees build in Lab 3
against `src/BookTracker` (same `IVectorStore` / `EmbeddingService` / `TextChunker` shapes, same
package versions: `Anthropic` 12.31.0, `Qdrant.Client` 1.18.1, `OllamaSharp` 5.4.25,
`Microsoft.Extensions.AI` 10.7.0).

## Prerequisites / setup

Stage all of this **before the section starts** (most of it is on the night-before checklist):

- [ ] .NET 10 SDK on the demo machine; `dotnet build` of `src/Demo5.Rag.slnx` succeeds.
- [ ] **Docker Desktop running** and the image pulled so the start is instant:

  ```bash
  docker pull qdrant/qdrant
  ```

- [ ] **Ollama installed** with the embedding model pulled and the server reachable:

  ```bash
  ollama pull nomic-embed-text
  ollama serve   # skip if the Ollama app is already running (check http://localhost:11434)
  ```

- [ ] **Anthropic API key in user-secrets** (never in appsettings or the shell history on screen):

  ```bash
  cd "demos/Day 2/05-rag-grounded-vs-ungrounded/src/Demo5.Rag"
  dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
  ```

- [ ] Smoke-test the night before: run the app, hit both curl commands from the script below, and
  confirm the grounded answer cites real catalog titles.
- [ ] Two terminals ready, large font: one for the app + Docker, one for curl. Optional third
  browser tab on the Qdrant dashboard (<http://localhost:6333/dashboard>) to show vectors landing.
- [ ] `appsettings.json` has `"VectorStore:Provider": "Qdrant"` (the shipped default). Know where
  that line is — flipping it to `"InMemory"` is your live rescue.

## Step-by-step script

1. **Frame it (30 s).** Point at slide 11. Say: "Same endpoint, same question, one flag. First
   we let Claude answer from its imagination; then we make it answer from our data. Watch what
   changes."

2. **Start Qdrant** (alongside slide 9 if you haven't already):

   ```bash
   cd "demos/Day 2/05-rag-grounded-vs-ungrounded"
   docker compose up -d
   ```

   or equivalently, without compose:

   ```bash
   docker run -d -p 6333:6333 -p 6334:6334 qdrant/qdrant
   ```

   **Say:** "Two ports, and it matters: 6333 is REST and the dashboard; **6334 is gRPC, and
   that's the one the .NET `Qdrant.Client` talks to**. Map both or the client can't connect."

3. **Run the app — ingestion happens at startup:**

   ```bash
   dotnet run --project src/Demo5.Rag
   ```

   **Expected output** (first run; later runs report `0 chunks` because ingestion is idempotent):

   ```text
   info: Demo5.Rag[0]
         RAG corpus ready (Qdrant): 18 chunks ingested (0 = already populated).
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5250
   ```

   **Say:** "On startup we chunked 18 book descriptions sentence-aware, embedded each chunk with
   `nomic-embed-text` — 768 dimensions — and upserted them into a Qdrant collection created with
   **exactly 768** as its vector size. That number matching the model is not optional."
   Optionally show the collection in the dashboard at <http://localhost:6333/dashboard>.

4. **Ask UNGROUNDED first** — the same question everyone will compare against:

   ```bash
   curl -s "http://localhost:5250/api/recommend?q=recommend%20me%20a%20book%20about%20surviving%20alone%20on%20a%20hostile%20planet&grounded=false" | jq
   ```

   **Expected:** `"grounded": false`, an empty `sources` array, and a confident reply
   recommending "books from your BookTracker catalog" — typically inventing plausible titles
   (or naming books like *The Martian* that are **not** in the catalog) and presenting them as
   if they were yours.

   **Say:** "Read the sources field: empty. Claude never saw our catalog — it was told one
   exists and did what LLMs do: filled the gap fluently. Every title here is a guess. This is
   what hallucination looks like in a product."

5. **Ask GROUNDED — the exact same query, one flag flipped:**

   ```bash
   curl -s "http://localhost:5250/api/recommend?q=recommend%20me%20a%20book%20about%20surviving%20alone%20on%20a%20hostile%20planet&grounded=true" | jq
   ```

   **Expected:** `"grounded": true`; `sources` lists real catalog titles (this query reliably
   pulls **Project Hail Mary** and **Dune**, often *The Left Hand of Darkness*'s glacier trek);
   `retrievedContext` shows the top-5 chunks with cosine scores; and the reply recommends and
   cites only those books.

   **Say:** "Same model, same question. The only difference is fifteen lines of C#: embed the
   query, cosine-search top-5, paste those chunks into the system block. Now it cites *Project
   Hail Mary* because that's what's actually in our data — and the `retrievedContext` field shows
   you exactly what it was allowed to see."

6. **Prove the honesty case (if time, ~1 min).** Ask for something the catalog can't answer:

   ```bash
   curl -s "http://localhost:5250/api/recommend?q=a%20cozy%20romance%20set%20in%20a%20bakery&grounded=true" | jq
   ```

   **Expected:** the grounded prompt instructs Claude to say the context has no good match rather
   than invent — it declines or offers the nearest real genre. **Say:** "Grounding isn't just
   better answers — it's the ability to say *we don't have that*, which no ungrounded prompt
   will ever do reliably."

7. **Close (30 s).** "Lab 3 has you build exactly this pipeline into BookTracker — vector store,
   embeddings, retrieval, augmented prompt. Prioritize Parts A and B."

## Recovery

- **Qdrant down / Docker won't start:** flip one line in `src/Demo5.Rag/appsettings.json` —
  `"VectorStore:Provider": "InMemory"` — and restart the app. Same `IVectorStore` interface, same
  cosine ranking, zero Docker; the demo proceeds identically (you lose only the dashboard visual).
  Narrate the swap — it is itself a teaching moment about coding to the port, not the store.
- **Vector-size mismatch (upserts fail / gRPC `InvalidArgument`):** the Qdrant collection's vector
  size must equal the embedding model's output dimensions — **768 for `nomic-embed-text`**. If a
  stale collection was created with the wrong size, delete it and re-ingest:

  ```bash
  curl -s -X DELETE "http://localhost:6333/collections/demo5-books"
  curl -s -X POST "http://localhost:5250/api/ingest?force=true"
  ```

- **Client can't connect but Qdrant is running:** you mapped only 6333. The .NET client needs the
  **gRPC port 6334** — restart the container with both ports (`docker compose up -d` maps both).
- **Startup logs "Corpus ingestion skipped":** Ollama isn't running or the model isn't pulled.
  Run `ollama pull nomic-embed-text` (and start Ollama), then re-ingest without restarting:

  ```bash
  curl -s -X POST "http://localhost:5250/api/ingest?force=false"
  ```

- **401 from the Anthropic API:** the key isn't in user-secrets for *this* project (it has its own
  `UserSecretsId`). Re-run `dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."` from
  `src/Demo5.Rag` and restart.
- **Ungrounded answer happens to be sensible:** it can happen — Claude knows real books. Point at
  the empty `sources` array and re-run once; the point is it *cannot cite your data*, not that
  every ungrounded answer is wrong. A backup screenshot of a fabricated answer is cheap insurance.

## Teaching callouts

- **Quality comes from chunking, not the LLM.** The same model gives great or useless grounded
  answers depending on `ChunkSize`/`ChunkOverlap` and respecting sentence boundaries. The chunker
  is ~40 lines of C# and it is the whole quality lever — Lab 3 Part C is tuning it. If asked "which
  model should we use for RAG?", redirect: "First ask how you chunked."
- **Tie back to prompt caching (Section 1).** In `RecommendService.RecommendGroundedAsync`, the
  instructions + retrieved context live in the system block marked `CacheControlEphemeral`. The
  retrieved context is the static prefix of a session — repeat queries against it bill cached-input
  rates (~10%). RAG and caching compose; show the `CacheControl` line if there's time.
- **The pipeline is plain C#; the LLM only shows up at the end.** Ingest, chunk, embed, store,
  retrieve — all deterministic, testable .NET code. Grounding is an engineering practice, not a
  model feature.
- **The interface seam is the rescue *and* the architecture.** `IVectorStore` has two
  implementations (Qdrant, in-memory) selected by config; `IEmbeddingService` hides Ollama behind
  `Microsoft.Extensions.AI`. Swapping providers touched zero pipeline code — the same seam takes
  you to OpenAI embeddings (1536 dims — change `VectorSize` to match!) or a managed vector DB.
- **RAG vs fine-tuning:** new book added → searchable instantly, no retraining. Keep your data
  outside the model and retrieve at query time.
