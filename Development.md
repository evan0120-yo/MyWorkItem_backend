# Backend Development

## AI 目前認知

如果現在直接開始開發，我腦中的後端專案模型如下：

- 這是一個以 `Work Item 主資料 + 使用者個人確認狀態` 為核心的 API 專案。
- 後端真正需要處理的是兩層資料責任：
  - 管理員維護的 Work Item 主資料
  - 一般使用者對每個 Work Item 的個人化確認結果
- 我不會把此專案做成 DDD 風格，也不會做過度抽象。
- 我會把它做成 `一般三層架構 + UseCase 層居中` 的 .NET 專案。
- 我心中的三層不是理論化的多專案切法，而是更市場常見的：
  - Controller
  - UseCase
  - Service
  - Repository
- 我會把 UseCase 視為整個後端最重要的閱讀入口：
  - Controller 只負責 HTTP 邊界
  - UseCase 負責流程編排
  - Service 負責具體行為實作
  - Repository 只負責資料存取
  - 流程、規則、分支、組裝都集中在 UseCase
- 我認知中的主流程關係是：
  - `Controller -> UseCase -> Service -> Repository`
- 我會保留 `Auth` 作為獨立區塊：
  - 不是因為 Phase 1 就要做完整 JWT
  - 而是因為使用者識別 / 角色 / 權限邊界本來就會存在
  - Phase 1 放 mock auth
  - Phase 2 再補 JWT
- 這樣 UseCase 只依賴目前使用者抽象，不直接依賴 token 細節。
- 如果一個人只看 UseCase，就應該能快速理解系統主要流程。
- 因為你明確要求 UseCase 要對應 PRD 流程，而且要對 AI 掃描友善，所以我會把後端設計重心放在：
  - UseCase 命名清楚
  - 流程步驟清楚
  - 註解密度高
  - 減少跨層跳讀
- 我會預設 Phase 1 先完成可運作的 mock 權限版本：
  - 先讓 user / admin 流程都能跑
  - 不先把時間花在完整 auth framework 上
- PostgreSQL 在此專案中只是穩定可用的持久層，不是要展示資料庫花樣。
- 測試我會優先壓在 UseCase 層，因為那裡最接近實際流程，也最符合你要的維護方式。
- 測試我會先分兩層：
  - 第一層用 EF Core InMemory provider 保住 UseCase / Service / Auth 的快速回饋
  - 第二層再補 PostgreSQL integration tests，驗證 Npgsql / SQL translation / transaction 等 provider-specific 行為
- 前後端本機聯調會是跨 origin：
  - Frontend 在 `http://localhost:5173`
  - Backend 在 `http://localhost:5032`
  - 而且 request 會帶 `X-Mock-*` headers，所以 API 端必須補 dev CORS policy
- 如果沒有新的決策，我會自然朝這個方向寫：
  - `Controller -> UseCase -> Service -> Repository`
  - `EF Core + PostgreSQL`
  - `Mock User Context`
  - `以流程測試保住主要用例`

## 來源依序

1. PDF
2. MyWorkItem_PDF_Readthrough.md
3. MyWorkItem_Missing_And_Odd_Info.md
4. Backend/Question.md
5. 本文件

## 後端目標

- `來源明文`
  - 提供 Work Item 相關資料存取與 API
  - 支援前台使用者查看 Work Item 列表
  - 支援前台使用者查看 Work Item 詳情
  - 支援前台使用者確認 / 撤銷確認
  - 支援管理員新增 / 修改 / 刪除 Work Item
  - 狀態需持久化
- `已確認`
  - 技術方向：ASP.NET Core Web API
  - API 風格：Controllers
  - .NET 版本：.NET 8
  - 資料存取：EF Core
  - 資料庫：PostgreSQL
  - Phase 1：Mock 權限 / user switcher
  - Phase 2：若有時間，再補 JWT
  - 測試方向：先做後端流程測試
  - 測試實作：FlowTests / ServiceTests / AuthTests 先用 InMemory provider；PostgreSQL integration tests 後補

## 架構原則

- `已確認`
  - 不採完整 DDD
  - 採一般三層架構思路
  - 額外導入 UseCase 層
  - UseCase 層需盡量對應 PRD 流程概念
  - UseCase 層需採高密度註解，讓讀者單看 UseCase 也能理解主流程
- `AI 推估 / 待覆核`
  - 先採單一 API 專案 + 單一測試專案
  - 不先拆多個 class library
  - 主 API 專案放在 `Backend/MyWorkItem.Api`
  - 測試專案放在 `Backend/MyWorkItem.Tests`
  - 以 `Module` 管理 API 專案內的業務資料夾
  - `.Api` 是常見命名慣例，放在主 API 專案名稱是合理的
  - 這版採 `MyWorkItem.Api + MyWorkItem.Tests`，讓主專案與測試專案角色明確分離
  - 核心責任鏈固定為：
    - Controller -> UseCase -> Service -> Repository
  - 目前導入 UseCase 的主要目的不是進一步切細 Service
  - 目前導入 UseCase 的主要目的是：
    - 直接對應 PRD 流程
    - 直接對應流程測試 / 單元測試
    - 讓人與 AI 掃 UseCase 就能快速理解主流程
  - Service 仍維持較寬的行為實作層
  - 先不為了解決 Service 肥大問題而額外往 DDD / 更細 module 切
  - 原因是目前優先順序仍是：
    - 流程可讀
    - 測試可對應
    - junior 可讀
    - 結構不過度抽象
  - 若後續明確出現更複雜的 domain 規則，或團隊本身採完整 DDD，再考慮往下細拆

## 專案切分

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
│  ├─ FlowTests
│  ├─ ServiceTests
│  ├─ AuthTests
│  └─ TestSupport
└─ MyWorkItem.sln
```

- `AI 推估 / 待覆核`
  - `MyWorkItem.Api/Module/WorkItem/Controller`：只負責 HTTP 邊界、request/response、授權入口、錯誤轉換
  - `MyWorkItem.Api/Module/WorkItem/Usecase`：放完整流程型用例與流程編排
  - `MyWorkItem.Api/Module/WorkItem/Service`：放具體行為實作、共用商業動作與可重用邏輯
  - `MyWorkItem.Api/Module/WorkItem/Repository`：放資料存取
  - `MyWorkItem.Api/Auth`：放 current user accessor、mock auth、後續 JWT、policy 相關邊界
  - `MyWorkItem.Api/Module/WorkItem/Validator`：放 request / DTO 驗證
  - `MyWorkItem.Api/Infrastructure`：放 DbContext、EF Core 設定、其他外部依賴整合
  - `Program.cs` / `appsettings*.json` / `.csproj` 留在 `MyWorkItem.Api` 根層是正常的 API 專案結構
  - `bin` / `obj` 是建置輸出，不是業務結構的一部分，會留在 `MyWorkItem.Api` 或 `MyWorkItem.Tests` 內但應由 `.gitignore` 忽略

## UseCase 設計方向

- `已確認`
  - UseCase 需對應 PRD 流程圖概念
  - 需對 AI 掃描友善
  - 需對流程測試友善
  - 需對單元測試對應友善
- `AI 推估 / 待覆核`
  - 命名以「動作 + 對象」為主
  - 一個 UseCase 對應一條主要流程
  - Controller 不直接寫商業流程
  - UseCase 不直接實作細節行為
  - Service 不吞掉完整流程型邏輯
  - Repository 不承擔流程分支
  - UseCase 原則上不直接碰 Repository
  - UseCase 透過 Service 完成主要行為

### 建議 UseCase 清單

- `來源明文 + AI 推估整理`
  - ListWorkItemsUseCase
  - GetWorkItemDetailUseCase
  - ConfirmWorkItemsUseCase
  - RevertWorkItemConfirmationUseCase
  - CreateWorkItemUseCase
  - UpdateWorkItemUseCase
  - DeleteWorkItemUseCase

### UseCase 內部基本形態

```text
進入 UseCase
│
├─ 驗證輸入
├─ 取得目前使用者資訊
├─ 讀取主資料 / 關聯狀態
├─ 執行流程分支
├─ 寫回資料
└─ 組 response DTO
```

### UseCase 範例模型

以下範例不是此專案的實際功能，而是用來說明 UseCase 應長什麼樣子。

```text
Controller
  -> PayOrderUseCase

PayOrderUseCase
  1. 驗證訂單存在
  2. 驗證支付條件
  3. 建立支付請求
  4. 寫入支付紀錄
  5. 發送 MQ
  6. 回傳結果

然後每一行會去 call：
  orderService
  paymentService
  paymentRepository
  mqService
```

```csharp
public async Task<PayOrderResult> ExecuteAsync(PayOrderCommand command)
{
    // 1. 驗證訂單存在
    var order = await _orderService.GetRequiredOrderAsync(command.OrderId);

    // 2. 驗證支付條件
    await _paymentService.ValidatePayableAsync(order);

    // 3. 建立支付請求
    var payment = await _paymentService.CreatePaymentAsync(order, command);

    // 4. 寫入支付紀錄
    await _paymentService.SavePaymentAsync(payment);

    // 5. 發送 MQ
    await _paymentService.PublishPaymentCreatedAsync(payment);

    // 6. 回傳結果
    return _paymentService.BuildResult(payment);
}
```

- `已確認`
  - 這就是本專案對 UseCase 的目標形狀
  - UseCase 要能直接對應流程圖
  - 讀 UseCase 應能快速理解主流程
  - 要看實作細節再往下看 Service / Repository
  - UseCase 的價值是把流程顯性化，不是要把 Service 完全拆薄

## Service 目前定位

- `已確認`
  - Service 目前先維持較寬的行為實作層
  - 不把 Service 再往更細的 domain service / manager / handler 類型拆開
  - 目前不把「解決 Service 肥大」當成第一優先
- `AI 推估 / 待覆核`
  - 現階段 Service 的存在價值是：
    - 承接 UseCase 每一步驟的具體實作
    - 統一封裝可重用行為
    - 視需要再往 Repository / Auth / 外部整合呼叫
  - 這樣做的前提是：
    - UseCase 已經負責主要流程對應
    - 測試主要壓在 UseCase
    - 流程圖與程式碼入口已能對上
  - 因此目前先接受 Service 仍可能偏大
  - 若未來出現更複雜的 domain 邏輯，再評估是否引入更細分層

## 資料模型

- `來源明文`
  - Work Item 有主資料
  - 每位使用者對 Work Item 有個人化確認狀態
- `AI 推估 / 待覆核`
  - 主要表：
    - Users
    - WorkItems
    - UserWorkItemStatuses

### AI 推估資料欄位

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

- `已確認`
  - Work Item Detail 顯示的 Status 目前以使用者個人狀態回傳
  - 刪除策略目前為 hard delete
  - 刪除後 UserWorkItemStatuses 由資料庫關聯一併刪除

## API 邊界

- `來源明文`
  - 前台至少有列表、詳情、確認、撤銷確認
  - 後台至少有新增、修改、刪除
- `AI 推估 / 待覆核`
  - 建議 API：
    - `GET    /api/work-items`
    - `GET    /api/work-items/{id}`
    - `POST   /api/work-items/confirm`
    - `POST   /api/work-items/{id}/revert-confirmation`
    - `POST   /api/admin/work-items`
    - `PUT    /api/admin/work-items/{id}`
    - `DELETE /api/admin/work-items/{id}`

## 權限策略

- `已確認`
  - Phase 1 使用 Mock 權限
  - 目標是先完成主要功能流程
- `AI 推估 / 待覆核`
  - Mock 權限先放在 `Auth` 區塊
  - Mock 權限目前由 request header 傳入 user identity
  - admin 與 user 角色先用 mock role 區分
  - Phase 2 若補 JWT，優先沿用同一組 current user abstraction
  - UseCase 不直接碰 token 字串或 claims parsing

### Auth 區塊方向

- `AI 推估 / 待覆核`
  - Auth 是權限與目前使用者資訊的邊界，不只是放 token
  - Phase 1：
    - MockUserContextProvider
    - CurrentUserAccessor
    - Mock role 判斷
  - Phase 2：
    - JwtOptions
    - JwtCurrentUserAccessor
    - claims mapping
    - policy / role rules
  - 目標是讓 Phase 1 -> Phase 2 的切換，不需要大量改 UseCase

## 驗證與錯誤處理

- `來源明文`
  - 標題不得為空
  - 更新失敗需顯示錯誤訊息
  - 刪除失敗需顯示錯誤提示
- `AI 推估 / 待覆核`
  - UseCase / Service 層統一做輸入驗證
  - middleware 統一轉成 HTTP response
  - 用 ProblemDetails 或一致錯誤格式回傳

## 測試方向

- `已確認`
  - 先做後端流程測試
- `AI 推估 / 待覆核`
  - 優先測 UseCase
  - 先不以 Controller 測試為主

### 優先測項

- ListWorkItemsUseCase
- ConfirmWorkItemsUseCase
- RevertWorkItemConfirmationUseCase
- CreateWorkItemUseCase
- UpdateWorkItemUseCase
- DeleteWorkItemUseCase

## 待確認

- admin 是否一定要有 UI
- 分頁是否必做
- 第 8 頁 Relational Database 是否只是示意

## 開發模式

- `已確認`
  - 具體操作細節以既有 skill 為準
  - 本文件只保留高層流程
  - 開發順序先走：
    - 需求確認
    - 補 BDD / SDD / TDD 文件
    - 依文件實作
    - 補 code review 文件
- `AI 推估 / 待覆核`
  - 此專案採 `文件先行 -> 實作 -> 回寫文件` 的模式
  - 不強制整個專案採嚴格的 executable test-first
  - 但也不採完全先寫程式、最後再補測試
  - 後端較適合的方式是：

```text
確認需求
│
├─ 補齊未定義點與決策
├─ 寫 BDD
├─ 寫 SDD
├─ 寫 TDD 文件
├─ 選定一條要開發的垂直流程
├─ 依文件把該流程一次做完
│  ├─ Controller
│  ├─ UseCase
│  ├─ Service
│  ├─ Repository
│  ├─ Validator / Auth / Error Handling
│  ├─ 必要的資料表變更
│  └─ 該流程對應測試
├─ 跑測試
└─ 回寫 code review 文件
```

- `AI 推估 / 待覆核`
  - 這裡的重點是 `完整切片開發 + test-close-following`
  - 也就是：
    - TDD 文件先寫
    - 每次只進一條流程
    - 但該流程進入開發後，就在既定範圍內一次到位
    - 該流程完成前，不切去下一條
  - 目前不主張一開始就把所有 executable unit test 全寫完再開始寫程式
  - 目前也不接受先鋪骨架、placeholder、假流程，之後再回頭補實作
  - 原因是：
    - 在 AI 協作下，若允許先寫骨架，後續最常發生的是表面流程存在，但細節永遠補不齊
    - 若一開始把 executable test 寫得過滿，容易出現只為了過測試而過度貼合測試的實作
    - 若測試先寫得太細，程式結構還沒穩時，會增加反覆修改測試的成本
    - 若完全不先整理測試，又容易讓 AI 偏離 BDD / SDD / TDD 文件
  - 因此目前建議是：
    - `文件先定義`
    - `每次只選一條流程`
    - `選中的流程在範圍內一次做完`
    - `測試緊跟該流程補上`
    - `通過後再進下一條流程`
  - 這種方式在 AI 偷懶條件下的優點是：
    - 仍有文件約束，不容易亂補需求
    - 仍有測試約束，不容易只做表面通路
    - 不會因先鋪骨架而留下大量「之後再補」的技術債
    - 不會因過早寫滿測試而讓 AI 過度針對測試樣板優化
  - 後端測試仍以 UseCase / 流程對應為主，不追求所有層都先寫 test-first

## 開發順序

```text
Phase 1
│
├─ 建立 Solution / Projects
├─ 建 DbContext / Migration / Seed
├─ 建 Mock User Context
├─ 先完成前台查詢與確認流程
├─ 再完成 admin CRUD
└─ 補流程測試

Phase 2
│
├─ JWT
├─ 真正角色驗證
└─ 補更多錯誤處理與安全細節
```
