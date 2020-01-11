using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Void : MonoBehaviour {
    public Ease[] eases = new Ease[] {
        Ease.InOutSine,
        Ease.InOutElastic,
        Ease.InOutBounce,
        Ease.InOutCirc
    };

    public Vector3[] points, startingPoints;
    public int[] index;

    [Range(0.01f, 2f)]
    public float moveVariation = 1.1f, minDuration = 0.1f, maxDuration = 0.5f;

    public Material material;

    GameObject voidObj;

    PolygonCollider2D poly;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public Vector3[] variation;

    public Ease GetEase() {
        return eases[Random.Range(0, eases.Length - 1)];
    }

    public float GetDuration() {
        return Random.Range(minDuration, maxDuration);
    }

    void Start() {
        GetComponents();

        startingPoints = (Vector3[])points.Clone();

        DoTheThing();
    }

    void GetComponents() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        poly = GetComponent<PolygonCollider2D>();
    }

    int i = 0;
    void CalcVariations() {
        variation = new Vector3[points.Length];
        
        for (i = 0; i < points.Length; i++) {
            variation[i] = startingPoints[i] + new Vector3(Random.Range(-moveVariation, moveVariation), Random.Range(-moveVariation, moveVariation), 0);
        }
    }

    void DoTheThing() {
        CalcVariations();

        float duration = GetDuration();
        DOTween.To(() => points[0], x => points[0] = x, variation[0], duration).SetEase(GetEase());
        DOTween.To(() => points[1], x => points[1] = x, variation[1], duration).SetEase(GetEase());
        DOTween.To(() => points[2], x => points[2] = x, variation[2], duration).SetEase(GetEase()).OnComplete(DoTheThing);
    }

    void Update() {
        meshRenderer.material = material;
        meshFilter.mesh = CreateMesh();

        UpdatePolygon();
    }

    void UpdatePolygon() {
        poly.points = new Vector2[] { points[0], points[1], points[2] };
        poly.SetPath(0, new Vector2[] { points[0], points[1], points[2] });
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

    void OnDrawGizmos() {
        GetComponents();
        UpdatePolygon();
    }
}
