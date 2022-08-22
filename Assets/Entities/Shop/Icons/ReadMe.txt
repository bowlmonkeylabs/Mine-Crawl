Unity Forum link for edit script "Repack atlas to Sprite"
https://forum.unity.com/threads/text-mesh-pro-does-not-work-with-spriteatlas-assets.697088/

Steps for adding store icons to text mesh pro:

1. Use Tool -> RapidIcon (Asset store asset) to create .png icon of prefab to add
    a. Export this icon to this folder (Assets/Entities/Shop/Icons)
2. Set this icon to be type of sprite
3. Add this icon to StoreAtlas.spriteatlasv2
4. Right click on StoreAtlas and select "Repack atlas to Sprite"
    a. This editor script will pack the atlas and export as png that TMP can use (overwriting the previous version)
    b. For this to work StoreAtlas needs "AllowRotation" and "TightPacking" unchecked
5. Navigate to Assets > Private > TextMesh Pro > Resources > Sprite Assets
6. Select StoreIcons and click the "Update Sprite Asset" button at the top
    a. This should add the new sprite to the list
7. Use this sprite icon with text mesh pro text field using <sprite="StoreIcons" name="<name of icon here>>">
    a. Example: <sprite="StoreIcons" name="EnemyResource_Icon">