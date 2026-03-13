using System.Security.Claims;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itenium.SkillForge.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedTeams(db);
        await SeedCourses(db);
        await SeedSkillCatalogue(db);
        await app.SeedTestUsers();
        await app.SeedConsultants(db);
        await app.SeedResources(db);
    }

    private static async Task SeedTeams(AppDbContext db)
    {
        if (!await db.Teams.AnyAsync())
        {
            db.Teams.AddRange(
                new TeamEntity { Id = 1, Name = "Java" },
                new TeamEntity { Id = 2, Name = ".NET" },
                new TeamEntity { Id = 3, Name = "PO & Analysis" },
                new TeamEntity { Id = 4, Name = "QA" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCourses(AppDbContext db)
    {
        if (!await db.Courses.AnyAsync())
        {
            db.Courses.AddRange(
                new CourseEntity { Id = 1, Name = "Introduction to Programming", Description = "Learn the basics of programming", Category = "Development", Level = "Beginner" },
                new CourseEntity { Id = 2, Name = "Advanced C#", Description = "Master C# programming language", Category = "Development", Level = "Advanced" },
                new CourseEntity { Id = 3, Name = "Cloud Architecture", Description = "Design scalable cloud solutions", Category = "Architecture", Level = "Intermediate" },
                new CourseEntity { Id = 4, Name = "Agile Project Management", Description = "Learn agile methodologies", Category = "Management", Level = "Beginner" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedSkillCatalogue(AppDbContext db)
    {
        if (await db.Skills.AnyAsync())
        {
            return;
        }

        // ── Universal skills present in all or multiple profiles ──────────────

        var cleanCode = new SkillEntity
        {
            Id = 1,
            Name = "Clean Code",
            Category = "Craftsmanship",
            Description = "Writing readable, maintainable, and expressive code following established conventions.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Aware of naming and formatting conventions; applies them with guidance." },
                new() { Level = 2, Description = "Applies clean code principles at function and class level consistently." },
                new() { Level = 3, Description = "Applies clean code at architectural level; coaches others; identifies design smells." }
            ]
        };

        var solidPrinciples = new SkillEntity
        {
            Id = 2,
            Name = "SOLID Principles",
            Category = "Craftsmanship",
            Description = "Understanding and applying the five SOLID object-oriented design principles.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Knows the acronym and can explain each principle with examples." },
                new() { Level = 2, Description = "Applies SOLID in daily code; recognises violations and refactors them." },
                new() { Level = 3, Description = "Uses SOLID to guide architectural decisions; mentors others on trade-offs." }
            ]
        };

        var git = new SkillEntity
        {
            Id = 3,
            Name = "Git & Version Control",
            Category = "Tooling",
            Description = "Effective use of Git for branching, merging, code review, and history management.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Can commit, push, pull, and create feature branches." },
                new() { Level = 2, Description = "Manages merge conflicts, rebases, and follows a team branching strategy." },
                new() { Level = 3, Description = "Defines branching conventions; uses advanced history rewriting; trains others." }
            ]
        };

        var agileScrum = new SkillEntity
        {
            Id = 4,
            Name = "Agile / Scrum",
            Category = "Way of Working",
            Description = "Participating effectively in agile ceremonies and contributing to sprint delivery.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Attends ceremonies; understands sprint cadence and backlog basics." },
                new() { Level = 2, Description = "Contributes to refinement; raises impediments; meets sprint commitments reliably." },
                new() { Level = 3, Description = "Drives retrospective improvements; coaches on agile practices; works with PO on scope." }
            ]
        };

        var securityAwareness = new SkillEntity
        {
            Id = 5,
            Name = "Security Awareness",
            Category = "Quality & Security",
            Description = "Recognising and avoiding common security vulnerabilities in delivered software.",
            LevelCount = 1,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Aware of OWASP Top 10; applies input validation and avoids obvious vulnerabilities." }
            ]
        };

        var communication = new SkillEntity
        {
            Id = 6,
            Name = "Communication & Collaboration",
            Category = "Way of Working",
            Description = "Communicating clearly with team members, stakeholders, and clients.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Communicates progress and blockers within the team." },
                new() { Level = 2, Description = "Adapts communication style to technical and non-technical audiences." },
                new() { Level = 3, Description = "Facilitates cross-team and client conversations; manages conflict constructively." }
            ]
        };

        // ── .NET specific skills ───────────────────────────────────────────────

        var csharp = new SkillEntity
        {
            Id = 10,
            Name = "C# Language",
            Category = ".NET",
            Description = "Proficiency with C# language features including generics, LINQ, async/await, and modern syntax.",
            LevelCount = 5,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes basic C# with classes, methods, and control flow." },
                new() { Level = 2, Description = "Uses LINQ, generics, interfaces, and exception handling effectively." },
                new() { Level = 3, Description = "Applies async/await, delegates, and extension methods; understands the type system." },
                new() { Level = 4, Description = "Masters advanced features: spans, records, source generators, pattern matching." },
                new() { Level = 5, Description = "Deep CLR knowledge; contributes to library design; optimises for performance." }
            ]
        };

        var aspNetCore = new SkillEntity
        {
            Id = 11,
            Name = "ASP.NET Core",
            Category = ".NET",
            Description = "Building web APIs and web applications using ASP.NET Core middleware, controllers, and minimal APIs.",
            LevelCount = 4,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Can scaffold a controller and handle basic HTTP requests." },
                new() { Level = 2, Description = "Configures middleware, dependency injection, and routing; implements authentication." },
                new() { Level = 3, Description = "Designs RESTful APIs with proper status codes, validation, and versioning." },
                new() { Level = 4, Description = "Optimises performance; implements custom middleware, filters, and model binding." }
            ]
        };

        var entityFramework = new SkillEntity
        {
            Id = 12,
            Name = "Entity Framework Core",
            Category = ".NET",
            Description = "Using EF Core for data access: migrations, relationships, querying, and performance tuning.",
            LevelCount = 4,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Performs basic CRUD operations with DbContext and migrations." },
                new() { Level = 2, Description = "Configures relationships, navigation properties, and fluent API." },
                new() { Level = 3, Description = "Optimises queries; understands N+1 problem; uses raw SQL where appropriate." },
                new() { Level = 4, Description = "Writes custom conventions, interceptors, and value converters; advanced migrations." }
            ]
        };

        var restApiDesign = new SkillEntity
        {
            Id = 13,
            Name = "REST API Design",
            Category = "Architecture",
            Description = "Designing resource-oriented HTTP APIs following REST constraints and best practices.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Understands HTTP verbs, status codes, and resource naming conventions." },
                new() { Level = 2, Description = "Designs consistent APIs with proper error contracts, pagination, and filtering." },
                new() { Level = 3, Description = "Leads API design decisions; defines standards across teams; handles versioning." }
            ]
        };

        var ddd = new SkillEntity
        {
            Id = 14,
            Name = "Domain-Driven Design",
            Category = "Architecture",
            Description = "Applying DDD tactical patterns (aggregates, value objects, domain events) and strategic patterns (bounded contexts).",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Understands bounded contexts, entities, and value objects conceptually." },
                new() { Level = 2, Description = "Implements aggregates, domain events, and repositories in a real project." },
                new() { Level = 3, Description = "Drives strategic DDD decisions; defines context maps; coaches the team." }
            ]
        };

        var unitTestingDotNet = new SkillEntity
        {
            Id = 15,
            Name = "Unit Testing (.NET)",
            Category = "Quality & Security",
            Description = "Writing effective unit and integration tests using NUnit/xUnit, Moq, and test doubles.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes basic unit tests with assertions and a test runner." },
                new() { Level = 2, Description = "Uses mocks/stubs; writes meaningful test names; achieves good coverage on new code." },
                new() { Level = 3, Description = "Practices TDD; designs testable code; builds integration test suites." }
            ]
        };

        var cleanArchitecture = new SkillEntity
        {
            Id = 16,
            Name = "Clean Architecture",
            Category = "Architecture",
            Description = "Structuring applications with dependency inversion: use cases, ports & adapters, layered responsibilities.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Understands layered architecture; can identify dependencies between layers." },
                new() { Level = 2, Description = "Implements use cases, ports, and adapters; keeps domain free of infrastructure." },
                new() { Level = 3, Description = "Architects entire solutions using clean architecture; evangelises the approach." }
            ]
        };

        // ── Java specific skills ───────────────────────────────────────────────

        var javaLanguage = new SkillEntity
        {
            Id = 20,
            Name = "Java Language",
            Category = "Java",
            Description = "Proficiency with Java language features including generics, streams, lambdas, and the module system.",
            LevelCount = 5,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes basic Java with classes, collections, and control flow." },
                new() { Level = 2, Description = "Uses generics, streams, lambdas, and the Optional type effectively." },
                new() { Level = 3, Description = "Applies concurrency primitives and understands the JVM memory model." },
                new() { Level = 4, Description = "Masters modern Java (records, sealed classes, pattern matching)." },
                new() { Level = 5, Description = "Deep JVM knowledge; contributes to open-source or internal library design." }
            ]
        };

        var springBoot = new SkillEntity
        {
            Id = 21,
            Name = "Spring Boot",
            Category = "Java",
            Description = "Building production-ready Spring Boot applications with auto-configuration, starters, and Actuator.",
            LevelCount = 4,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Bootstraps a Spring Boot app; configures basic beans and properties." },
                new() { Level = 2, Description = "Implements REST controllers, security, and persistence with Spring Data." },
                new() { Level = 3, Description = "Configures profiles, custom auto-configuration, and Actuator endpoints." },
                new() { Level = 4, Description = "Optimises startup performance; implements custom starters; deep Spring internals." }
            ]
        };

        var hibernateJpa = new SkillEntity
        {
            Id = 22,
            Name = "Hibernate / JPA",
            Category = "Java",
            Description = "Mapping domain objects to relational databases using JPA annotations and Hibernate.",
            LevelCount = 4,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Maps entities with basic annotations; performs CRUD via EntityManager or Spring Data." },
                new() { Level = 2, Description = "Configures relationships, fetch strategies, and cascade types." },
                new() { Level = 3, Description = "Writes JPQL/Criteria queries; tunes N+1 issues with join fetch." },
                new() { Level = 4, Description = "Implements second-level caching; custom types; advanced schema generation." }
            ]
        };

        var mavenGradle = new SkillEntity
        {
            Id = 23,
            Name = "Maven / Gradle",
            Category = "Tooling",
            Description = "Managing Java project builds, dependencies, and multi-module structures.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Manages dependencies; runs build lifecycle; understands POM/build.gradle basics." },
                new() { Level = 2, Description = "Configures multi-module builds; manages plugin configuration and profiles." },
                new() { Level = 3, Description = "Authors custom plugins; optimises build performance; manages BOM and dependency convergence." }
            ]
        };

        var microservices = new SkillEntity
        {
            Id = 24,
            Name = "Microservices",
            Category = "Architecture",
            Description = "Designing and operating distributed microservice systems with clear service boundaries.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Understands microservice principles; can work within an existing service." },
                new() { Level = 2, Description = "Designs service boundaries; implements async communication (messaging/events)." },
                new() { Level = 3, Description = "Leads microservice decomposition; defines observability and resiliency patterns." }
            ]
        };

        var unitTestingJava = new SkillEntity
        {
            Id = 25,
            Name = "Unit Testing (Java)",
            Category = "Quality & Security",
            Description = "Writing effective tests using JUnit 5, Mockito, and AssertJ.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes basic unit tests with JUnit assertions." },
                new() { Level = 2, Description = "Uses Mockito for mocking; writes parameterised tests; follows AAA pattern." },
                new() { Level = 3, Description = "Practices TDD; builds integration test suites with Testcontainers." }
            ]
        };

        // ── QA specific skills ─────────────────────────────────────────────────

        var testPlanning = new SkillEntity
        {
            Id = 30,
            Name = "Test Planning",
            Category = "QA",
            Description = "Defining test strategy, scope, entry/exit criteria, and risk-based test coverage.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes test cases and checklists for defined acceptance criteria." },
                new() { Level = 2, Description = "Creates a test plan with scope, risks, and coverage matrix." },
                new() { Level = 3, Description = "Defines test strategy across projects; aligns stakeholders on quality goals." }
            ]
        };

        var manualTesting = new SkillEntity
        {
            Id = 31,
            Name = "Manual Testing",
            Category = "QA",
            Description = "Executing exploratory and scripted manual tests; reporting defects with clear reproduction steps.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Executes test cases and logs defects with basic reproduction steps." },
                new() { Level = 2, Description = "Conducts exploratory testing; uses risk-based heuristics; triages defects." },
                new() { Level = 3, Description = "Designs testing charters; mentors others on exploratory techniques." }
            ]
        };

        var automatedTesting = new SkillEntity
        {
            Id = 32,
            Name = "Automated Testing",
            Category = "QA",
            Description = "Designing and maintaining automated test suites integrated into CI/CD pipelines.",
            LevelCount = 4,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Maintains existing automated tests; fixes broken assertions." },
                new() { Level = 2, Description = "Writes new automated tests; integrates with CI pipeline." },
                new() { Level = 3, Description = "Designs maintainable test frameworks; defines automation strategy." },
                new() { Level = 4, Description = "Architects end-to-end automation infrastructure; champions shift-left testing." }
            ]
        };

        var seleniumPlaywright = new SkillEntity
        {
            Id = 33,
            Name = "Selenium / Playwright",
            Category = "QA",
            Description = "Browser automation for end-to-end testing using Selenium WebDriver or Playwright.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Runs existing UI tests; understands locator strategies." },
                new() { Level = 2, Description = "Writes stable UI tests using page object pattern; handles async behaviour." },
                new() { Level = 3, Description = "Designs robust test frameworks; implements cross-browser strategies." }
            ]
        };

        var apiTesting = new SkillEntity
        {
            Id = 34,
            Name = "API Testing",
            Category = "QA",
            Description = "Testing REST APIs with tools like Postman, REST-assured, or Playwright API client.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Executes API tests with Postman; validates status codes and response bodies." },
                new() { Level = 2, Description = "Writes automated API test suites; tests edge cases and error responses." },
                new() { Level = 3, Description = "Designs contract tests; integrates API testing into CI; defines coverage standards." }
            ]
        };

        var performanceTesting = new SkillEntity
        {
            Id = 35,
            Name = "Performance Testing",
            Category = "QA",
            Description = "Load and performance testing to identify bottlenecks under expected and peak load.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Runs basic load tests with k6 or JMeter; reads summary reports." },
                new() { Level = 2, Description = "Designs realistic load scenarios; interprets latency percentiles and error rates." },
                new() { Level = 3, Description = "Correlates results to application metrics; defines SLAs; drives optimisation." }
            ]
        };

        // ── PO & Analysis specific skills ─────────────────────────────────────

        var requirementsEngineering = new SkillEntity
        {
            Id = 40,
            Name = "Requirements Engineering",
            Category = "PO & Analysis",
            Description = "Eliciting, documenting, and validating functional and non-functional requirements.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Documents requirements in a standard format; asks clarifying questions." },
                new() { Level = 2, Description = "Facilitates requirements workshops; distinguishes functional from non-functional." },
                new() { Level = 3, Description = "Defines traceability matrices; manages scope change; aligns on acceptance criteria." }
            ]
        };

        var userStoryWriting = new SkillEntity
        {
            Id = 41,
            Name = "User Story Writing",
            Category = "PO & Analysis",
            Description = "Writing well-formed user stories with clear value statements and testable acceptance criteria.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Writes basic user stories following the As/I want/So that format." },
                new() { Level = 2, Description = "Writes stories with clear acceptance criteria in Given/When/Then format." },
                new() { Level = 3, Description = "Slices epics into independent, negotiable, valuable stories; coaches teams." }
            ]
        };

        var backlogManagement = new SkillEntity
        {
            Id = 42,
            Name = "Backlog Management",
            Category = "PO & Analysis",
            Description = "Maintaining a prioritised, refined, and healthy product backlog.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Adds items to the backlog; participates in grooming sessions." },
                new() { Level = 2, Description = "Prioritises backlog by business value; maintains ready items for two sprints ahead." },
                new() { Level = 3, Description = "Manages multi-team backlog; drives roadmap alignment with stakeholders." }
            ]
        };

        var stakeholderManagement = new SkillEntity
        {
            Id = 43,
            Name = "Stakeholder Management",
            Category = "PO & Analysis",
            Description = "Identifying, engaging, and aligning stakeholders throughout the product lifecycle.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Identifies key stakeholders; communicates sprint outcomes clearly." },
                new() { Level = 2, Description = "Manages competing stakeholder interests; facilitates review sign-off." },
                new() { Level = 3, Description = "Navigates organisational politics; drives consensus on strategic trade-offs." }
            ]
        };

        var businessAnalysis = new SkillEntity
        {
            Id = 44,
            Name = "Business Analysis",
            Category = "PO & Analysis",
            Description = "Analysing business processes, identifying gaps, and proposing solutions that deliver value.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Documents as-is processes; identifies obvious pain points." },
                new() { Level = 2, Description = "Models to-be processes; quantifies business impact of proposed changes." },
                new() { Level = 3, Description = "Leads business capability mapping; aligns solutions to strategic objectives." }
            ]
        };

        var processModelling = new SkillEntity
        {
            Id = 45,
            Name = "Process Modelling",
            Category = "PO & Analysis",
            Description = "Visualising business and system processes using BPMN, flow diagrams, or similar notations.",
            LevelCount = 3,
            LevelDescriptors =
            [
                new() { Level = 1, Description = "Creates basic flow diagrams to illustrate a process." },
                new() { Level = 2, Description = "Models end-to-end processes in BPMN with swim lanes and decision gateways." },
                new() { Level = 3, Description = "Facilitates process improvement workshops; uses models to drive system design." }
            ]
        };

        db.Skills.AddRange(
            cleanCode, solidPrinciples, git, agileScrum, securityAwareness, communication,
            csharp, aspNetCore, entityFramework, restApiDesign, ddd, unitTestingDotNet, cleanArchitecture,
            javaLanguage, springBoot, hibernateJpa, mavenGradle, microservices, unitTestingJava,
            testPlanning, manualTesting, automatedTesting, seleniumPlaywright, apiTesting, performanceTesting,
            requirementsEngineering, userStoryWriting, backlogManagement, stakeholderManagement, businessAnalysis, processModelling);
        await db.SaveChangesAsync();

        // ── Prerequisite links ────────────────────────────────────────────────

        db.SkillPrerequisites.AddRange(
            // DDD requires Clean Code 2 + SOLID 2 + Clean Architecture 1
            new SkillPrerequisiteEntity { SkillId = ddd.Id, RequiredSkillId = cleanCode.Id, RequiredLevel = 2 },
            new SkillPrerequisiteEntity { SkillId = ddd.Id, RequiredSkillId = solidPrinciples.Id, RequiredLevel = 2 },
            new SkillPrerequisiteEntity { SkillId = ddd.Id, RequiredSkillId = cleanArchitecture.Id, RequiredLevel = 1 },
            // Clean Architecture requires SOLID 2
            new SkillPrerequisiteEntity { SkillId = cleanArchitecture.Id, RequiredSkillId = solidPrinciples.Id, RequiredLevel = 2 },
            // Microservices requires REST API Design 2 + Clean Architecture 1
            new SkillPrerequisiteEntity { SkillId = microservices.Id, RequiredSkillId = restApiDesign.Id, RequiredLevel = 2 },
            new SkillPrerequisiteEntity { SkillId = microservices.Id, RequiredSkillId = cleanArchitecture.Id, RequiredLevel = 1 },
            // EF Core requires C# 2
            new SkillPrerequisiteEntity { SkillId = entityFramework.Id, RequiredSkillId = csharp.Id, RequiredLevel = 2 },
            // ASP.NET Core requires C# 2
            new SkillPrerequisiteEntity { SkillId = aspNetCore.Id, RequiredSkillId = csharp.Id, RequiredLevel = 2 },
            // Hibernate/JPA requires Java 2
            new SkillPrerequisiteEntity { SkillId = hibernateJpa.Id, RequiredSkillId = javaLanguage.Id, RequiredLevel = 2 },
            // Spring Boot requires Java 2
            new SkillPrerequisiteEntity { SkillId = springBoot.Id, RequiredSkillId = javaLanguage.Id, RequiredLevel = 2 },
            // Automated Testing requires Manual Testing 2
            new SkillPrerequisiteEntity { SkillId = automatedTesting.Id, RequiredSkillId = manualTesting.Id, RequiredLevel = 2 },
            // Selenium/Playwright requires Automated Testing 1
            new SkillPrerequisiteEntity { SkillId = seleniumPlaywright.Id, RequiredSkillId = automatedTesting.Id, RequiredLevel = 1 }
        );
        await db.SaveChangesAsync();

        // ── Competence Centre Profiles ────────────────────────────────────────

        var dotNetProfile = new CompetenceCentreProfileEntity
        {
            Id = 1,
            Name = ".NET",
            Description = "Full-stack .NET consultant profile covering C#, ASP.NET Core, EF Core, and architecture."
        };
        var javaProfile = new CompetenceCentreProfileEntity
        {
            Id = 2,
            Name = "Java",
            Description = "Full-stack Java consultant profile covering Java, Spring Boot, and microservices."
        };
        var qaProfile = new CompetenceCentreProfileEntity
        {
            Id = 3,
            Name = "QA",
            Description = "Quality assurance profile covering manual, automated, and performance testing."
        };
        var poProfile = new CompetenceCentreProfileEntity
        {
            Id = 4,
            Name = "PO & Analysis",
            Description = "Product owner and business analyst profile covering backlog, requirements, and stakeholder management."
        };

        db.CompetenceCentreProfiles.AddRange(dotNetProfile, javaProfile, qaProfile, poProfile);
        await db.SaveChangesAsync();

        // .NET profile skills
        db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = cleanCode.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = solidPrinciples.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = git.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = agileScrum.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = securityAwareness.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = csharp.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = aspNetCore.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = entityFramework.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = restApiDesign.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = ddd.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = unitTestingDotNet.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = dotNetProfile.Id, SkillId = cleanArchitecture.Id }
        );

        // Java profile skills
        db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = cleanCode.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = solidPrinciples.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = git.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = agileScrum.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = securityAwareness.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = javaLanguage.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = springBoot.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = hibernateJpa.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = mavenGradle.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = microservices.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = unitTestingJava.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = javaProfile.Id, SkillId = restApiDesign.Id }
        );

        // QA profile skills
        db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = agileScrum.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = communication.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = git.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = testPlanning.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = manualTesting.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = automatedTesting.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = seleniumPlaywright.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = apiTesting.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = qaProfile.Id, SkillId = performanceTesting.Id }
        );

        // PO & Analysis profile skills
        db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = agileScrum.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = communication.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = requirementsEngineering.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = userStoryWriting.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = backlogManagement.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = stakeholderManagement.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = businessAnalysis.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = poProfile.Id, SkillId = processModelling.Id }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedTestUsers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // BackOffice admin - no team claim (manages all)
        if (await userManager.FindByEmailAsync("backoffice@test.local") == null)
        {
            var admin = new ForgeUser
            {
                UserName = "backoffice",
                Email = "backoffice@test.local",
                EmailConfirmed = true,
                FirstName = "BackOffice",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(admin, "AdminPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["backoffice"]);
            }
        }

        // Local user for Java team only
        if (await userManager.FindByEmailAsync("java@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "java",
                Email = "java@test.local",
                EmailConfirmed = true,
                FirstName = "Java",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
            }
        }

        // Local user for .NET team only
        if (await userManager.FindByEmailAsync("dotnet@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "dotnet",
                Email = "dotnet@test.local",
                EmailConfirmed = true,
                FirstName = "DotNet",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // User with access to multiple teams (Java + .NET)
        if (await userManager.FindByEmailAsync("multi@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "multi",
                Email = "multi@test.local",
                EmailConfirmed = true,
                FirstName = "Multi",
                LastName = "Team"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // Learner user — assigned to .NET team so CoachNicolas can manage them
        if (await userManager.FindByEmailAsync("learner@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "learner",
                Email = "learner@test.local",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Learner"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "learner");
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET team → CoachNicolas
            }
        }
    }

    private static async Task SeedConsultants(this WebApplication app, AppDbContext db)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // Seed data: (email, firstName, lastName, teamId, daysAgoActivity)
        // null daysAgo = never active (shown as inactive)
        (string Email, string First, string Last, int TeamId, int? DaysAgo)[] consultants =
        [
            // .NET team — coach: dotnet@test.local + CoachNicolas (team 2)
            ("learner@test.local", "Test",    "Learner",      2, 3),    // active — pre-existing user
            ("lea@test.local",     "Lea",     "Van Den Berg", 2, 2),    // active
            ("thomas@test.local",  "Thomas",  "De Smedt",     2, 25),   // inactive — no activity 25 days
            ("amber@test.local",   "Amber",   "Jacobs",       2, 8),    // active
            ("olivier@test.local", "Olivier", "Maes",         2, null), // inactive — never active

            // Java team — coach: java@test.local (team 1)
            ("sander@test.local", "Sander", "Claes",        1, 1),    // active
            ("lucas@test.local",  "Lucas",  "Peeters",      1, 30),   // inactive
            ("emma@test.local",   "Emma",   "Willems",      1, 5),    // active

            // QA team (team 4)
            ("sophie@test.local", "Sophie", "Goossens",     4, 10),   // active
            ("noah@test.local",   "Noah",   "Vermeersch",   4, 28),   // inactive

            // PO & Analysis team (team 3)
            ("julie@test.local",  "Julie",  "Dubois",       3, 3),    // active
            ("max@test.local",    "Max",    "Leemans",      3, null),  // inactive — never active
        ];

        foreach (var (email, first, last, teamId, daysAgo) in consultants)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new ForgeUser
                {
                    UserName = email.Split('@')[0],
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = first,
                    LastName = last,
                };

                var result = await userManager.CreateAsync(newUser, "UserPassword123!");
                if (!result.Succeeded)
                {
                    continue;
                }

                await userManager.AddToRoleAsync(newUser, "learner");
                existingUser = newUser;
            }

            // Create ConsultantProfile if it doesn't exist yet
            if (!await db.ConsultantProfiles.AnyAsync(p => p.UserId == existingUser.Id))
            {
                db.ConsultantProfiles.Add(new ConsultantProfileEntity
                {
                    UserId = existingUser.Id,
                    TeamId = teamId,
                    LastActivityAt = daysAgo.HasValue
                        ? DateTime.UtcNow.AddDays(-daysAgo.Value)
                        : null,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedResources(this WebApplication app, AppDbContext db)
    {
        if (await db.Resources.AnyAsync())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        var backofficeUser = await userManager.FindByEmailAsync("backoffice@test.local");
        if (backofficeUser == null)
        {
            return;
        }

        var userId = backofficeUser.Id;

        db.Resources.AddRange(
            new ResourceEntity
            {
                Title = "Clean Code by Robert C. Martin",
                Url = "https://www.goodreads.com/book/show/3735293-clean-code",
                Type = "book",
                SkillId = 1, // Clean Code
                FromLevel = 1,
                ToLevel = 2,
                AddedByUserId = userId,
            },
            new ResourceEntity
            {
                Title = "SOLID Principles in C# – YouTube Playlist",
                Url = "https://www.youtube.com/results?search_query=SOLID+principles+csharp",
                Type = "video",
                SkillId = 2, // SOLID Principles
                FromLevel = 1,
                ToLevel = 3,
                AddedByUserId = userId,
            },
            new ResourceEntity
            {
                Title = "Microsoft Docs – ASP.NET Core overview",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core",
                Type = "article",
                SkillId = 11, // ASP.NET Core
                FromLevel = 1,
                ToLevel = 2,
                AddedByUserId = userId,
            },
            new ResourceEntity
            {
                Title = "Domain-Driven Design by Eric Evans",
                Url = "https://www.domainlanguage.com/ddd/",
                Type = "book",
                SkillId = 14, // Domain-Driven Design
                FromLevel = 2,
                ToLevel = 3,
                AddedByUserId = userId,
            },
            new ResourceEntity
            {
                Title = "Pro Git – free online book",
                Url = "https://git-scm.com/book/en/v2",
                Type = "book",
                SkillId = 3, // Git & Version Control
                AddedByUserId = userId,
            }
        );

        await db.SaveChangesAsync();
    }
}
