# Backend TDD

## 文件定位

- 本文件定義後端目前的測試設計與驗證策略。
- 本文件中的 `TDD` 代表本版測試驅動與測試設計規劃。
- 本文件目標不是要求所有測試都先於 production code 完成，而是要求：
  - 先有測試設計
  - 每條流程完成時立即補齊對應測試
  - 測試直接對應 UseCase 與可觀察行為

## 本版測試原則

```text
先文件
│
├─ BDD
├─ SDD
└─ TDD

再進實作
│
├─ 每次只進一條流程
├─ 該流程一次做到位
├─ 測試緊跟該流程補上
└─ 通過後再進下一條
```

- 不採「全部測試先寫完再開始實作」。
- 不採「先寫一堆程式最後才補測試」。
- 不接受只有表面 API 通路，沒有對應流程測試。

## 測試範圍

### 第一優先

- UseCase flow tests
- Service 層關鍵規則單元測試
- 權限與錯誤邏輯測試

### 第二優先

- Repository 與 DbContext 的整合測試

### 目前不優先

- Controller 細碎單元測試
- 純框架行為測試

## 測試專案結構

```text
Backend/MyWorkItem.Api.Tests
├─ FlowTests
│  ├─ WorkItems
│  └─ AdminWorkItems
├─ ServiceTests
├─ AuthTests
└─ TestSupport
```

## 測試分層策略

### FlowTests

- 以 UseCase 為主。
- 驗證：
  - 流程順序結果
  - 資料寫入結果
  - 權限結果
  - 錯誤出口

### ServiceTests

- 驗證單一步驟邏輯。
- 例如：
  - 狀態切換規則
  - 更新欄位規則
  - 建立資料時欄位組裝規則

### AuthTests

- 驗證 mock auth 邏輯。
- 驗證：
  - user 解析
  - role 解析
  - unauthorized / forbidden 行為

## 測試資料策略

```text
測試資料來源
│
├─ 使用最小必要資料集
│  ├─ users
│  ├─ workItems
│  └─ user statuses
│
├─ 每個測試只建立該案例需要的資料
└─ 不共用會互相污染的 mutable state
```

- 使用者至少準備：
  - 一位 `User`
  - 一位 `Admin`
- Work Item 測試資料需能覆蓋：
  - Pending
  - Confirmed
  - 不存在的 id

## 優先流程測試清單

### T-001 ListWorkItemsUseCase

```text
要驗證
│
├─ 預設排序為 createdAt desc
├─ 指定 asc / desc 可切換
├─ 每筆結果都帶目前使用者的 status
└─ 沒有個人狀態時預設為 Pending
```

### T-002 GetWorkItemDetailUseCase

```text
要驗證
│
├─ 存在資料時能回傳 detail DTO
├─ DTO 內含 createdAt / updatedAt / status
└─ 不存在時回傳 not found
```

### T-003 ConfirmWorkItemsUseCase

```text
要驗證
│
├─ 可一次確認多筆 Work Item
├─ ids 為空時回傳 validation error
├─ request 中任一 id 不存在時整批失敗
├─ 已存在狀態時不建立重複資料
└─ 寫入後 status 為 Confirmed
```

### T-004 RevertWorkItemConfirmationUseCase

```text
要驗證
│
├─ 可把目前使用者狀態改回 Pending
├─ 不影響其他使用者狀態
└─ 目標不存在時回傳 not found
```

### T-005 CreateWorkItemUseCase

```text
要驗證
│
├─ Admin 可成功建立
├─ title 為空時回傳 validation error
├─ createdAt / updatedAt 正確寫入
└─ User 角色不可建立
```

### T-006 UpdateWorkItemUseCase

```text
要驗證
│
├─ Admin 可成功更新 title / description
├─ updatedAt 需更新
├─ title 為空時回傳 validation error
├─ 目標不存在時回傳 not found
└─ User 角色不可更新
```

### T-007 DeleteWorkItemUseCase

```text
要驗證
│
├─ Admin 可成功刪除
├─ 刪除後列表查不到
├─ 刪除後詳情查不到
├─ 不存在時回傳 not found
└─ User 角色不可刪除
```

## 關鍵 Service 測試清單

### WorkItemStatusService

- 已存在個人狀態時，confirm 需走更新而非重複建立。
- revert 後狀態需為 Pending。
- 只允許改動目前使用者自己的狀態資料。

### WorkItemCommandService

- create 時需正確填入 createdAt / updatedAt。
- update 時只更新允許欄位。
- delete 的資料存取行為需和 Repository 契約一致。

### CurrentUserService

- 能解析目前使用者資訊。
- 角色判斷錯誤時能正確丟出 forbidden。

## 錯誤測試矩陣

```text
400
├─ title 空白
└─ confirm ids 空陣列

401
└─ 無法取得目前使用者資訊

403
└─ User 呼叫 admin API / admin use case

404
├─ detail id 不存在
├─ update id 不存在
├─ delete id 不存在
└─ confirm request 含不存在的 id

500
└─ 未預期例外需能被統一轉換
```

## 測試命名規則

```text
方法名格式
  MethodName_Scenario_ExpectedResult
```

範例：

- `ListWorkItems_WhenNoUserStatusExists_ReturnsPendingStatus`
- `ConfirmWorkItems_WhenAnyIdDoesNotExist_ThrowsNotFound`
- `DeleteWorkItem_WhenCallerIsUser_ThrowsForbidden`

## 測試實作原則

- 每條流程至少有：
  - 成功案例
  - validation 案例
  - 權限案例
  - not found 案例
- 測試名稱直接反映行為。
- Arrange / Act / Assert 要清楚分段。
- 若測試資料過多，優先抽到 `TestSupport`，不要把大量 setup 混在測試主體。

## 執行順序建議

```text
先做
│
├─ ListWorkItemsUseCase
├─ ConfirmWorkItemsUseCase
├─ RevertWorkItemConfirmationUseCase
├─ GetWorkItemDetailUseCase
├─ CreateWorkItemUseCase
├─ UpdateWorkItemUseCase
└─ DeleteWorkItemUseCase
```

- 這個順序對應目前最核心的資料流：
  - 先查
  - 再改個人狀態
  - 最後做管理資料流

## 測試完成定義

- Flow test 通過。
- 錯誤出口有覆蓋。
- 權限出口有覆蓋。
- 測試名稱可直接對應 UseCase 行為。
- 測試結果可回頭驗證 BDD 的 Given / When / Then。

## 待確認但不阻塞本版

- 分頁若之後納入需求，需補 list 相關測試。
- 若 delete 最終改為 soft delete，需補刪除後資料可見性測試。
- 若 auth 從 mock 改為 JWT，需補 auth provider 與 claims mapping 測試。
