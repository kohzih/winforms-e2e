# WinForms E2E with FlaUI

.NET Framework 4.8 の WinForms アプリを FlaUI.UIA3 で操作し、公開 GitHub リポジトリの標準 Windows ランナーで E2E テストを実行できるか検証します。

入力欄に `FlaUI` と入力して **Greet** を押すと、結果欄に `Hello, FlaUI!` が表示されます。同じ画面を次の 2 方式で別々に操作し、結果欄の表示を照合します。

| テスト | 操作 |
| --- | --- |
| `UiaPatterns_UpdateResult` | UI Automation の `ValuePattern.SetValue` で入力し、`Invoke` でボタンを押す |
| `MouseAndKeyboard_UpdateResult` | FlaUI のクリックで入力欄にフォーカスし、キー入力して、ボタンをクリックする |

## ローカルで実行

Windows、.NET 10 SDK、.NET Framework 4.8 Developer Pack を用意し、ログイン済みでロックされていないデスクトップの PowerShell で実行します。マウスとキーボードを使うテストがアプリを前面に出すため、実行中は画面を操作せずに待ちます。

```powershell
dotnet test ./tests/WinFormsSample.E2E/WinFormsSample.E2E.csproj -c Release --logger 'console;verbosity=detailed' -- RunConfiguration.TreatNoTestsAsError=true
```

## GitHub Actions で実行

`main` への push で **WinForms E2E** が起動します。GitHub の **Actions** タブから手動実行もできます。実行ページのジョブサマリーに方式ごとの結果が表示されます。操作が失敗した場合は **Run FlaUI tests** のログで失敗箇所を確認できます。

このワークフローは公開リポジトリで無料の標準 `windows-2025` ランナーを使います。ジョブの上限は 10 分です。テスト結果の TRX はランナーの一時領域でサマリー生成に使い、結果はログとサマリーに残します。 [GitHub Actions の課金仕様](https://docs.github.com/en/billing/concepts/product-billing/github-actions)

### 実行中の画面を保存

手動実行で `capture_screenshot` を有効にすると、マウスとキー入力のテストで結果が表示された直後に、サンプルのウィンドウを PNG にします。画像は Base64 でジョブログに残るので、成果物ストレージの使用量には加算されません。

実行ページの URL 末尾にある実行 ID を `$runId` に入れ、認証済みの GitHub CLI と PowerShell で PNG に戻します。

```powershell
$runId = '<実行 ID>'
$capture = gh run view $runId --repo kohzih/winforms-e2e --log |
  Select-String 'WINFORMS_SCREENSHOT_PNG_BASE64=([A-Za-z0-9+/=]+)$' |
  Select-Object -Last 1
if (-not $capture) { throw 'スクリーンショットがログにありません。' }
$png = [Convert]::FromBase64String($capture.Matches[0].Groups[1].Value)
[IO.File]::WriteAllBytes((Join-Path (Get-Location) 'winforms-e2e.png'), $png)
```
