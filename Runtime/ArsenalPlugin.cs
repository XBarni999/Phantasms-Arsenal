using BepInEx;

namespace PhantasmsArsenal
{
    [BepInPlugin("ua.ncmod.phantasms-arsenal", "Phantasm's Arsenal", "1.0.1")]
    [BepInDependency("com.nikkorap.blueprinter", "2.0.1")]
    public sealed class ArsenalPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Phantasm's Arsenal 1.0.1: Poseidon, HSM-290 Killjoy, Apex and Circuit Breaker in one DLL.");
        }
    }
}
