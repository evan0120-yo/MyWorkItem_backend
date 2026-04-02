# Backend Question Notes

## 目的

- 記錄目前討論過、但不屬於考題明文規範的後端選型與待確認事項
- 後續可作為討論與決策紀錄

## 目前已定

- 技術方向：ASP.NET Core Web API
- .NET 版本：.NET 8
- API 風格：Controllers
- 資料存取：EF Core
- 資料庫：PostgreSQL
- 使用者識別：Mock login / user switcher
- 權限策略：
  - Phase 1：Mock 權限，先滿足核心功能
  - Phase 2：若有時間，再補 JWT
- 測試方向：先做後端流程測試
- 測試實作：
  - FlowTests / ServiceTests / AuthTests 目前先用 EF Core InMemory provider 跑
  - PostgreSQL integration tests 仍要另外補
- 架構方向：一般三層架構 + UseCase 層
- 本機前後端聯調：
  - API 需允許 `http://localhost:5173` 與 `http://127.0.0.1:5173` 的 CORS
  - request 會帶 `X-Mock-*` headers，所以必須能通過 browser preflight
- admin create API 回 `201 Created`
- create / update request validation 已對齊 DB 長度限制：
  - `Title <= 200`
  - `Description <= 2000`

## 目前共識

- 沒有明文要求 DDD
- 不採完整 DDD
- 採一般台灣市場較常見的三層架構思路
- 額外導入 UseCase 層，對應 PRD 的流程概念
- 需要把選型理由保留下來，方便後續延續

## 討論筆記

- PostgreSQL 是因為本機已有環境，先以可快速啟動為優先
- PostgreSQL 若有需要可透過 pgvector extension 儲存向量資料
- SQL Server 目前也有 vector data type，但主要適用於 SQL Server 2025 Preview / Azure SQL 相關環境，不先假設本機舊版 SQL Server 可直接使用
- 不先做完整正式權限系統，先以 mock 權限完成主要流程
- 若時間允許，再進入 Phase 2 補 JWT 驗證
- 測試先採 InMemory provider：
  - 理由是先保住 UseCase / Service / Auth 測試的回饋速度
  - 理由是降低日常測試對本機 PostgreSQL 狀態的耦合
  - 目前主要先驗證流程順序、權限邊界、錯誤出口
- 這不代表 PostgreSQL 可以不測：
  - Npgsql provider 行為仍需測
  - SQL translation / FK / constraint / transaction / migration 仍需另外補 integration tests
- confirm 流程目前已把 user upsert 與 status 寫入收斂成同一次 `SaveChanges`
  - 對 relational provider 來說會落在同一個 EF Core transaction
  - 但仍未補 PostgreSQL integration tests 驗證 provider 實際行為
- 本機前後端聯調要補 CORS：
  - 因為 Frontend `http://localhost:5173` 與 Backend `http://localhost:5032` 是跨 origin
  - 前端 request 會帶 `X-Mock-User-Id` / `X-Mock-User-Name` / `X-Mock-Role`
  - 瀏覽器會先送 preflight，所以 API 端必須明確放行來源、method 與 headers
- UseCase 層的理由：
  - 更貼近 PRD 與流程圖
  - 單元 / 流程測試更容易直接對應 UseCase
  - 對 AI 掃描程式碼更友善，因為流程集中，降低跨層跳讀成本
  - 若每行補足註解，別人只看 UseCase 也能快速理解整體流程
  - 目前導入 UseCase 的主要目的不是繼續把 Service 切細
- UseCase 層期望：
  - 盡量完整對應 PRD 的流程概念
  - 重要流程程式碼需加註解
  - 目標是讓 UseCase 成為接近「程式碼版架構圖」的入口
- 我們要的三層實際語意是：
  - Controller
  - UseCase
  - Service
  - Repository
- 主流程責任鏈定義為：
  - Controller -> UseCase -> Service -> Repository
- Validators 保留：
  - 集中做 request / DTO 驗證
- Auth 也保留，而且和其他主要資料夾同階層：
  - 理由不是現在就要完整 JWT
  - 而是使用者識別 / 角色 / 權限邊界本來就獨立存在
  - Phase 1 放 mock auth
  - Phase 2 再補 JWT
  - 讓 UseCase 只依賴 current user abstraction，不直接依賴 token 細節
- Service 目前維持較寬的行為實作層：
  - 這次調整主要先解決流程對應與測試對應問題
  - 不先把重點放在處理 Service 肥大
  - 若再往下切更細，會增加 junior 閱讀成本
  - 若後續明確知道團隊採完整 DDD 或 domain 規則顯著變複雜，再評估往下細拆
- 後端專案命名採：
  - `MyWorkItem.Api`
  - `MyWorkItem.Tests`
- `.Api` 放在主 API 專案名稱是合理的：
  - 這是常見命名慣例
  - 也能清楚區分主 API 專案與測試專案
- 目前傾向 `單一 API 專案 + 單一測試專案 + Module 結構`：
  - Backend/MyWorkItem.Api
  - Backend/MyWorkItem.Tests
  - MyWorkItem.Api/Module/WorkItem
  - MyWorkItem.Api/Auth
  - MyWorkItem.Api/Infrastructure
  - MyWorkItem.Api/Migrations
  - MyWorkItem.Api/Program.cs
  - MyWorkItem.Api/appsettings.json
- `bin` / `obj` 留在各自專案根層是正常的建置輸出，不屬於業務資料夾

## 待確認

- admin 端是否一定要有 UI，還是只有 API 也可接受
- 詳情頁的 status 是個人狀態還是 Work Item 共用狀態
- 分頁是否為必做項
- 刪除 Work Item 後，使用者個人狀態資料如何處理
- 第 8 頁架構圖畫的是 Relational Database，是否代表偏好關聯式資料庫，或只是示意

## 待我們自己決定

- Service 與 UseCase 的邊界
- UseCase / Services / Repositories 的命名規則
- Auth 內部資料夾是否要從一開始就拆成：
  - Mock
  - Jwt
  - Abstractions
- PostgreSQL 連線方式與本機啟動方式

## UseCase 形狀共識

- UseCase 是流程編排層
- Service 是行為實作層
- Repository 是資料存取層
- UseCase 應盡量做到：
  - 一行對應一個主要行為
  - 直接對應 PRD / 流程圖
  - 讀 UseCase 就能快速理解主流程
- UseCase 只保留：
  - 流程順序
  - if / else
  - 分支控制
  - 成功 / 失敗出口
- 具體行為實作優先下放到 Service
- 目前 UseCase + Service 的關係是：
  - UseCase 負責把流程顯性化
  - Service 負責承接每個流程步驟的實作
  - 不要求現在就把 Service 拆成更細小的結構

### 支付流程範例

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
