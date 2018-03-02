using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repository
{
    public class CredentialsRepository
    {
        public bool CheckUserBrand(string username, string brand)
        {
            using (var db = new ImageServerEntities())
            {
                var result = db.Credentials.Where(q => q.Username == username && q.Brand == brand).FirstOrDefault();
                if (result != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool InsertNew(string brand, string username)
        {
            bool res;
            using (var db = new ImageServerEntities())
            {
                try
                {
                    Credential credential = new Credential()
                    {
                        Id = 1,
                        Username = username,
                        Brand = brand
                    };
                    db.Credentials.Add(credential);
                    db.SaveChanges();
                    res = true;
                }
                catch (Exception)
                {
                    res = false;
                }
            }
            return res;
        }

        public bool RemoveRecord(string username, string brand)
        {
            bool res = false;
            using (var db = new ImageServerEntities())
            {
                try
                {
                    Credential credential = db.Credentials.Where(q => q.Username == username && q.Brand == brand).FirstOrDefault();
                    if(credential != null)
                    {
                        db.Credentials.Remove(credential);
                        db.SaveChanges();
                        res = true;
                    }
                    res = false;
                }
                catch (Exception)
                {
                    res = false;
                }
            }
            return res;
        }
    }
}
