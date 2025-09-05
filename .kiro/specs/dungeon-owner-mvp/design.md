# 設計書

## 概要

ダンジョンオーナーMVPは、Unity 2Dエンジンを使用したモバイル向けリアルタイム防衛ゲームです。プレイヤーはダンジョンの神となり、最深部のダンジョンコアを侵入者から守ります。ScriptableObjectベースのデータ駆動アーキテクチャを採用し、縦画面での片手操作に最適化されています。

## アーキテクチャ

### 全体アーキテクチャ

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Presentation  │    │    Game Logic   │    │      Data       │
│      Layer      │◄──►│      Layer      │◄──►│     Layer       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
│                      │                      │
├─ UI Controllers     ├─ Game Manager       ├─ ScriptableObjects
├─ Input Handlers     ├─ Wave Manager       ├─ JSON Data Files
├─ Visual Effects    ├─ Economy Manager    ├─ Save System
└─ Audio Manager     ├─ Research Manager   └─ Addressables
                     ├─ Pathfinding
                     ├─ Combat System
                     └─ State Machines
```

### データ駆動設計

- **ScriptableObjects**: モンスター、ボスキャラ、自キャラクター、侵入者、罠アイテムの定義
- **JSON設定ファイル**: バランス調整可能なパラメータ（経済、レベル、階層コスト）
- **Addressables**: 動的コンテンツ配信対応

## コンポーネントとインターフェース

### 1. コアシステム

#### GameManager
```csharp
public class GameManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; }
    public int CurrentFloor { get; private set; }
    public float GameSpeed { get; private set; }
    
    public void ChangeState(GameState newState);
    public void PauseGame();
    public void ResumeGame();
    public void SetGameSpeed(float multiplier);
    public void ExpandFloor();
}
```

#### FloorSystem
```csharp
public class FloorSystem : MonoBehaviour
{
    public int MaxFloors { get; private set; }
    public List<Floor> Floors { get; private set; }
    
    public bool CanPlaceMonster(int floorIndex, Vector2 position);
    public void PlaceMonster(int floorIndex, IMonster monster, Vector2 position);
    public void RemoveMonster(int floorIndex, IMonster monster);
    public bool CanExpandFloor();
    public int GetExpansionCost(int targetFloor);
}
```

### 2. モンスターシステム

#### IMonster インターフェース
```csharp
public interface IMonster
{
    MonsterType Type { get; }
    int Level { get; set; }
    float Health { get; }
    float MaxHealth { get; }
    float Mana { get; }
    float MaxMana { get; }
    Vector2 Position { get; set; }
    MonsterAbility Ability { get; }
    IParty Party { get; set; }
    
    void TakeDamage(float damage);
    void Heal(float amount);
    void UseAbility();
    void JoinParty(IParty party);
    void LeaveParty();
}
```

#### モンスター実装クラス（拡張可能）
```csharp
// 初期モンスタークラス
public class Slime : MonoBehaviour, IMonster // 自動体力回復
public class LesserSkeleton : MonoBehaviour, IMonster // 自動復活
public class LesserGhost : MonoBehaviour, IMonster // 自動復活
public class LesserGolem : MonoBehaviour, IMonster // 高耐久
public class Goblin : MonoBehaviour, IMonster // 攻撃特化
public class LesserWolf : MonoBehaviour, IMonster // 速度特化

// 階層拡張で解放されるモンスター
public class GreaterSlime : MonoBehaviour, IMonster // 上位スライム
public class Skeleton : MonoBehaviour, IMonster // 上位スケルトン
public class Ghost : MonoBehaviour, IMonster // 上位ゴースト
public class Golem : MonoBehaviour, IMonster // 上位ゴーレム
public class Orc : MonoBehaviour, IMonster // 上位ゴブリン
public class Wolf : MonoBehaviour, IMonster // 上位ウルフ
public class Dragon : MonoBehaviour, IMonster // 特殊モンスター
public class Lich : MonoBehaviour, IMonster // 特殊モンスター
public class Demon : MonoBehaviour, IMonster // 特殊モンスター
```

### 3. 侵入者システム

#### IInvader インターフェース
```csharp
public interface IInvader
{
    int Level { get; }
    float Health { get; }
    float MaxHealth { get; }
    Vector2 Position { get; set; }
    InvaderState State { get; }
    IParty Party { get; set; }
    
    void TakeDamage(float damage);
    void Move(Vector2 targetPosition);
    void JoinParty(IParty party);
    void LeaveParty();
}
```

#### 侵入者実装クラス（拡張可能）
```csharp
// 基本侵入者クラス
public class Warrior : MonoBehaviour, IInvader // 前衛タイプ
public class Mage : MonoBehaviour, IInvader // 魔法使いタイプ
public class Rogue : MonoBehaviour, IInvader // 盗賊タイプ
public class Cleric : MonoBehaviour, IInvader // 回復タイプ

// 上位侵入者クラス（階層拡張で解放）
public class Knight : MonoBehaviour, IInvader // 上位前衛
public class Archmage : MonoBehaviour, IInvader // 上位魔法使い
public class Assassin : MonoBehaviour, IInvader // 上位盗賊
public class HighPriest : MonoBehaviour, IInvader // 上位回復
public class Paladin : MonoBehaviour, IInvader // 特殊タイプ
public class Necromancer : MonoBehaviour, IInvader // 特殊タイプ
```

### 4. パーティシステム

#### IParty インターフェース
```csharp
public interface IParty
{
    List<ICharacter> Members { get; }
    Vector2 Position { get; set; }
    
    void AddMember(ICharacter character);
    void RemoveMember(ICharacter character);
    void DistributeDamage(float totalDamage);
    void ApplyPartyHealing(float healAmount);
    void MoveParty(Vector2 targetPosition);
}
```

#### PartyManager
```csharp
public class PartyManager : MonoBehaviour
{
    public IParty CreateParty(List<ICharacter> members);
    public void DisbandParty(IParty party);
    public void HandlePartyMovement(IParty party, Vector2 destination);
    public void HandlePartyCombat(IParty attackers, IParty defenders);
}
```

### 5. 経済システム

#### ResourceManager
```csharp
public class ResourceManager : MonoBehaviour
{
    public int Gold { get; private set; }
    public DateTime LastDailyReward { get; private set; }
    
    public bool CanAfford(int cost);
    public void SpendGold(int amount);
    public void AddGold(int amount);
    public void ProcessDailyReward();
    public int CalculateFloorExpansionCost(int targetFloor);
}
```

### 6. 退避スポットシステム

#### ShelterManager
```csharp
public class ShelterManager : MonoBehaviour
{
    public List<IMonster> ShelterMonsters { get; private set; }
    public int MaxCapacity { get; private set; }
    
    public bool CanShelter(IMonster monster);
    public void ShelterMonster(IMonster monster);
    public void DeployMonster(IMonster monster, int floorIndex, Vector2 position);
    public void SellMonster(IMonster monster);
    public void UpdateRecovery(float deltaTime);
}
```

## データモデル

### ScriptableObject定義

#### MonsterData
```csharp
[CreateAssetMenu(fileName = "MonsterData", menuName = "DungeonOwner/MonsterData")]
public class MonsterData : ScriptableObject
{
    public MonsterType type;
    public MonsterRarity rarity; // Common, Rare, Epic, Legendary
    public int goldCost;
    public float baseHealth;
    public float baseMana;
    public float baseAttackPower;
    public float moveSpeed;
    public MonsterAbility ability;
    public int unlockFloor; // 解放される階層
    public List<MonsterType> evolutionTargets; // 進化先モンスター
    public GameObject prefab;
}
```

#### PlayerCharacterData
```csharp
[CreateAssetMenu(fileName = "PlayerCharacterData", menuName = "DungeonOwner/PlayerCharacterData")]
public class PlayerCharacterData : ScriptableObject
{
    public PlayerCharacterType type;
    public float baseHealth;
    public float baseMana;
    public float baseAttackPower;
    public List<PlayerAbility> abilities;
    public GameObject prefab;
}
```

#### InvaderData
```csharp
[CreateAssetMenu(fileName = "InvaderData", menuName = "DungeonOwner/InvaderData")]
public class InvaderData : ScriptableObject
{
    public InvaderType type;
    public InvaderRank rank; // Novice, Veteran, Elite, Champion
    public float baseHealth;
    public float baseAttackPower;
    public float moveSpeed;
    public int goldReward;
    public float trapItemDropRate;
    public int minAppearanceFloor; // 出現開始階層
    public List<InvaderAbility> abilities; // 侵入者固有アビリティ
    public GameObject prefab;
}
```

#### BossData
```csharp
[CreateAssetMenu(fileName = "BossData", menuName = "DungeonOwner/BossData")]
public class BossData : ScriptableObject
{
    public BossType type;
    public float baseHealth;
    public float baseAttackPower;
    public List<BossAbility> abilities;
    public float respawnTime;
    public int goldReward;
    public GameObject prefab;
}
```

#### TrapItemData
```csharp
[CreateAssetMenu(fileName = "TrapItemData", menuName = "DungeonOwner/TrapItemData")]
public class TrapItemData : ScriptableObject
{
    public TrapItemType type;
    public float damage;
    public float effectDuration;
    public float range;
    public GameObject effectPrefab;
}
```

### セーブデータ構造

```csharp
[System.Serializable]
public class SaveData
{
    public int currentFloor;
    public int gold;
    public DateTime lastDailyReward;
    public PlayerCharacterType selectedPlayerCharacter;
    public List<FloorData> floorLayouts;
    public List<MonsterSaveData> placedMonsters;
    public List<MonsterSaveData> shelterMonsters;
    public List<TrapItemSaveData> inventory;
    public DateTime lastSaveTime;
}

[System.Serializable]
public class MonsterSaveData
{
    public MonsterType type;
    public int level;
    public float currentHealth;
    public float currentMana;
    public Vector2 position;
    public int floorIndex;
    public bool isInShelter;
}

[System.Serializable]
public class FloorData
{
    public int floorIndex;
    public Vector2 upStairPosition;
    public Vector2 downStairPosition;
    public List<Vector2> wallPositions;
    public BossType bossType;
    public int bossLevel;
}
```

## シーン構成

### 1. MainMenuScene
- タイトル画面
- 自キャラクター選択
- 設定メニュー
- 進行状況表示

### 2. GameplayScene
- メインゲームプレイ
- 階層表示システム
- リアルタイム戦闘
- UI オーバーレイ

### 3. ShelterScene
- 退避スポット管理
- モンスター売却
- パーティ編成

### 4. TutorialScene
- チュートリアル専用シーン
- ガイド付きゲームプレイ

## 主要クラス/コンポーネント

### ゲーム管理
- `GameManager`: ゲーム全体の状態管理
- `FloorManager`: 階層管理システム
- `SaveManager`: データ永続化
- `TimeManager`: 時間制御システム

### ゲームプレイ
- `InvaderSpawner`: 侵入者生成システム
- `CombatManager`: リアルタイム戦闘管理
- `PartyManager`: パーティシステム管理
- `AbilityManager`: アビリティ発動管理

### UI システム
- `UIManager`: UI全体の管理
- `MonsterPlacementUI`: モンスター配置インターフェース
- `ResourceDisplayUI`: リソース表示
- `SpeedControlUI`: 倍速制御
- `ShelterUI`: 退避スポット管理UI

### 入力システム
- `InputManager`: タッチ入力処理
- `MonsterPlacementHandler`: モンスター配置操作
- `CameraController`: カメラ制御
- `FloorNavigationHandler`: 階層切り替え操作

## エラーハンドリング

### 1. 入力エラー
- 無効な位置への配置試行
- 不十分なリソースでの操作
- 範囲外グリッドアクセス

### 2. システムエラー
- セーブデータ破損
- リソース読み込み失敗
- ネットワーク接続エラー

### 3. パフォーマンスエラー
- フレームレート低下検出
- メモリ不足警告
- バッテリー消費最適化

## テスト戦略

### 1. ユニットテスト
- 経路探索アルゴリズム
- 戦闘計算式
- 経済システム計算
- セーブ/ロード機能

### 2. 統合テスト
- ウェーブ進行フロー
- UI操作シーケンス
- シーン遷移

### 3. パフォーマンステスト
- 60FPS維持確認
- メモリ使用量測定
- バッテリー消費測定

### 4. デバイステスト
- iOS実機テスト
- Android実機テスト
- 異なる画面サイズ対応

## 技術的考慮事項

### パフォーマンス最適化
- オブジェクトプール使用（モンスター・侵入者・エフェクト）
- リアルタイム戦闘の効率的な衝突判定
- UI更新頻度制御
- テクスチャアトラス使用
- パーティ単位での処理最適化

### メモリ管理
- 不要オブジェクトの適切な破棄
- ScriptableObjectの効率的な使用
- ガベージコレクション最小化

### モバイル最適化
- タッチ操作レスポンス向上
- バッテリー消費削減
- 通信量最小化
- ストレージ使用量最適化