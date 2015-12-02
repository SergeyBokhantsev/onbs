namespace Interfaces
{
    public static class Extensions
    {
        public static void InvertBoolSetting(this IConfig cfg, string name)
        {
            var value = cfg.GetBool(name);
            cfg.Set(name, !value);
        }
    }
}
