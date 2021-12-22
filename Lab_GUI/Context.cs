using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections;

namespace Lab_GUI
{
    public class Context
    {
        public int ObjectId { get; set; }
        public float x1 { get; set; }
        public float y1 { get; set; }
        public float x2 { get; set; }
        public float y2 { get; set; }
        public byte[] Image { get; set; }
        public DBresult Type { get; set; }

    }

    public class DBresult
    {
        public int Rid { get; set; }
        public string Type { get; set; }
        public ICollection<Object> Objects { get; set; }
        public override string ToString()
        {
            return Type;
        }
    }
    public class context : DbContext
    {
        public DbSet<DBresult> results { get; set; }
        public DbSet<Object> Object { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o)
           => o.UseSqlite(@"Data Source=C:\Users\91930\Desktop\c#\database.db");

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public void Add(string type, float[] BBox, Bitmap bitmap)
        {
            var db = new Context();
            var query = results.Where(p => type == p.Type);
            if (query.Count() > 0)
            {
                db.Type = query.First();
            }
            else
            {
                db.Type = new DBresult();
                db.Type.Type = type;
                results.Add(db.Type);
            }
            SaveChanges();
        }
        public IEnumerable<string> GetTypes()
        {
            foreach (var res in results)
            {
                yield return res.Type;
            }
        }
        public void Delete(string type)
        {
            foreach (var db in Object)
            {

                Object.Remove(db);

            }


            SaveChanges();
        }
    }
}
