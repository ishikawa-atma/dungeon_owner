using UnityEngine;
using UnityEditor;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;

namespace Scripts.Editor
{
    /// <summary>
    /// 罠アイテムデータ作成用エディタスクリプト
    /// </summary>
    public class TrapItemDataCreator : EditorWindow
    {
        [MenuItem("DungeonOwner/Create Trap Item Data")]
        public static void CreateTrapItemData()
        {
            CreateBasicTrapItems();
        }
        
        private static void CreateBasicTrapItems()
        {
            string basePath = "Assets/Resources/Data/TrapItems/";
            
            // フォルダが存在しない場合は作成
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
                AssetDatabase.CreateFolder("Assets/Resources", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/TrapItems"))
                AssetDatabase.CreateFolder("Assets/Resources/Data", "TrapItems");
            
            // スパイクトラップ
            CreateTrapItem(basePath, "SpikeTrap", TrapItemType.SpikeTrap, 
                "スパイクトラップ", "地面から鋭い棘が出現し、侵入者にダメージを与える", 
                25f, 1f, 2f, 0.12f);
            
            // ファイアトラップ
            CreateTrapItem(basePath, "FireTrap", TrapItemType.FireTrap,
                "ファイアトラップ", "炎の爆発で範囲内の侵入者を焼き尽くす",
                40f, 2f, 2.5f, 0.10f);
            
            // アイストラップ
            CreateTrapItem(basePath, "IceTrap", TrapItemType.IceTrap,
                "アイストラップ", "氷の結晶で侵入者を凍らせ、動きを封じる",
                30f, 3f, 2.2f, 0.11f);
            
            // ポイズントラップ
            CreateTrapItem(basePath, "PoisonTrap", TrapItemType.PoisonTrap,
                "ポイズントラップ", "毒ガスで継続的にダメージを与える",
                20f, 5f, 3f, 0.13f);
            
            // ライトニングトラップ
            CreateTrapItem(basePath, "LightningTrap", TrapItemType.LightningTrap,
                "ライトニングトラップ", "雷撃で瞬間的に大ダメージを与える",
                60f, 1.5f, 1.8f, 0.08f);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("基本的な罠アイテムデータを作成しました");
        }
        
        private static void CreateTrapItem(string basePath, string fileName, TrapItemType type,
            string itemName, string description, float damage, float duration, float range, float dropRate)
        {
            string fullPath = basePath + fileName + ".asset";
            
            // 既に存在する場合はスキップ
            if (AssetDatabase.LoadAssetAtPath<TrapItemData>(fullPath) != null)
            {
                Debug.Log($"{fileName} は既に存在します");
                return;
            }
            
            TrapItemData trapItem = ScriptableObject.CreateInstance<TrapItemData>();
            trapItem.type = type;
            trapItem.itemName = itemName;
            trapItem.description = description;
            trapItem.damage = damage;
            trapItem.effectDuration = duration;
            trapItem.range = range;
            trapItem.dropRate = dropRate;
            trapItem.maxStackSize = 5;
            trapItem.cooldownTime = GetCooldownTime(type);
            trapItem.effectColor = GetEffectColor(type);
            
            AssetDatabase.CreateAsset(trapItem, fullPath);
            Debug.Log($"作成: {fullPath}");
        }
        
        private static float GetCooldownTime(TrapItemType type)
        {
            return type switch
            {
                TrapItemType.SpikeTrap => 2f,
                TrapItemType.FireTrap => 3f,
                TrapItemType.IceTrap => 2.5f,
                TrapItemType.PoisonTrap => 4f,
                TrapItemType.LightningTrap => 5f,
                _ => 2f
            };
        }
        
        private static Color GetEffectColor(TrapItemType type)
        {
            return type switch
            {
                TrapItemType.SpikeTrap => Color.gray,
                TrapItemType.FireTrap => Color.red,
                TrapItemType.IceTrap => Color.cyan,
                TrapItemType.PoisonTrap => Color.green,
                TrapItemType.LightningTrap => Color.yellow,
                _ => Color.white
            };
        }
    }
}