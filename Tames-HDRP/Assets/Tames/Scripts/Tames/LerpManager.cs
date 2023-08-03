using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class LerpManager
{
    public int[] time;
    public int sum;
    public int[] start;
    private float timeUnit;
    private float progressUnit;
    private Vector2[] O = new Vector2[2];
    private float[] end = new float[4];
    private float p, q, A, B, alpha, beta, m, R, slope, mO;
    public bool valid = true;
    public bool curved = false;
    private const float Third = 1f / 3;
    private Vector2 u, v;
    //  private float mu, m
    private enum CurveType { NegLine, Negative, Line, Positive, PosLine };
    CurveType type = CurveType.Negative;

    public LerpManager(float P, float Q)
    {
        curved = true;
        p = P / 2;
        q = Q / 2;
        if (P == Q)
            type = CurveType.Line;
        else if (P < Q)
        {
            type = CurveType.Negative;
            m = (0.5f - q) / (0.5f - p);
            B = (m * p - Third * q) / (0.5f * Third * Mathf.Sqrt(p));
            A = (0.5f * q - m * p) / (0.5f * Third * Mathf.Pow(p, 1f / 3));
        }
        else
        {
            type = CurveType.Positive;
            m = (0.5f - q) / (0.5f - p);
            B = (3 * q - p * m) / (p * p);
            A = (p * m - 2 * q) / Mathf.Pow(p, 3);
         //   Debug.Log("SLERP: " + p + " " + q + " " + A + " " + B);
        }

    }
    public LerpManager(float s)
    {
        curved = true;
        if (s == 0)
            type = CurveType.Line;
        else if (s == -1)
            type = CurveType.NegLine;
        else if (s == 1)
            type = CurveType.PosLine;
        else if (s > 0)
            type = CurveType.Positive;
        Vector2 p;
        float n = 0, q;
        if (type == CurveType.Positive)
        {
            m = s / 2;
            alpha = s * MathF.PI / 4;
            beta = MathF.PI / 4 - alpha;
            slope = MathF.Tan(beta);
            u = m * new Vector2(MathF.Cos(beta), MathF.Sin(beta));
            v = m * new Vector2(MathF.Sin(beta), MathF.Cos(beta));
            n = MathF.Sqrt(2) / (4 * MathF.Cos(alpha));
            R = (n - m) / MathF.Tan(alpha);

            O[0] = u - R * new Vector2(u.y, -u.x).normalized;
            O[1] = Vector2.one - u + R * new Vector2(u.y, -u.x).normalized;


            end[0] = MathF.Sin(beta) * m;
            end[1] = 0.5f - MathF.Cos(alpha) * m;
            end[2] = 0.5f + MathF.Cos(alpha) * m;
            end[3] = 1 - MathF.Sin(beta) * m;
        }
        else if (type == CurveType.Negative)
        {
            m = s / 2;
            alpha = s * MathF.PI / 4;
            beta = alpha + MathF.PI / 4;
            slope = MathF.Tan(beta);
            u = m * new Vector2(MathF.Cos(beta), MathF.Sin(beta));
            v = m * new Vector2(MathF.Sin(beta), MathF.Cos(beta));
            n = MathF.Sqrt(2) / (4 * MathF.Cos(alpha));
            R = (n - m) / MathF.Tan(alpha);

            O[0] = u + R * new Vector2(u.y, -u.x);
            O[1] = Vector2.one - u - R * new Vector2(u.y, -u.x);


            end[0] = MathF.Sin(beta) * m;
            end[1] = 0.5f - MathF.Cos(alpha) * m;
            end[2] = 0.5f + MathF.Cos(alpha) * m;
            end[3] = 1 - MathF.Sin(beta) * m;
        }
  //      Debug.Log("SLERP: " + alpha + ", " + beta + ", " + n);
    //    Debug.Log("SLERP: " + end[0] + ", " + end[1] + ", " + end[2] + ", " + end[3]);
    //    Debug.Log("SLERP: " + R + " " + O[0].ToString("0.00") + O[1].ToString("0.00"));
    }
    public LerpManager(int[] d)
    {
        time = d;
        start = new int[d.Length];
        sum = 0;
        for (int i = 0; i < time.Length; i++)
        {
            start[i] = sum;
            sum += d[i];
        }

        progressUnit = 1f / sum;
        timeUnit = 1f / time.Length;

    }
    public float On(float t)
    {
        if (curved)
            return Curved(t);
        else
            return Linear(t);
    }
    public float Linear(float t)
    {
        if (t >= 1f)
            return 1f;
        else if (t <= 0f)
            return 0f;
        else
        {
            float r = t / timeUnit;
            int n = (int)r;
            float d = r - n;
            Debug.Log("slerp: " + n + " " + time.Length + " " + t);
            r = (start[n] + d * time[n]) * progressUnit;
            return r;
        }
    }
    public float Curved(float t)
    {
        float y;
        if (t >= 1f)
            return 1f;
        else if (t <= 0f)
            return 0f;
        else switch (type)
            {
                case CurveType.Line: return t;
                case CurveType.NegLine: return 0.5f;
                case CurveType.PosLine: return t < 0.5f ? 0 : 1;
                case CurveType.Positive:
                    if (t <= p)
                        return A * t * t * t + B * t * t;
                    else if (t >= 1 - p)
                        return 1 - A * Mathf.Pow(1 - t, 3) - B * Mathf.Pow(1 - t, 2);
                    else
                        return q + (t - p) * m;
                default:
                    if (t <= p)
                        return A * Mathf.Pow(t, Third) + B * Mathf.Sqrt(t);
                    else if (t >= 1 - p)
                        return 1 - A * Mathf.Pow(1 - t, Third) - B * Mathf.Sqrt(1 - t);
                    else
                        return q + (t - p) * m;
            }
    }
    public static LerpManager FromString(string line)
    {
        string[] s = line.Split(',');
        if (s.Length == 2)
        {
            try
            {
                float P = float.Parse(s[0]);
                float Q = float.Parse(s[1]);
           //     Debug.Log("SLERP: " + P + " " + Q);
                return new LerpManager(P, Q);

            }
            catch (Exception e)
            {
                return null;
            }
        }
        else
        {
            int[] d = ReadString(s);
            if (d != null)
            {

                return new LerpManager(d);
            }
            else
                return null;
        }
    }
    private static int[] ReadString(string[] s)
    {
        List<int> ints = new List<int>();
        int n;
        for (int i = 0; i < s.Length; i++)
            if (s[i].Length > 0)
                try
                {
                    n = int.Parse(s[i]);
                    if (n >= 0) ints.Add(n);
                    else
                        return null;
                }
                catch (Exception)
                {
                    return null;
                }
        return ints.Count > 0 ? ints.ToArray() : null;
    }
    private float DeterminantOfMatrix(float[,] mat)
    {
        float ans;
        ans = mat[0, 0] * (mat[1, 1] * mat[2, 2] - mat[2, 1] * mat[1, 2])
              - mat[0, 1] * (mat[1, 0] * mat[2, 2] - mat[1, 2] * mat[2, 0])
              + mat[0, 2] * (mat[1, 0] * mat[2, 1] - mat[1, 1] * mat[2, 0]);
        return ans;
    }

    // This function finds the solution of system of
    // linear equations using cramer's rule
    float[] FindSolution3(float[,] coeff)
    {
        // Matrix d using coeff as given in cramer's rule
        float[,] d = {
            { coeff[0,0], coeff[0,1], coeff[0,2] },
        { coeff[1,0], coeff[1,1], coeff[1,2] },
        { coeff[2,0], coeff[2,1], coeff[2,2] },
    };
        // Matrix d1 using coeff as given in cramer's rule
        float[,] d1 = {
            { coeff[0,3], coeff[0,1], coeff[0,2] },
        { coeff[1,3], coeff[1,1], coeff[1,2] },
        { coeff[2,3], coeff[2,1], coeff[2,2] },
    };
        // Matrix d2 using coeff as given in cramer's rule
        float[,] d2 = {
            { coeff[0,0], coeff[0,3], coeff[0,2] },
        { coeff[1,0], coeff[1,3], coeff[1,2] },
        { coeff[2,0], coeff[2,3], coeff[2,2] },
    };
        // Matrix d3 using coeff as given in cramer's rule
        float[,] d3 = {
            { coeff[0,0], coeff[0,1], coeff[0,3] },
        { coeff[1,0], coeff[1,1], coeff[1,3] },
        { coeff[2,0], coeff[2,1], coeff[2,3] },
    };

        // Calculating Determinant of Matrices d, d1, d2, d3
        float D = DeterminantOfMatrix(d);
        float D1 = DeterminantOfMatrix(d1);
        float D2 = DeterminantOfMatrix(d2);
        float D3 = DeterminantOfMatrix(d3);

        // Case 1
        if (D != 0)
        {
            // Coeff have a unique solution. Apply Cramer's Rule
            float x = D1 / D;
            float y = D2 / D;
            float z = D3 / D; // calculating z using cramer's rule
            return new float[] { x, y, z };
        }
        // Case 2
        else
        {
            return null;
        }
    }
    private float[] FindSolution2(float[,] coeff)
    {
        float a0 = coeff[0, 0];
        float a1 = coeff[1, 0];
        float b0 = coeff[0, 1];
        float b1 = coeff[1, 1];
        float c0 = coeff[0, 2];
        float c1 = coeff[1, 2];
        float k = a1 * b0 - a0 * b1;
        if (k == 0)
            return null;
        else
        {
            float y = (c1 * a0 - c0 * a1) / k;
            if (y == float.NaN)
                return null;
            float x = (c0 * b1 - c1 * b0) / k;
            if (y == float.NaN)
                return null;
            return new float[] { x, y };
        }

    }

    public LerpManager Clone()
    {
       return new LerpManager(p * 2, q * 2);
    }
}

