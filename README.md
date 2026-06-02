# AI skills

Skills for agent working

work with [ai-skills-manager](https://github.com/InsonusK/ai-skills-manager)

# Dotnet
## 1. Контекст архитектурных принципов (backend)
### 1.1 Общая архитектурная модель
- **Pragmatic Modular Monolith**  
    Основная архитектура системы.  
    Монолит с четкими модулями (bounded contexts), без преждевременного разбиения на микросервисы.    
- **Future Microservice Readiness (non-invasive)**  
    Модули проектируются как кандидаты на будущие сервисы, но без:
    - разрыва FK        
    - отказа от JOIN        
    - введения distributed patterns без необходимости   

---

### 1.2 Модульность и границы
- **Controlled Module Boundaries**  
    Модули изолированы логически, но не физически.  
    Разрешено:
    - cross-module reads (JOIN, specifications)
    Ограничено:    
    - cross-module writes (только через владельца агрегата)
- **Shared Relational Model**  
    Единая БД для всех модулей.  
    FK между модулями допустимы и используются как часть модели.

---

### 1.3 CQRS модель
- **Behavioral CQRS (не strict CQRS)**    
    - Commands изменяют состояние и могут возвращать результат        
    - Queries не изменяют бизнес-данные        

---

### 1.4 Транзакции и согласованность
- **Transactional Consistency First**  
    Приоритет ACID транзакций внутри монолита.  
    Eventual consistency не используется без необходимости.    
- **Reservation-Based Process Consistency**  
    Для сложных бизнес-процессов используется:    
    - резервирование ресурсов        
    - промежуточные статусы        
    - финализация операции
        
---

### 1.5 Интеграции и события
- **Transactional Outbox Pattern**  
    Integration events сохраняются в БД в одной транзакции с изменениями.    
- **Event Separation Model**    
    - Domain Events → внутри агрегата/модуля        
    - Application Notifications → внутри приложения        
    - Integration Events → внешние системы (Kafka и т.д.)        

---

### 1.6 Application слой (MediatR)
- **MediatR Pipeline Architecture**  
    Поток обработки:  
    API → Command/Query → Pipeline → Handler    
- Pipeline включает:    
    - validation        
    - transaction boundary        
    - logging / cross-cutting concerns        

---

### 1.7 CQRS реализация
- **Commands**    
    - изменяют состояние        
    - возвращают результат (updated state allowed)        
- **Queries**    
    - только чтение        
    - допускается логирование/метрики, но не бизнес-изменения        

---

### 1.8 Domain Model подход
- **Hybrid Domain Model**  
    Комбинация:    
    - rich domain (инварианты внутри entity)        
    - anemic patterns (для orchestration и сложной логики)        
- **Thin Handler Pattern**  
    Handler = orchestration слой, не содержит бизнес-логики.    
- **Focused Domain Services**  
    Используются только для:    
    - cross-aggregate logic        
    - orchestration        
    - сложных вычислений        

---

### 1.9 Validation модель
- **Layered Validation Strategy**    
    - Input validation (DTO / FluentValidation)        
    - Domain validation (invariants)        
    - Business process validation (workflow rules)        

---

### 1.10 Query / Read model

- **Specification Pattern (Ardalis.Specification)**  
    Используется для:
    
    - reusable queries
        
    - cross-module reads
        
    - complex filtering logic
        

---

### 1.11 API архитектура

- **Controller vs Minimal API split**
    
    - Controllers → entity/CRUD operations
        
    - Minimal API → processes, integrations, utilities
        

---

## 2. Контекст работы со Skill системой

---

### 2.1 Общая модель skills

- **Skill System as Execution Graph**  
    Skills не просто документы, а узлы графа исполнения:
    
    - определяют поведение агента
        
    - формируют pipeline выполнения задачи
        

---

### 2.2 Типы skills

- **Declarative Skills**  
    Определяют:
    
    - архитектурные ограничения
        
    - правила системы
        
    - invariants  
        ❗ не выполняют работу, только задают рамки
        
- **Workflow Skills**  
    Определяют:
    
    - последовательность действий
        
    - разбиение задачи
        
    - выбор downstream skills  
        ❗ не содержат реализации
        
- **Developing Skills**  
    Определяют:
    
    - конкретные способы реализации кода
        
    - шаблоны создания объектов (Controller, Command, Handler и т.д.)
        

---

### 2.3 Execution model

- **Feature-first workflow**  
    Любая задача начинается с:
    
    - анализа (workflow skill)
        
    - затем перехода в chain workflow → developing skills
        
- **Skill chaining**  
    Workflow skill:
    
    - не реализует систему
        
    - выбирает следующие skills
        

---

### 2.4 Skill boundaries (ключевой принцип)

- Workflow skills:
    
    - orchestration only
        
    - no business logic definition
        
    - no implementation rules
        
- Developing skills:
    
    - only implementation rules
        
    - no architectural decisions
        
- Declarative skills:
    
    - only constraints and invariants
        
    - no steps or implementation
        

---

### 2.5 Skill linking model

- **Stable ID-based linking**
    
    - primary identifier = GUID or stable id
        
    - human name = mutable
        
    - reference format: `use skill [name](skill:id)`
        

---

### 2.6 Skill design principles

- Skills должны быть:
    
    - компактными
        
    - детерминированными
        
    - без лишнего текста
        
    - строго scoped
        
- Один skill = одна ответственность
    
- Skills не дублируют друг друга
    

---

### 2.7 Workflow design principle

- Workflow skill = orchestration layer
    
- Workflow skill:
    
    - анализирует задачу
        
    - определяет тип изменений
        
    - выбирает downstream skills
        
    - формирует execution plan
        
    - НЕ пишет код
        

---

### 2.8 Dependency model

- Skills имеют:
    
    - `requires` (dependency graph)
        
    - `triggers` (activation conditions)
        
- Dependency resolution:
    
    - based on stable IDs
        
    - not on names
        

---

Если хочешь, следующий шаг логично сделать так:  
собрать **минимальный набор developing skills (5–7 штук)** и от них построить первый полный execution pipeline.