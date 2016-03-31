using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using URSA.Example.WebApplication.Data;

namespace URSA.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CredentialCache.DefaultNetworkCredentials.UserName = ConfigurationManager.AppSettings["DefaultCredentials"].Split(':')[0];
            CredentialCache.DefaultNetworkCredentials.Password = ConfigurationManager.AppSettings["DefaultCredentials"].Split(':')[1];
            var uri = new Uri(new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ServerUri"].ConnectionString).DataSource);
            var client = new PersonClient(uri, ConfigurationManager.AppSettings["DefaultAuthenticationScheme"]);
            var person = new Person() { Firstname = "Test", Lastname = "Testing", Roles = new[] { "Role" } };
            person.Key = client.Create(person);
            Console.WriteLine("Created person '{0}'.", person.Key);
            int totalEntities;
            var persons = client.List(out totalEntities, 0, 0, null);
            Console.WriteLine("Listing all {0} person(s):", totalEntities);
            foreach (var item in persons)
            {
                Console.WriteLine("Person {0}", person.Key);
            }

            Console.ReadLine();
        }
    }
}