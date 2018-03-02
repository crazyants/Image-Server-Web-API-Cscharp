using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class CredentialRepository
    {
        public void abc()
        {
            using (var db = new ImageServerEntities())
            {
                var result = db.Books.SingleOrDefault(b => b.BookNumber == bookNumber);
                if (result != null)
                {
                    result.SomeValue = "Some new value";
                    db.SaveChanges();
                }
            }
        }
    }
}
