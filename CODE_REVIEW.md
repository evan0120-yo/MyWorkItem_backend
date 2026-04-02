# Backend CODE REVIEW

## BLOCK 1: AI 對產品的想像

我現在看到的後端，比較像一個內部使用的 Work Item 狀態系統。

它的主軸很明確：
- 管理者維護 Work Item 主資料
- 一般使用者各自保存自己對 Work Item 的確認狀態

它刻意不走很重的架構路線，重點是：
- UseCase 好讀
- 流程清楚
- API 可以直接跑
- 個人狀態和主資料責任分開

我對它的規模想像是小到中型服務，不像完整平台。

它不是什麼：
- 不是正式 JWT / OAuth 系統
- 不是 DDD 或事件驅動系統
- 不是已經有 pagination、search、audit trail 的完整版本
- 不是 migration 驅動的正式上線版本

## BLOCK 2: 讀者模式

### 1. 啟動與身分

這個 API 啟動後，會把 DbContext、Repository、Service、UseCase 接起來，直接連 PostgreSQL，並且開一條給本機前端聯調用的 CORS policy。

目前沒有正式登入。每次 request 直接從 header 讀使用者資訊：

```text
Request
  │
  ├─ 有 X-Mock-User-Id / X-Mock-Role 嗎？
  │    ├─ 沒有 -> unauthorized
  │    └─ 有 -> 解析目前使用者
  │
  └─ 進入對應 UseCase
```

> 注意: 這不是 token auth，也沒有 session。

> 注意: `Users` 表目前不是完整帳號系統。它主要是為了個人狀態關聯存在，而且只有 confirm 流程會主動補或更新 user record。

> 注意: CORS 目前只放行本機前端開發來源 `http://localhost:5173` / `http://127.0.0.1:5173`。這是因為前端 request 會帶 `X-Mock-*` headers，瀏覽器一定會先做 preflight。

### 2. 前台查詢

前台查詢有兩條：列表和詳情。

列表的邏輯是：
- 先確認現在是誰
- 讀出 Work Item 主資料
- 讀這個人自己的狀態
- 把兩邊合併回 response

```text
使用者看列表
     │
     ├─ 解析目前使用者
     ├─ 讀 WorkItems
     ├─ 讀這個 user 的 statuses
     └─ 合併成 items[id, title, status]
```

如果這個人沒有任何狀態資料，系統就把該筆視為 `Pending`。

詳情頁也是同樣思路，只是從一批 item 變成單筆 item。

```text
使用者看詳情
     │
     ├─ 解析目前使用者
     ├─ 讀目標 Work Item
     │    ├─ 不存在 -> not found
     │    └─ 存在 -> 繼續
     ├─ 讀這個 user 對這筆 item 的 status
     └─ 回傳 detail
```

> 注意: `status` 永遠是目前這個人的狀態，不是全體共用狀態。

> 注意: 列表只支援 `asc / desc` 排序方向切換，沒有 pagination / search / filter。

### 3. 確認與撤銷確認

確認流程不會改 Work Item 主資料，它只會改 `UserWorkItemStatuses`。

```text
使用者送出 confirm
     │
     ├─ 驗證 workItemIds
     │    ├─ null / 空陣列 / Guid.Empty -> validation error
     │    └─ 合法 -> 繼續
     ├─ 解析目前使用者
     ├─ 讀 request 內所有 Work Item
     │    ├─ 少任何一筆 -> not found，整批失敗
     │    └─ 全存在 -> 繼續
     ├─ 確保 Users 表有這個 user
     ├─ 已有 status？
     │    ├─ 有 -> 更新成 Confirmed
     │    └─ 沒有 -> 新增一筆 Confirmed
     └─ 存檔
```

撤銷確認更保守，它只改目前這個人的那筆狀態。

```text
使用者送出 revert
     │
     ├─ 解析目前使用者
     ├─ 讀目標 Work Item
     │    ├─ 不存在 -> not found
     │    └─ 存在 -> 繼續
     ├─ 讀這個 user 的 status
     │    ├─ 有 -> 改成 Pending
     │    └─ 沒有 -> 不寫資料
     └─ 回傳 Pending
```

> 注意: confirm 是整批全有或全無，沒有部分成功。

> 注意: confirm 目前會把 user upsert 與 status 寫入收斂到同一次 `SaveChanges`；在 PostgreSQL 這會落在同一個 EF Core transaction 內，但仍未補 provider-level integration tests 去驗證真正的 transaction 行為。

### 4. 管理端 CRUD

管理端目前只有 create、update、delete，沒有額外的 admin list API。

所有 admin 路徑都會先驗證角色是不是 `Admin`。

```text
admin API
    │
    ├─ 先看目前角色是不是 Admin
    │    ├─ 否 -> forbidden
    │    └─ 是 -> 繼續
    │
    ├─ create -> 驗證 title -> 建立 Work Item
    ├─ update -> 驗證 title -> 找目標 -> 更新主資料
    └─ delete -> 找目標 -> hard delete
```

> 注意: create 現在回 `201 Created`，`Location` 會指向 `/api/work-items/{id}`；update 維持 `200 OK`。

> 注意: delete 目前是真刪除。刪掉後，列表和詳情都不會再查到；關聯的 `UserWorkItemStatuses` 也會 cascade delete。

### 5. 錯誤與狀態

這個後端的錯誤出口很一致。業務流程主要是丟應用層例外，其他未預期例外也會被 middleware 接住，最後統一轉成 HTTP response。

```text
AppValidationException   -> 400
AppUnauthorizedException -> 401
AppForbiddenException    -> 403
AppNotFoundException     -> 404
其他未預期例外            -> 500
```

狀態變化目前其實很簡單：

```text
PersonalStatus
  Pending -> Confirmed
  Confirmed -> Pending

WorkItem
  create -> update -> hard delete
```

> 注意: `WorkItem` 本身沒有獨立狀態欄位；這裡描述的是資料生命週期，不是 entity 內真的有一個 state machine。

> 注意: 現在沒有 soft delete、沒有 archived、沒有 draft。

> 注意: 啟動初始化目前走 `EnsureCreated`，不是 migration pipeline。

## BLOCK 3: 技術補充

### 1. 啟動與身分

#### 1.1 啟動 wiring

```text
Program.cs
  -> AddControllers + JsonStringEnumConverter
  -> AddDbContext(MyWorkItemDbContext, UseNpgsql)
  -> DI:
     - ICurrentUserAccessor -> HttpContextCurrentUserAccessor
     - repositories / services / validators / usecases
  -> ExceptionHandlingMiddleware
  -> Swagger (development only)
  -> MapControllers()
  -> InitializeDatabaseAsync()
```

#### 1.2 mock auth 來源

Headers:

| Header | 用途 |
| --- | --- |
| `X-Mock-User-Id` | 目前使用者 id |
| `X-Mock-User-Name` | 目前使用者名稱 |
| `X-Mock-Role` | `User` / `Admin` |

行為：
- 少 `UserId` 或 `Role` -> 回傳 `null`
- `Role` 解析失敗 -> 回傳 `null`
- `UserName` 空白 -> fallback 成 `UserId`
- `DisplayName` 目前直接跟 `UserName` 一樣

`CurrentUser` 結構：

```text
CurrentUser
  - UserId
  - UserName
  - DisplayName
  - Role
```

#### 1.3 啟動資料庫初始化

`DatabaseInitializationExtensions.InitializeDatabaseAsync()` 目前只做：

```text
CreateScope
  -> resolve MyWorkItemDbContext
  -> Database.EnsureCreatedAsync()
```

目前真相：
- 有 DbContext
- 沒有 migration class
- 啟動時直接確保 schema 存在

### 2. 前台查詢

#### 2.1 列表 API

Route:
- `GET /api/work-items`

Call chain:

```text
WorkItemsController.ListAsync
  -> ListWorkItemsUseCase.ExecuteAsync
     -> ListWorkItemsRequestValidator.ValidateAndGetSortDirection
     -> CurrentUserService.GetRequiredCurrentUserAsync
     -> WorkItemQueryService.GetListAsync
        -> WorkItemRepository.ListAsync
        -> UserWorkItemStatusRepository.GetByUserAndWorkItemIdsAsync
```

列表 response:

| Field | 來源 |
| --- | --- |
| `id` | `WorkItems.Id` |
| `title` | `WorkItems.Title` |
| `status` | 個人狀態；無資料時 `Pending` |

`sortDirection` 規則：
- 空白或沒帶 -> 預設 `desc`
- `asc` / `desc` -> 合法
- 其他值 -> `400 ValidationProblemDetails`

#### 2.2 詳情 API

Route:
- `GET /api/work-items/{id}`

Call chain:

```text
WorkItemsController.GetDetailAsync
  -> GetWorkItemDetailUseCase.ExecuteAsync
     -> CurrentUserService.GetRequiredCurrentUserAsync
     -> WorkItemQueryService.GetDetailAsync
        -> WorkItemRepository.GetByIdAsync(asTracking: false)
        -> UserWorkItemStatusRepository.GetByUserAndWorkItemIdAsync(asTracking: false)
```

detail response:

| Field | 來源 |
| --- | --- |
| `id` | `WorkItems.Id` |
| `title` | `WorkItems.Title` |
| `description` | `WorkItems.Description` |
| `createdAt` | `WorkItems.CreatedAt` |
| `updatedAt` | `WorkItems.UpdatedAt` |
| `status` | 個人狀態；無資料時 `Pending` |

### 3. 確認與撤銷確認

#### 3.1 confirm API

Route:
- `POST /api/work-items/confirm`

Call chain:

```text
WorkItemsController.ConfirmAsync
  -> ConfirmWorkItemsUseCase.ExecuteAsync
     -> ConfirmWorkItemsRequestValidator.ValidateAndGetDistinctIds
     -> CurrentUserService.GetRequiredCurrentUserAsync
     -> WorkItemStatusService.ConfirmAsync
        -> WorkItemRepository.GetByIdsAsync
        -> EnsureUserRecordAsync
           -> UserRepository.GetByIdAsync
           -> UserRepository.AddAsync
        -> UserWorkItemStatusRepository.GetByUserAndWorkItemIdsAsync(asTracking: true)
        -> UserWorkItemStatusRepository.AddRangeAsync
        -> UserWorkItemStatusRepository.SaveChangesAsync
```

`workItemIds` 規則：
- 不可為 `null`
- 不可空
- 不可含 `Guid.Empty`
- 會做去重複

#### 3.2 revert API

Route:
- `POST /api/work-items/{id}/revert-confirmation`

Call chain:

```text
WorkItemsController.RevertConfirmationAsync
  -> RevertWorkItemConfirmationUseCase.ExecuteAsync
     -> CurrentUserService.GetRequiredCurrentUserAsync
     -> WorkItemStatusService.RevertAsync
        -> WorkItemRepository.GetByIdAsync
        -> UserWorkItemStatusRepository.GetByUserAndWorkItemIdAsync
        -> UserWorkItemStatusRepository.SaveChangesAsync (只有原本有 status 才會寫)
```

### 4. 管理端 CRUD

#### 4.1 create API

Route:
- `POST /api/admin/work-items`

Call chain:

```text
AdminWorkItemsController.CreateAsync
  -> CreateWorkItemUseCase.ExecuteAsync
     -> CurrentUserService.EnsureAdminAsync
     -> CreateWorkItemRequestValidator.Validate
     -> WorkItemCommandService.CreateAsync
```

目前狀態碼：
- `201 Created`

#### 4.2 update API

Route:
- `PUT /api/admin/work-items/{id}`

Call chain:

```text
AdminWorkItemsController.UpdateAsync
  -> UpdateWorkItemUseCase.ExecuteAsync
     -> CurrentUserService.EnsureAdminAsync
     -> UpdateWorkItemRequestValidator.Validate
     -> WorkItemCommandService.UpdateAsync
```

目前狀態碼：
- `200 OK`

#### 4.3 delete API

Route:
- `DELETE /api/admin/work-items/{id}`

Call chain:

```text
AdminWorkItemsController.DeleteAsync
  -> DeleteWorkItemUseCase.ExecuteAsync
     -> CurrentUserService.EnsureAdminAsync
     -> WorkItemCommandService.DeleteAsync
```

目前狀態碼：
- `204 No Content`

### 5. 目前狀態、測試與 gap

#### 5.1 錯誤回應格式

middleware 最後會回兩種格式：

| 類型 | 何時使用 | 格式 |
| --- | --- | --- |
| Validation error | `AppValidationException` | `ValidationProblemDetails` |
| 其他錯誤 | `401 / 403 / 404 / 500` | `ProblemDetails` |

回應特徵：
- `ValidationProblemDetails`
  - `title = Validation failed.`
  - `detail = exception.Message`
  - `instance = request path`
  - `errors = 欄位錯誤集合`
- `ProblemDetails`
  - `title = Request failed.`
  - `detail = exception.Message`，但 `500` 會固定成 `An unexpected error occurred.`
  - `instance = request path`

#### 5.2 DbContext 與資料表約束

目前 entity 配置重點：

| Entity | 重要約束 |
| --- | --- |
| `WorkItemEntity` | `Title` max 200, required |
| `WorkItemEntity` | `Description` max 2000, required |
| `WorkItemEntity` | `CreatedAt` / `UpdatedAt` required |
| `UserEntity` | `Id` max 100 |
| `UserEntity` | `UserName` / `DisplayName` max 100, required |
| `UserWorkItemStatusEntity` | composite key = `(UserId, WorkItemId)` |
| `UserWorkItemStatusEntity` | `Status` 以 string 形式存 DB |
| `UserWorkItemStatusEntity` | `UpdatedAt` required |

關聯：
- `Users -> UserWorkItemStatuses` cascade delete
- `WorkItems -> UserWorkItemStatuses` cascade delete

目前資料表：

| Table | 用途 |
| --- | --- |
| `WorkItems` | 主資料 |
| `Users` | 使用者關聯資料 |
| `UserWorkItemStatuses` | 每個 user 對每個 Work Item 的個人狀態 |

#### 5.3 目前測試與限制

目前已有測試：
- UseCase flow tests
- Service tests
- Auth tests
- 上述測試目前都透過 `TestDependencyFactory + EF Core InMemory provider` 執行

目前限制：
- 沒有正式 JWT / token auth
- 沒有 pagination / search / filter
- 沒有 admin list API
- 沒有 migration 檔，只有 `EnsureCreated`
- 目前沒有 PostgreSQL integration tests，所以 Npgsql / SQL translation / FK / transaction 行為還沒被自動測試覆蓋
- create validator / update validator 現在已對齊 DB 長度限制：`Title <= 200`、`Description <= 2000`
- create 目前回 `201 Created`
- confirm 已收斂成同一次 `SaveChanges`，但尚未有 PostgreSQL integration test 去驗證 transaction 邊界
- `DisplayName` 目前沒有獨立來源
