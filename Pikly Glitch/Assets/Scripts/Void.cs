using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Void : MonoBehaviour {
    public Vector3[] points;

    [Range(0.01f, 2f)]
    public float moveVariation = 1.1f, speed = 0.1f;

    public Material material;

    GameObject voidObj;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public Vector3[] p;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        CalcVariations();
    }

    void CalcVariations() {
        p = new Vector3[points.Length];

        //int i = 0;
        //foreach (Vector3 point in points) {
        //    p[i] = new Vector3(Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation));
        //    DOTween.To(() => point, x => point = x, p[i], speed).SetEase(Ease.InExpo).SetLoops(-1, LoopType.Yoyo);
        //    i++;
        int i = 0;
        for (i = 0; i < points.Length; i++) {
            p[i] = new Vector3(Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation));
            DOTween.To(() => points[i], x => points[i] = x, p[i], speed).SetEase(Ease.InExpo).SetLoops(-1, LoopType.Yoyo);
        }
        //}
    }

    void Update() {
        UpdatePolygon();
    }

    void UpdatePolygon() {
        meshRenderer.material = material;
        meshFilter.mesh = CreateMesh();
    }

    Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        var vertex = new Vector3[points.Length];

        for (int x = 0; x < points.Length; x++) {
            vertex[x] = points[x];
        }

        var uvs = new Vector2[vertex.Length];
        for (int x = 0; x < vertex.Length; x++) {
            if ((x % 2) == 0) {
                uvs[x] = Vector2.zero;
            } else {
                uvs[x] = Vector2.one;
            }
        }

        var tris = new int[3 * (vertex.Length - 2)];
        int C1;
        int C2;
        int C3;

        C1 = 0;
        C2 = vertex.Length - 1;
        C3 = vertex.Length - 2;

        for (int x = 0; x < tris.Length; x += 3) {
            tris[x] = C1;
            tris[x + 1] = C2;
            tris[x + 2] = C3;

            C2--;
            C3--;
        }

        mesh.vertices = vertex;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.name = "VoidMesh";

        return mesh;
    }
}
