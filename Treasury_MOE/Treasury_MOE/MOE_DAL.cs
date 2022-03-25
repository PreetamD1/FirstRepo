using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Treasury_MOE
{
    class MOE_DAL
    {
        public OleDbConnection conn = new OleDbConnection();

        OleDbCommand cmd = new OleDbCommand();
        OleDbDataAdapter da = new OleDbDataAdapter();

        public void OpenConnection()
        {
            if (conn.State != ConnectionState.Open)
            {
                try
                {
                    //conn.ConnectionString = "Data Source=DOADataSource;User Id=doa;Password=doa1;";
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["doaConnString"].ConnectionString;
                    cmd.Connection = conn;
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void CloseConnection()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
        }

        public int update_moeerror(string moe_dt, string CNRid, string UTAid)
        {
            //try
            //{
            //    Log("update_moeerror");
            //    OpenConnection();
            //    cmd.Parameters.Clear();
            //    // string query = "update DOA.rbi_moe_files set retrieved = 'Y' where to_char(file_date, 'dd/MM/yyyy') = substr(:moedt1, 0, 10) and type=:type";
            //    string query = "update DOA.gstn_moerror set moegenerated = 'Y' where to_char(escrolldate, 'dd/MM/yyyy') = substr(:moedt1, 0, 10) ";
            //    cmd.CommandText = query;
            //    cmd.Parameters.Add("moedt1", OleDbType.VarChar).Value = moe_dt;
            //    // cmd.Parameters.Add("type", OleDbType.VarChar).Value = type;
            //    int value = cmd.ExecuteNonQuery();
            //    CloseConnection();
            //    Log("update_moeerror closed");
            //    return value;
            //}


            try
            {

                Log("update_moeerror");
                OpenConnection();
                cmd.Parameters.Clear();
                string query = "";
                int value = 0;
                //rbi_moe_files1
                // string query = "update DOA.rbi_moe_files set retrieved = 'Y' where to_char(file_date, 'dd/MM/yyyy') = substr(:moedt1, 0, 10) and type=:type";
                if (CNRid != "")
                {
                    query = "update DOA.gstn_moerror set moegenerated = 'Y' , moecaseid=? where to_char(escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and moetype is null";
                    cmd.CommandText = query;
                    cmd.Parameters.Add("?", OleDbType.VarChar).Value = CNRid;
                    cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                    value = cmd.ExecuteNonQuery();
                }
                if (UTAid != "")
                {
                    query = "update DOA.gstn_moerror set moegenerated = 'Y', moecaseid=? where to_char(escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and moetype is not null ";
                    cmd.CommandText = query;
                    cmd.Parameters.Add("?", OleDbType.VarChar).Value = UTAid;
                    cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                    value = cmd.ExecuteNonQuery();
                }


                CloseConnection();
                Log("update_moeerror closed");
                return value;
            }
            catch (Exception e)
            {
                Log("update_moeerror:" + e.Message);
                return 0;
            }
        }//used

        public DataSet get_moe_date()
        {
            DataSet ds = new DataSet();
            try
            {
                OpenConnection();
                cmd.Parameters.Clear();
                //rbi_moe_files1
                //  string query = "select distinct(to_char(FILE_DATE,'dd/MM/yyyy')) as dt from DOA.rbi_moe_files where retrieved = 'N'";
                string query = "select distinct(to_char(escrolldate,'dd/MM/yyyy')) as dt from DOA.gstn_moerror where moegenerated = 'N'";

                cmd.CommandText = query;
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(ds);
                CloseConnection();
                return (ds);
            }
            catch (Exception e)
            {
                Log(e.Message);
                return null;
            }
        }//used

        public DataTable getdata_UA(string moe_dt, string type)//used
        {
            try
            {
                Log("getdata_UA");
                DataTable dt = new DataTable();
                OpenConnection();
                cmd.Parameters.Clear();
                //rbi_cn_txdtls1 gstn_moerror1
                string query = "select a.msgid,a.endtoendid,a.mmbid,b.amt,b.sgst_total,b.txdttm,b.ERRORCODE from DOA.rbi_cn_txdtls a  join  DOA.gstn_moerror b on a.endtoendid=b.cin where to_char(b.escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and b.moetype is not null";
                cmd.CommandText = query;
                cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                // cmd.Parameters.Add("?", OleDbType.VarChar).Value = type;
                OleDbDataAdapter da1 = new OleDbDataAdapter(cmd);
                da1.Fill(dt);

                Log("getdata_UA closed");
                CloseConnection();
                return dt;
            }
            catch (Exception ex)
            {

                Log("getdata_UA error:" + ex.Message);
                return null;

            }
        }
        public DataTable getdata_CNR(string moe_dt, string type)//used
        {
            try
            {
                Log("getdata_CNR");
                DataTable dt = new DataTable();
                OpenConnection();

                cmd.Parameters.Clear();
                //gstn_moerror1
                string query0 = "select count(*) from DOA.gstn_moerror where to_char(escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and source=1 and   moetype is null ";
                cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                cmd.CommandText = query0;
                int a = Convert.ToInt32(cmd.ExecuteScalar());

                if (a > 0)
                {

                    cmd.Parameters.Clear();
                    string query01 = "select '' as msgid ,cin as endtoendid,'' as mmbid,amt as amt  from DOA.gstn_moerror  where to_char(escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and source=1 and   moetype is null";
                    cmd.CommandText = query01;
                    cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                    OleDbDataAdapter da01 = new OleDbDataAdapter(cmd);
                    da01.Fill(dt);

                }



                cmd.Parameters.Clear();
                //rbi_cn_txdtls1 gstn_moerror1
                string query = "select a.msgid,a.endtoendid,a.mmbid,b.amt from DOA.rbi_cn_txdtls a  join  DOA.gstn_moerror b on a.endtoendid=b.cin where to_char(b.escrolldate, 'dd/MM/yyyy') = substr(?, 0, 10) and b.moetype is null and source=2 ";
                cmd.CommandText = query;
                cmd.Parameters.Add("?", OleDbType.VarChar).Value = moe_dt;
                OleDbDataAdapter da1 = new OleDbDataAdapter(cmd);
                da1.Fill(dt);
                Log("getdata_CNR closed");
                CloseConnection();
                return dt;

            }
            catch (Exception ex)
            {

                Log("getdata_CNR error:" + ex.Message);
                return null;

            }

        }

        public string get_Running_Seq_bizmsgidr()
        {
            OpenConnection();
            cmd.Parameters.Clear();
            string query = "select lpad(DOA.rbi_bizmsgidr_seq.nextval,6,0) from dual";
            cmd.CommandText = query;
            string seq = (string)cmd.ExecuteScalar();
            CloseConnection();
            return (seq);
        }
        public string get_Running_caseSequence()
        {
            OpenConnection();
            cmd.Parameters.Clear();
            string query = "select lpad(DOA.rbi_moexmlfile_caseseq.nextval,6,0) from dual";
            cmd.CommandText = query;
            string seq = (string)cmd.ExecuteScalar();
            CloseConnection();
            return (seq);
        }
        public string get_Running_UASequence()
        {
            OpenConnection();
            cmd.Parameters.Clear();
            string query = "select lpad(DOA.rbi_moexmlfile_UAseq.nextval,6,0) from dual";
            cmd.CommandText = query;
            string seq = (string)cmd.ExecuteScalar();
            CloseConnection();
            return (seq);

        }
        public DataTable get_master_data()
        {
            DataTable dt = new DataTable();
            try
            {
                cmd.Parameters.Clear();
                ///GSTN_MASTER1
                string query = "select RBI_USERNAME, RBI_IPADDRESS, RBI_PWD, RBI_REMOTEPATH, LOCAL_FILEPATH from DOA.GSTN_MASTER ";
                // string query = "select RBI_USERNAME, RBI_IPADDRESS, RBI_PWD, RBI_REMOTEPATH from GSTN_MASTER ";
                OpenConnection();
                cmd.CommandText = query;
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                dt.Dispose();
                CloseConnection();
            }
        }
        public DataSet select_moe_data(string dt)
        {
            DataSet ds = new DataSet();
            DataTable data = new DataTable("moe_data");
            OpenConnection();
            cmd.Parameters.Clear();
            //OleDbTransaction trans = conn.BeginTransaction();
            try
            {
                //ESCROLLDATE,CIN,MOETYPE,ERRORCODE,AMT,SOURCE
                string query = "select CIN, to_char(ESCROLLDATE,'dd/MM/yyyy') as date1,AMT,case when ERRORCODE = 2 then 'CIN missing in GSTN'" +
                    " when ERRORCODE = 3 then 'CIN missing in RBI' when ERRORCODE = 7 then 'Invalid CIN' " +
                    "when ERRORCODE = 8 then 'Reported Amount More than Actual' when ERRORCODE = 9 then 'Reported Amount Less than Actual'" +
                    " else '' end as toe from doa.gstn_moerror where to_char(ESCROLLDATE, 'dd/MM/yyyy') = substr(?,0, 10) and AMT>0";

                cmd.CommandText = query;
                cmd.Parameters.Add("?", OleDbType.VarChar).Value = dt;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(data);
                ds.Tables.Add(data);
            }
            catch (Exception ex)
            {
            }
            CloseConnection();

            return ds;
        }
        public static void Log(string logMessage)
        {
            DateTime dt = DateTime.Now;
            string filename = dt.Day.ToString() + "-" + dt.Month.ToString() + "-" + dt.Year.ToString();
            string ASSem = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            string path = System.IO.Path.GetDirectoryName(ASSem);
            string locpath = new Uri(path).LocalPath;

            if (!Directory.Exists(locpath + "\\Logs"))
                Directory.CreateDirectory(locpath + @"\\Logs");

            using (StreamWriter w = File.AppendText((locpath + "\\Logs\\" + filename + ".txt")))
            {
                w.WriteLine();
                w.WriteLine("{0} :{1}", DateTime.Now.ToLongTimeString(), logMessage);

            }
        }

    }
}
