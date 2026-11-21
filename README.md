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
