# DNP1 Forum

Forum-app i stil med Reddit, bygget gennem DNP1-afleveringerne (VIA, 3. semester).

## Assignment 1 - Entities and Repositories

### Domain model

```mermaid
classDiagram
    class User {
        int Id
        string Username
        string Password
    }
    class Post {
        int Id
        string Title
        string Body
        int UserId
    }
    class Comment {
        int Id
        string Body
        int PostId
        int UserId
    }
    User "1" -- "0..*" Post : writes
    User "1" -- "0..*" Comment : writes
    Post "1" -- "0..*" Comment : has
```

Relationer er modelleret med fremmednøgler (`UserId`, `PostId`), ikke associationer.

### Projekter

| Projekt | Indhold | Afhænger af |
|---|---|---|
| `Server/Entities` | `User`, `Post`, `Comment` | - |
| `Server/RepositoryContracts` | `IUserRepository`, `IPostRepository`, `ICommentRepository` | Entities |
| `Server/InMemoryRepositories` | Implementeringer der gemmer i en `List` | Entities, RepositoryContracts |
