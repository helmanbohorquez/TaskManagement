# 🧪 .NET - Technical Interview Exercise

## 📌 Project Overview

Your task is to develop a simple web application with an API and data layer using:

- .NET C#
- ASP.NET MVC
- Web API
- A database or data store

While adhering to:

- Clean Architecture principles  
- Test-Driven Development (TDD) methodologies  

Your development should be driven by an **informal user story** that you will create, and which should be included in your presentation.

The application should allow users to:

- Create
- Read
- Update
- Delete  

records via API endpoints.

Additionally, you should:

- Create a user
- Log in as the user
- Store user information in the data store

To showcase your ability to work with modern data storage systems, you **cannot use**:

- Entity Framework ❌  
- Dapper ❌  
- Mediator ❌  

---

# ⚙️ Requirements

## 🔙 Backend

### 🗄️ Database

- Create a database or data storage solution with:
  - At least one table/object/container for application data
  - One additional table/object/container for users

- Each data structure must include:
  - A unique identifier (primary key)
  - At least two additional fields

---

### 🔌 API

- Develop an ASP.NET Web API with endpoints to perform:
  - CRUD operations on data

- Each endpoint should include:
  - Appropriate HTTP verbs
  - Parameters
  - Return values

- Additionally, implement a second API with endpoints for:
  - User creation
  - User login
  - Authorized and non-authorized endpoints

---

### 🧱 Data Layer

- Develop a data access layer that:
  - Interacts with the data store
  - Provides CRUD operations for API endpoints

---

### 🧠 Business Logic Layer

- Develop a business logic layer that:
  - Contains all business rules and validation
  - Is independent of:
    - Data layer
    - API layer

---

### 🧪 Unit Tests

- Write unit tests for:
  - Data access layer
  - Business logic layer
  - API endpoints

---

## 🎨 Frontend

While the focus is backend, full stack skills are required.

### Requirements:

- Use a frontend framework of your choice:
  - React
  - Vue
  - etc.

### Key Criteria:

- Responsive and user-friendly UI
- CRUD operations integrated with backend
- Clean code structure:
  - Organized components
  - Proper state management

---

## 📦 Submission Guidelines

- Include a **README** with:
  - Setup instructions
  - Any relevant documentation

- Provide:
  - Seeded data / demo credentials

---

# 🤖 Generative AI Tools Section

## Scenario

You are tasked with generating a RESTful API for a **task management system** with:

- CRUD operations for tasks
- Task fields:
  - title
  - description
  - status
  - due_date

- Tasks associated with a user

---

## Instructions

Using a GenAI coding tool (e.g.):

- Cursor
- Claude Code
- Windsurf
- GitHub Copilot

### You must:

- Write the **prompt** used to generate the API
- Show:
  - The output code (or a representative sample)

---

## Explain:

### ✅ Validation
- How you verified the AI output

### 🛠️ Improvements
- What you corrected or improved

### ⚠️ Edge Cases
- Authentication
- Validation
- Error handling

---

# 🎤 Presentation and Code Review

You will present your project to a technical panel via:

- Google Meet or Zoom

### During the presentation:

Explain:

- Your user story
- Design choices
- Technical architecture
- Application functionality

---

## 🔍 Code Review

After the presentation, you will:

- Walk through your code
- Answer technical questions

---

# 📊 Evaluation Criteria

## 🧱 Clean Architecture
- Proper separation of concerns
- Independence between layers

## 🧪 Testing
- Good test coverage
- Preferably TDD

## ✨ Code Quality
- Clean and readable code
- Best practices applied

## ⚙️ Functionality
- Meets all requirements
- No bugs or errors
- (Bonus) No browser console warnings

## 🗣️ Presentation
- Clear and structured explanation
- Strong understanding of decisions

## 🤖 GenAI Usage
- Strong prompt engineering
- Critical thinking about AI-generated code
