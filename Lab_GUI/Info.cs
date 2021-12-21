using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lab_GUI
{
    public class Iofo : IEnumerable<string>
    {
        public string Info { get; set; }
        public List<string> list { get; set; }

        public Iofo(string newType, string newImage)
        {
            Info = newType;
            list = new List<string>();
            list.Add(newImage);
        }

        

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}
