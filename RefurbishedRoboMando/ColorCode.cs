namespace RefurbishedRoboMando
{
    public static class ColorCode
    {
        public enum FontColor
        {
            cStack,
            cIsDamage,
            cIsHealth,
            cIsUtility,
            cIsHealing,
            cDeath,
            cSub,
            cKeywordName
        };

        public static string Style(this string self, FontColor style)
        {
            return "<style=" + style + ">" + self + "</style>";
        }
    }
}
