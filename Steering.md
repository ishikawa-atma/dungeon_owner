# Steering (Dungeon Owner)

## Language
- すべての出力メッセージ・タスクタイトル・コミットメッセージは日本語で開始すること
- 英語を必要に応じて補足に使ってよいが、先頭は必ず日本語にすること

## Tech & Style
- Engine: Unity (2D, 縦画面, 60fps目標)
- Code: C# 10, .NET Standard 2.1 互換
- Architecture: ScriptableObjectでデータ駆動（Rooms/Traps/Monsters/Waves）
- Pathfinding: A* on rectangular grid
- Patterns: State machine (Enemy/Trap), Object Pooling
- Data: /Assets/Data/*.json を Addressables 配信可能に

## Testing
- Unity Test Runnerで EditMode/PlayMode を用意
- 重要ロジック（経路探索/戦闘計算/経済）はユニットテスト必須

## Git
- main: 安定ブランチ / develop: 受け皿 / feature/*: 機能単位
- 最小PR(～300行)。自動テスト通過必須。

## UX
- 片手操作、縦画面、倍速(1x/1.5x/2x)
- 配置はゴースト表示＋スナップ＋Undo

## Definition of Done
- テスト緑 / ビルド通過 / iOS・Androidいずれか実機で1分間プレイ可能

