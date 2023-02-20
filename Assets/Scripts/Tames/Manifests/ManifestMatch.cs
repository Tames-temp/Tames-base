using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tames
{
    public class ManifestMatch
    {
        public List<string> a = new List<string>();
        public List<string> b = new List<string>();
        public static int Read(string[] lines, int index, List<TameMatch> tms)
        {
            string s = lines[index];
            List<string> ss = new List<string>();
            bool txt = false, first = true, afinished = false;
            int bstarts = -1;
            string tmp = "";
            string clean;
            List<string> a = new List<string>();
            List<string> b = new List<string>();
            ManifestMatch tmm = new ManifestMatch();
            for (int i = 0; i < s.Length; i++)
            {
                if (" \t".IndexOf(s[i]) >= 0)
                { if (!first) tmp += s[i]; }
                else if (s[i] == ',')
                {
                    if (txt)
                    {
                        txt = false;
                        ss.Add(tmp);
                    }
                }
                else if (s[i] == ';')
                {
                    if (afinished) break;
                    else
                    {
                        afinished = true;
                        bstarts = ss.Count;
                        if (txt)
                            ss.Add(tmp);
                    }
                }
                else tmp += s[i];
            }
            if (bstarts > 0)
            {
                for (int i = 0; i < bstarts; i++)
                {
                    clean = Utils.Clean(ss[i]);
                    if (clean.Length > 0)
                        a.Add(clean);
                }
                if (a.Count > 0)
                {
                    for (int i = bstarts; i < ss.Count; i++)
                    {
                        clean = Utils.Clean(ss[i]);
                        if (clean.Length > 0)
                            b.Add(clean);
                    }
                }
                if (b.Count == 0)
                    a.Clear();
            }
            if (b.Count > 0)
            {
                tmm.a = a;
                tmm.b = b;
                TameMatch tm = new TameMatch() { manifest = tmm };
                tms.Add(tm);
            }
            return index;
        }
    }
}
