using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pikl {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Void : MonoBehaviour {
        public Ease[] eases = new Ease[] {
            Ease.InOutSine,
            Ease.InOutElastic,
            Ease.InOutBounce,
            Ease.InOutCirc
        };

        public Vector3[] points, abPoints, startingPoints;
        public int[] index;

        [Range(0.01f, 2f)]
        public float moveVariation = 1.1f, minDuration = 0.1f, maxDuration = 0.5f;

        public Material material, red, yellow, green;
        Material _abMaterial;

        GameObject _voidObj;

        PolygonCollider2D _poly;
        MeshRenderer _meshRenderer, _abRenderer;
        MeshFilter _meshFilter, _abFilter;

        Transform _player;

        public Vector3[] variation, abVariation;
        const string MESH_NAME = "VoidMesh";
    
        Ease GetEase() => eases[Random.Range(0, eases.Length - 1)];
        float GetDuration() => Random.Range(minDuration, maxDuration);

        void Start() {
            GetComponents();

            startingPoints = (Vector3[])points.Clone();
            abPoints = (Vector3[])points.Clone();

            switch (Random.Range(0, 3)) {
                case 0: _abMaterial = red; break;
                case 1: _abMaterial = yellow; break;
                case 2: _abMaterial = green; break;
            }

            DoTheThing();
            InvokeRepeating("SlowUpdate", 0, 0.035f);
        }

        void GetComponents() {
            _player = GameObject.Find("Player")?.transform;

            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _poly = GetComponent<PolygonCollider2D>();
            _abRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _abFilter= transform.GetChild(0).GetComponent<MeshFilter>();

        }

        int i = 0;
        void CalcVariations() {
            variation = new Vector3[points.Length];
            abVariation = new Vector3[points.Length];

            for (i = 0; i < points.Length; i++) {
                variation[i] = startingPoints[i] + new Vector3(Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation), 0);
                abVariation[i] = startingPoints[i] + new Vector3(Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation), 0);
            }
        }

        void DoTheThing() {
            CalcVariations();

            float duration = GetDuration();
            DOTween.To(() => points[0], x => points[0] = x, variation[0], duration).SetEase(GetEase());
            DOTween.To(() => points[1], x => points[1] = x, variation[1], duration).SetEase(GetEase());
            DOTween.To(() => points[2], x => points[2] = x, variation[2], duration).SetEase(GetEase()).OnComplete(DoTheThing);

            DOTween.To(() => abPoints[0], x => abPoints[0] = x, abVariation[0], duration).SetEase(GetEase());
            DOTween.To(() => abPoints[1], x => abPoints[1] = x, abVariation[1], duration).SetEase(GetEase());
            DOTween.To(() => abPoints[2], x => abPoints[2] = x, abVariation[2], duration).SetEase(GetEase());
        }

        void SlowUpdate() {
            if (_player == null || Vector2.Distance(_player.position, transform.position) > 20) {
                DOTween.Pause(this);
                return;
            }
        
            _meshRenderer.material = material;
            _meshFilter.mesh = CreateMesh(points);

            _abRenderer.material = _abMaterial;
            _abFilter.mesh = CreateMesh(abPoints);

            UpdatePolygon();
        }

        void UpdatePolygon() {
            _poly.points = new Vector2[] { points[0], points[1], points[2] };
            _poly.SetPath(0, new Vector2[] { points[0], points[1], points[2] });
        }
    
        const int C1 = 0;

        static Mesh CreateMesh(IReadOnlyList<Vector3> _points) {
            Mesh mesh = new Mesh();
            Vector3[] vertex = new Vector3[_points.Count];

            for (int x = 0; x < _points.Count; x++) {
                vertex[x] = _points[x];
            }

            Vector2[] uvs = new Vector2[vertex.Length];
            for (int x = 0; x < vertex.Length; x++) {
                if ((x % 2) == 0) {
                    uvs[x] = Vector2.zero;
                } else {
                    uvs[x] = Vector2.one;
                }
            }

            int[] tris = new int[3 * (vertex.Length - 2)];

        
            int c2 = vertex.Length - 1;
            int c3 = vertex.Length - 2;

            for (int x = 0; x < tris.Length; x += 3) {
                tris[x] = C1;
                tris[x + 1] = c2;
                tris[x + 2] = c3;

                c2--;
                c3--;
            }

            mesh.vertices = vertex;
            mesh.uv = uvs;
            mesh.triangles = tris;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            mesh.name = MESH_NAME;

            return mesh;
        }

        void OnDrawGizmos() {
            GetComponents();
            UpdatePolygon();
        
        }
    }
}
