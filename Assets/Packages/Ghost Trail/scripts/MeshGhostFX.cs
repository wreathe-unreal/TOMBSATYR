using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Haipeng.Ghost_trail
{
    public class MeshGhostFX : MonoBehaviour
    {
        [Header("mesh_filter")]
        public MeshFilter mesh_filter;

        [Header("submesh index��If your mesh contains multiple materials, this parameter represents the index of the material")]
        public int submesh_index = 0;

        [Header("The material of the trail")]
        public Material mat_trail;

        [Header("Time between each generation of trail��In seconds")]
        [Range(0, 1)]
        public float time_interval;

        [Header("trail display time, in seconds")]
        [Range(0, 2)]
        public float time_trail_show;

        private List<DrawingMesh> list_drawing_mesh = new List<DrawingMesh>();

        private Coroutine coroutine;

        void LateUpdate()
        {
            bool is_need_remove = false;

            foreach (var drawing_mesh in this.list_drawing_mesh)
            {
                Graphics.DrawMesh(drawing_mesh.mesh, Matrix4x4.identity, drawing_mesh.material, gameObject.layer, null, 0, null, false);

                drawing_mesh.left_time -= Time.deltaTime;

                if (drawing_mesh.material.HasProperty("_BaseColor"))
                {
                    Color c = Color.white;
                    c.a = Mathf.Max(0, drawing_mesh.left_time / this.time_trail_show * 0.5f);

                    drawing_mesh.material.SetColor("_BaseColor", c);

                }
                if (drawing_mesh.left_time <= 0)
                    is_need_remove = true;
            }

            if (is_need_remove)
                this.list_drawing_mesh.RemoveAll(x => x.left_time <= 0);
        }

        public void play()
        {
            if (this.coroutine != null)
                StopCoroutine(this.coroutine);
            this.list_drawing_mesh.Clear();

            this.coroutine = StartCoroutine(this.start_enumerator());
        }

        public void play(float time_total_duration, float time_interval, float time_trail_show)
        {
            this.time_trail_show = time_total_duration;
            this.time_interval = time_interval;
            this.time_trail_show = time_trail_show;

            if (this.coroutine != null)
                StopCoroutine(this.coroutine);
            this.list_drawing_mesh.Clear();

            this.coroutine = StartCoroutine(this.start_enumerator());
        }

        public void stop()
        {
            if (this.coroutine != null)
                StopCoroutine(this.coroutine);
        }

        public IEnumerator start_enumerator()
        {
            if (this.mesh_filter != null)
            {
                while (true)
                {
                    this.create_single_trail();
                    yield return new WaitForSeconds(this.time_interval);
                }
            }
            yield return null;
        }

        //combine mesh
        private Mesh combine_mesh()
        {
            MeshFilter render = this.mesh_filter;
            Mesh mesh = Instantiate((render.sharedMesh != null) ? render.sharedMesh : render.mesh);

            CombineInstance conbine_instacne = new CombineInstance
            {
                mesh = mesh,
                transform = render.gameObject.transform.localToWorldMatrix,
                subMeshIndex = this.submesh_index
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
                mesh = this.combine_mesh(),

                material = new Material(this.mat_trail),

                left_time = this.time_trail_show,
            };

            this.list_drawing_mesh.Add(drawing_mesh);
        }
    }
}