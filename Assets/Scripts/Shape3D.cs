using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape3D : MonoBehaviour
{
    [Range(0.001f, 4f)]
    public float obj_scale = 1f; //A fraction to rescale the 3D shape by
    [Range(0.001f, 10f)]
    public float grab_range = 1f; //A fraction to fine tune grab interaction range
    
    public Vector3 center = Vector3.zero; //The center points of the shape
    [SerializeField] private int[] temp_edges; //Holds the adjacency information of shape's points. Inputed manuly
    
    private Vector3[] vertices; //Holds a copy of shape's vertex's positions as if shape was at the origin
    private float minp; //The largest y coordinate of all the vertices
    private float maxp; //The smalles y coordinate of all the vertices

    //Objects in the scene Shape3D needs to run:
    private Transform mylight;
    private Transform skeleton;
    private Transform points;
    private Transform edges;
    private Transform projection;
    private Transform shadow_points;
    private Transform shadow_edges;
    private Transform my_light;

    // Start is called before the first frame update
    void Start()
    {
        //Loads data, stores points in vertices, calcultes minp and maxp,
        // and one time orients 3D edges and 2D shadow
        load_objects();
        center = skeleton.position;
        vertices = new Vector3[points.childCount];
        Vertex_Store();
        minmaxp();
        Update_Edges();
        Shadow_Update();
    }

    // OnValidate is called when the script is loaded or when a value is changed in the inspector
    void OnValidate()
    {
        //Scales object when inspector obj_scale is changed
        this.gameObject.transform.GetChild(0).localScale = new Vector3(obj_scale, obj_scale, obj_scale);
    }

    //Loads the memory locations of all objects Shape3D needs
    private void load_objects()
    {
        skeleton = this.gameObject.transform.GetChild(0);
        points = skeleton.GetChild(0);
        edges = skeleton.GetChild(1);
        projection = this.gameObject.transform.GetChild(1);
        shadow_points = projection.GetChild(0);
        shadow_edges = projection.GetChild(1);
        my_light = GameObject.Find("My Light").transform;
    }

    //Stores the positions of skeleton's points in vertices[]
    // as if skeleton was centered at the origin
    public void Vertex_Store()
    {
        for (int i = 0; i < points.childCount; i++)
        {
            vertices[i] = points.GetChild(i).position - center;
        }
    }

    //Sets minp and maxp based on skeleton's points
    private void minmaxp()
    {
        int max = 0;
        int min = 0;
        for (int i = 0; i < points.childCount; i++)
        {
            if (points.GetChild(i).position.y > points.GetChild(max).position.y)
            { max = i; }
            else if (points.GetChild(i).position.y < points.GetChild(min).position.y)
            { min = i; }
        }
        maxp = points.GetChild(max).position.y;
        minp = points.GetChild(min).position.y;
    }

    //Rotates the points of skeleton, using vertices[] current rotation
    //Updates shape's edges and shadow afterwards
    public void Rotate(float xRotation, float yRotation, float zRotation)
    {
        Vector3[] rotation = Matrix.rotate3d(xRotation, yRotation, zRotation);
        for (int i = 0; i < points.childCount; i++)
        {
            Transform sphere = points.GetChild(i);
            sphere.position = Matrix.rotate_point(vertices[i], rotation) + center;
        }
        Update_Edges();
        Shadow_Update();
    }

    //Changes the position of skeleton, using vertices[] current rotation
    //Updates shape's edges and shadow afterwads
    public void Translate(Vector3 translation)
    { 
        center = translation;
        for (int i = 0; i < points.childCount; i++)
        {
            Transform sphere = points.GetChild(i);
            sphere.position = vertices[i] + center;
        }
        Update_Edges();
        Shadow_Update();
    }

    //Iterates through all cylinders and orients them properly
    public void Update_Edges()
    {
        for (int i = 0; i < edges.childCount; i++)
            Matrix.update_edge(points.GetChild(temp_edges[i * 2]), points.GetChild(temp_edges[i * 2 + 1]), edges.GetChild(i));
    }

    //Orients and colors the objects of shadow based on skeleton's vertices and my light
    public void Shadow_Update()
    {
        minmaxp();
        //for each vertex of skeleton
        for (int i = 0; i < points.childCount; i++)
        {
            //Set the position of the vertex's shadow
            Transform sphere = points.GetChild(i);
            Transform shadow = shadow_points.GetChild(i);
            float[] sv = Matrix.y_3d_2d(my_light.position, sphere.position, minp, maxp);
            shadow.position = new Vector3(sv[0], Mathf.Clamp(sv[1], -1f, 0.001f), sv[2]);

            //Scale the shadow based on the origional vertex's lost y value
            float scale = sphere.localScale.x * (obj_scale + sv[1] / 2f);
            shadow.localScale = new Vector3(scale, 0.0001f, scale);

            //Change the color of the shadow based on the origional vertex's lost y value
            var shadowrend = (shadow.gameObject).GetComponent<Renderer>();
            shadowrend.material.SetColor("_BaseColor", new Color(sv[1], 0f, 1f - sv[1], 0.8f));
        }
        //for each edge of skeleton
        for (int i = 0; i < edges.childCount; i++)
        {
            //Orient the shadow of edge so that it connects its two shadow vertices
            Matrix.update_shadow_edge(shadow_points.GetChild(temp_edges[i * 2]), shadow_points.GetChild(temp_edges[i * 2 + 1]), shadow_edges.GetChild(i));

            //Create a new mesh and array of vertices for the edge shadow
            Mesh mesh = shadow_edges.GetChild(i).gameObject.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            //Create new color array where the colors will be stored.
            Color[] colors = new Color[vertices.Length];

            //Store the colors of the two vertices of this edge
            var p1c = shadow_points.GetChild(temp_edges[i * 2]).GetComponent<Renderer>().material.color;
            var p2c = shadow_points.GetChild(temp_edges[i * 2 + 1]).GetComponent<Renderer>().material.color;

            //Calculate the color of each vertex
            float minz = vertices[0].z;
            float maxz = vertices[vertices.Length - 1].z;
            for (int j = 0; j < vertices.Length; j++)
            {
                colors[j] = Color.Lerp(p2c, p1c, (vertices[j].z-minz)/(maxz-minz));
            }

            // assign the array of colors to the Mesh.
            mesh.colors = colors;
        }
    }
}