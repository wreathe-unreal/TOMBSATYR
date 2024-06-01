using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Haipeng.Ghost_trail
{
    public class SkinnedMeshGhostFX : MonoBehaviour
    {
        [Header("skinned mesh render")]
        public SkinnedMeshRenderer SMR;

        [Header("submesh index, If your mesh contains multiple materials, this parameter represents the index of the material")]
        public int SubmeshIndex = 0;

        [Header("The material of the trail")]
        public Material M_Trail;

        [Header("Time between each generation of trail£¬In seconds")]
        [Range(0, 1)]
        public float TimeInterval;

        [Header("trail display time, in seconds")]
        [Range(0, 2)]
        public float TimeTrailShow;

        private List<DrawingMesh> DrawingMeshes = new List<DrawingMesh>();

        private Coroutine Coro;


        void LateUpdate()
        {
            bool is_need_remove = false;

            foreach (var drawing_mesh in this.DrawingMeshes)
            {
            
                Graphics.DrawMesh(drawing_mesh.mesh, Matrix4x4.identity, drawing_mesh.material, gameObject.layer, Camera.main, 0, null, drawing_mesh.shadowCastingMode, false, null);
                drawing_mesh.left_time -= Time.deltaTime;
                

                if (drawing_mesh.material.HasProperty("_BaseColor"))
                {
                    //Color c = Color.white;
                    Color newColor = drawing_mesh.material.color;
                    newColor.a = Mathf.Max(0, drawing_mesh.left_time / this.TimeTrailShow * 0.5f);

                    drawing_mesh.material.SetColor("_BaseColor", newColor);
                }
                else if (drawing_mesh.material.HasProperty("_Color"))
                {
                    //Color c = Color.white;
                    Color newColor = drawing_mesh.material.color;
                    newColor.a = Mathf.Max(0, drawing_mesh.left_time / this.TimeTrailShow * 0.5f);

                    drawing_mesh.material.SetColor("_Color", newColor);
                }
                if (drawing_mesh.left_time <= 0)
                    is_need_remove = true;
            }

            if (is_need_remove)
                this.DrawingMeshes.RemoveAll(x => x.left_time <= 0);
        }

        public void play()
        {
            if(this.Coro!=null)
                StopCoroutine(this.Coro);
            this.DrawingMeshes.Clear();

            this.Coro = StartCoroutine(this.start_enumerator());
        }

        public void play(float time_total_duration, float time_interval, float time_trail_show)
        {
            this.TimeTrailShow = time_total_duration;
            this.TimeInterval = time_interval;
            this.TimeTrailShow = time_trail_show;

            if (this.Coro != null)
                StopCoroutine(this.Coro);
            this.DrawingMeshes.Clear();

            this.Coro = StartCoroutine(this.start_enumerator());
        }

        public void stop()
        {
            if (this.Coro != null)
                StopCoroutine(this.Coro);
        }

        public IEnumerator start_enumerator()
        {
            if (this.SMR != null)
            {
                while (true)
                {
                    this.create_single_trail();
                    yield return new WaitForSeconds(this.TimeInterval);

                }
            }
            yield return null;
        }

        //combine mesh
        private Mesh combine_all_mesh()
        {
            SkinnedMeshRenderer render = this.SMR;
            Mesh mesh = new Mesh();
            render.BakeMesh(mesh);

            CombineInstance conbine_instacne = new CombineInstance
            {
                mesh = mesh,
                transform = render.gameObject.transform.localToWorldMatrix,
                subMeshIndex = this.SubmeshIndex
            };

            CombineInstance[] array_conbine_instacne = new CombineInstance[1];
            array_conbine_instacne[0] = conbine_instacne;

            Mesh combined_mesh = new Mesh();
            combined_mesh.CombineMeshes(array_conbine_instacne, true, true);

            return combined_mesh;
        }

        //create single trail
        private void create_single_trail()
        {
            DrawingMesh drawing_mesh = new DrawingMesh()
            {
                mesh = this.combine_all_mesh(),
       
                material = new Material(this.M_Trail),

                left_time = this.TimeTrailShow,
                
                shadowCastingMode = ShadowCastingMode.Off
            };

            this.DrawingMeshes.Add(drawing_mesh);
        }
    }

}