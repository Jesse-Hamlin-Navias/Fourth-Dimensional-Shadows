using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    //multiplies vectors and matrices together. An array of vectors is a matrix
    public static Vector3 matmul(Vector3[] r, Vector3 v3)
    {
        return new Vector3((v3.x * r[0].x + v3.y * r[1].x + v3.z * r[2].x),
                           (v3.x * r[0].y + v3.y * r[1].y + v3.z * r[2].y),
                           (v3.x * r[0].z + v3.y * r[1].z + v3.z * r[2].z));
    }

    public static Vector4 matmul(Vector4[] r, Vector4 v4)
    {
        return new Vector4((v4.x * r[0].x + v4.y * r[1].x + v4.z * r[2].x + v4.w * r[3].x),
                           (v4.x * r[0].y + v4.y * r[1].y + v4.z * r[2].y + v4.w * r[3].y),
                           (v4.x * r[0].z + v4.y * r[1].z + v4.z * r[2].z + v4.w * r[3].z),
                           (v4.x * r[0].w + v4.y * r[1].w + v4.z * r[2].w + v4.w * r[3].w));
    }

    public static Vector3[] matmul(Vector3 v3, Vector3[] r)
    {
        float r0 = r[0].x + r[0].y + r[0].z;
        float r1 = r[1].x + r[1].y + r[1].z;
        float r2 = r[2].x + r[2].y + r[2].z;
        return new Vector3[] {new Vector3(v3.x * r0, v3.y * r0, v3.z * r0),
                              new Vector3(v3.x * r1, v3.y * r1, v3.z * r1),
                              new Vector3(v3.x * r2, v3.y * r2, v3.z * r2) };
    }

    public static Vector3[] matmul(Vector3[] a, Vector3[] b)
    {
        Vector3[] holder = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                holder[i][j] = a[0][j] * b[i][0] + a[1][j] * b[i][1] + a[2][j] * b[i][2];
            }
        }

        return holder;
    }

    public static Vector4[] matmul(Vector4[] a, Vector4[] b)
    {
        Vector4[] holder = new Vector4[4];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                holder[i][j] = a[0][j] * b[i][0] + a[1][j] * b[i][1] + a[2][j] * b[i][2] + +a[3][j] * b[i][3];
            }
        }

        return holder;
    }

    //Generates a 3D rotation matrix around the x axis given an angle
    public static Vector3[] rotate_x(float angle)
    {
        return new Vector3[] { new Vector3(1f, 0f, 0f),
                               new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle)),
                               new Vector3(0f, -1f * Mathf.Sin(angle), Mathf.Cos(angle)) };
    }

    //Generates a 3D rotation matrix around the y axis given an angle
    public static Vector3[] rotate_y(float angle)
    {
        return new Vector3[] { new Vector3(Mathf.Cos(angle), 0f, -1f * Mathf.Sin(angle)),
                               new Vector3(0f, 1f, 0f),
                               new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) };
    }

    //Generates a 3D rotation matrix around the z axis given an angle
    public static Vector3[] rotate_z(float angle)
    {
        return new Vector3[] { new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f),
                               new Vector3(-1f * Mathf.Sin(angle), Mathf.Cos(angle), 0f),
                               new Vector3(0f, 0f, 1f) };
    }

    //Generates a 3D rotation matrix given an x, y, and z angle
    public static Vector3[] rotate3d(float xa, float ya, float za)
    {
        float cosx = Mathf.Cos(-xa);
        float cosy = Mathf.Cos(-ya);
        float cosz = Mathf.Cos(-za);
        float sinx = Mathf.Sin(-xa);
        float siny = Mathf.Sin(-ya);
        float sinz = Mathf.Sin(-za);
        return new Vector3[] { new Vector3(cosy*cosz, sinx*siny*cosz-cosx*sinz, cosx*siny*cosz+sinx*sinz),
                               new Vector3(cosy*sinz, sinx*siny*sinz+cosx*cosz, cosx*siny*sinz-sinx*cosz),
                               new Vector3(-siny,     sinx*cosy,                cosx*cosy) };
    }

    //Generates a 4D rotation matrix on the xy plane, given an angle
    public static Vector4[] rotate_xy(float angle)
    {
        return new Vector4[] { new Vector4(1f, 0f, 0f, 0f),
                               new Vector4(0f, 1f, 0f, 0f),
                               new Vector4(0f, 0f, Mathf.Cos(angle), Mathf.Sin(angle)),
                               new Vector4(0f, 0f, -1f * Mathf.Sin(angle), Mathf.Cos(angle))    };
    }

    //Generates a 4D rotation matrix on the xz plane, given an angle
    public static Vector4[] rotate_xz(float angle)
    {
        return new Vector4[] { new Vector4(1f, 0f, 0f, 0f),
                               new Vector4(0f, Mathf.Cos(angle), 0f, Mathf.Sin(angle)),
                               new Vector4(0f, 0f, 1f, 0f),
                               new Vector4(0f, -1f * Mathf.Sin(angle), 0f, Mathf.Cos(angle))    };
    }

    //Generates a 4D rotation matrix on the xw plane, given an angle
    public static Vector4[] rotate_xw(float angle)
    {
        return new Vector4[] { new Vector4(1f, 0f, 0f, 0f),
                               new Vector4(0f, Mathf.Cos(angle), Mathf.Sin(angle), 0f),
                               new Vector4(0f, -1f * Mathf.Sin(angle), Mathf.Cos(angle), 0f),
                               new Vector4(0f, 0f, 0f, 1f)    };
    }

    //Generates a 4D rotation matrix on the yz plane, given an angle
    public static Vector4[] rotate_yz(float angle)
    {
        return new Vector4[] { new Vector4(Mathf.Cos(angle), 0f, 0f, Mathf.Sin(angle)),
                               new Vector4(0f, 1f, 0f, 0f),
                               new Vector4(0f, 0f, 1f, 0f),
                               new Vector4(-1f * Mathf.Sin(angle), 0f, 0f, Mathf.Cos(angle))};
    }

    //Generates a 4D rotation matrix on the yw plane, given an angle
    public static Vector4[] rotate_yw(float angle)
    {
        return new Vector4[] { new Vector4(Mathf.Cos(angle), 0f, Mathf.Sin(angle), 0f),
                               new Vector4(0f, 1f, 0f, 0f),
                               new Vector4(-1f * Mathf.Sin(angle), 0f, Mathf.Cos(angle), 0f),
                               new Vector4(0f, 0f, 0f, 1f)  };
    }

    //Generates a 4D rotation matrix on the zw plane, given an angle
    public static Vector4[] rotate_zw(float angle)
    {
        return new Vector4[] { new Vector4(Mathf.Cos(angle), Mathf.Sin(angle), 0f, 0f),
                               new Vector4(-1f * Mathf.Sin(angle), Mathf.Cos(angle), 0f, 0f),
                               new Vector4(0f, 0f, 1f, 0f),
                               new Vector4(0f, 0f, 0f, 1f)  };
    }

    //Rotates a 3D point, given a point and a 3D rotation matrix.
    //Rotates around the origin, but can be given an alternative center point
    public static Vector3 rotate_point(Vector3 p, Vector3[] rotate)
    { return matmul(rotate, p); }

    public static Vector3 rotate_point(Vector3 p, Vector3 c, Vector3[] rotate)
    { return (matmul(rotate, p - c)) + c; }

    //Rotates a 4D point, given a point and a 4D rotation matrix.
    //Rotates around the origin, but can be given an alternative center point
    public static Vector4 rotate_point(Vector4 p, Vector4[] rotate)
    { return matmul(rotate, p); }

    public static Vector4 rotate_point(Vector4 p, Vector4 c, Vector4[] rotate)
    { return (matmul(rotate, p - c)) + c; }

    //Given a 3D light l and a 3D point p, returns the 2D point
    //of the perspective projection of p via l onto the ground plane.
    //Also returns a scaling factor based on the points fractional hight
    //in respect to the lowest and highest point of a shape
    public static float[] y_3d_2d(Vector3 l, Vector3 p, float min_y, float max_y)
    {
        //Is the light above the plane
        if (l.y > 0f)
        {
            //Is the light above the point and is the point above the plane
            if (l.y > p.y & p.y > 0)
            {
                float den = (p.y - l.y) / l.y;
                float new_x = l.x - ((p.x - l.x) / den);
                float new_z = l.z - ((p.z - l.z) / den);
                float scaling = Mathf.Clamp((p.y - min_y) / (max_y - min_y), 0f, 1f);
                return new float[] {new_x, scaling, new_z};
            }
            //Scalings of -1 are interpreted as points not to be shown
            else { return new float[] { 0f, -1f, 0f }; }
        }
        //Isomentric projection
        else { return new float[] { p.x, 0.5f, p.z }; }

    }

    //Given a 4D light l and a 4D point p, returns the 3D point
    //of the perspective projection of p via l onto our 3D space.
    //Also returns a scaling factor based on the points fractional w coordinate
    //in respect to the lowest and highest w coordinate of a shape
    public static float[] w_4d_3d(Vector4 l, Vector4 p, float min_w, float max_w)
    {
        //Is the light above our 3D space
        if (l.w > 0f)
        {
            //Is the light above the point
            if (l.w > p.w)
            {
                float den = (p.w - l.w) / l.w;
                float new_x = l.x - ((p.x - l.x) / den);
                float new_y = l.y - ((p.y - l.y) / den);
                float new_z = l.z - ((p.z - l.z) / den);
                float scaling = Mathf.Clamp((p.w - min_w) / (max_w - min_w), 0f, 1f);
                return new float[] { new_x, new_y, new_z, scaling };
            }
            //Hides the point below the ground plane of the 3D space
            else { return new float[] { 0f, -1f, 0f, 1f }; }
        }
        //Isometric Projection
        else { return new float[] { p.x, p.y, p.z, 0.5f }; }

    }

    //Orients a cylinder such that it connects two points in space
    public static void update_edge(Transform point1, Transform point2, Transform edge)
    {
        Vector3 point1v = point1.position;
        Vector3 point2v = point2.position;
        
        //Scales the cylinder to be the magnitude between the two points
        edge.localScale = new Vector3(edge.localScale.x,
                          (Vector3.Distance(point1v, point2v) / 2f) * (1f / edge.parent.parent.localScale.x),
                          edge.localScale.z);

        //Rotates the cylinder to be the angle between the two points
        edge.position = point2v;
        edge.LookAt(point1);
        edge.Rotate(90f, 0, 0);

        //Positions the cylinder to be between the two points
        edge.position = (point1v + point2v) / 2f;
    }

    //Orients a plane such that it connects two points in space
    public static void update_shadow_edge(Transform point1, Transform point2, Transform edge)
    {
        Vector3 point1v = point1.position;
        Vector3 point2v = point2.position;
        float scale = point1.localScale.x * 4f;

        //Scales the plane to be the magnitude between the two points
        edge.localScale = new Vector3(0.005f * scale, 0.0003f, 
            (Vector3.Distance(point1v, point2v) / 2f) * (1f / edge.parent.parent.localScale.x) * 0.2f);

        //Rotates the plane to be the angle between the two points
        edge.position = point2v;
        edge.LookAt(point1, Vector3.zero);

        //Positions the plan to be between the two points
        edge.position = (point1v + point2v) / 2f;
    }
}
