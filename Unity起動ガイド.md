# Unity起動ガイド

## 1. Unityのインストール状況確認

### macOSの場合
```bash
# Unity Hubの確認
ls -la /Applications/ | grep -i unity

# Unity Hubがある場合
open -a "Unity Hub"

# Unityエディタが直接ある場合
ls -la /Applications/Unity/

# コマンドラインツールの確認
which unity
```

### Windowsの場合
```cmd
# Unity Hubの確認
dir "C:\Program Files\Unity Hub"

# Unityエディタの確認
dir "C:\Program Files\Unity"
```

## 2. Unityがインストールされていない場合

### A. Unity Hubをインストール（推奨）
1. https://unity.com/download にアクセス
2. 「Download Unity Hub」をクリック
3. ダウンロードしたファイルを実行してインストール

### B. Unity Hubから Unity エディタをインストール
1. Unity Hubを起動
2. 「Installs」タブをクリック
3. 「Install Editor」をクリック
4. 推奨バージョン（LTS版）を選択してインストール

## 3. プロジェクトを開く手順

### Unity Hubから（推奨）
1. Unity Hubを起動
2. 「Projects」タブをクリック
3. 「Open」または「Add project from disk」をクリック
4. プロジェクトフォルダ（dungeon-owner-mvp）を選択
5. プロジェクトが一覧に表示されたらクリックして開く

### 直接Unityエディタから
1. Unityエディタを起動
2. スタート画面で「Open」をクリック
3. プロジェクトフォルダを選択
4. 「Select Folder」をクリック

## 4. 初回起動時の注意点

### プロジェクト読み込み
- 初回は「Importing Assets」が表示され、時間がかかります
- 完了まで待機してください（数分〜十数分）

### エラーが出た場合
- Console ウィンドウ（Window > General > Console）でエラー内容を確認
- 多くの場合、依存関係の問題なので、しばらく待つと解決します

## 5. エディタが起動したら

### 基本的なウィンドウ構成
- **Scene**: ゲーム画面の編集
- **Game**: プレイ時の画面
- **Hierarchy**: シーン内のオブジェクト一覧
- **Project**: プロジェクト内のファイル一覧
- **Inspector**: 選択したオブジェクトの詳細
- **Console**: ログとエラー表示

### 最初に確認すること
1. Console ウィンドウを開く（Window > General > Console）
2. エラーがないか確認
3. Play ボタン（▶️）を押してゲームが起動するか確認

## 6. ビルドテストの実行

エディタが起動したら、以下の手順でテストを実行：

1. **コンパイルテスト**
   - Ctrl+R (Cmd+R) でスクリプト再コンパイル
   - Console でエラーがないことを確認

2. **プレイテスト**
   - Play ボタン（▶️）をクリック
   - ゲームが正常に動作することを確認
   - Stop ボタン（⏹️）で終了

3. **ビルドテスト**
   - File > Build Settings を開く
   - Platform を選択（iOS/Android）
   - Build ボタンをクリック

## トラブルシューティング

### Unity Hubが見つからない場合
- 公式サイトから再ダウンロード: https://unity.com/download

### プロジェクトが開けない場合
- Unity のバージョンが古い可能性
- Unity Hub で適切なバージョンをインストール

### エラーが多数出る場合
- プロジェクトの依存関係の問題
- しばらく待ってから再度確認
- 必要に応じて Package Manager で依存パッケージを確認