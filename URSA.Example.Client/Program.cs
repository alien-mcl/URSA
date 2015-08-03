using System;
using System.Configuration;
using System.Data.SqlClient;
using URSA.ComponentModel;
using URSA.Example.WebApplication.Data;

namespace URSA.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new PersonClient(new Uri(new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ServerUri"].ConnectionString).DataSource));
            var person = new Person() { Firstname = "Test", Lastname = "Testing", Roles = new[] { "Role" } };
            person.Id = client.Create(person);
            Console.WriteLine("Created person '{0}'.", person.Id);
        }
    }
}