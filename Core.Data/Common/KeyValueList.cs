using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Data.Common
{
    public class KeyValueList : IEnumerable
    {
        protected Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public void Add(string name, string value)
        {
            if (Attributes.ContainsKey(name))
                Attributes[name] = value;
            else
                Attributes.Add(name, value);
        }

        public string Get(string name)
        {
            if (Attributes.ContainsKey(name))
                return Attributes[name];
            else
                return string.Empty;
        }

        public void Set(string name, string value)
        {
            Add(name, value);
        }

        public string SerializeToText()
        {
            List<string> d = new List<string>();
            foreach (var i in Attributes)
                d.Add(i.Key + "=" + i.Value);

            return string.Join(";", d.ToArray());
        }

        public static KeyValueList DeserlizeFromString(string text)
        {
            KeyValueList l = new KeyValueList();

            if (text == null || text == string.Empty)
                return l;

            string[] parts = text.Split(";".ToCharArray());
            foreach (var p in parts)
            {
                string[] comps = p.Split("=".ToCharArray(), 2);
                if (comps.Length != 2)
                    continue;
                l.Add(comps[0], comps[1]);
            }

            return l;
        }

        public IEnumerator GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }

        public string this[string key] { get { return Attributes[key]; } set { Attributes[key] = value; } }


        public int Count { get { return Attributes.Count; } }


        public void Clear()
        {
            Attributes.Clear();
        }

        public bool ContainsKey(string key)
        {
            return Attributes.ContainsKey(key);
        }
    }
}
