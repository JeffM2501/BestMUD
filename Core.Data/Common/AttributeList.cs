using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Common
{
    public class AttributeList : IEnumerable
    {
        protected Dictionary<string, int> Attributes = new Dictionary<string, int>();

        public void Add(string name, int value)
        {
            if (Attributes.ContainsKey(name))
                Attributes[name] = value;
            else
                Attributes.Add(name, value);
        }

        public int Get(string name)
        {
            if (Attributes.ContainsKey(name))
                return Attributes[name];
            else
                return 0;
        }

        public void Set(string name, int value)
        {
            Add(name, value);
        }

        public string SerializeToText()
        {
            List<string> d = new List<string>();
            foreach (var i in Attributes)
                d.Add(i.Key + "=" + i.Value.ToString());

            return string.Join(";", d.ToArray());
        }

        public static AttributeList DeserlizeFromString(string text)
        {
            AttributeList l = new AttributeList();

            string[] parts = text.Split(";".ToCharArray());
            foreach (var p in parts)
            {
                string[] comps = p.Split("=".ToCharArray(), 2);
                if (comps.Length != 2)
                    continue;

                int v = 0;
                int.TryParse(comps[1], out v);
                l.Add(comps[0], v);
            }

            return l;
        }

        public IEnumerator GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }

        public int this[string key] { get { return Attributes[key]; } set {  Attributes[key] = value; } }

   
        public int Count { get { return Attributes.Count; } }
   

        public void Clear()
        {
            Attributes.Clear();
        }

        public bool ContainsKey(string key)
        {
            return Attributes.ContainsKey(key);
        }

        public void Merge(AttributeList l)
        {
            Dictionary<string, int> atts = new Dictionary<string, int>();

            foreach (var a in Attributes)
                atts.Add(a.Key,a.Value + l.Get(a.Key));

            Attributes = atts;
        }
    }
}
