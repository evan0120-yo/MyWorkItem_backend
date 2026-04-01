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
- 目前只建立專案結構與啟動設定，尚未開始實作業務流程

## 目前專案方向

- .NET 8
- ASP.NET Core Web API
- Controllers
- Swagger
- EF Core
- PostgreSQL
- 測試專案已先建立

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
   └─ FlowTests
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

目前因為尚未開始實作 DbContext 與 migration，API 專案本身先不依賴資料表。

## 測試方式

```powershell
cd Backend
dotnet test .\MyWorkItem.sln
```

目前測試專案只有專案結構與參考關係，尚未加入流程測試內容。

## 下一步建議

- 先補 BDD / SDD / TDD 文件
- 再依流程逐條落實 UseCase / Service / Repository
- 每完成一條流程，就補上對應流程測試
