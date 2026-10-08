using System.Collections.Generic;
using UnityEngine;

namespace CircuitBreaker
{
    internal sealed class EclipseMountCollisions : MonoBehaviour
    {
        Aircraft carrier;
        float nextRefresh;
        int lastPairs = -1;
        readonly HashSet<Collider> bodyColliders = new HashSet<Collider>();

        internal void Bind(Aircraft aircraft)
        {
            carrier = aircraft;
            Refresh();
        }

        void FixedUpdate()
        {
            if (Time.time < nextRefresh) return;
            nextRefresh = Time.time + .5f;
            Refresh();
        }

        void Refresh()
        {
            if (!carrier) return;
            bodyColliders.Clear();
            foreach (var collider in carrier.GetComponentsInChildren<Collider>(true))
                if (collider) bodyColliders.Add(collider);
            // Articulated UnitPart bodies may be reparented to Datum.origin.
            // Aircraft.partLookup remains the authoritative carrier-part registry.
            foreach (var part in carrier.partLookup)
            {
                if (!part || part.IsDetached()) continue;
                foreach (var collider in part.GetComponentsInChildren<Collider>(true))
                    if (collider) bodyColliders.Add(collider);
            }
            int pairs = 0;
            foreach (var mounted in GetComponentsInChildren<Collider>(true))
            {
                if (!mounted || !mounted.enabled || !mounted.gameObject.activeInHierarchy) continue;
                foreach (var body in bodyColliders)
                {
                    if (!body || body == mounted || body.transform.IsChildOf(transform) ||
                        !body.enabled || !body.gameObject.activeInHierarchy) continue;
                    Physics.IgnoreCollision(mounted, body, true);
                    pairs++;
                }
            }
            // Reapply for newly initialized parts and rearmed mounted rounds.
            if (pairs != lastPairs)
            {
                lastPairs = pairs;
                Plugin.Diagnostic?.Invoke("Eclipse registered-part collision isolation: parts=" + carrier.partLookup.Count + ", pairs=" + pairs);
            }
        }
    }
}
