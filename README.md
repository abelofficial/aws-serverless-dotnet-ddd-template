# AWS Serverless .NET 8 DDD Template

A modern, production-ready template for building AWS Lambda serverless applications in .NET 8 using Domain-Driven Design (DDD), CQRS with MediatR, and best practices for local development and deployment.

---

## 🚀 Features

-   **.NET 8** with clean DDD architecture
-   **CQRS** with MediatR
-   **AWS Lambda** entry points with API Gateway support
-   **DynamoDB** integration
-   **Local debugging** with AWS Lambda Test Tool
-   **Full AWS simulation** with AWS SAM (Serverless Application Model)
-   **Custom Lambda Serializer** with case-insensitive JSON handling
-   **Base Functions** with unified error handling and response formatting
-   **Easy deployment** to AWS
-   **NPM scripts** for streamlined development workflow

---

## 🗂️ Project Structure

```
aws-serverless-dotnet-ddd-template/
├── AwsServerlessDotnetDddTemplate.sln  # Solution file
├── src/
│   ├── Domain/                     # Domain layer (entities, interfaces)
│   ├── Application/                # Application layer (CQRS, handlers)
│   ├── Infrastructure/             # Infrastructure layer
│   └── LambdaFunctions/
│       ├── Functions/              # Lambda entry points
│       │   ├── BaseFunctions.cs    # Base class with common functionality
│       │   ├── Hello.cs           # Hello function
│       │   └── GoodBye.cs         # GoodBye function
│       ├── Settings/               # Custom serializers and settings
│       ├── template.yaml           # SAM template (main infra config)
│       ├── aws-lambda-tools.default.json # Lambda Test Tool config
│       ├── start-api.sh           # Script for local API testing
│       └── package.json           # NPM scripts for development
├── events/                         # Test events for SAM local
│   └── hello-event.json
├── .vscode/                        # VS Code tasks and launch configs
└── README.md                       # This file
```

---

## 🎯 Quick Start

### **1. Clone and Customize**

```bash
# Clone the template
git clone <your-repo-url>
cd aws-serverless-dotnet-ddd-template

# Customize the project name (optional)
# See "Customization" section below for detailed instructions
```

### **2. Build and Test**

```bash
cd src/LambdaFunctions
dotnet build
dotnet lambda-test-tool-8.0
```

---

## 🟢 Local Development & Debugging

### **1. Build the Project**

```bash
cd src/LambdaFunctions
# Always build before running tools
dotnet build
dotnet publish -c Release
```

### **2. Debug with AWS Lambda Test Tool (Recommended for Dev)**

```bash
cd src/LambdaFunctions
dotnet lambda-test-tool-8.0
```

-   Opens web UI at [http://localhost:5050](http://localhost:5050)
-   Select your function, paste a test event, and invoke
-   Set breakpoints in VS Code and debug as usual

### **3. Test API Gateway Locally with SAM**

```bash
# Using npm scripts (recommended)
npm run api all                    # Start all endpoints
npm run api SayHelloFunction       # Start specific function
npm run api:all                    # Alternative way to start all

# Or using the shell script directly
./start-api.sh all
./start-api.sh SayHelloFunction
```

**Note:** SAM requires Docker to be installed and running.

### **4. Direct Lambda Invocation**

```bash
# Test individual functions
sam local invoke SayHelloFunction --event events/hello-event.json
sam local invoke GoodByeFunction --event events/goodbye-event.json
```

---

## 🟣 Deployment

```bash
# Build and deploy to AWS
npm run deploy:dev                 # Deploy to dev environment
npm run deploy:staging             # Deploy to staging
npm run deploy:prod                # Deploy to production

# Or manually
sam build
sam deploy
```

---

## 🛠️ NPM Scripts & Commands

### **Development Scripts**

```bash
npm run build                      # Build the project
npm run debug                      # Start Lambda Test Tool
npm run api all                    # Start API Gateway locally
npm run api SayHelloFunction       # Start specific function
npm run local:api                  # Start API Gateway (alternative)
npm run local:lambda               # Start Lambda service only
```

### **Deployment Scripts**

```bash
npm run deploy:dev                 # Deploy to dev
npm run deploy:staging             # Deploy to staging
npm run deploy:prod                # Deploy to production
npm run build:sam                  # Build with SAM
```

### **Utility Scripts**

```bash
npm run validate                   # Validate SAM template
npm run logs                       # View CloudWatch logs
```

---

## 🧪 Testing

### **Lambda Test Tool (Direct Function Testing)**

```json
{
    "Name": "World"
}
```

### **API Gateway Testing (Full HTTP Simulation)**

```bash
# Test with curl
curl -X POST http://127.0.0.1:3000/hello \
  -H "Content-Type: application/json" \
  -d '{"Name": "World"}'

# Test with Postman
POST http://127.0.0.1:3000/hello
Content-Type: application/json

{
  "Name": "World"
}
```

### **Test Events**

-   Use simple POCO JSON for Lambda Test Tool
-   Use full API Gateway event JSON for SAM Local (see `events/hello-event.json`)
-   Both tools can be used independently for local testing

---

## 🏗️ Architecture & Best Practices

### **Base Functions Pattern**

All Lambda functions inherit from `BaseFunctions` which provides:

-   **Unified error handling** with proper HTTP status codes
-   **API Gateway response formatting** with CORS headers
-   **MediatR integration** for CQRS
-   **Serilog logging** with structured logging
-   **Request extraction** from API Gateway events

### **Custom Serializer**

-   **Case-insensitive JSON deserialization**
-   **Automatic API Gateway response wrapping**
-   **Proper error handling** and logging

### **CQRS with MediatR**

-   **Commands and Queries** separated
-   **Handler-based architecture**
-   **Dependency injection** support
-   **Clean separation of concerns**

### **Development Workflow**

-   Keep all Lambda and infra config in `src/LambdaFunctions/`
-   Use `aws-lambda-tools.default.json` for Lambda Test Tool
-   Use `template.yaml` for SAM
-   Always build before running either tool
-   Use breakpoints and logs for debugging
-   Keep handler signatures and config in sync

---

## 📚 Resources

-   [AWS Lambda Test Tool](https://docs.aws.amazon.com/lambda/latest/dg/with-dotnet-test-tool.html)
-   [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/)
-   [.NET 8](https://docs.microsoft.com/en-us/dotnet/)
-   [MediatR](https://github.com/jbogard/MediatR)
-   [Serilog](https://serilog.net/)

---

## ❓ FAQ

-   **Q: Can I use both AWS Lambda Test Tool and SAM?**
    -   **A:** Yes! Use the Test Tool for fast debugging, and SAM for full AWS simulation and deployment.
-   **Q: Where is my main infra config?**
    -   **A:** `src/LambdaFunctions/template.yaml`
-   **Q: Where do I put test events?**
    -   **A:** In the `events/` folder (or anywhere you like).
-   **Q: What if my handler isn't detected?**
    -   **A:** Check your handler signature, build output, and `aws-lambda-tools.default.json`.
-   **Q: How do I add a new Lambda function?**
    -   **A:** Create a new class inheriting from `BaseFunctions`, add it to `template.yaml`, and create an npm script if needed.
-   **Q: Why do I get 500 errors?**
    -   **A:** Check your handler exists, request format is correct, and all dependencies are properly injected.
-   **Q: How do I customize this template for my project?**
    -   **A:** See the "Customization" section below for step-by-step instructions.

---

Happy coding! 🎉
