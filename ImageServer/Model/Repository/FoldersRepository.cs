using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repository
{
    public class FoldersRepository
    {
        public Folder GetByToken(string token)
        {
            using (var db = new ImageServerEntities())
            {
                var result = db.Folders.Where(q => q.Token == token).FirstOrDefault();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public string InsertNew(string brand, string path, int parentId)
        {
            string res;
            using (var db = new ImageServerEntities())
            {
                try
                {
                    string token = GenToken(DateTime.Now);
                    Folder folder = new Folder()
                    {
                        Id = 1,
                        Brand = brand,
                        Path = path,
                        ParentPathId = parentId,
                        Token = token
                    };
                    db.Folders.Add(folder);
                    db.SaveChanges();
                    res = token;
                }
                catch (Exception)
                {
                    res = "";
                }
            }
            return res;
        }

        public bool RemoveRecord(Folder targetfolder)
        {
            bool res = false;
            using (var db = new ImageServerEntities())
            {
                try
                {

                    Folder folder = db.Folders.Find(targetfolder.Id);
                    if (folder != null)
                    {
                        db.Folders.Remove(folder);
                        db.SaveChanges();
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }

                    
                }
                catch (Exception e)
                {
                    res = false;
                }
            }
            return res;
        }

        /// <summary>
        /// Return numberic token generate by giving time
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        private string GenToken(DateTime when)
        {
            ulong kind = (ulong)(int)when.Kind;
            var code = (kind << 62) | (ulong)when.Ticks;
            return code + "";
        }
    }
}
