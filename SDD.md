# Backend SDD

## 文件定位

- 本文件定義後端的結構設計與模組責任。
- 本文件目標是讓後續實作時，能直接對應：
  - Controller
  - UseCase
  - Service
  - Repository
  - Auth
  - Validator
- 本文件不追求理論化分層，而是採目前已定的實作用語。

## 來源明文

- 技術方向：
  - ASP.NET Core Web API
  - .NET 8
  - EF Core
  - PostgreSQL
- 權限策略：
  - Phase 1 使用 mock 權限 / user switcher
  - Phase 2 視情況補 JWT
- 結構方向：
  - `Controller -> UseCase -> Service -> Repository`

## 本版設計總覽

```text
HTTP Request
│
├─ Controller
│  ├─ 接 request DTO
│  ├─ 做 model binding
│  ├─ 轉呼叫 UseCase
│  └─ 把結果轉成 HTTP response
│
├─ UseCase
│  ├─ 表達流程順序
│  ├─ 控制成功 / 失敗出口
│  ├─ 組裝主要分支
│  └─ 呼叫 Service 完成每一步驟
│
├─ Service
│  ├─ 實作單一步驟行為
│  ├─ 封裝共用邏輯
│  ├─ 視需要呼叫 Repository
│  └─ 視需要呼叫 Auth / 外部整合
│
└─ Repository
   ├─ 讀寫資料庫
   ├─ 查詢 Work Item
   └─ 查詢 / 寫入個人狀態
```

## 專案結構

```text
Backend
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
├─ MyWorkItem.Tests
│  └─ FlowTests
└─ MyWorkItem.sln
```

## 各層責任

### Controllers

- 只負責 HTTP 邊界。
- 不寫業務流程。
- 不直接操作 Repository。
- 負責：
  - Request DTO 進入點
  - 呼叫 UseCase
  - 成功回應轉換
  - 錯誤回應轉換

### UseCases

- UseCase 是本專案主要閱讀入口。
- 每個 UseCase 對應一條主要流程。
- 主要責任：
  - 流程編排
  - 分支控制
  - 呼叫順序
  - 成功 / 失敗出口
- UseCase 原則：
  - 一行對應一個主要行為
  - 不下沉資料存取細節
  - 不把整條流程吞進 Service

### Services

- Service 是行為實作層。
- Service 目前維持較寬的實作責任，不再細拆更多層。
- 主要責任：
  - 驗證單一步驟
  - 封裝共用邏輯
  - 組裝 entity / state 變更
  - 呼叫 Repository
  - 呼叫 Auth 抽象
- 不負責：
  - 完整流程編排
  - HTTP 邊界

### Repositories

- Repository 只做資料存取。
- 主要責任：
  - Work Item 查詢
  - Work Item 新增 / 更新 / 刪除
  - 個人狀態查詢 / 寫入
- 不負責：
  - 流程分支
  - 權限判斷
  - HTTP 回應

### Auth

- Auth 是身分與角色的邊界。
- Phase 1 放 mock auth。
- Phase 2 可沿用同一個邊界補 JWT。
- UseCase 與 Service 只依賴目前使用者抽象，不依賴 token 字串細節。

### Validator

- Validator 集中處理 request / DTO 驗證。
- 驗證失敗直接回到應用層錯誤，不進資料寫入。

## 專案根層說明

- `MyWorkItem.Api/MyWorkItem.Api.csproj`
  - 後端主 API 專案。
- `MyWorkItem.Tests/MyWorkItem.Tests.csproj`
  - 後端測試專案。
- `.Api`
  - 是常見命名慣例。
- `Program.cs` / `appsettings*.json`
  - 正常位於 `MyWorkItem.Api` 專案根層。
- `bin` / `obj`
  - 正常位於各自專案根層的建置輸出。
  - 不屬於業務模組結構。

## 模組清單

### Controllers

```text
WorkItemsController
├─ GET    /api/work-items
├─ GET    /api/work-items/{id}
├─ POST   /api/work-items/confirm
└─ POST   /api/work-items/{id}/revert-confirmation

AdminWorkItemsController
├─ POST   /api/admin/work-items
├─ PUT    /api/admin/work-items/{id}
└─ DELETE /api/admin/work-items/{id}
```

### UseCases

```text
ListWorkItemsUseCase
GetWorkItemDetailUseCase
ConfirmWorkItemsUseCase
RevertWorkItemConfirmationUseCase
CreateWorkItemUseCase
UpdateWorkItemUseCase
DeleteWorkItemUseCase
```

### Services

```text
WorkItemQueryService
WorkItemCommandService
WorkItemStatusService
CurrentUserService
```

- `WorkItemQueryService`
  - 處理列表與詳情查詢的組裝。
- `WorkItemCommandService`
  - 處理建立、更新、刪除 Work Item 主資料。
- `WorkItemStatusService`
  - 處理確認、撤銷確認、狀態查詢。
- `CurrentUserService`
  - 封裝目前使用者資訊讀取與角色檢查。

### Repositories

```text
IWorkItemRepository
IUserWorkItemStatusRepository
IUserRepository
```

## 資料模型

```text
WorkItems
├─ Id
├─ Title
├─ Description
├─ CreatedAt
└─ UpdatedAt

Users
├─ Id
├─ UserName
└─ DisplayName

UserWorkItemStatuses
├─ UserId
├─ WorkItemId
├─ Status
└─ UpdatedAt
```

## 資料關係

```text
Users 1 --- n UserWorkItemStatuses n --- 1 WorkItems
```

- `WorkItems`
  - 代表主資料。
- `UserWorkItemStatuses`
  - 代表個人化狀態。
- 列表與詳情 API 需要把兩者組裝後回傳給目前使用者。

## API 設計

### GET /api/work-items

```text
輸入
  - sortDirection 可選

輸出
  - items[]
    - id
    - title
    - status
```

### GET /api/work-items/{id}

```text
輸出
  - id
  - title
  - description
  - createdAt
  - updatedAt
  - status
```

### POST /api/work-items/confirm

```text
輸入
  - workItemIds[]

輸出
  - confirmedCount
  - status
```

### POST /api/work-items/{id}/revert-confirmation

```text
輸入
  - path id

輸出
  - workItemId
  - status
```

### POST /api/admin/work-items

```text
輸入
  - title
  - description

輸出
  - id
  - title
  - description
  - createdAt
  - updatedAt
```

### PUT /api/admin/work-items/{id}

```text
輸入
  - path id
  - title
  - description

輸出
  - id
  - title
  - description
  - updatedAt
```

### DELETE /api/admin/work-items/{id}

```text
輸入
  - path id

輸出
  - no content 或一致化成功結果
```

## 核心流程設計

### ListWorkItemsUseCase

```text
進入 UseCase
│
├─ 解析目前使用者
├─ 驗證排序參數
├─ 讀取 Work Item 主資料清單
├─ 讀取目前使用者的狀態清單
├─ 依 workItemId 合併主資料與個人狀態
└─ 回傳列表 DTO
```

### GetWorkItemDetailUseCase

```text
進入 UseCase
│
├─ 解析目前使用者
├─ 讀取 Work Item 主資料
├─ 若不存在 -> not found
├─ 讀取目前使用者對該 Work Item 的狀態
├─ 合併 detail DTO
└─ 回傳結果
```

### ConfirmWorkItemsUseCase

```text
進入 UseCase
│
├─ 驗證 request workItemIds
├─ 解析目前使用者
├─ 讀取 request 內所有 Work Item
├─ 若有缺漏 -> not found
├─ 建立或更新目前使用者的 Confirmed 狀態
├─ 寫回資料
└─ 回傳成功結果
```

### RevertWorkItemConfirmationUseCase

```text
進入 UseCase
│
├─ 驗證 path id
├─ 解析目前使用者
├─ 讀取目標 Work Item
├─ 若不存在 -> not found
├─ 將目前使用者狀態寫回 Pending
└─ 回傳成功結果
```

### Create / Update / Delete WorkItem UseCase

```text
共通流程
│
├─ 驗證目前角色為 Admin
├─ 驗證 request
├─ 讀取或建立目標主資料
├─ 呼叫 command service 完成資料變更
├─ 寫回資料
└─ 回傳成功結果
```

## 驗證策略

```text
Controller
  -> 只處理 model binding 層級失敗

Validator
  -> request / DTO 規則

UseCase / Service
  -> 流程條件與資源存在性驗證
```

- `title`
  - 不可為空
  - 不可全空白
- `confirm request`
  - workItemIds 不可為空
- `update / delete / detail`
  - 目標 Work Item 必須存在

## 錯誤處理

```text
ValidationException
  -> 400

UnauthorizedException
  -> 401

ForbiddenException
  -> 403

NotFoundException
  -> 404

UnexpectedException
  -> 500
```

- Controller 層負責把應用層錯誤轉成 HTTP response。
- 回應格式需一致。

## Auth 設計

### Phase 1

```text
Request
  -> Mock auth provider
    -> 解析目前 userId / role
      -> 提供給 UseCase / Service 使用
```

- 本版只要求目前使用者抽象可用。
- 不要求正式登入。

### Phase 2

```text
Request
  -> JWT auth
    -> claims mapping
      -> current user abstraction
        -> UseCase / Service
```

- Phase 2 只替換 auth 來源。
- UseCase 不應因 auth 來源切換而重寫流程。

## 非功能方向

- 以可讀性與流程對應為優先。
- 不先導入完整 DDD。
- 不先為了抽象而拆多專案。
- 測試優先壓在 UseCase。

## 待確認但不阻塞本版

- 分頁是否為必做。
- admin 是否一定要配 UI。
- detail `status` 是否最終仍採個人狀態。
- delete 最終要採 hard delete 或 soft delete。
- mock auth 最終要從 header、query 或其他來源讀取。
