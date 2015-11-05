using System;
using System.Configuration;
using System.Data.SqlClient;
using URSA.Example.WebApplication.Data;

namespace URSA.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new PersonClient(new Uri(new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ServerUri"].ConnectionString).DataSource));
            var person = new Person() { Firstname = "Test", Lastname = "Testing", Roles = new[] { "Role" } };
            person.Key = client.Create(person);
            Console.WriteLine("Created person '{0}'.", person.Key);
            int totalEntities;
            var persons = client.List(out totalEntities, 0, 0);
            Console.WriteLine("Listing all {0} person(s):", totalEntities);
            foreach (var item in persons)
            {
                Console.WriteLine("Person {0}", person.Key);
            }

            Console.ReadLine();
        }
    }
}