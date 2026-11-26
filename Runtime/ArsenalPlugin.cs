using BepInEx;

namespace PhantasmsArsenal
{
    [BepInPlugin("ua.ncmod.phantasms-arsenal", "Phantasm's Arsenal", "1.1.13")]
    [BepInDependency("com.nikkorap.blueprinter", "2.0.1")]
    public sealed class ArsenalPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            gameObject.AddComponent<ITGT.Display>();
            Logger.LogInfo("Phantasm's Arsenal 1.1.13: PB-22 Killjoy carrier options.");
        }
    }
}
