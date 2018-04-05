using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace drportal.Helpers {
    public class DBHelper {
        string ConnnectionString { get; set; }
        
        public DBHelper(string connectionString) {
            ConnnectionString = connectionString;
        }

        public int ExecuteNonQuery(bool isSP, string query, Dictionary<string, object> paramters = null) {
            DataSet dataSet = new DataSet();
            using (var sqlConnection = new SqlConnection(ConnnectionString)) {
                try {
                    if(sqlConnection.State == ConnectionState.Closed) {
                        sqlConnection.Open();
                    }
                    using (var sqlCommand = new SqlCommand(query, sqlConnection)) {
                        if (isSP) {
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                        }

                        if (paramters != null) {
                            foreach(var param in paramters) {
                                sqlCommand.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }

                        return sqlCommand.ExecuteNonQuery();
                    }
                }
                catch {
                    throw;
                }
                finally {
                    if(sqlConnection.State != ConnectionState.Closed) {
                        sqlConnection.Close();
                    }
                }
            }
        }

        public DataSet ExecuteQuery(bool isSP, string query, Dictionary<string, object> paramters = null) {
            using (var sqlConnection = new SqlConnection(ConnnectionString)) {
                try {
                    if(sqlConnection.State == ConnectionState.Closed) {
                        sqlConnection.Open();
                    }
                    using (var sqlCommand = new SqlCommand(query, sqlConnection)) {
                        if (isSP) {
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                        }

                        if (paramters != null) {
                            foreach(var param in paramters) {
                                sqlCommand.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }

                        var sqlAdapter = new SqlDataAdapter(sqlCommand);
                        var dataSet = new DataSet();
                        sqlAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
                catch {
                    throw;
                }
                finally {
                    if(sqlConnection.State != ConnectionState.Closed) {
                        sqlConnection.Close();
                    }
                }
            }
        }
    }
}

