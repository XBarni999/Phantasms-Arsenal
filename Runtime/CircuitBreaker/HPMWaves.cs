using UnityEngine;
using System.Linq;
namespace CircuitBreaker
{
    public sealed class HPMWaves : MonoBehaviour
    {
        MeshRenderer[] rings; MaterialPropertyBlock[] properties; float started=-100;
        static readonly int Opacity=Shader.PropertyToID("_Opacity"),Phase=Shader.PropertyToID("_Phase");
        public void Bind(Missile missile){if(rings!=null)return;rings=missile.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("HPMWave")).OrderBy(r=>r.name).ToArray();properties=rings.Select(r=>new MaterialPropertyBlock()).ToArray();foreach(var ring in rings)ring.enabled=false;}
        public void Pulse(){started=Time.time;}
        void Update(){if(rings==null)return;for(int i=0;i<rings.Length;i++){
            float phase=(Time.time-started-i*.14f)/1.15f;bool visible=phase>=0&&phase<1;rings[i].enabled=visible;if(!visible)continue;
            float radius=Mathf.Lerp(.65f,3.4f,phase);rings[i].transform.localScale=new Vector3(radius,radius,radius*.6f);
            rings[i].transform.localPosition=new Vector3(0,0,1.2f-phase*.7f);
            properties[i].SetFloat(Phase,phase);properties[i].SetFloat(Opacity,Mathf.Sin(phase*Mathf.PI)*.12f);rings[i].SetPropertyBlock(properties[i]);
        }}
        void OnDisable(){if(rings!=null)foreach(var ring in rings)if(ring)ring.enabled=false;}
    }
}
