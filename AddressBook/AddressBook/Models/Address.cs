using System.Data.Entity;

namespace AddressBook.Models
{
    public class Address
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Email { get; set; }

        public class AddressDBContext : DbContext
        {
            public DbSet<Address> Addresses { get; set; }
        }
    }
}