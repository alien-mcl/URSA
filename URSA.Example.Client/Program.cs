using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using RDeF.Entities;
using URSA.Example.WebApplication.Data;
using URSA.Web.Http;
using Vocab;

namespace URSA.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CredentialCache.DefaultNetworkCredentials.UserName = ConfigurationManager.AppSettings["DefaultCredentials"].Split(':')[0];
            CredentialCache.DefaultNetworkCredentials.Password = ConfigurationManager.AppSettings["DefaultCredentials"].Split(':')[1];
            var url = (HttpUrl)UrlParser.Parse(new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ServerUri"].ConnectionString).DataSource);
            TestPerson(url);
            TestProduct(url);
        }

        private static void TestPerson(HttpUrl url)
        {
            var client = new PersonClient(url, ConfigurationManager.AppSettings["DefaultAuthenticationScheme"]);
            var person = new Person() { Firstname = "Test", Lastname = "Testing", Roles = new[] { "Role" } };
            client.Create(person);
            Console.WriteLine("Created person.");
            int totalEntities;
            var persons = client.List(out totalEntities, 0, 0, null);
            Console.WriteLine("Listing all {0} person(s):", totalEntities);
            foreach (var item in persons)
            {
                Console.WriteLine("Person {0}", item.Key);
            }

            Console.ReadLine();
        }

        private static void TestProduct(HttpUrl url)
        {
            var entityContextFactory = EntityContextFactory.FromConfiguration("in-memory");
            var entityContext = entityContextFactory.Create();
            var client = new ProductClient(url, ConfigurationManager.AppSettings["DefaultAuthenticationScheme"]);
            var product = entityContext.Create<IProduct>(new Iri((Uri)(url + "/api/product")));
            product.Name = "Test";
            product.Price = 1.0;
            product.Features.Add("Feature");
            entityContext.Commit();
            client.Create(product);
            Console.WriteLine("Created product.");
            int totalEntities;
            var products = client.List(out totalEntities, 0, 0, null);
            Console.WriteLine("Listing all {0} products(s):", totalEntities);
            foreach (var item in products)
            {
                Console.WriteLine("Product {0}", item.Key);
            }

            Console.ReadLine();
        }
    }
}
