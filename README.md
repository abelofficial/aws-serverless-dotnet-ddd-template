# AWS Serverless .NET 10 DDD Template

A template for building AWS Lambda functions with .NET 10 using Domain-Driven Design and a lightweight CQRS pattern.

---

## Prerequisites

| Tool | Notes |
|------|-------|
| .NET SDK 10.0.x | Pinned via `global.json` |
| AWS SAM CLI | For local simulation and deployment |
| Docker | Required by SAM |
| Node.js / npm | For npm scripts |
| AWS Lambda Test Tool | `dotnet tool install -g Amazon.Lambda.TestTool-10.0` |

---

## Project Structure

```
src/
├── Domain/            # Entities, domain exceptions, core interfaces
├── Application/       # CQRS contracts (IRequest/IResponse), handlers, response models
├── Infrastructure/    # External integrations (DynamoDB, etc.)
└── LambdaFunctions/
    ├── Functions/
    │   ├── BaseFunctions.cs              # Shared response helpers + exception policy
    │   ├── ApiExceptionPolicyHandler.cs  # Per-exception HTTP status mapping
    │   └── Hello.cs                      # Example function
    ├── Exceptions/    # Lambda-layer exceptions (e.g. BadRequestException)
    ├── Settings/      # Custom Lambda serializer + DI installer
    ├── template.yaml  # SAM template
    └── package.json   # NPM scripts
```

---

## Local Development

### Build

```bash
cd src/LambdaFunctions
npm run build
```

### Debug with Lambda Test Tool

```bash
npm run debug
# Opens http://localhost:5050
```

### Run API locally with SAM

```bash
npm run api           # standard
npm run api:debug     # with debugger on port 5890 (set SAM_DEBUG_PORT to override)
```

Test it:

```bash
curl -X POST http://127.0.0.1:3000/sayHello \
  -H "Content-Type: application/json" \
  -d '{"name": "World"}'
```

---

## NPM Scripts

| Script | Description |
|--------|-------------|
| `npm run build` | `dotnet build` |
| `npm run debug` | Start Lambda Test Tool UI |
| `npm run api` | Publish + SAM local start-api |
| `npm run api:debug` | SAM local with debugger attached |
| `npm run deploy:dev` | `sam build && sam deploy` |
| `npm run deploy:staging` | Deploy to staging config |
| `npm run deploy:prod` | Deploy to production config |
| `npm run validate` | Validate SAM template |

---

## Response Shape

```json
{ "data": { ... } }
{ "error": { "status": "400", "message": "..." } }
```

---

## Exception Policy

Register expected exceptions per function with a custom HTTP status and optional message:

```csharp
policy.HandleException<BadRequestException>(HttpStatusCode.BadRequest, "Bad Request");
policy.HandleException<NotFoundException>(HttpStatusCode.NotFound);
// No message → uses exception.Message
// Unregistered exception → 500 + exception.Message
```

---

## Adding a New Function

1. Add request/response models in `Application/CQRS/`
2. Add handler interface + implementation in `Application/Handlers/`
3. Register the handler via `IExtensionsInstaller` in the appropriate layer
4. Create a class in `LambdaFunctions/Functions/` inheriting `BaseFunctions`
5. Add the resource to `template.yaml`

---

## Deployment

```bash
cd src/LambdaFunctions
npm run deploy:dev
```
