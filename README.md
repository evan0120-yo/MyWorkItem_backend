# Backend

## 目前內容

- 文件：
  - Backend/BDD.md
  - Backend/SDD.md
  - Backend/TDD.md
  - Backend/Development.md
  - Backend/Question.md
- Solution：Backend/MyWorkItem.sln
- API 專案：Backend/MyWorkItem.Api/MyWorkItem.Api.csproj
- 測試專案：Backend/MyWorkItem.Tests/MyWorkItem.Tests.csproj
- 已完成：
  - mock auth
  - Work Item user flows
  - admin CRUD
  - UseCase flow tests

## 目前專案方向

- .NET 8
- ASP.NET Core Web API
- Controllers
- Swagger
- EF Core
- PostgreSQL
- mock auth 由 request headers 提供

## 專案資料夾

```text
Backend
├─ BDD.md
├─ Development.md
├─ Question.md
├─ README.md
├─ SDD.md
├─ TDD.md
├─ MyWorkItem.sln
├─ MyWorkItem.Api
│  ├─ Module
│  │  └─ WorkItem
│  │     ├─ Controller
│  │     ├─ Dto
│  │     ├─ Entity
│  │     ├─ Repository
│  │     ├─ Service
│  │     ├─ Usecase
│  │     └─ Validator
│  ├─ Auth
│  ├─ Infrastructure
│  ├─ Migrations
│  ├─ Properties
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  └─ MyWorkItem.Api.csproj
└─ MyWorkItem.Tests
   ├─ FlowTests
   ├─ ServiceTests
   ├─ AuthTests
   └─ TestSupport
```

## 命名補充

- `.Api` 是常見 C# / ASP.NET Core 命名慣例。
- 這版主專案採 `MyWorkItem.Api`，測試專案採 `MyWorkItem.Tests`。
- `bin` / `obj` 會出現在各自專案根層，這是正常建置輸出，不是業務資料夾。

## PostgreSQL 預設連線

目前先寫在：

- Backend/MyWorkItem.Api/appsettings.Development.json

預設值如下：

```text
Host=localhost
Port=5432
Database=MyWorkItem
Username=postgres
Password=sky77619
```

如果你本機資料庫名稱不同，只要改 `Database` 即可。

## Phase 1 mock auth headers

目前 API 會從 request headers 讀取目前使用者資訊：

```text
X-Mock-User-Id
X-Mock-User-Name
X-Mock-Role
```

角色目前支援：

```text
User
Admin
```

範例：

```text
X-Mock-User-Id: user-1
X-Mock-User-Name: user-1
X-Mock-Role: User
```

## 本機前後端聯調 CORS

API 目前已允許：

- http://localhost:5173
- http://127.0.0.1:5173

原因是前端開發站與 API 不同 origin，而且 request 會帶：

- X-Mock-User-Id
- X-Mock-User-Name
- X-Mock-Role

瀏覽器會先送 CORS preflight，所以 API 端必須明確放行本機開發來源與 request headers。

## 啟動前準備

1. 確認本機已安裝 .NET 8 SDK
2. 確認本機 PostgreSQL 已啟動
3. 若本機還沒有 `MyWorkItem` 資料庫，先建立一個

可用其中一種方式建立資料庫：

```sql
CREATE DATABASE "MyWorkItem";
```

或使用命令列：

```powershell
createdb -U postgres MyWorkItem
```

## 啟動方式

在專案根目錄執行：

```powershell
cd Backend
dotnet restore .\MyWorkItem.sln
dotnet build .\MyWorkItem.sln
dotnet run --project .\MyWorkItem.Api\MyWorkItem.Api.csproj
```

啟動後可用：

- http://localhost:5032/swagger
- https://localhost:7119/swagger

## 測試方式

```powershell
cd Backend
dotnet test .\MyWorkItem.sln
```

目前已加入 40 個測試，包含：

- UseCase flow tests
- Auth tests
- Service tests

目前這批 `dotnet test` 會透過 EF Core InMemory provider 跑，不直接連 PostgreSQL。

原因是先保住 UseCase / Service / Auth 的快速回饋，並降低日常測試對本機資料庫狀態的耦合。

這不代表 PostgreSQL 可以不測；Repository / DbContext / API 層的 PostgreSQL integration tests 仍需另外補。

## 下一步建議

- 接著補 migration 與資料初始化策略
- 補 controller / API 層級整合測試
- 若要進入 Phase 2，再補 JWT 與正式授權
