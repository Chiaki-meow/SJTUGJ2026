using Gameplay;
using UnityEditor;
using UnityEngine;

public static class RoomCardDataTools
{
    private const string SpriteFolder = "Assets/Art/RoomSprites";

    [MenuItem("Tools/Room Cards/Fill Selected From Asset Names")]
    private static void FillSelectedFromAssetNames()
    {
        foreach (Object selectedObject in Selection.objects)
        {
            if (!(selectedObject is RoomCardData roomCardData))
                continue;

            Undo.RecordObject(roomCardData, "Fill Room Card From Asset Name");
            roomCardData.FillFromAssetName();

            Sprite sprite = FindSpriteByName(roomCardData.name);
            if (sprite != null)
            {
                roomCardData.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite named '{roomCardData.name}' found in {SpriteFolder}.", roomCardData);
            }

            EditorUtility.SetDirty(roomCardData);
        }

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Room Cards/Fill Selected From Asset Names", true)]
    private static bool CanFillSelectedFromAssetNames()
    {
        foreach (Object selectedObject in Selection.objects)
        {
            if (selectedObject is RoomCardData)
                return true;
        }

        return false;
    }

    private static Sprite FindSpriteByName(string spriteName)
    {
        string[] spriteGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { SpriteFolder });

        foreach (string spriteGuid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(spriteGuid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        return null;
    }
}
