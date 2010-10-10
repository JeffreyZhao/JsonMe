using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonMe;

namespace SimpleConsole
{
    public class User
    {
        public string UserName { get; set; }
        public int Age { get; set; }
        public bool IsAdult { get { return this.Age >= 18; } }
    }

    public class Post
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public User Author { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class DataTimeConverter : IJsonConverter
    {
        public object ToJsonValue(Type type, object value)
        {
            return ((DateTime)value).ToString("R");
        }

        public object FromJsonValue(Type type, object value)
        {
            return DateTime.ParseExact((string)value, "R", null);
        }
    }

    class Program
    {
        private static JsonContract<User> s_userContract;
        private static JsonContract<Post> s_postContract;
        private static JsonContract<Category> s_categoryContract;

        static Program()
        {
            s_userContract = new JsonContract<User>();
            s_userContract.SimpleProperty(u => u.UserName).Name("Name");
            s_userContract.SimpleProperty(u => u.Age);

            s_postContract = new JsonContract<Post>();
            s_postContract.SimpleProperty(p => p.Title);
            s_postContract.SimpleProperty(p => p.CreateTime).Converter(new DataTimeConverter());

            s_categoryContract = new JsonContract<Category>();
            s_categoryContract.SimpleProperty(p => p.Name);
            s_categoryContract.ComplexProperty(p => p.Author).Contract(s_userContract);
            s_categoryContract.ArrayProperty(p => p.Posts).ElementContract(s_postContract);
        }

        static void SerializeTest()
        {
            var user = new User { UserName = "Tom", Age = 20 };
            Console.WriteLine(JsonSerializer.SerializeObject(user, s_userContract));
            Console.WriteLine();

            var post = new Post { Title = "Good day today.", CreateTime = DateTime.Now };
            Console.WriteLine(JsonSerializer.SerializeObject(post, s_postContract));
            Console.WriteLine();

            var category = new Category
            {
                Name = "Default",
                Author = new User { UserName = "Jerry", Age = 15 },
                Posts = new List<Post>
                {
                    new Post { Title = "Post 1", CreateTime = new DateTime(2010, 1, 1) },
                    new Post { Title = "Post 2", CreateTime = new DateTime(2010, 2, 1) },
                    new Post { Title = "Post 3", CreateTime = new DateTime(2010, 3, 1) }
                }
            };
            var jsonCategory = JsonSerializer.SerializeObject(category, s_categoryContract);
            Console.WriteLine(jsonCategory);
            Console.WriteLine();

            var value = new { v = JsonSerializer.SerializeObject(category, s_categoryContract) };
            Console.WriteLine(JsonSerializer.Serialize(value));
        }

        static void DeserializeTest()
        {
            var jsonString = "{ 'name' : 'hello', 'age' : 15, a : [ 1, 2, 3, 4 ] }";
            var jsonObj = JsonSerializer.Deserialize(jsonString);

            var jsonUser = "{ Name : 'Tom', Age : 20 }";
            var user = JsonSerializer.DeserializeObject<User>(jsonUser, s_userContract);

            var jsonCategory = "{\"Name\":\"Default\",\"Author\":{\"Name\":\"Jerry\",\"Age\":15},\"Posts\":[{\"Title\":\"Post 1\",\"CreateTime\":\"Fri, 01 Jan 2010 00:00:00 GMT\"},{\"Title\":\"Post 2\",\"CreateTime\":\"Mon, 01 Feb 2010 00:00:00 GMT\"},{\"Title\":\"Post 3\",\"CreateTime\":\"Mon, 01 Mar 2010 00:00:00 GMT\"}]}";
            var category = JsonSerializer.DeserializeObject<Category>(jsonCategory, s_categoryContract);
        }

        static void Main(string[] args)
        {
            SerializeTest();

            DeserializeTest();

            Console.ReadLine();
        }
    }
}
