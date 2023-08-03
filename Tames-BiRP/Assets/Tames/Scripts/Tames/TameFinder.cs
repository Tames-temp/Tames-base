using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tames
{
    /// <summary>
    /// this class helps other classes to identify objects based on their names and relation (see <see cref="Relations"/>
    /// </summary>
    public class TameFinder
    {
        /// <summary>
        /// indicates which types of objects or parents are included in the found names
        /// </summary>
        public bool[] includes = new bool[] { false, false, false, false, false, false };
        public const int Grip = 0;
        public const int Hand = 1;
        public const int Head = 2;
        public const int Time = 3;
        public const int Object = 4;
        public const int Tame = 5;


        /// <summary>
        /// the list of objects whose positions will be tracked (only if <see cref="trackMode"/> is true
        /// </summary>
        public List<TameGameObject> objectList = new List<TameGameObject>();
        /// <summary>
        /// the list of elements whose progresses will be tracked (only if <see cref="trackMode"/> is false
        /// </summary>
        public List<TameElement> elementList = new List<TameElement>();
        /// <summary>
        /// the element for which the names should be found
        /// </summary>
        public TameElement owner;
        /// <summary>
        ///  not used
        /// </summary>
        public bool declaration;
        /// <summary>
        /// indicating if the names belong to trackable objects not <see cref="TameElement"/>s. This will be the case if the first name in the <see cref="header"/>'s items begins with '@'
        /// </summary>
        public byte trackMode = TrackBasis.Tame;
        /// <summary>
        /// the manifest header of the line in the manifest file that prompts for the search objects.
        /// </summary>
        public ManifestHeader header = new ManifestHeader();
        /// <summary>
        /// finds the relation between two parts of a string, where each part corresponds with a potentail game object's name (the first part can be empty which indicates the first object is the <see cref="owner"/>'s associated game object). The string may include certain operators to define these relations. In general, =, &lt and &gt repectively indicate same level of ancestry (i.e. siblings), lower level (i.e. child) and higher level (i.e. parent) of ancestry. Combining = with either &lt or &gt considers siblings of parents or children of siblings, respectively. Furthermore, using double &lt&lt or &gt&gt would allow multiple generation difference (allowing for <see cref="RelationTypes.Grand"/>). For example, &lt&lt= means a relationship that cane be parents, grand parents, uncles or grand uncles. In addition to the relationship between the two sides of the operator, it is possible to determine if the names are exact or only the start of a name with an astrisk (*) after the name (for example, "light*" will search for all objects whose names start with "light"). Another feature of the naming is the possibility to differentiate between types of parent updates (see <see cref="inputRotation"/>), though this is only applicable to updating manifests. An extensive list of the operators are listed below:
        /// X=Y: search for objects named X that are <see cref="RelationTypes.Sibling"/> of an object named Y  
        /// X&ltY: search for objects named X that are <see cref="RelationTypes.Child"/> of an object named Y  
        /// X&gtY: search for objects named X that are <see cref="RelationTypes.Parent"/> of an object named Y  
        /// X&lt=Y: search for objects named X that are <see cref="RelationTypes.Child"/> or <see cref="RelationTypes.Nephew"/> of an object named Y  
        /// X&gt=Y: search for objects named X that are <see cref="RelationTypes.Parent"/> or <see cref="RelationTypes.Uncle"/> of an object named Y  
        /// X&lt&ltY: search for objects named X which have their <see cref="RelationTypes.Parent"/> or ancestor named Y 
        /// X&gt&gtY: search for objects named X which have a <see cref="RelationTypes.Child"/> or descndant with name Y  
        /// X&lt&lt=Y: search for objects named X which have their <see cref="RelationTypes.Parent"/>, an ancestor, an <see cref="RelationTypes.Uncle"/> or grand uncle named Y  
        /// X&gt&gt=Y: search for objects named X which have a <see cref="RelationTypes.Child"/>, <see cref="RelationTypes.Nephew"/>, descndant or grand nephew with name Y
        /// *: In any of the above adding an astrisk after X or/and Y expands to search for when X or/and Y are the starting portion of the names
        /// ~X..Y: Regardless of what relation X has with Y, the ~ indicates that once an X is found, the rotation of it will affect the <see cref="owner"/>'s change not its sliding. In it's absence, the sliding or update would affect the latter (See <see cref="TameEffect.effect"/> and <see cref="TameEffect.input"/>).
        /// </summary>
        /// <param name="s"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private RelationTypes[] Relations(string s, string[] s2)
        {
            RelationTypes[] r = GetRelations(s, s2, ">>=", RelationTypes.Uncle, RelationTypes.Grand);
            if (r != null) return r; else r = GetRelations(s, s2, "<<=", RelationTypes.Nephew, RelationTypes.Grand);
            if (r != null) return r; else r = GetRelations(s, s2, "<<", RelationTypes.Child, RelationTypes.Grand);
            if (r != null) return r; else r = GetRelations(s, s2, ">>", RelationTypes.Parent, RelationTypes.Grand);
            if (r != null) return r; else r = GetRelations(s, s2, ">=", RelationTypes.Uncle, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, "<=", RelationTypes.Nephew, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, ">=", RelationTypes.Uncle, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, "<=", RelationTypes.Nephew, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, "<", RelationTypes.Child, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, ">", RelationTypes.Parent, RelationTypes.None);
            if (r != null) return r; else r = GetRelations(s, s2, "=", RelationTypes.Sibling, RelationTypes.None);
            if (r != null) return r; else { s2[0] = s; return null; }
        }
        /// <summary>
        /// checks if the <see cref="Relations"/> operator exists in a string. If it exists, the either sides of the operator are stored in a string array and the specified type of relation and the "grand" (multi-generation state) is returned.  
        /// </summary>
        /// <param name="s">the string containing the relationship and the names</param>
        /// <param name="s2">the stored sides of the operator</param>
        /// <param name="operatr">the operator</param>
        /// <param name="type">type of relationship (see <see cref="Relations"/>) </param>
        /// <param name="grand"><see cref="RelationTypes.Grand"/> or <see cref="RelationTypes.None"/></param>
        /// <returns>returns a <see cref="RelationTypes"/> array with two elements which are the same as type and grand arguments". It returns null if s doesn't contain the operator</returns>
        private RelationTypes[] GetRelations(string s, string[] s2, string operatr, RelationTypes type, RelationTypes grand)
        {
            int i = s.IndexOf(operatr);
            if (i == 0)
            {
                s2[0] = "";
                s2[1] = s.Substring(operatr.Length);
                return new RelationTypes[] { type, grand };
            }
            else if (i > 0)
            {
                s2[0] = s.Substring(0, i);
                s2[1] = s.Substring(i + operatr.Length);
                return new RelationTypes[] { type, grand };
            }
            return null;
        }
        public void RemoveDuplicate(int type)
        {
            if (type == Object)
            {
                for (int i = 0; i < objectList.Count - 1; i++)
                    for (int j = objectList.Count - 1; j > i; j--)
                        if (objectList[j] == objectList[i])
                            objectList.RemoveAt(j);
            }
            else
                for (int i = 0; i < elementList.Count - 1; i++)
                    for (int j = elementList.Count - 1; j > i; j--)
                        if (elementList[j] == elementList[i])
                            elementList.RemoveAt(j);
        }
        /// <summary>
        /// populates the <see cref="elementList"/> from the provided list of <see cref="TameElement"/> by the relationships defined in the <see cref="header"/>. This method is called from <see cref="TameManager"/> class to identify elements when defined in the manifest or are affected by an interactor (see <see cref="TameArea"/>)
        /// </summary>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        /// <param name="tgos">list of game objects descending from the root interactive object</param>
        public void PopulateElements(List<TameElement> tes, List<TameGameObject> tgos)
        {
            string[] s2 = new string[2];
            RelationTypes[] type;
            string sn;
            for (int i = 0; i < includes.Length; i++) includes[i] = false;
            foreach (string s in header.items)
            {
                type = Relations(s, s2);
                //    Debug.Log("item = " + s + " " + (type == null ? s2[0] : s2[1]));
                if (type == null)
                    Find(header.key, s2[0], tes);
                else
                    switch (type[0])
                    {
                        case RelationTypes.Child: FindChild(header.key, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                        case RelationTypes.Parent: FindParent(header.key, s2[0], s2[1], tes, type[1] == RelationTypes.Grand); break;
                        case RelationTypes.Sibling: FindSibling(header.key, s2[0], s2[1], tes); break;
                        //       case Uncle: progresssTrack.AddRange(FindUncle(s2[1], s2[0], owner, tgos, type[1] == 1)); break;
                        case RelationTypes.Nephew: FindNephews(header.key, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                    }
            }
        }
        public void PopulateMovers(List<TameElement> tes, List<TameGameObject> tgos)
        {
            string[] s2 = new string[2];
            RelationTypes[] type;
            string sn;
            TameGameObject tgo;
            for (int i = 0; i < includes.Length; i++) includes[i] = false;
            foreach (string s in header.items)
            {
                type = Relations(s, s2);
                //    Debug.Log("item = " + s + " " + (type == null ? s2[0] : s2[1]));
                if (type == null)
                    Find(header.key, s2[0], tes);
                else
                    switch (type[0])
                    {
                        case RelationTypes.Child: FindChild(header.key, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                        case RelationTypes.Parent: FindParent(header.key, s2[0], s2[1], tes, type[1] == RelationTypes.Grand); break;
                        case RelationTypes.Sibling: FindSibling(header.key, s2[0], s2[1], tes); break;
                        //       case Uncle: progresssTrack.AddRange(FindUncle(s2[1], s2[0], owner, tgos, type[1] == 1)); break;
                        case RelationTypes.Nephew: FindNephews(header.key, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                    }
            }
            foreach (TameElement te in elementList)
                if (te.tameType == TameKeys.Object)
                    if ((tgo = FindTGO(((TameObject)te).mover, tgos)) != null)
                        objectList.Add(tgo);
        }
        /// <summary>
        /// populates the <see cref="objectList"/> from the provided list of <see cref="TameGameObject"/> by the relationships defined in the <see cref="header"/>. This method is called from <see cref="TameManager"/> class to identify game objects attached to interactors.
        /// </summary>
        /// <param name="tgos">list of game objects descending from the root interactive object</param>
        public void PopulateObjects(List<TameGameObject> tgos)
        {
            string[] s2 = new string[2];
            RelationTypes[] type;
            string sn;
            for (int i = 0; i < includes.Length; i++) includes[i] = false;
            foreach (string s in header.items)
            {
                type = Relations(s, s2);
                if (type == null)
                    objectList.AddRange(Find(s2[0], tgos));
                else
                    switch (type[0])
                    {
                        case RelationTypes.Child: objectList.AddRange(FindChild(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                        case RelationTypes.Parent: objectList.AddRange(FindParent(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                        case RelationTypes.Sibling: objectList.AddRange(FindSibling(s2[0], s2[1], owner, tgos)); break;
                        //    case Uncle: positionTrack.AddRange(FindChild(s2[1], s2[0], owner, tgos, type[1] == 1)); break;
                        case RelationTypes.Nephew: objectList.AddRange(FindNephews(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                    }

            }
        }
        /// <summary>
        /// populates either the <see cref="elementList"/> from the provided list of <see cref="TameElement"/> or the <see cref="objectList"/> from the provided list of <see cref="TameGameObject"/>, by the relationships defined in the <see cref="header"/>. This method is called from <see cref="TameManager"/> class to identify elements or objects that affect the update of elements. For each header or update manifest, only one list is accepted, that is determined if the items in the header begin with '@'. If so, they are considered names of gameobjects, otherwise they are treated as elements. 
        /// </summary>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        /// <param name="tgos">list of game objects descending from the root interactive object</param>
        public void Populate(List<TameElement> tes, List<TameGameObject> tgos)
        {
            string[] s2 = new string[2];
            RelationTypes[] type;
            for (int i = 0; i < includes.Length; i++) includes[i] = false;
            List<TameElement> el = new List<TameElement>();
            TameGameObject tgo;
            foreach (string s in header.items)
                switch (trackMode)
                {
                    case TrackBasis.Object:
                        switch (s.ToLower())
                        {
                            case "head": includes[Head] = true; break;
                            case "hand": includes[Hand] = true; break;
                            // case "grip": includes[Grip] = true; break;
                            // case "time": includes[Time] = true; break;
                            default:
                                type = Relations(s, s2);
                                if (type == null)
                                    objectList.AddRange(Find(s2[0], tgos));
                                else
                                    switch (type[0])
                                    {
                                        case RelationTypes.Child: objectList.AddRange(FindChild(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                                        case RelationTypes.Parent: objectList.AddRange(FindParent(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                                        case RelationTypes.Sibling: objectList.AddRange(FindSibling(s2[0], s2[1], owner, tgos)); break;
                                        //    case Uncle: positionTrack.AddRange(FindChild(s2[1], s2[0], owner, tgos, type[1] == 1)); break;
                                        case RelationTypes.Nephew: objectList.AddRange(FindNephews(s2[0], s2[1], owner, tgos, type[1] == RelationTypes.Grand)); break;
                                    }
                                break;
                        }
                        break;
                    case TrackBasis.Tame:
                    case TrackBasis.Mover:
                        type = Relations(s, s2);
                        if (type == null)
                            Find(TameKeys.None, s2[0], tes);
                        else
                            switch (type[0])
                            {
                                case RelationTypes.Child: FindChild(TameKeys.None, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                                case RelationTypes.Parent: FindParent(TameKeys.None, s2[0], s2[1], tes, type[1] == RelationTypes.Grand); break;
                                case RelationTypes.Sibling: FindSibling(TameKeys.None, s2[0], s2[1], tes); break;
                                //       case Uncle: progresssTrack.AddRange(FindUncle(s2[1], s2[0], owner, tgos, type[1] == 1)); break;
                                case RelationTypes.Nephew: FindNephews(TameKeys.None, s2[0], s2[1], tes, tgos, type[1] == RelationTypes.Grand); break;
                            }
                        if (trackMode == TrackBasis.Mover)
                            foreach (TameElement te in elementList)
                                if (te.tameType == TameKeys.Object)
                                    if ((tgo = FindTGO(((TameObject)te).mover, tgos)) != null)
                                        objectList.Add(tgo);
                        break;
                }
        }
        public static TameGameObject FindTGO(GameObject go, List<TameGameObject> tgos)
        {
            foreach (TameGameObject tg in tgos)
                if (tg.gameObject == go)
                    return tg;
            return null;
        }
        /// <summary>
        /// populate <see cref="elementList"/> by elements of a specified type with a certain name (without relateion but with possible * or ~ operators)
        /// </summary>
        /// <param name="type">the type of elements</param>
        /// <param name="name">the searched name</param>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        public void Find(TameKeys type, string name, List<TameElement> tes)
        {
            bool startsWith = name[name.Length - 1] == '*';
            string nameL = startsWith ? name.Substring(0, name.Length - 1) : name;
            //      nameL = rot ? nameL.Substring(1, nameL.Length - 1) : nameL;
            nameL = nameL.ToLower();
            //      Debug.Log("lict " + nameL + "|" + startsWith);
            foreach (TameElement te in tes)
            {
                if (startsWith)
                {
                    if (te.name.ToLower().StartsWith(nameL)) { elementList.Add(te); }
                }
                else if (te.name.ToLower().Equals(nameL)) { elementList.Add(te); }
                //      Debug.Log("\"" + te.name + "\" =? \"" + nameL + "\" " + te.name.ToLower().Equals(nameL) + " " + elementList.Count);
            }

        }
        /// <summary>
        /// populate <see cref="elementList"/> by elements whose <see cref="TameElement.owner"/> is named parent and has the child
        /// </summary>
        /// <param name="type">the type of elements</param>
        /// <param name="child">the searched name of the parent</param>
        /// <param name="parent">the serached name of the elements</param>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public void FindChild(TameKeys type, string child, string parent, List<TameElement> tes, List<TameGameObject> tgos, bool grand)
        {
            bool childStartsWith = child[child.Length - 1] == '*';
            string childName = childStartsWith ? child.Substring(0, child.Length - 1) : child;
            childName = childName.ToLower();
            bool parentStartsWith = parent[parent.Length - 1] == '*';
            string parentName = parentStartsWith ? parent.Substring(0, parent.Length - 1) : parent;
            //    parentName = rot ? parentName.Substring(1, parentName.Length - 1) : parentName;
            //  string ch = cstart ? child.Substring(0, child.Length - 1) : child;
            parentName = parentName.ToLower();
            List<TameGameObject> children = new List<TameGameObject>();
            // finds all game object matching the child name
            foreach (TameGameObject tgo in tgos)
            {
                if (childStartsWith)
                {
                    if (tgo.gameObject.name.ToLower().StartsWith(childName))
                        children.Add(tgo);
                }
                else
                if (tgo.gameObject.name.ToLower().Equals(childName))
                    children.Add(tgo);
            }
            // if there in parent (so the parent is this element
            if (parentName.Length == 0)
            {
                foreach (TameGameObject achild in children)
                    if (achild.tameParent.IsChildOf(owner.owner, grand))
                    {
                        elementList.Add(achild.tameParent);
                        //     inputRotation.Add(rot);
                        break;
                    }
            }
            // if parent is specified
            else
                foreach (TameElement te in tes)
                    if ((type == te.tameType) || (type == TameKeys.None))
                    {
                        if ((parentStartsWith && te.name.ToLower().StartsWith(parentName)) || ((!parentStartsWith) && te.name.ToLower().Equals(parentName)))
                            foreach (TameGameObject tgo in children)
                                if (te.IsChildOf(tgo.gameObject, grand))
                                {
                                    elementList.Add(te);
                                    break;
                                }
                    }
        }

        /// <summary>
        /// populate <see cref="elementList"/> by elements whose <see cref="TameElement.owner"/> is named parent and has the child
        /// </summary>
        /// <param name="type">the type of elements</param>
        /// <param name="parent">the searched name of the parent</param>
        /// <param name="child">the serached name of the elements</param>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public void FindParent(TameKeys type, string parent, string child, List<TameElement> tes, bool grand)
        {
            bool parentStartsWith = parent[parent.Length - 1] == '*';
            string parentName = parentStartsWith ? parent.Substring(0, parent.Length - 1) : parent;
            parentName = parentName.ToLower();
            bool childStartsWith = child[child.Length - 1] == '*';
            //  bool rot = child[0] == '~';
            string childName = childStartsWith ? child.Substring(0, child.Length - 1) : child;
            // childName = rot ? childName.Substring(1, childName.Length - 1) : childName;
            childName = childName.ToLower();
            List<TameElement> children = new List<TameElement>();
            if (parentName.Length == 0)
                children.Add(owner);
            else
                foreach (TameElement te in tes)
                {
                    if ((type == te.tameType) || (type == TameKeys.None))
                    {
                        if (childStartsWith)
                        {
                            if (te.name.ToLower().StartsWith(childName))
                                children.Add(te);
                        }
                        else
                        if (te.name.ToLower().Equals(childName))
                            children.Add(te);
                    }
                }
            foreach (TameElement te in children)
                if (te.IsChildOf(parentName, parentStartsWith, grand))
                {
                    elementList.Add(te);
                    //           inputRotation.Add(rot);
                }
        }
        /// <summary>
        /// populate <see cref="elementList"/> by elements whose <see cref="TameElement.owner"/> is named bigbro and has the sibling
        /// </summary>
        /// <param name="type">the type of elements</param>
        /// <param name="sibling">the searched name of the parent</param>
        /// <param name="bigbro">the serached name of the elements</param>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        public void FindSibling(TameKeys type, string sibling, string bigbro, List<TameElement> tes)
        {
            bool sibStartsWith = sibling[sibling.Length - 1] == '*';
            string sibName = sibStartsWith ? sibling.Substring(0, sibling.Length - 1) : sibling;
            sibName = sibName.ToLower();
            bool broStartsWith = bigbro[bigbro.Length - 1] == '*';
            string broName = broStartsWith ? bigbro.Substring(0, bigbro.Length - 1) : bigbro;
            //      bool rot = bigbro[0] == '~';
            //    broName = rot ? broName.Substring(1, broName.Length - 1) : broName;
            broName = broName.ToLower();
            List<TameElement> cs = new List<TameElement>();
            if (broName.Length == 0)
                cs.Add(owner);
            else
                foreach (TameElement te in tes)
                {
                    if ((type == te.tameType) || (type == TameKeys.None))
                    {
                        if (broStartsWith)
                        {
                            if (te.name.ToLower().StartsWith(broName))
                                cs.Add(te);
                        }
                        else
                        if (te.name.ToLower().Equals(broName))
                            cs.Add(te);
                    }
                }
            foreach (TameElement te in cs)
                if (te.IsSiblingOf(sibName, sibStartsWith))
                {
                    elementList.Add(te);
                    //         inputRotation.Add(rot);
                }
        }
        /// <summary>
        /// adds siblings of a <see cref="GameObject"/> to a list of <see cref="GameObject"/>s. 
        /// </summary>
        /// <param name="go">the gameobject</param>
        /// <param name="gos">list of gameobjects</param>
        private static void GetSiblings(GameObject go, List<GameObject> gos)
        {
            Transform t = go.transform.parent;
            int cc = t.childCount;
            bool f;
            for (int i = 0; i < cc; i++)
                if (t.GetChild(i) != go.transform)
                {
                    f = false;
                    for (int j = 0; j < gos.Count; j++)
                        if (gos[j] == t.GetChild(i))
                        {
                            f = true;
                            break;
                        }
                    if (!f) gos.Add(t.GetChild(i).gameObject);
                }
        }
        /// <summary>
        /// populate <see cref="elementList"/> by elements whose <see cref="TameElement.owner"/> is named uncle and has nephews named the nephew
        /// </summary>
        /// <param name="type">the type of elements</param>
        /// <param name="nephew">the searched name of the parent</param>
        /// <param name="uncle">the serached name of the elements</param>
        /// <param name="tes">list of all <see cref="TameElement"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public void FindNephews(TameKeys type, string nephew, string uncle, List<TameElement> tes, List<TameGameObject> tgos, bool grand)
        {
            List<TameElement> r = new List<TameElement>();
            bool nephewStartsWith = nephew[nephew.Length - 1] == '*';
            string nephewName = nephewStartsWith ? nephew.Substring(0, nephew.Length - 1) : nephew;
            nephewName = nephewName.ToLower();
            bool uncleStartsWith = uncle[uncle.Length - 1] == '*';
            //  bool rot = uncle[0] == '~';
            string uncleName = uncleStartsWith ? uncle.Substring(0, uncle.Length - 1) : uncle;
            //   uncleName = rot ? uncleName.Substring(1, uncleName.Length - 1) : uncleName;
            uncleName = uncleName.ToLower();
            List<GameObject> uncles = new List<GameObject>();
            if (uncleName.Length == 0) uncles.Add(owner.owner.gameObject);
            else foreach (TameGameObject tgo in tgos)
                {
                    if (uncleStartsWith)
                    {
                        if (tgo.gameObject.name.ToLower().StartsWith(uncleName))
                            uncles.Add(tgo.gameObject);
                    }
                    else
                    if (tgo.gameObject.name.ToLower().Equals(uncleName))
                        uncles.Add(tgo.gameObject);
                }
            List<GameObject> sib = new List<GameObject>();
            foreach (GameObject tgo in uncles)
                GetSiblings(tgo.gameObject, sib);
            foreach (TameElement te in tes)
                if ((type == te.tameType) || (type == TameKeys.None))
                {
                    if ((nephewStartsWith && te.name.ToLower().StartsWith(nephewName)) || ((!nephewStartsWith) && te.name.ToLower().Equals(nephewName)))
                        foreach (GameObject s in sib)
                            if (te.IsChildOf(s, grand))
                            {
                                r.Add(te);
                                //                  inputRotation.Add(rot);
                                break;
                            }
                }
        }
        /// <summary>
        /// populate <see cref="objectList"/> by objects with name
        /// </summary>
        /// <param name="name">the name of object</param>
        /// <param name="tgos">list of all <see cref="TameGameObject"/>s in the project</param>
        public static List<TameGameObject> Find(string name, List<TameGameObject> tgos)
        {
            List<TameGameObject> r = new List<TameGameObject>();
            bool pstart = name[name.Length - 1] == '*';
            string nameL = pstart ? name.Substring(0, name.Length - 1) : name;
            nameL = nameL.ToLower();
            foreach (TameGameObject tgo in tgos)
            {
                if (pstart)
                {
                    if (tgo.gameObject.name.ToLower().StartsWith(nameL))
                        r.Add(tgo);
                }
                else
                if (tgo.gameObject.name.ToLower().Equals(nameL))
                    r.Add(tgo);
            }
            return r;
        }
        /// <summary>
        /// checks if a <see cref="GameObject"/> is the parent of another <see cref="GameObject"/>
        /// </summary>
        /// <param name="parent">the parent game object</param>
        /// <param name="child">the child game object</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        private static bool IsParent(GameObject child, GameObject parent, bool grand)
        {
            GameObject p = child.transform.parent.gameObject;
            if (!grand)
                return parent == p;
            else
                while (p != null)
                {
                    if (parent == p)
                        return true;
                    else
                        p = p.transform.parent != null ? p.transform.parent.gameObject : null;
                }
            return false;
        }
        /// <summary>
        /// populate <see cref="objectList"/> by objects named parent and has child
        /// </summary>
        /// <param name="child">the searched name of the child</param>
        /// <param name="parent">the serached name of the parent</param>
        /// <param name="tgos">list of all <see cref="TameGameObject"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public static List<TameGameObject> FindChild(string child, string parent, TameElement t, List<TameGameObject> tgos, bool grand)
        {
            //         Debug.Log("finding child " + child + " of " + parent + " " + t.owner);
            List<TameGameObject> r = new List<TameGameObject>();
            bool childStartsWith = child[child.Length - 1] == '*';
            string childName = childStartsWith ? child.Substring(0, child.Length - 1) : child;
            childName = childName.ToLower();
            List<GameObject> potentialParents = new List<GameObject>();
            if (parent.Length == 0)
                potentialParents.Add(t.owner);
            else
            {
                bool parentStartsWith = parent[parent.Length - 1] == '*';
                string parentName = parentStartsWith ? parent.Substring(0, parent.Length - 1) : parent;
                parentName = parentName.ToLower();
                foreach (TameGameObject tgo in tgos)
                    if (parentStartsWith)
                    {
                        if (tgo.gameObject.name.ToLower().StartsWith(parentName))
                            potentialParents.Add(tgo.gameObject);
                    }
                    else
                    if (tgo.gameObject.name.ToLower().Equals(parentName))
                        potentialParents.Add(tgo.gameObject);
            }
            foreach (TameGameObject tgo in tgos)
                if ((childStartsWith && tgo.gameObject.name.ToLower().StartsWith(childName)) || ((!childStartsWith) && tgo.gameObject.name.Equals(childName)))
                    foreach (GameObject go in potentialParents)
                        if (IsParent(tgo.gameObject, go, grand))
                        {
                            r.Add(tgo);
                            break;
                        }
            //      Debug.Log("found " + r.Count);
            return r;
        }
        /// <summary>
        /// checks if a <see cref="GameObject"/> has a parent with a specified name
        /// </summary>
        /// <param name="g">the game object</param>
        /// <param name="name">the searched name of the parent</param>
        /// <param name="starts">if the name is the starting portion of the searched names</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        private static bool HasObjectParent(GameObject g, string name, bool starts, bool grand)
        {
            GameObject p = g.transform.parent.gameObject;
            if (!grand)
                return starts ? p.name.StartsWith(name) : p.name.Equals(name);
            else
                while (p != null)
                {
                    if (starts ? p.name.StartsWith(name) : p.name.Equals(name))
                        return true;
                    else
                        p = p.transform.parent != null ? p.transform.parent.gameObject : null;
                }
            return false;
        }
        /// <summary>
        /// populate <see cref="objectList"/> by objects named child and has parent
        /// </summary>
        /// <param name="parent">the serached name of the parent</param>
        /// <param name="child">the searched name of the child</param>
        /// <param name="tgos">list of all <see cref="TameGameObject"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public static List<TameGameObject> FindParent(string parent, string child, TameElement t, List<TameGameObject> tgos, bool grand)
        {
            List<TameGameObject> r = new List<TameGameObject>();
            bool pstart = parent[parent.Length - 1] == '*';
            string par = pstart ? parent.Substring(0, parent.Length - 1) : parent;
            par = par.ToLower();
            bool cstart = child[child.Length - 1] == '*';
            string ch = cstart ? child.Substring(0, child.Length - 1) : child;
            ch = ch.ToLower();
            List<TameGameObject> cs = new List<TameGameObject>();
            if (ch.Length == 0)
                cs.Add(new TameGameObject() { gameObject = t.mover, tameParent = t });
            else
                foreach (TameGameObject te in tgos)
                {
                    if (cstart)
                    {
                        if (te.gameObject.name.ToLower().StartsWith(ch))
                            cs.Add(te);
                    }
                    else
                    if (te.gameObject.name.ToLower().Equals(ch))
                        cs.Add(te);
                }
            foreach (TameGameObject te in cs)
                if (HasObjectParent(te.gameObject, par, pstart, grand))
                    r.Add(te);
            return r;
        }

        /// <summary>
        /// populate <see cref="objectList"/> by objects named uncle and has nephews
        /// </summary>
        /// <param name="nephew">the serached name of the parent</param>
        /// <param name="uncle">the searched name of the child</param>
        /// <param name="tgos">list of all <see cref="TameGameObject"/>s in the project</param>
        /// <param name="grand">if grandparents should be looked at to</param>
        public static List<TameGameObject> FindNephews(string nephew, string uncle, TameElement t, List<TameGameObject> tgos, bool grand)
        {
            List<TameGameObject> r = new List<TameGameObject>();
            bool pstart = nephew[nephew.Length - 1] == '*';
            string par = pstart ? nephew.Substring(0, nephew.Length - 1) : nephew;
            par = par.ToLower();
            bool cstart = uncle[uncle.Length - 1] == '*';
            string ch = cstart ? uncle.Substring(0, uncle.Length - 1) : uncle;
            ch = ch.ToLower();
            List<TameGameObject> ps = new List<TameGameObject>();
            if (par.Length == 0)
                ps.Add(new TameGameObject() { gameObject = t.mover, tameParent = t });
            else
                foreach (TameGameObject tgo in tgos)
                {
                    if (pstart)
                    {
                        if (tgo.gameObject.name.ToLower().StartsWith(par))
                            ps.Add(tgo);
                    }
                    else
                    if (tgo.gameObject.name.ToLower().Equals(par))
                        ps.Add(tgo);
                }
            List<GameObject> sib = new List<GameObject>();
            foreach (TameGameObject tgo in ps)
                GetSiblings(tgo.gameObject, sib);
            foreach (TameGameObject tgo in tgos)
                if ((cstart && tgo.gameObject.name.ToLower().StartsWith(ch)) || ((!cstart) && tgo.gameObject.name.ToLower().Equals(ch)))
                    foreach (GameObject s in sib)
                        if (IsParent(tgo.gameObject, s, grand))
                        {
                            r.Add(tgo);
                            break;
                        }

            return r;
        }
        /// <summary>
        /// checks if a <see cref="GameObject"/> has a sibling with a specified name
        /// </summary>
        /// <param name="g">the game object</param>
        /// <param name="name">the searched name of the sibling</param>
        /// <param name="starts">if name is the starting portion of the search names</param>
        public static bool HasSibling(GameObject g, string name, bool starts)
        {
            GameObject owner = g.transform.parent.gameObject;
            int cc = owner.transform.childCount;
            for (int i = 0; i < cc; i++)
                if (g != owner.transform.GetChild(i).gameObject)
                    if (starts ? owner.transform.GetChild(i).name.StartsWith(name) : owner.transform.GetChild(i).name.Equals(name))
                        return true;
            return false;
        }
        /// <summary>
        /// populate <see cref="objectList"/> by objects named bigbro and has sibling
        /// </summary>
        /// <param name="sibling">the serached name of the siblings</param>
        /// <param name="bigbro">the searched name of the bigbro</param>
        /// <param name="tgos">list of all <see cref="TameGameObject"/>s in the project</param>
        public static List<TameGameObject> FindSibling(string sibling, string bigbro, TameElement t, List<TameGameObject> tgos)
        {
            Debug.Log("sibling: " + sibling + " > " + bigbro);
            List<TameGameObject> r = new List<TameGameObject>();
            bool siblingStartWith = sibling[sibling.Length - 1] == '*';
            string siblingName = siblingStartWith ? sibling.Substring(0, sibling.Length - 1) : sibling;
            siblingName = siblingName.ToLower();
            List<GameObject> broList = new List<GameObject>();
            if (bigbro.Length == 0)
            {
                broList.Add(t.owner);
            }
            else
            {
                bool broStartWith = bigbro[bigbro.Length - 1] == '*';
                string bro = broStartWith ? bigbro.Substring(0, bigbro.Length - 1) : bigbro;
                bro = bro.ToLower();
                foreach (TameGameObject te in tgos)
                {
                    if (broStartWith)
                    {
                        if (te.gameObject.name.ToLower().StartsWith(bro))
                            broList.Add(te.gameObject);
                    }
                    else
                      if (te.gameObject.name.ToLower().Equals(bro))
                        broList.Add(te.gameObject);
                }
            }
            int c;
            GameObject g;
            foreach (TameGameObject te in tgos)
            {
                if ((siblingStartWith && te.gameObject.name.ToLower().StartsWith(siblingName)) || ((!siblingStartWith) && te.gameObject.name.ToLower().Equals(siblingName)))
                    foreach (GameObject go in broList)
                        if (go.transform.parent == te.transform.parent)
                        {
                            r.Add(te);
                            break;
                        }

            }
            Debug.Log("sibling found: " + r.Count);
            return r;
        }

    }
}
