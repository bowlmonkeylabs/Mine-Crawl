using System;

namespace uPalette.Generated
{
public enum ColorTheme
    {
        Default,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.Default:
                    return "d03a838d-31b9-443f-8065-c76f2ba90373";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum ColorEntry
    {
        UI_Normal,
        UI_Highlighted,
        UI_Pressed,
        UI_Selected,
        UI_Disabled,
        UI_Text_Fill,
        UI_Button_Fill,
        UI_Button_Outline,
        Level_Primary_Dark,
        Level_Primary_Light,
        Level_Goal,
        UI_Button_Fill_Store,
        UI_Button_Outline_Store,
        UI_Heart_Fill,
        UI_Heart_Outline,
        UI_Health_Temp_Fill,
        UI_Health_Temp_Outline,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry)
        {
            switch (entry)
            {
                case ColorEntry.UI_Normal:
                    return "fda3ed78-1e7a-4d9e-9987-74fec525fb0c";
                case ColorEntry.UI_Highlighted:
                    return "dc0671f1-8a87-464b-9b71-15352f9c77f3";
                case ColorEntry.UI_Pressed:
                    return "b6359dee-92d3-410c-8942-5f063321d68d";
                case ColorEntry.UI_Selected:
                    return "7e847a8f-34c6-4070-bd56-d0b9c71c94a6";
                case ColorEntry.UI_Disabled:
                    return "7c2804b6-21d9-4eb4-a255-8acd4d6d852e";
                case ColorEntry.UI_Text_Fill:
                    return "f77ebb67-8f0e-4a8e-a27f-1a8e072fde22";
                case ColorEntry.UI_Button_Fill:
                    return "0ff7b1cd-9074-44ac-acdd-9baec8dbfef1";
                case ColorEntry.UI_Button_Outline:
                    return "fd12a671-a97f-40ce-8bf1-e900d8649383";
                case ColorEntry.Level_Primary_Dark:
                    return "66757cbe-bdd1-41f1-9047-31cf9794b71b";
                case ColorEntry.Level_Primary_Light:
                    return "683ac46b-95f5-451b-ad08-5f99edd22e8f";
                case ColorEntry.Level_Goal:
                    return "3a739793-18e1-43e4-819f-3c4e0d237a00";
                case ColorEntry.UI_Button_Fill_Store:
                    return "8e2ee6d9-01b6-4ab6-bc50-601e4b8ee0e9";
                case ColorEntry.UI_Button_Outline_Store:
                    return "0ca68693-3185-43dc-b191-23f5e93eae1b";
                case ColorEntry.UI_Heart_Fill:
                    return "fc6c4e21-ea11-4293-b0d1-150a86c3631a";
                case ColorEntry.UI_Heart_Outline:
                    return "427e632d-64a0-4d88-a430-7d0f789206dc";
                case ColorEntry.UI_Health_Temp_Fill:
                    return "b7ea61b9-49b4-4400-9ccd-86fd495d6f43";
                case ColorEntry.UI_Health_Temp_Outline:
                    return "54f82ee6-f2e3-448c-a9f2-aac757e57d51";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme)
        {
            switch (theme)
            {
                case GradientTheme.Default:
                    return "4b83e5e3-e7ee-48e5-8164-53a74741b334";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum GradientEntry
    {
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTheme.Default:
                    return "3d47bc27-f183-4f0c-a6fe-e62ef17051c0";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTMPTheme.Default:
                    return "3b19284f-d816-41c2-b339-004833ba3cc1";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }
}
