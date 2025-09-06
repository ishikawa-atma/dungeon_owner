# MVP最小システム定義

## 🎯 要件書に基づく核となる機能

### ✅ 必須システム（MVP）
1. **GameManager** - ゲーム状態管理
2. **FloorSystem** - 階層管理（3階層）
3. **TimeManager** - 時間制御（1x/1.5x/2x速度）
4. **ResourceManager** - 金貨管理
5. **基本UI** - 操作インターフェース
6. **SaveManager** - セーブ/ロード

### ❌ 段階的実装（Phase 2以降）
- 複雑な戦闘システム
- パーティシステム
- 高度なモンスターAI
- パフォーマンス最適化
- 詳細なバランス調整

## 🔧 実行した対応

### 1. 複雑な機能を Complex_Features_Disabled に移動
- FPSOptimizer.cs
- PerformanceManager.cs
- PerformanceMonitor.cs
- BaseBoss.cs
- Cleric.cs
- Mage.cs

### 2. GameManager の簡素化
- PartyCombatSystem の初期化を無効化
- 基本的なゲーム管理機能に集中

### 3. 要件に基づく段階的開発
- Phase 1: 基本的なダンジョン管理
- Phase 2: 戦闘システム
- Phase 3: 高度な機能

## 📊 期待される結果

- エラー数が大幅に減少（20件以下）
- 基本的なゲーム機能が動作
- 要件書の核となる機能が実装済み

## 🎮 動作する機能

### 基本ゲームフロー
1. ゲーム開始
2. 階層表示・切り替え
3. 時間制御
4. 金貨管理
5. セーブ/ロード

### UI機能
- 基本的な操作インターフェース
- リソース表示
- 速度制御

この最小システムで要件書の核となる機能をすべてカバーし、段階的に機能を追加していきます。