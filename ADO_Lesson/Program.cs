using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Xml.Serialization;

namespace lesson
{
    internal class Program
    {

        private const string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;
            Initial Catalog=NewDB;
            Integrated Security=True;
            Persist Security Info=False;
            Pooling=False;
            MultipleActiveResultSets=False;
            Encrypt=True;
            TrustServerCertificate=True;
            Command Timeout=0";

        static void Main(string[] args)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            Menu(connection);
        }

        private static void Menu(SqlConnection connection)
        {
            while (true)
            {
                Console.WriteLine("1. Create table");
                Console.WriteLine("2. Show users");
                Console.WriteLine("3. Register user");
                Console.WriteLine("4. Find user by name");
                Console.WriteLine("5. Find user by email");
                Console.WriteLine("6. Delete user");
                Console.WriteLine("0. Exit");
                Console.WriteLine("Enter Action:");
                int choice = Int32.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        SetupDB(connection);
                        break;
                    case 2:
                        ShowUsers(connection);
                        break;
                    case 3:
                        Register(connection);
                        break;
                    case 4:
                        FindUsersByName(connection);
                        break;
                    case 5:
                        FindUsersByEmail(connection);
                        break;
                    case 6:
                        DeleteUserByName(connection);
                        break;
                    case 0:
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        // Account check

        private static void SetupDB(SqlConnection connection)
        {
            string query = @"
            USE NewDB;
            
            DROP TABLE IF EXISTS Users;

            CREATE TABLE Users(
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Username NVARCHAR(50) UNIQUE NOT NULL,
                Email NVARCHAR(50) UNIQUE NOT NULL CHECK(Email LIKE '%@%.%'),
                BirthDate DATETIME NOT NULL CHECK(BirthDate < GETDATE()),
                RegistrationDate DATETIME NOT NULL DEFAULT GETDATE()
            );
            ";
            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        private static void ShowUsers(SqlConnection connection)
        {
            string query = @"
                   SELECT Id, Username, Email, BirthDate, RegistrationDate
                   FROM Users";
            using SqlCommand cmd = new SqlCommand(query, connection);

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("Users:");
                Console.WriteLine($"Id | Username | Email | BirthDate | RegistrationDate");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Id"]} | {reader["Username"]} | {reader["Email"]} | {reader["BirthDate"]} | {reader["RegistrationDate"]}");
                }
            }
            else
            {
                Console.WriteLine("No users in DB");
            }
        }

        private static void Register(SqlConnection connection)
        {
            Console.Write("Write your username: ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Write your email address: ");
            string email = Console.ReadLine() ?? string.Empty;

            Console.Write("Write your birthdate (yyyy-mm-dd): ");
            string? input = Console.ReadLine();

            if (!DateTime.TryParse(input, out DateTime birthdate))
            {
                Console.WriteLine("Invalid date format!");
                return;
            }

            string query = @"INSERT INTO Users (Username, Email, BirthDate) 
                     VALUES (@Username, @Email, @BirthDate)";

            try
            {
                using SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = email;
                cmd.Parameters.Add("@BirthDate", SqlDbType.DateTime).Value = birthdate;

                cmd.ExecuteNonQuery();

                Console.WriteLine("User registered successfully.");
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void FindUsersByName(SqlConnection connection)
        {
            Console.WriteLine("Enter your search query: ");
            string search = Console.ReadLine() ?? string.Empty;

            string query = @"
                SELECT Id, Username, Email, BirthDate, RegistrationDate
                FROM Users 
                WHERE Username LIKE @searchQuery;";


            Console.WriteLine($"[DEBUG] executing query: {query}");

            using SqlCommand cmd = new SqlCommand(query, connection);

            cmd.Parameters.Add(new SqlParameter("@searchQuery", search));

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("Users:");
                Console.WriteLine($"Id | Username | Email | BirthDate | RegistrationDate");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Id"]} | {reader["Username"]} | {reader["Email"]} | {reader["BirthDate"]} | {reader["RegistrationDate"]}");
                }
            }
            else
            {
                Console.WriteLine("No users in DB");
            }
        }

        private static void FindUsersByEmail(SqlConnection connection)
        {
            Console.WriteLine("Enter your search query: ");
            string search = Console.ReadLine() ?? string.Empty;

            string query = @"
                SELECT Id, Username, Email, BirthDate, RegistrationDate
                FROM Users 
                WHERE Email LIKE @searchQuery;";


            Console.WriteLine($"[DEBUG] executing query: {query}");

            using SqlCommand cmd = new SqlCommand(query, connection);

            cmd.Parameters.Add(new SqlParameter("@searchQuery", search));

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("Users:");
                Console.WriteLine($"Id | Username | Email | BirthDate | RegistrationDate");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Id"]} | {reader["Username"]} | {reader["Email"]} | {reader["BirthDate"]} | {reader["RegistrationDate"]}");
                }
            }
            else
            {
                Console.WriteLine("No users in DB");
            }
        }

        private static void DeleteUserByName(SqlConnection connection)
        {
            Console.WriteLine("Enter username to delete: ");
            string search = Console.ReadLine() ?? string.Empty;

            string query = @"
                    DELETE FROM Users 
                    WHERE Username LIKE @searchQuery;";

            using SqlCommand cmd = new SqlCommand(query, connection);

            cmd.Parameters.Add("@searchQuery", SqlDbType.NVarChar, 50)
                          .Value = "%" + search + "%";

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                Console.WriteLine($"Deleted {rowsAffected} user(s).");
            }
            else
            {
                Console.WriteLine("No user found to delete.");
            }
        }
    }
}