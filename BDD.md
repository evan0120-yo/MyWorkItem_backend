# Backend BDD

## 文件定位

- 本文件定義後端目前要達成的可觀察行為。
- 本文件以 API 行為、資料變化、權限判斷、錯誤回應為主。
- 本文件目標是讓後續 UseCase、流程測試、驗收檢查能直接對應。
- 本文件不寫佔位章節，也不保留「之後再補」的空白區塊。

## 來源明文

- Work Item 需要支援：
  - 前台列表
  - 前台詳情
  - 確認
  - 撤銷確認
  - 後台新增
  - 後台修改
  - 後台刪除
- 狀態需持久化。
- 每位使用者只能看到自己的確認結果。
- 後台管理需要新增 / 修改 / 刪除 Work Item。

## 本版實作基線

- Phase 1 使用 mock 使用者識別與 mock 角色。
- Phase 1 的 mock 身分資訊由 request headers 提供：
  - `X-Mock-User-Id`
  - `X-Mock-User-Name`
  - `X-Mock-Role`
- 角色分成：
  - `User`
  - `Admin`
- Work Item 主資料與使用者個人確認狀態分開保存。
- 詳情頁 `status` 先採「目前使用者的個人確認狀態」解讀。
- 刪除 Work Item 目前採 hard delete。
- 本版先不把分頁寫進後端行為定義。

## 名詞定義

- `Work Item`
  - 由管理者維護的主資料。
- `Personal Status`
  - 某一位使用者對某一筆 Work Item 的個人確認狀態。
- `Pending`
  - 尚未確認。
- `Confirmed`
  - 已確認。

## 系統總規則

```text
Work Item 資料責任
│
├─ 主資料
│  ├─ id
│  ├─ title
│  ├─ description
│  ├─ createdAt
│  └─ updatedAt
│
└─ 個人狀態
   ├─ userId
   ├─ workItemId
   └─ status
```

- 前台 API 回傳的 `status` 都以目前使用者視角解讀。
- 同一筆 Work Item 可被不同使用者看到不同的 `status`。
- `Confirm` 與 `Revert` 只影響目前使用者自己的狀態，不改動 Work Item 主資料。
- admin API 只修改 Work Item 主資料，不直接操作個人確認狀態。

## 使用者流程

### B-USER-001 列出 Work Item

```text
Given
  目前請求已帶有可識別的使用者身分
When
  使用者呼叫取得 Work Item 列表 API
Then
  API 回傳 Work Item 清單
  每筆資料至少包含：
    - id
    - title
    - status
  status 依目前使用者的個人狀態組裝
  若使用者尚未確認該 Work Item
    -> status = Pending
  預設排序為 createdAt 由新到舊
```

### B-USER-002 列表支援切換排序方向

```text
Given
  目前請求已帶有可識別的使用者身分
When
  使用者以排序參數要求列表升序或降序
Then
  API 依要求回傳排序後的結果
  若未提供排序參數
    -> 使用預設 createdAt desc
```

### B-USER-003 取得 Work Item 詳情

```text
Given
  目前請求已帶有可識別的使用者身分
  且該 Work Item 存在
When
  使用者呼叫 Work Item 詳情 API
Then
  API 回傳：
    - id
    - title
    - description
    - createdAt
    - updatedAt
    - status
  其中 status 以目前使用者的個人狀態回傳
```

### B-USER-004 取得不存在的 Work Item 詳情

```text
Given
  目前請求已帶有可識別的使用者身分
  且該 Work Item 不存在
When
  使用者呼叫 Work Item 詳情 API
Then
  API 回傳 not found
  不回傳部分成功資料
```

### B-USER-005 批次確認 Work Item

```text
Given
  目前請求已帶有可識別的使用者身分
  且 request 內包含一組 Work Item ids
  且 request 內所有 id 都存在
When
  使用者呼叫確認 API
Then
  request 內每一筆 Work Item 對目前使用者都被設為 Confirmed
  已經是 Confirmed 的資料仍維持 Confirmed
  不建立重複的個人狀態資料
```

### B-USER-006 批次確認時 ids 為空

```text
Given
  目前請求已帶有可識別的使用者身分
  且 request 內沒有任何 Work Item id
When
  使用者呼叫確認 API
Then
  API 回傳 validation error
  不寫入任何資料
```

### B-USER-007 批次確認含不存在的 Work Item

```text
Given
  目前請求已帶有可識別的使用者身分
  且 request 內至少有一筆 id 不存在
When
  使用者呼叫確認 API
Then
  API 回傳 not found
  本次 request 不做部分成功
  本次 request 不寫入任何資料
```

### B-USER-008 撤銷確認

```text
Given
  目前請求已帶有可識別的使用者身分
  且目標 Work Item 存在
When
  使用者呼叫撤銷確認 API
Then
  目前使用者對該 Work Item 的狀態變為 Pending
  其他使用者的狀態不受影響
  Work Item 主資料不受影響
```

### B-USER-009 撤銷確認不存在的 Work Item

```text
Given
  目前請求已帶有可識別的使用者身分
  且目標 Work Item 不存在
When
  使用者呼叫撤銷確認 API
Then
  API 回傳 not found
```

## 管理流程

### B-ADMIN-001 建立 Work Item

```text
Given
  目前請求具有 Admin 角色
  且 title 合法
When
  管理者呼叫建立 API
Then
  系統建立新的 Work Item 主資料
  新資料至少包含：
    - id
    - title
    - description
    - createdAt
    - updatedAt
  API 回傳建立成功結果
```

### B-ADMIN-002 建立 Work Item 時 title 為空

```text
Given
  目前請求具有 Admin 角色
  且 title 為空或全空白
When
  管理者呼叫建立 API
Then
  API 回傳 validation error
  不建立任何資料
```

### B-ADMIN-003 更新 Work Item

```text
Given
  目前請求具有 Admin 角色
  且目標 Work Item 存在
  且 title 合法
When
  管理者呼叫更新 API
Then
  系統更新該 Work Item 主資料
  updatedAt 需更新
  API 回傳更新成功結果
```

### B-ADMIN-004 更新不存在的 Work Item

```text
Given
  目前請求具有 Admin 角色
  且目標 Work Item 不存在
When
  管理者呼叫更新 API
Then
  API 回傳 not found
```

### B-ADMIN-005 刪除 Work Item

```text
Given
  目前請求具有 Admin 角色
  且目標 Work Item 存在
When
  管理者呼叫刪除 API
Then
  API 回傳刪除成功結果
  後續列表查詢不再出現該 Work Item
  後續詳情查詢不可再取得該 Work Item
```

### B-ADMIN-006 刪除不存在的 Work Item

```text
Given
  目前請求具有 Admin 角色
  且目標 Work Item 不存在
When
  管理者呼叫刪除 API
Then
  API 回傳 not found
```

## 權限行為

### B-AUTH-001 一般使用者不可呼叫 admin API

```text
Given
  目前請求只有 User 角色
When
  使用者呼叫 admin API
Then
  API 回傳 forbidden
  不寫入任何資料
```

### B-AUTH-002 無法識別目前使用者

```text
Given
  request 中無法解析目前使用者資訊
When
  呼叫需要身分資訊的 API
Then
  API 回傳 unauthorized
```

## 錯誤回應規則

```text
輸入錯誤
  -> validation error

找不到資源
  -> not found

角色不符
  -> forbidden

無法識別身分
  -> unauthorized

未預期失敗
  -> internal server error
```

- 錯誤回應格式需一致。
- 例外統一由 middleware 轉成 HTTP 回應。

## 待確認但不阻塞本版

- admin 是否一定要有獨立 UI，不影響後端先提供 admin API。
- 分頁是否為必做，本版先不寫進 API 行為。
- detail `status` 是否百分之百等於個人狀態，若之後決策不同需回修本文件。
