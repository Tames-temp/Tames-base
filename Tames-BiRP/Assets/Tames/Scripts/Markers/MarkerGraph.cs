using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Markers
{
    public enum GraphType { Bar, Pie, Line }
    public class MarkerGraph : MonoBehaviour
    {
        public bool transpose = false;
        [TextAreaAttribute(5, 10)]
        private string data;
        public GraphType type;
        public GameObject[] visual;
        public GameObject[] labels;
        public Color[] colors;
        public string Data { get { return data; } }
        public InputSetting control;

        public float[,] GetValues(out int col, out int row)
        {
            int n = 0;
            col = 0;
            char[] d = new char[] { ' ', '\t', ',' };
            string[] lines = data.Split("\n");
            row = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                n = 0; for (int j = 0; j < lines[i].Length; j++)
                    if ("\t ,".IndexOf(lines[i][j]) >= 0)
                        n++;
            }
            if (n > col) col = n + 1;
            float[,] v;
            float f;
            string[] words;

            if (transpose)
            {
                v = new float[row, col];
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < col; j++)
                        v[i, j] = float.NaN;
                for (int i = 0; i < row; i++)
                {
                    words = lines[i].Split(d, StringSplitOptions.None);
                    for (int j = 0; j < words.Length; j++)
                        if (Utils.SafeParse(words[j], out f))
                            v[i, j] = f;
                        else v[i, j] = float.NaN;
                }
                n = col;
                col = row;
                row = n;
            }
            else
            {
                v = new float[col, row];
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < col; j++)
                        v[j, i] = float.NaN;
                for (int i = 0; i < row; i++)
                {
                    words = lines[i].Split(d, StringSplitOptions.None);
                    for (int j = 0; j < words.Length; j++)
                        if (Utils.SafeParse(words[j], out f))
                            v[j, i] = f;
                        else v[j, i] = float.NaN;
                }
            }
            bool found, valid = false;
            int x0 = -1, y0 = -1, x1 = -1, y1 = -1;
            for (int i = 0; i < col; i++)
            {
                found = false;
                for (int j = 0; j < row; j++)
                    if (v[i, j] != float.NaN) { n = j; found = true; break; }
                if (found)
                {
                    if (!valid) { y0 = n; valid = true; }
                    else y1 = n;
                }
            }
            valid = false;
            for (int j = 0; j < row; j++)
            {
                found = false;
                for (int i = 0; i < col; i++)
                    if (v[i, j] != float.NaN) { n = j; found = true; break; }
                if (found)
                {
                    if (!valid) { x0 = n; valid = true; }
                    else x1 = n;
                }
            }
            if (x0 >= 0)
            {
                col = x1 - x0 + 1;
                row = y1 - y0 + 1;
                float[,] r = new float[col, row];
                for (int i = x0; i <= x1; i++)
                    for (int j = y0; j <= y1; j++)
                        r[i, j] = v[i, j];
                return r;
            }
            return null;
        }
    }
}
