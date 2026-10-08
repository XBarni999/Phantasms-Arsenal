using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PhantasmsArsenal.ITGT
{
    internal static class Guidance
    {
        internal sealed class Release
        {
            public MountedMissile mount;
            public Unit owner;
            public GlobalPosition point;
        }
        static readonly List<Release> pending = new List<Release>();
        static Release spawning;
        static readonly FieldInfo SeekerMissile = AccessTools.Field(typeof(MissileSeeker), "missile");
        static readonly FieldInfo TargetUnit = AccessTools.Field(typeof(MissileSeeker), "targetUnit");
        static readonly FieldInfo Fired = AccessTools.Field(typeof(MountedMissile), "fired");
        static readonly FieldInfo Hardpoint = AccessTools.Field(typeof(Weapon), "hardpoint");
        static readonly FieldInfo WeaponIndex = AccessTools.Field(typeof(WeaponStation), "weaponIndex");
        internal static Hardpoint Pylon(MountedMissile mount) => mount ? Hardpoint.GetValue(mount) as Hardpoint : null;
        internal static bool Available(MountedMissile mount) => mount && mount.ammo > 0 && mount.IsAttached() && !(bool)Fired.GetValue(mount);
        internal static MountedMissile NextStore(WeaponStation station)
        {
            if (station == null) return null;
            int index = (int)WeaponIndex.GetValue(station);
            return index >= 0 && index < station.Weapons.Count ? station.Weapons[index] as MountedMissile : null;
        }

        public static bool Supports(WeaponInfo info)
        {
            if (!info || !info.weaponPrefab || (!info.bomb && !info.glideBomb && !info.missile)) return false;
            var seeker = info.weaponPrefab.GetComponent<MissileSeeker>();
            // A bare MissileSeeker, high-drag dispenser or unguided bomb cannot steer.
            return seeker is OpticalSeekerBomb || seeker is OpticalSeeker || seeker is OpticalSeekerCruiseMissile ||
                seeker is BallisticMissileGuidance || ((info.bomb || info.glideBomb) && seeker is LaserSeeker);
        }

        public static void Install(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(WeaponManager), "Fire"), prefix: new HarmonyMethod(typeof(Guidance), nameof(Fire)));
            harmony.Patch(AccessTools.Method(typeof(WeaponManager), "FireGuns"), prefix: new HarmonyMethod(typeof(Guidance), nameof(Guns)));
            harmony.Patch(AccessTools.Method(typeof(MountedMissile), "Fire"), prefix: new HarmonyMethod(typeof(Guidance), nameof(Capture)) { priority = Priority.First });
            harmony.Patch(AccessTools.Method(typeof(Spawner), "SpawnMissile", new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Unit), typeof(Unit) }),
                prefix: new HarmonyMethod(typeof(Guidance), nameof(Spawn)), postfix: new HarmonyMethod(typeof(Guidance), nameof(SpawnBound)),
                finalizer: new HarmonyMethod(typeof(Guidance), nameof(SpawnEnd)));
            foreach (var type in new[] { typeof(OpticalSeekerBomb), typeof(OpticalSeeker), typeof(OpticalSeekerCruiseMissile), typeof(BallisticMissileGuidance), typeof(LaserSeeker) })
            {
                harmony.Patch(AccessTools.Method(type, "Initialize"), prefix: new HarmonyMethod(typeof(Guidance), nameof(Initialize)),
                    postfix: new HarmonyMethod(typeof(Guidance), nameof(Initialized)) { before = new[] { "ua.ncmod.kh47m2", "ua.ncmod.circuitbreaker" } });
                harmony.Patch(AccessTools.Method(type, "Seek"), prefix: new HarmonyMethod(typeof(Guidance), nameof(Seek)));
            }
            harmony.Patch(AccessTools.Method(typeof(OpticalSeekerCruiseMissile), "PreTerminalMode"), prefix: new HarmonyMethod(typeof(Guidance), nameof(PreTerminal)));
            harmony.Patch(AccessTools.Method(typeof(OpticalSeekerCruiseMissile), "SlowChecks"), prefix: new HarmonyMethod(typeof(Guidance), nameof(CruiseChecks)));
        }

        static bool Guns(WeaponManager __instance) => !Display.PointerOverDisplay || __instance.GetComponentInParent<Aircraft>() != Display.Aircraft;
        static bool Fire(WeaponManager __instance)
        {
            var aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft != Display.Aircraft) return true;
            if (Display.PointerOverDisplay) return false;
            var station = __instance.currentWeaponStation;
            if (!Display.TryDesignation(aircraft, station, out var point))
            {
                if (Display.GpsArmedFor(aircraft, station?.WeaponInfo)) { Display.UnassignedNotice(); return false; }
                return true;
            }
            // Bypass native object-target salvos only while GPS is armed. One normal release per trigger.
            if (station.Ready() && !station.SafetyIsOn(aircraft) && !station.SalvoInProgress && !aircraft.remoteSim)
            {
                station.LaunchMount(aircraft, null, point);
                __instance.InvokeOnStationFired();
            }
            return false;
        }

        static void Capture(MountedMissile __instance, Unit owner, ref Unit target, ref GlobalPosition aimpoint)
        {
            if (!owner || !owner.IsServer || (bool)Fired.GetValue(__instance) || !__instance.IsAttached() || !Supports(__instance.info)) return;
            if (!Display.TryDesignation(owner, __instance, out var point)) return;
            target = null; aimpoint = point;
            pending.RemoveAll(p => !p.mount || !p.owner || p.owner.disabled);
            pending.Add(new Release { mount = __instance, owner = owner, point = point });
            Display.ConsumeStore(__instance);
        }

        static void Spawn(GameObject missile, Vector3 launchPosition, Unit owner, ref Unit target, out Release __state)
        {
            __state = spawning;
            spawning = null;
            pending.RemoveAll(p => !p.mount || !p.owner || p.owner.disabled);
            // Match the releasing rail, not just weapon type: adjacent stations may have different marks.
            int index = pending.FindIndex(p => p.owner == owner && p.mount.info.weaponPrefab == missile &&
                (p.mount.transform.position - launchPosition).sqrMagnitude < 1);
            if (index < 0) return;
            spawning = pending[index]; pending.RemoveAt(index); target = null;
        }
        static Exception SpawnEnd(Exception __exception, Release __state) { spawning = __state; return __exception; }
        static void SpawnBound(Missile __result)
        {
            // Some network spawning paths defer LocalStart. Carry the designation on the spawned object too.
            if (spawning == null || !__result || __result.GetComponent<CoordinateFlight>()) return;
            __result.gameObject.AddComponent<CoordinateFlight>().Bind(__result, __result.GetComponent<MissileSeeker>(), spawning.point);
        }

        static void Initialize(MissileSeeker __instance, ref Unit target, ref GlobalPosition aimpoint)
        {
            var missile = SeekerMissile.GetValue(__instance) as Missile;
            if (!missile || !missile.LocalSim) return;
            var flight = missile.GetComponent<CoordinateFlight>();
            if (!flight)
            {
                if (spawning == null || missile.owner != spawning.owner) return;
                flight = missile.gameObject.AddComponent<CoordinateFlight>();
                flight.Bind(missile, __instance, spawning.point);
            }
            target = null; aimpoint = flight.Point; missile.SetTarget(null);
        }

        static void Initialized(MissileSeeker __instance)
        {
            var flight = __instance.GetComponent<CoordinateFlight>();
            if (!flight) return;
            Set(__instance, "knownPos", flight.Point);
            Set(__instance, "knownVel", Vector3.zero);
            Set(__instance, "aimPos", flight.Point);
            TargetUnit.SetValue(__instance, null);
            if (__instance is LaserSeeker) Set(__instance, "positionError", Vector3.zero);
            flight.Missile.SetAimpoint(flight.Point, Vector3.zero);
            if (__instance is OpticalSeeker) AccessTools.Method(__instance.GetType(), "CalcDistAndTimeToTarget").Invoke(__instance, null);
            Display.Trace("I-TGT released " + flight.Missile.GetWeaponInfo().weaponName + " at " + flight.Point);
        }

        internal static void Set(MissileSeeker seeker, string name, object value) => AccessTools.Field(seeker.GetType(), name)?.SetValue(seeker, value);
        internal static float Number(MissileSeeker seeker, string name, float fallback)
        {
            var field = AccessTools.Field(seeker.GetType(), name);
            return field != null ? (float)field.GetValue(seeker) : fallback;
        }
        internal static void Acquire(CoordinateFlight flight, Unit target)
        {
            var seeker = flight.Seeker;
            TargetUnit.SetValue(seeker, target);
            Set(seeker, "targetTransform", target.transform);
            Set(seeker, "targetPart", target.transform);
            Set(seeker, "targetHQAtLaunch", target.NetworkHQ);
            // Keep the known GPS point until the native optical/laser check establishes visibility.
            flight.Missile.SetTarget(target);
            if (seeker.proximityFuse) flight.Missile.SetProxyFuse(target.transform, target.rb);
            flight.Acquired = true;
            Display.Trace("I-TGT terminal acquisition: " + target.unitName);
        }

        static void Seek(MissileSeeker __instance) => __instance.GetComponent<CoordinateFlight>()?.Step();
        static bool PreTerminal(OpticalSeekerCruiseMissile __instance)
        {
            var flight = __instance.GetComponent<CoordinateFlight>();
            if (!flight || flight.Acquired || flight.Missile.GetComponent<CircuitBreaker.BlackoutFlight>()) return true;
            if (!flight.CourseUpdateDue()) return false;
            float distance = (flight.Point - flight.Missile.GlobalPosition()).magnitude;
            if (distance > Number(__instance, "terminalRange", 2000))
                flight.Missile.SetAimpoint(__instance.TerrainWaypoint(flight.Point), Vector3.zero);
            else
            {
                flight.Missile.Arm();
                float seconds = Mathf.Min(distance / Mathf.Max(flight.Missile.speed, 10), 4);
                flight.Missile.SetAimpoint(flight.Point + Vector3.up * (seconds * seconds * 4.905f), Vector3.zero);
            }
            return false;
        }
        static bool CruiseChecks(OpticalSeekerCruiseMissile __instance)
        {
            var flight = __instance.GetComponent<CoordinateFlight>();
            if (!flight || flight.Acquired || flight.Missile.GetComponent<CircuitBreaker.BlackoutFlight>()) return true;
            var missile = flight.Missile;
            if (!missile.disabled)
            {
                missile.UpdateRadarAlt();
                if (missile.timeSinceSpawn > 10 && (missile.LosingGround() || missile.MissedTarget() || missile.speed < 100))
                    missile.Detonate(missile.rb.velocity, false, false);
                if (!missile.IsTangible() && missile.timeSinceSpawn > 2) missile.SetTangible(true);
            }
            return false;
        }
        public static void Clear() { pending.Clear(); spawning = null; }
    }

    public sealed class CoordinateFlight : MonoBehaviour
    {
        internal Missile Missile;
        internal MissileSeeker Seeker;
        internal GlobalPosition Point;
        internal bool Acquired;
        float nextSearch;
        float nextCourse;
        readonly List<Unit> candidates = new List<Unit>();
        internal void Bind(Missile missile, MissileSeeker seeker, GlobalPosition point) { Missile = missile; Seeker = seeker; Point = point; }
        internal bool CourseUpdateDue()
        {
            if (Time.time < nextCourse) return false;
            nextCourse = Time.time + .5f;
            return true;
        }
        internal void Step()
        {
            if (!Missile || !Missile.LocalSim || Missile.disabled || Acquired || Seeker is BallisticMissileGuidance ||
                Missile.GetComponent<CircuitBreaker.BlackoutFlight>()) return;
            // Laser bombs gain a coordinate midcourse; their terminal laser still requires real illumination.
            if (Seeker is LaserSeeker) { Guidance.Set(Seeker, "knownPos", Point); Guidance.Set(Seeker, "positionError", Vector3.zero); }
            float terminal = Guidance.Number(Seeker, "terminalRange", 5000);
            if (Time.time < nextSearch) return;
            nextSearch = Time.time + .5f;
            if (Seeker is OpticalSeeker) AccessTools.Method(Seeker.GetType(), "CalcDistAndTimeToTarget").Invoke(Seeker, null);
            if ((Point - Missile.GlobalPosition()).sqrMagnitude > terminal * terminal) return;
            float radius = Mathf.Clamp(Guidance.Number(Seeker, "terminalSearchRadius", Guidance.Number(Seeker, "searchRadius", 300)), 25, 1000);
            float best = radius * radius;
            Unit chosen = null;
            candidates.Clear();
            BattlefieldGrid.GetUnitsInRangeNonAlloc(Point, radius, candidates);
            foreach (var unit in candidates)
            {
                if (!unit || unit.disabled || unit is Missile || unit is Aircraft || !unit.NetworkHQ || !Missile.NetworkHQ || unit.NetworkHQ == Missile.NetworkHQ) continue;
                float distance = (unit.GlobalPosition() - Point).sqrMagnitude;
                if (distance >= best || !unit.LineOfSight(transform.position, 1000)) continue;
                if (Vector3.Angle(unit.transform.position - transform.position, transform.forward) > Guidance.Number(Seeker, "searchAngle", 90)) continue;
                if (Seeker is LaserSeeker && !Missile.NetworkHQ.IsTargetLased(unit)) continue;
                chosen = unit; best = distance;
            }
            if (chosen) Guidance.Acquire(this, chosen);
        }
    }
}
