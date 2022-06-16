using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape4D : MonoBehaviour
{
    [Range(0.001f, 2f)]
    public float obj_scale = 1f; //Scale of the shape

    public Vector4 center = new Vector4(0f, 0f, 0f, 0f); //4D center of the shape
    public Vector4[] save_points; //Copy of the shape's vertices, but centered at 4D origin
    [SerializeField] private Vector4[] points; //The current vertice positions
    public int[] temp_edges; //Adjacency list of the vertices in the shape

    private float minp; //The largest w coordinate of all the vertices
    private float maxp; //The smallest w coordinate of all the vertices

    //Objects in the scene Shape4D needs
    private Transform projection;
    private Transform shadow_points;
    private Transform shadow_edges;
    private Transform my_light;

    // Start is called before the first frame update
    void Start()
    {
        //Loads data, stores points in points, calcultes minp and maxp,
        // and one time orients 3D shadow
        load_objects();
        points = new Vector4[save_points.Length];
        for (int i = 0; i < save_points.Length; i++)
        { save_points[i] = save_points[i] * obj_scale; }
        for (int i = 0; i < save_points.Length; i++)
        { points[i] = save_points[i] + center; }
        minmaxp();
        Shadow_Update();
        Update_Edges();
    }

    // OnValidate is called when the script is loaded or when a value is changed in the inspector
    void OnValidate()
    {
        //Scales object when inspector obj_scale is changed
        /*for (int i = 0; i < save_points.Length; i++)
        {
            save_points[i] = (save_points[i] / last_obj_scale) * obj_scale;
        }
        last_obj_scale = obj_scale;*/
    }

    //Loads the memory locations of all objects Shape4D needs
    public void load_objects()
    {
        projection = this.gameObject.transform.GetChild(0);
        shadow_points = projection.GetChild(0);
        shadow_edges = projection.GetChild(1);
        my_light = GameObject.Find("My Light").transform;
    }

    //Stores the positions of points[] vectors in save_points[]
    // as if points[] was centered at the origin
    public void Vertex_Store()
    {
        for (int i = 0; i < save_points.Length; i++)
        {
            save_points[i] = points[i] - center;
        }
    }

    //Sets minp and maxp based on points[] vectors
    private void minmaxp()
    {
        int max = 0;
        int min = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].w > points[max].w)
            { max = i; }
            else if (points[i].w < points[min].w)
            { min = i; }
        }
        maxp = points[max].w;
        minp = points[min].w;
    }

    //Returns the smallest right rectangular prism
    // that has a face parallel with the ground plane
    // that the shadow of the shape could fit into
    public Vector3[] boundingbox()
    {
        Vector3 def = shadow_points.GetChild(0).position;
        Vector3 max = def;
        Vector3 min = def;

        for (int i = 1; i < shadow_points.childCount; i++)
        {
            Vector3 shadow = shadow_points.GetChild(i).position;

            if (shadow.x > max.x)
            { max.x = shadow.x; }
            else if (shadow.x < min.x)
            { min.x = shadow.x; }

            if (shadow.y > max.y)
            { max.y = shadow.y; }
            else if (shadow.y < min.y)
            { min.y = shadow.y; }

            if (shadow.z > max.z)
            { max.z = shadow.z; }
            else if (shadow.z < min.z)
            { min.z = shadow.z; }
        }
        return new Vector3[] { min, max };
    }

    //Rotates the vertices of points[], using save_points[] current rotation
    //Updates shape's shadow afterwards
    public void Rotate(float xyRotation, float xzRotation, float xwRotation, float yzRotation, float ywRotation, float zwRotation)
    {
        Vector4[] xyrot = Matrix.rotate_xy(xyRotation);
        Vector4[] xzrot = Matrix.rotate_xz(xzRotation);
        Vector4[] xwrot = Matrix.rotate_xw(xwRotation);
        Vector4[] yzrot = Matrix.rotate_yz(yzRotation);
        Vector4[] ywrot = Matrix.rotate_yw(ywRotation);
        Vector4[] zwrot = Matrix.rotate_zw(zwRotation);
        Vector4[] rotation = Matrix.matmul(Matrix.matmul(Matrix.matmul(Matrix.matmul(Matrix.matmul(xyrot, xzrot), xwrot), yzrot), ywrot), zwrot);
        for (int i = 0; i < save_points.Length; i++)
        {
            points[i] = Matrix.rotate_point(save_points[i], rotation) + center;
        }
        Shadow_Update();
        Update_Edges();
    }

    //Changes the position of the shape, using save_points[] current rotation
    //Updates shape's shadow afterwards
    public void Translate(Vector4 translation)
    {
        center = translation;
        for (int i = 0; i < save_points.Length; i++)
        {
            points[i] = save_points[i] + center;
        }
        Shadow_Update();
        Update_Edges();
    }

    //Iterates through all the shadow's cylinders and orients them properly
    public void Update_Edges()
    {
        for (int i = 0; i < shadow_edges.childCount; i++)
        {
            Matrix.update_edge(shadow_points.GetChild(temp_edges[i * 2]), shadow_points.GetChild(temp_edges[i * 2 + 1]), shadow_edges.GetChild(i));
        }
    }

    //Orients and colors the objects of shadow based on points[] vertices and my light
    public void Shadow_Update()
    {
        minmaxp();
        //for each vertex in points
        for (int i = 0; i < save_points.Length; i++)
        {
            //Set the position of the vertex's shadow
            Transform shadow = shadow_points.GetChild(i);
            Vector4 sphere = points[i];
            Vector4 light4d = new Vector4(my_light.position.x, my_light.position.y, my_light.position.z, my_light.gameObject.GetComponent<MyLight>().w);
            float[] shadow_v3 = Matrix.w_4d_3d(light4d, sphere, minp, maxp);
            shadow.position = new Vector3(shadow_v3[0], shadow_v3[1], shadow_v3[2]);

            //Scale the shadow based on the origional vertex's lost y value
            float scale = (obj_scale * 0.2f) + (shadow_v3[3] / 12f);
            shadow.localScale = new Vector3(scale, scale, scale);

            //Change the color of the shadow based on the origional vertex's lost y value
            var shadowrend = (shadow.gameObject).GetComponent<Renderer>();
            shadowrend.material.SetColor("_BaseColor", new Color(shadow_v3[3], 0f, 1f - shadow_v3[3], 0.8f));
        }
        //for each edge of skeleton
        for (int i = 0; i < shadow_edges.childCount; i++)
        {
            //Create a new mesh and array of vertices for the edge shadow
            Mesh mesh = shadow_edges.GetChild(i).gameObject.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            //Create new color array where the colors will be stored.
            Color[] colors = new Color[vertices.Length];

            //Store the colors of the two vertices of this edge
            var p1c = shadow_points.GetChild(temp_edges[i * 2]).GetComponent<Renderer>().material.color;
            var p2c = shadow_points.GetChild(temp_edges[i * 2 + 1]).GetComponent<Renderer>().material.color;

            //Calculate the color of each vertex
            float miny = vertices[0].y;
            float maxy = vertices[vertices.Length - 1].y;
            for (int j = 0; j < vertices.Length; j++)
            {
                colors[j] = Color.Lerp(p2c, p1c, (vertices[j].y - miny) / (maxy - miny));
            }

            // assign the array of colors to the Mesh.
            mesh.colors = colors;
        }
    }

}