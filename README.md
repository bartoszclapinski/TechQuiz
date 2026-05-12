# TechQuiz

> A multi-user platform for testing technical knowledge — with AI-generated questions, spaced repetition, and progress tracking.

TechQuiz is a personal learning platform inspired by Pluralsight Skill IQ. Users test their knowledge across .NET, ASP.NET Core, SQL, design patterns, and other technical topics. AI generates new questions on demand using each user's own API key, and all generated questions are saved to a shared public pool to grow the question bank for everyone.

This is a portfolio project demonstrating Clean Architecture, multi-provider AI integration, full-stack .NET development, and modern frontend practices.

---

## Tech Stack

**Backend**
- ASP.NET Core 9 Web API
- Entity Framework Core 9 + PostgreSQL
- MediatR (CQRS)
- FluentValidation
- ASP.NET Core Identity + JWT bearer auth
- Serilog with structured logging
- xUnit + FluentAssertions + NSubstitute

**Frontend**
- React 18 with TypeScript
- Vite
- TanStack Query
- React Router v6
- Tailwind CSS
- Monaco Editor (for code questions)
- Recharts (for progress dashboards)

**AI Integration**
- Multi-provider abstraction (OpenAI, Anthropic Claude, extensible)
- User-supplied API keys, encrypted at rest via ASP.NET Core Data Protection
- Public question pool — AI-generated questions saved for community reuse

**Infrastructure**
- Docker + docker-compose for local development
- Seq for structured log search and observability
- GitHub Actions for CI (build + test on every PR)

---

## Features

### Implemented in MVP (Phase 1)
- Multi-user authentication with JWT
- Quiz flow: select category → answer questions → see results (test mode)
- Backend domain logic developed with TDD
- 10–20 seed questions across C# Basics and ASP.NET Core categories
- Demo user with historical quiz attempts for development

### Coming in Phase 2
- Progress dashboard with bento grid layout
- Historical attempt tracking with filters
- Spaced repetition (SM-2 simplified) for review mode
- Per-category statistics and streak counters

### Coming in Phase 3
- AI question generator with multi-provider support
- Encrypted per-user API key vault
- Public AI-generated question pool with community moderation
- Code questions with Monaco Editor (output prediction, bug finding, fill-in)
- AI evaluator for code responses with feedback

### Coming in Phase 4
- Gamification (XP, levels, badges, streaks)
- Email confirmation and password reset
- Production deployment to Railway or Azure
- Full CI/CD pipeline with automated testing
- README polish with screenshots and demo GIF

---

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Docker Desktop
- Git

### Local Development

```bash
# Clone the repository
git clone https://github.com/bartoszclapinski/techquiz.git
cd techquiz

# Start PostgreSQL and Seq via Docker
docker compose up -d

# Apply database migrations
dotnet ef database update --project src/TechQuiz.Infrastructure --startup-project src/TechQuiz.API

# Run the API (in one terminal)
dotnet run --project src/TechQuiz.API

# Run the frontend (in another terminal)
cd client
npm install
npm run dev
```

The API will be available at `http://localhost:5000`, the frontend at `http://localhost:5173`, and Seq at `http://localhost:5341`.

### Demo Credentials
```
Email:    demo@techquiz.dev
Password: Demo123!
```

The demo user has historical quiz attempts seeded for dashboard testing.

---

## Project Structure

```
TechQuiz/
├── src/
│   ├── TechQuiz.Domain/          # Entities, value objects, domain logic
│   ├── TechQuiz.Application/     # CQRS handlers, services, validators
│   ├── TechQuiz.Infrastructure/  # EF Core, Identity, AI providers
│   └── TechQuiz.API/             # ASP.NET Core Web API
├── tests/
│   ├── TechQuiz.Domain.Tests/
│   └── TechQuiz.Application.Tests/
├── client/                       # React + TypeScript frontend
├── docs/
│   ├── ARCHITECTURE.md           # Technical decisions and design
│   └── DECISION_LOG.md           # ADR-style decision history
├── docker-compose.yml
└── README.md
```

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for a detailed technical overview and [`docs/DECISION_LOG.md`](docs/DECISION_LOG.md) for the reasoning behind major decisions.

---

## Development Workflow

This project follows **GitHub Flow** with feature branches and pull requests:

1. Create a branch: `git checkout -b feature/short-description`
2. Make changes following TDD for domain and application layers
3. Push and open a PR
4. Verify CI passes (build + tests)
5. Self-review the diff before merging
6. Squash and merge to `main`

Commits follow the **Conventional Commits** specification, enforced via commitlint:

```
feat: add quiz scoring service
fix: handle null reference in attempt completion
refactor: extract JWT generation from auth controller
test: add edge cases for spaced repetition
docs: update API setup instructions
chore: bump EF Core to 9.0.1
```

---

## Author

**Bartosz Clapinski** — .NET Developer with AI Integration focus

[GitHub](https://github.com/bartoszclapinski) · [LinkedIn](https://linkedin.com/in/bartoszclapinski)

---

## License

MIT
