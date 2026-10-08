using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PoseidonTELRuntime
{
    [BepInPlugin(PluginId, "R-460 Poseidon TEL Targeting", "1.4.5")]
    public sealed class PoseidonTELRuntime : BaseUnityPlugin
    {
        private const string PluginId = "ua.ncmod.poseidon.tel-targeting";
        private const string LauncherKey = "R460_Poseidon_TEL";
        private const string MissileKey = "R_460_Poseidon_TEL";
        private const float MaxRangeMeters = 108000f;
        private const float MaxTrackAgeSeconds = 45f;

        private void Awake()
        {
            var harmony = new Harmony(PluginId);

            var targetSearch = AccessTools.Method(
                typeof(FactionHQ),
                nameof(FactionHQ.GetTargetsWithinRange),
                new[] { typeof(List<TrackingInfo>), typeof(Transform), typeof(float), typeof(bool) });
            var filter = AccessTools.Method(typeof(PoseidonTELRuntime), nameof(FilterPoseidonTargets));
            if (targetSearch != null && filter != null)
            {
                harmony.Patch(targetSearch, postfix: new HarmonyMethod(filter));
            }
            else
            {
                Logger.LogError("Could not attach the R-460 TEL naval target filter.");
            }

            var spawnMissileMethod = AccessTools.Method(
                typeof(Spawner),
                nameof(Spawner.SpawnMissile),
                new[] { typeof(UnitDefinition), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Unit), typeof(Unit) });

            if (spawnMissileMethod == null)
            {
                spawnMissileMethod = AccessTools.Method(
                    typeof(Spawner),
                    nameof(Spawner.SpawnMissile),
                    new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Unit), typeof(Unit) });
            }

            var fixMissileOwner = AccessTools.Method(typeof(PoseidonTELRuntime), nameof(FixMissileOwnerPostfix));
            if (spawnMissileMethod != null && fixMissileOwner != null)
            {
                harmony.Patch(spawnMissileMethod, postfix: new HarmonyMethod(fixMissileOwner));
                Logger.LogInfo("Hooked Spawner.SpawnMissile for Poseidon owner transfer.");
            }
            else
            {
                Logger.LogWarning("Could not find Spawner.SpawnMissile method to patch.");
            }

            Logger.LogInfo("R-460 TEL targeting and damage attribution patch loaded.");
        }

        private static void FixMissileOwnerPostfix(Missile __result, Unit owner)
        {
            if (__result == null || owner == null)
                return;

            if (__result.definition != null && (__result.definition.jsonKey == MissileKey || __result.definition.jsonKey == "R_460_Poseidon"))
            {
                __result.NetworkownerID = owner.persistentID;
                if (owner.NetworkHQ != null)
                {
                    __result.NetworkHQ = owner.NetworkHQ;
                }
            }
        }

        private static void FilterPoseidonTargets(FactionHQ __instance, Transform fromTransform,
            bool requireLineOfSight, List<TrackingInfo> __result)
        {
            if (__instance == null || fromTransform == null || __result == null)
                return;

            var launcher = fromTransform.GetComponent<GroundVehicle>();
            if (launcher == null || launcher.definition == null || launcher.definition.jsonKey != LauncherKey)
                return;

            var launchPosition = launcher.GlobalPosition();
            __result.Clear();
            foreach (var track in __instance.trackingDatabase.Values)
            {
                Unit target;
                if (track == null || !track.TryGetUnit(out target) || !(target is Ship) ||
                    target.disabled || target.NetworkHQ == null || target.NetworkHQ == __instance ||
                    Time.timeSinceLevelLoad - track.lastSpottedTime > MaxTrackAgeSeconds ||
                    !FastMath.InRange(launchPosition, track.GetPosition(), MaxRangeMeters) ||
                    (requireLineOfSight && !target.LineOfSight(fromTransform.position, 1000f)))
                    continue;

                __result.Add(track);
            }
        }
    }
}
