using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.Cryptography;

namespace myApp {
    public static class DateTimeExtensions
    {
        //calculate age
        public static int Age(this DateTime birthDate, DateTime laterDate)
        {
            int age;
            age = laterDate.Year - birthDate.Year;

            if (age > 0)
            {
                age -= Convert.ToInt32(laterDate.Date < birthDate.Date.AddYears(age));
            }
            else
            {
                age = 0;
            }

            return age;
        }
    }
    class Program
    {
        //create table in DB
        private static void AddTable(string connString)
        {
            string sqlExpression = "sp_AddTable";
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;
                var result = command.ExecuteNonQuery();
            }
        }

        // add person data
        private static void AddPerson(string name, SqlDateTime birthday, bool gender, string connString)
        {
            string sqlExpression = "sp_AddPerson";
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter nameParam = new SqlParameter
                {
                    ParameterName = "@name",
                    Value = name
                };
                command.Parameters.Add(nameParam);

                SqlParameter birthdayParam = new SqlParameter
                {
                    ParameterName = "@birthday",
                    Value = birthday
                };
                command.Parameters.Add(birthdayParam);

                SqlParameter genderParam = new SqlParameter
                {
                    ParameterName = "@gender",
                    Value = gender
                };
                command.Parameters.Add(genderParam);

                var result = command.ExecuteScalar();
                //var result = command.ExecuteNonQuery();

                Console.WriteLine("Id добавленного объекта: {0}", result);
            }
        }

        //retrieve names
        private static void GetPersonsNames(string connString)
        {
            string sqlExpression = "sp_GetPersonsNames";
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("{0}\t{1}\t{2}  Age", reader.GetName(1), reader.GetName(2), reader.GetName(3));
                    DateTime thisDay = DateTime.Today;
                    string iso = "yyyy-MM-dd";
                    while (reader.Read())
                    {
                        string name = reader.GetString(1);
                        DateTime birthday = reader.GetDateTime(2);
                        string gender = reader.GetBoolean(3) == true ? "Woman" : "Man";
                        Console.WriteLine("{0} \t{1} \t{2} \t{3}", name, birthday.ToString(iso), gender.ToString(), birthday.Age(thisDay).ToString());
                    }
                }
                reader.Close();
            }
        }

        //create million random records
        private static void AddPersonsRandom(string connString, int len)
        {
            string sqlExpression = "sp_AddPersonRandom";
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;

                Random r = new Random();
                for (int i = 0; i < len; i++)
                {
                        command.Parameters.Clear();
                        SqlParameter nameParam = new SqlParameter
                        {
                            ParameterName = "@name",
                            Value = RandomString(5) + " " + RandomString(5) + " " + RandomString(5)
                        };
                        command.Parameters.Add(nameParam);

                        SqlParameter birthdayParam = new SqlParameter
                        {
                            ParameterName = "@birthday",
                            Value = GetRandomDate(DateTime.Parse("00:00:00 01.01.1900"), DateTime.Now, r)
                        };
                        command.Parameters.Add(birthdayParam);

                        SqlParameter genderParam = new SqlParameter
                        {
                            ParameterName = "@gender",
                            Value = r.Next(10) % 2 == 0
                        };
                        command.Parameters.Add(genderParam);

                        var result = command.ExecuteScalar();
                        Console.WriteLine("Id добавленного объекта: {0}", result);
                }
            }
        }

        //selected records from table according to given criteria
        private static void Select(string connString, string sqlExpression)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("{0}\t{1}", reader.GetName(0), reader.GetName(1));
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string gender = reader.GetBoolean(1) == true ? "Woman" : "Man";
                        Console.WriteLine("{0} \t{1}", name, gender.ToString());
                    }
                }
                reader.Close();
            }
        }

        //creates random birthday dates
        public static SqlDateTime GetRandomDate(DateTime from, DateTime to, Random r)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(r.NextDouble() * range.Ticks));
            return SqlDateTime.Parse((from + randTimeSpan).Date.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        //creates random strings
        static string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyz";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }

        //connection BD params
        static readonly string connString = @"Data Source=DESKTOP-O5Q0ER0;Initial Catalog=DB;Integrated Security=True;
                            Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;
                            ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        static void Main(string[] args)
        {
            DateTime dold;
            switch (args[0])
            {
                case "1":
                    Console.WriteLine("run 1");
                    AddTable(connString);
                    break;
                case "2":
                    Console.WriteLine("run 2");
                    Console.Write("Input Name(input with spaces and [lenght string]<=25): ");
                    string name = Console.ReadLine();

                    Console.Write("Input Date(format: YYYYMMDD): ");
                    SqlDateTime birthday;
                    try
                    {
                        birthday = SqlDateTime.Parse(Console.ReadLine());
                    }
                    catch
                    {
                        Random r = new Random();
                        birthday = GetRandomDate(DateTime.Parse("00:00:00 01.01.1900"), DateTime.Now, r);
                    }
                    Console.Write("Input number Gender(man: 0,woman: 1): ");
                    bool gender = Console.ReadLine() == "1" ? true : false;

                    AddPerson(name, birthday, gender, connString);
                    Console.WriteLine();
                    break;
                case "3":
                    Console.WriteLine("run 3");
                    GetPersonsNames(connString);
                    break;
                case "4":
                    Console.WriteLine("run 4");
                    Console.Write("Input number of records: ");
                    int len;
                    try
                    {
                        len = int.Parse(Console.ReadLine());
                    }
                    catch
                    {
                        len = 10000;
                    }
                    dold = DateTime.Now;
                    AddPersonsRandom(connString, len);
                    Console.WriteLine(DateTime.Now - dold);
                    break;
                case "5":
                    Console.WriteLine("run 5");
                    dold = DateTime.Now;
                    Select(connString, "sp_Select");
                    Console.WriteLine(DateTime.Now - dold);
                    break;
                case "6":
                    Console.WriteLine("run 6");
                    dold = DateTime.Now;
                    Select(connString, "sp_SelectOptimized");
                    Console.WriteLine(DateTime.Now - dold);
                    break;
                default:
                    Console.WriteLine("Value does not exist..");
                    Console.WriteLine("HELPERS:");
                    Console.WriteLine("Input 1 create table");
                    Console.WriteLine("Input 2 adds person record");
                    Console.WriteLine("Input 3 retrieves data according to name and birthday criteria");
                    Console.WriteLine("Input 4 inserts 1000000 random records to table");
                    Console.WriteLine("Input 5 retrieves data (name and gender) where name starts by 'F' letter");
                    Console.WriteLine("Input 6 makes optimization of previous method");
                    Console.WriteLine("Close program..");
                    break;
            }
        }
    }
}
