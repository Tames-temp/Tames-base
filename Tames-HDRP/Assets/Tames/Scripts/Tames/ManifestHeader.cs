using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tames
{
    /// <summary>
    /// this class compiles a line of the manifest file for further use
    /// </summary>
    public class ManifestHeader
    {
        /// <summary>
        /// the header string. See <see cref="key"/> and <see cref="subKey"/> for valid header strings
        /// </summary>
        public string header = "";
        /// <summary>
        /// the major key of the line that declares start of a block for a <see cref="TameElement"/> or <see cref="TameArea"/>. The default is None, meaning that the line is not the start of a block. If the first line of the word is one of the following, the line is deemed as the start of a block corresponding to <see cref="TameKeys"/> of the same name: object, material, light, interact. 
        /// After the key, for the <see cref="TameElement"/> types their names (see <see cref="TameFinder.Relations"/> for patterns) will be written separated by commas. For <see cref="TameArea"/> the <see cref="TameArea.mode"/> is defined (inside, outside, inout, outin, grip, see <see cref="InteractionMode"/>).
        /// </summary>
        public TameKeys key = TameKeys.None;
        /// <summary>
        /// the subkey of the line that declares a feature or property of the major type. Accepted subkeys include:
        /// For all classes based on <see cref="TameElement"/>:
        /// update (<see cref="ManifestKeys.Update"/>): defines the slide parents of a <see cref="TameElement"/>. See <see cref="TameElement.parents"/> and for interactors see <see    cref="TameArea.attachedObjects"/>.
        /// trigger (<see cref="ManifestKeys.Trigger"/>): defines trigger for the element progress. If this is not defined or is invalid, the element is updated along with its parent. Otherwise it is updated according to the trigger values (see <see cref="TameProgress.trigger"/>). The trigger is defined by two numbers separated by comma.
        /// 
        /// For <see cref="TameObject"/>:
        /// slide (<see cref="ManifestKeys.Slide"/>): defines the slide parents of a <see cref="TameObject"/>. See <see cref="TameElement.slideParents"/>
        /// rotate (<see cref="ManifestKeys.Slide"/>): defines the rotate parents of a <see cref="TameObject"/>. See <see cref="TameElement.rotateParents"/>
        /// int (<see cref="ManifestKeys.Int"/>): adds interactors to a <see cref="TameObject"/>. See <see cref="TameElement.AddUpdate(ManifestKeys, List{TameGameObject})"/>
        /// 
        /// For <see cref="TameMaterial"/> and <see cref="TameLight"/>:
        /// intensity (<see cref="ManifestKeys.Bright"/>): defines a [float] <see cref="TameChanger"/> the intensity of light or the brightness of the material emission color. 
        /// color (<see cref="ManifestKeys.Color"/>): defines a <see cref="TameColor"/> for the base color of the material or light. For lights, it is the same as "spectrum".  
        /// spectrum (<see cref="ManifestKeys.Glow"/>): defines a <see cref="TameColor"/> for the emission color of the material or light. For lights, it is the same as "color"
        /// 
        /// For <see cref="TameMaterial"/>
        /// mapx (<see cref="ManifestKeys.MapX"/>): defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the main texture of a material.
        /// mapy (<see cref="ManifestKeys.MapY"/>): defines a [float] <see cref="TameChanger"/> for the y element of the texture offset for the main texture of a material.
        /// lightx (<see cref="ManifestKeys.LightX"/>):defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the emission map of a material.
        /// lighty (<see cref="ManifestKeys.LightY"/>): defines a [float] <see cref="TameChanger"/> for the x element of the texture offset for the emission map of a material.
        /// 
        /// For <see cref="TameArea"/>:
        /// size (<see cref="ManifestKeys.Size"/>):defines the object or size and shape of the interactor. It can have either the name of a game object or of up to three floats separated by commas. If it is the game object, the <see cref="TameArea.geometry"/> is considered <see cref="InteractionGeometry.Box"/> with the gameobject's transform. Otherwise, the number of floats defines the geometry (1: as the radius of a sphere, 2: as the radius and height of a cylinder, 3: as x, y, z scale of a box).
        /// attach (<see cref="ManifestKeys.Attach"/>): the name of game object that the interactor attaches to, subject to <see cref="TameFinder.Relations"/> patterns (without rotation ~ or position @ operators). The names are separated by comma and stored in <see cref="TameArea.attachedObjects"/>
        /// follow (<see cref="ManifestKeys.Follow"/>): the value of <see cref="TameArea.update"/> (fixed, parent, object or mover). 
        /// </summary>
        public int subKey = ManifestKeys.None;
        /// <summary>
        /// the list of delimited strings after a key or subkey (the key is not included). If the list includes names of objects, interactors or elements, they are delimited by comma, otherwise by space. 
        /// </summary>
        public List<string> items = new List<string>();
        /// <summary>
        /// creates a manifest header based on a line. It assigns the <see cref="key"/>, <see cref="subKey"/> and <see cref="items"/>. 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static ManifestHeader Read(string line)
        {
            // 7/25 11:43
            ManifestHeader r = new ManifestHeader();
            string cleanLine = Utils.Clean(line);
            //    Debug.Log("cleanline = " + cleanLine + ": " + line);
            cleanLine = Utils.RemoveDuplicate(cleanLine, " \t");
            cleanLine = Utils.RemoveDuplicate(cleanLine, ",");
            string header = cleanLine;
            r.header = header;
            string content = "";
            List<string> contentList;
            if (cleanLine.Length > 0)
            {
                for (int i = 0; i < cleanLine.Length; i++)
                    if (" \t".IndexOf(cleanLine[i]) >= 0)
                    {
                        header = cleanLine.Substring(0, i);
                        r.header = header;
                        content = cleanLine.Substring(i + 1);
                        break;
                    }
                //  Debug.Log(":::"+cleanLine + "+" + r.header + "+" + content);
                string delim = " ";
                if (header.Length > 0)
                    r.key = GetType(r.header);
                if (r.key == TameKeys.None)
                {
                    r.subKey = GetSubType(r.header);
                    switch (r.subKey)
                    {
                        case ManifestKeys.Update:
                        case ManifestKeys.Track:
                        case ManifestKeys.Follow:
                        case ManifestKeys.Area:
                        case ManifestKeys.Enforce:
                        case ManifestKeys.Affect:
                            //  case ManifestKeys.Rotate:
                            delim = ",";
                            break;
                    }
                }
                else
                    delim = ",";

                //     Debug.Log("subkey = " + r.subKey + " " + cleanLine);
                if ((r.subKey != ManifestKeys.None) || (r.key != TameKeys.None))
                {
                    contentList = Utils.Split(content, delim);
                    for (int i = 0; i < contentList.Count; i++)
                    {
                        cleanLine = Utils.Clean(contentList[i]);
                        r.items.Add(cleanLine);
                    }
                    //             Debug.Log("content: " + contentList.Count + " " + content);
                }
            }
            return r;
        }

        private static TameKeys GetType(string s)
        {
            int k = ManifestKeys.GetKey(s);
            //   Debug.Log("key = " + k + " " + s);
            if (k != 0)
                // 7/25 11:50
                return k switch
                {
                    ManifestKeys.Object => TameKeys.Object,
                    ManifestKeys.Material => TameKeys.Material,
                    ManifestKeys.Light => TameKeys.Light,
                    ManifestKeys.Custom => TameKeys.Custom,
                    ManifestKeys.Import => TameKeys.Import,
                    ManifestKeys.Walk => TameKeys.Walk,
                    ManifestKeys.Camera => TameKeys.Camera,
                    ManifestKeys.Eye => TameKeys.Eye,
                    ManifestKeys.Mode => TameKeys.Mode,
                    ManifestKeys.Alter => TameKeys.Alter,
                    ManifestKeys.Match => TameKeys.Match,
                    _ => TameKeys.None
                };
            else return TameKeys.None;
        }
        private static int GetSubType(string s)
        {
            return ManifestKeys.GetKey(s);
            // 7/25 11:50

        }
        /// <summary>
        /// reads two numbers separated by comma
        /// </summary>
        /// <param name="s"></param>
        /// <returns>an array containg the parsed floats, or null if invalid</returns>
     
    }
}
