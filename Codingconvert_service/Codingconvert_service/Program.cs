using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Configuration;

namespace Codingconvert_service
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["WWALMDB"].ConnectionString;
           
            //string connectionString = "data source=10.21.199.28;initial catalog=WWALMDB;persist security info=True;user id=sa;password=Aaaa-1111;";
            //string connectionString = "data source=GRISHAYEVK\\SQLEXPRESS;initial catalog=Test;Integrated Security=True;";
            //string db = "Test";
            string db = "WWALMDB";

            DateTime current_day = DateTime.Today;
            string date = current_day.ToString("yyyyMMdd");
            
            while (true)
            {
                DataTable comment_table = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))

                    try
                    {
                        DateTime now_time = DateTime.Now;
                        connection.Open();
                        Console.WriteLine(now_time);
                        Console.WriteLine("Connection to DB is opened");

                        //string select_comments = "Select [CommentId],[Comment],[CommentTime] from [" + db + "].[dbo].[Comment] where [CommentTime] >= '" + date + "'";
                       // string select_comments = "Select [CommentId],[Comment],[CommentTime] from [" + db + "].[dbo].[Comment] where [CommentTime] >= (select convert (datetime, DATEADD(MINUTE,-65,GETDATE())))";

                        string select_comments = "Select [CommentId],[Comment],[CommentTime] from [" + db + "].[dbo].[Comment] where [CommentTime] >= (select convert (datetime, DATEADD(HOUR,-25,GETDATE())))";
                        SqlCommand select_command = new SqlCommand(select_comments, connection);
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(select_command);
                        dataAdapter.Fill(comment_table);

                        List<string> lst = new List<string> { "à","á", "â", "ã", "ä", "å", "æ", "ç", "è", "é", "ê", "ë", "ì", "í",
                        "î","ï","ð","ñ","ò","ó","ô","õ","ö","÷","ø","ù","ú","û","ü","ý","þ","ÿ"};

                        bool consist = false;

                        foreach (DataRow data in comment_table.Rows)
                        {
                            string comment_id = data["CommentId"].ToString();
                            string old_coding = data["Comment"].ToString();

                            foreach (string symbol in lst)
                            {

                                if (old_coding.Contains(symbol))
                                {
                                    consist = true;
                                    break;
                                }

                                else
                                {
                                    consist = false;
                                }

                            }

                            if (consist == true)
                            {
                                var coding = Encoding.GetEncoding(1252).GetBytes(old_coding);
                                string new_coding = Encoding.GetEncoding(1251).GetString(coding);

                                Console.WriteLine("CommentId = " + comment_id + "\\  Old coding type is Windows 1252; Old alarm text:" + old_coding);
                                Console.WriteLine("                     New coding type is Windows 1251; New alarm text:" + new_coding);

                                string insert_command = "UPDATE [" + db + "].[dbo].[Comment] SET Comment = N'" + new_coding + "' WHERE CommentId = " + comment_id + ";";

                                SqlCommand insert = new SqlCommand(insert_command, connection);
                                insert.ExecuteNonQuery();

                            }

                            else
                            {
                                Console.WriteLine("CommentId = " + comment_id + "\\  Old coding type is normal");                                
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    finally
                    {
                        connection.Close();
                        Console.WriteLine("Connection to DB is closed");
                    }
                Console.WriteLine("");
                Console.WriteLine("");
                Thread.Sleep(300000);
 
            }

        }
    }
}
