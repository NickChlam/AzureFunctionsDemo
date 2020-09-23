using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System;

namespace AzureFunctionsDemo.Data
{
    public class DataAccess
    {
        public static string GetConnectionString()
        {
            return Environment.GetEnvironmentVariable("ConnectionString");
        }

        public static List<T> LoadData<T>(string sql) 
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                return cnn.Query<T>(sql).ToList();
                
            }
        }

        public static object SaveData<T>(string sql, T data)
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                return cnn.ExecuteScalar(sql, data);
                
            }
        }
    }
}