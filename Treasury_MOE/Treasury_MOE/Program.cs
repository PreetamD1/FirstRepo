using System;
using System.Xml;
using WinSCP;
using System.IO;
using System.Configuration;
using System.Data;

namespace Treasury_MOE
{
    class Program
    {
        public static string locpath = string.Empty;
        public static string moe_dt = string.Empty;
        public static string MOEXMLUPLOAD_path = "";

        public static MOE_BLL _bll = new MOE_BLL();

        static void Main(string[] args)
        {
            try
            {
                string ASSem = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;//
                string path = System.IO.Path.GetDirectoryName(ASSem);//
                locpath = new Uri(path).LocalPath;//

                if (!Directory.Exists(locpath + "\\MOEXMLUPLOAD"))//
                {
                    Directory.CreateDirectory(locpath + @"\\MOEXMLUPLOAD");
                }
                MOEXMLUPLOAD_path = locpath + "\\MOEXMLUPLOAD";
                // MOEXMLUPLOAD
                //string moe_dt = "17/04/2018";
                // string moe_dt = string.Empty;
                DataSet ds = _bll.get_moe_date();

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count >= 1)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            try
                            {
                                moe_dt = ds.Tables[0].Rows[i]["dt"].ToString();
                                Log("Generating moe for date:" + moe_dt);

                                string CNRid = moe_CNR_xml();
                                string UTAid = moe_UTA_xml();

                                _bll.update_moeerror(moe_dt, CNRid, UTAid);
                            }
                            catch (Exception ex)
                            {
                                Log("exception :" + ex.Message);
                            }

                        }
                    }
                }
                upload_moe_xml();

            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        protected static int upload_moe_xml()
        {
            try
            {
                Log("upload_moe_xml");
                DataTable dtMasterData = _bll.get_master_data();
                DataRow dr = dtMasterData.Rows[0];


                SessionOptions Sessionopt = new SessionOptions();
                Sessionopt.Protocol = Protocol.Sftp;

                Sessionopt.HostName = dr["RBI_IPADDRESS"].ToString();
                Sessionopt.UserName = dr["RBI_USERNAME"].ToString();
                Sessionopt.Password = dr["RBI_PWD"].ToString();
                Sessionopt.PortNumber = 22;
                Sessionopt.SshHostKeyFingerprint = "ssh-rsa 2048 f7:1d:7b:f3:68:d0:f1:22:d4:d9:53:cb:86:35:c9:51";// "ssh-rsa 2048 3f:80:1e:b0:61:f0:56:9c:cd:47:dc:55:72:8d:41:21";//

                Session session = new Session();
                session.SessionLogPath = "";
                session.Open(Sessionopt);
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;
                transferOptions.FilePermissions = null;
                transferOptions.PreserveTimestamp = false;
                transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
                TransferOperationResult transferResult;

                //{/home/archana}/home/archana/RBI/ERGST/MOE/CASE///--/home/archana/RBI
                transferResult = session.PutFiles(locpath + "\\MOEXMLUPLOAD\\*.xml", "/ERGST/MOE/CASE/", false, transferOptions);
                session.Close();
                //deleting file from local storage
                DirectoryInfo sf = new DirectoryInfo(locpath + "\\MOEXMLUPLOAD");
                foreach (FileInfo fi in sf.GetFiles())
                {
                    File.Delete(fi.FullName);
                }
                Log("upload_moe_xml closed");
                return 1;

            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return 0;
            }
        }//used

        public static void Log(string logMessage)
        {
            DateTime dt = DateTime.Now;
            string filename = dt.Day.ToString() + "-" + dt.Month.ToString() + "-" + dt.Year.ToString();

            if (!Directory.Exists(locpath + "\\Logs"))
                Directory.CreateDirectory(locpath + @"\\Logs");

            using (StreamWriter w = File.AppendText((locpath + "\\Logs\\" + filename + ".txt")))
            {
                w.WriteLine();
                w.WriteLine("{0} :{1}", DateTime.Now.ToLongTimeString(), logMessage);

            }
        }

        protected static string moe_CNR_xml()//used
        {
            Log("moe_CNR_xml");
            System.Data.DataTable dt = new System.Data.DataTable();
            dt = _bll.getdata_CNR(moe_dt, "");//MEIN01
            string bizmsgidr = string.Empty;
            if (dt.Rows.Count > 0)
            {

                 bizmsgidr = "NR0123" + "001586701018" + DateTime.Now.Year.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + _bll.get_Running_Seq_bizmsgidr();//NR+0123+Treasury Account (or) Agency Bank Code (12)+DateTime.Now.Year.ToString()+DateTime.Now.Month.ToString()+DateTime.Now.Day.ToString()+_bll.get_Running_Sequence()


                XmlWriter xmlWriter = XmlWriter.Create(MOEXMLUPLOAD_path + "\\" + bizmsgidr + ".xml");//MOEXML_NCR"+ DateTime.Now.Year.ToString()+ DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString()+ ".xml");

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("RequestPayload");
                //AppHdr Start
                xmlWriter.WriteStartElement("AppHdr");

                //FROM START
                xmlWriter.WriteStartElement("Fr");
                xmlWriter.WriteStartElement("FIId");
                xmlWriter.WriteStartElement("FinInstnId");
                xmlWriter.WriteStartElement("ClrSysMmbId");
                xmlWriter.WriteStartElement("MmbId");
                xmlWriter.WriteString("RBI");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Othr");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteString("0123");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                //FROM END

                //TO START
                xmlWriter.WriteStartElement("To");
                xmlWriter.WriteStartElement("FIId");
                xmlWriter.WriteStartElement("FinInstnId");
                xmlWriter.WriteStartElement("ClrSysMmbId");
                xmlWriter.WriteStartElement("MmbId");
                xmlWriter.WriteString("2224");//check
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                //TO END

                xmlWriter.WriteStartElement("BizMsgIdr");
                xmlWriter.WriteString(bizmsgidr);//Example
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("MsgDefIdr");
                xmlWriter.WriteString("camt.027.001.04");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("BizSvc");
                xmlWriter.WriteString("ClaimNonReceiptV04");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("CreDt");
                xmlWriter.WriteString(DateTime.Now.ToString());//creation date of moe
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                //AppHdr End 

                //Document Start 
                xmlWriter.WriteStartElement("Document");
                // xmlWriter.WriteStartElement("Assignment");
                xmlWriter.WriteStartElement("ClmNonRct");
                xmlWriter.WriteStartElement("Assgnmt");
                xmlWriter.WriteStartElement("Id");//msgId
                xmlWriter.WriteString(bizmsgidr);//Uniquely identifies the case assignment(same as file name).eg//NR072101234567890120161020000001
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Assgnr");
                xmlWriter.WriteStartElement("Pty");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Othr");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteString("0123");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Assgne");
                xmlWriter.WriteStartElement("Pty");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Othr");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteString("RBIS");//check--2224
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();//end of assignment

                foreach (DataRow dr in dt.Rows)//data fro type MEIN create loop for other rows of same date "MEIN02/MEIN03"
                {
                    xmlWriter.WriteStartElement("Case");
                    xmlWriter.WriteStartElement("Id");
                    //_bll.get_Running_Sequence();
                    xmlWriter.WriteString("300123" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + _bll.get_Running_caseSequence());//example--30+0123+yyyymmdd+
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Cretr");
                    xmlWriter.WriteStartElement("Pty");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteStartElement("OrgId");
                    xmlWriter.WriteStartElement("Othr");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteString("01586701018");//account no of entry fixed for all
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SchmeNm");
                    xmlWriter.WriteStartElement("Prtry");
                    xmlWriter.WriteString("SGST");//tax id of the entry
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of cretr

                    xmlWriter.WriteStartElement("Undrlyg");
                    xmlWriter.WriteStartElement("Initn");
                    xmlWriter.WriteStartElement("OrgnlGrpInf");
                    xmlWriter.WriteStartElement("OrgnlMsgId");
                    xmlWriter.WriteString(dr["msgid"].ToString());//"ERV504GSTN0000222420160808000001");//Original msg id as reported in CN
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("OrgnlEndToEndId");
                    xmlWriter.WriteString(dr["endtoendid"].ToString());//"SBINCPIN987654");//original challan identification
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("OrgnlInstdAmt");
                    xmlWriter.WriteString(dr["amt"].ToString());//"1000");//Actual reported amount in challan
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ReqdExctnDt");
                    xmlWriter.WriteString("date");//date at which the challan amount was credited in RBI 
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of undrlyg

                    xmlWriter.WriteStartElement("CoverDtls");
                    xmlWriter.WriteStartElement("MssngCoverInd");
                    xmlWriter.WriteString("True");//Indicates whether or not the claim is related to a missing cover  
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("CoverCrrctn");
                    xmlWriter.WriteStartElement("InstdRmbrsmntAgt");
                    xmlWriter.WriteStartElement("FinInstnId");
                    xmlWriter.WriteStartElement("ClrSysMmbId");
                    xmlWriter.WriteStartElement("MMbId");
                    xmlWriter.WriteString("408");//Reciever Identification  
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of CoverDtls

                    xmlWriter.WriteStartElement("SplmtryData");
                    xmlWriter.WriteStartElement("CoverCrrctn");
                    xmlWriter.WriteStartElement("CoverCrrctn");
                    xmlWriter.WriteString("");//additional value like expected value // CIN Date 20161015
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of SplmtryData

                    xmlWriter.WriteEndElement();//end of case


                }//end of for
                 // xmlWriter.WriteEndElement();//end of document
                xmlWriter.WriteEndDocument();//end of requestPayload

                xmlWriter.Close();
                return (bizmsgidr);
            }
            Log("end of moe_CNR_xml");
            return bizmsgidr;
        }//missing cin and ETE Id

        protected static string moe_UTA_xml()
        {

            Log("moe_UTA_xml");
            System.Data.DataTable dt = new System.Data.DataTable();
            dt = _bll.getdata_UA(moe_dt, "");
            string bizmsgidr = string.Empty;
            if (dt.Rows.Count > 0)
            {
                bizmsgidr = "UA0123" + "001586701018" + DateTime.Now.Year.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + _bll.get_Running_Seq_bizmsgidr();//NR+0123+Treasury Account (or) Agency Bank Code (12)+DateTime.Now.Year.ToString()+DateTime.Now.Month.ToString()+DateTime.Now.Day.ToString()+_bll.get_Running_Sequence()

                XmlWriter xmlWriter = XmlWriter.Create(MOEXMLUPLOAD_path + "\\" + bizmsgidr + ".xml");

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("RequestPayload");
                //AppHdr Start
                xmlWriter.WriteStartElement("AppHdr");

                //FROM START
                xmlWriter.WriteStartElement("Fr");
                xmlWriter.WriteStartElement("FIId");
                xmlWriter.WriteStartElement("FinInstnId");
                xmlWriter.WriteStartElement("ClrSysMmbId");
                xmlWriter.WriteStartElement("MmbId");
                xmlWriter.WriteString("RBI");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteStartElement("OrgId");
                xmlWriter.WriteStartElement("Othr");
                xmlWriter.WriteStartElement("Id");
                xmlWriter.WriteString("0721");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                //FROM END

                //TO START
                xmlWriter.WriteStartElement("To");
                xmlWriter.WriteStartElement("FIId");
                xmlWriter.WriteStartElement("FinInstnId");
                xmlWriter.WriteStartElement("ClrSysMmbId");
                xmlWriter.WriteStartElement("MmbId");
                xmlWriter.WriteString("2224");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                //TO END

                xmlWriter.WriteStartElement("BizMsgIdr");
                xmlWriter.WriteString(bizmsgidr);//Example
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("MsgDefIdr");
                xmlWriter.WriteString("camt.027.001.04");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("BizSvc");
                xmlWriter.WriteString("ClaimNonReceiptV04");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("CreDt");
                xmlWriter.WriteString(DateTime.Now.ToString());//"2017 - 05 - 17T12: 00:57");2016-10-20T12:22:35
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                //AppHdr End 

                //Document Start 
                xmlWriter.WriteStartElement("Document");
                // xmlWriter.WriteStartElement("Assignment");


                foreach (DataRow dr in dt.Rows)//data fro type MEIN create loop for other rows of same date "MEIN02/MEIN03"
                {


                    xmlWriter.WriteStartElement("UblToApply");
                    xmlWriter.WriteStartElement("Assgnmt");
                    xmlWriter.WriteStartElement("MsgId");
                    xmlWriter.WriteString(bizmsgidr);////example
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Assgnr");
                    xmlWriter.WriteStartElement("Pty");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteStartElement("OrgId");
                    xmlWriter.WriteStartElement("Othr");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteString("0721");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Assgne");
                    xmlWriter.WriteStartElement("Pty");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteStartElement("OrgId");
                    xmlWriter.WriteStartElement("Othr");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteString("2224");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of assignment

                    xmlWriter.WriteStartElement("Case");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteString("300123" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + _bll.get_Running_UASequence());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Cretr");
                    xmlWriter.WriteStartElement("Pty");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteStartElement("OrgId");
                    xmlWriter.WriteStartElement("Othr");
                    xmlWriter.WriteStartElement("Id");
                    xmlWriter.WriteString("01586701018");//account no of entry
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SchmeNm");
                    xmlWriter.WriteStartElement("Prtry");
                    xmlWriter.WriteString("SGST");//tax id of the entry
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of cretr

                    xmlWriter.WriteStartElement("Undrlyg");
                    xmlWriter.WriteStartElement("Initn");
                    xmlWriter.WriteStartElement("OrgnlGrpInf");
                    xmlWriter.WriteStartElement("OrgnlMsgId");
                    xmlWriter.WriteString(dr["msgid"].ToString());//Original msg id as reported in CN
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("OrgnlEndToEndId");
                    xmlWriter.WriteString(dr["endtoendid"].ToString());//original challan identification
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("OrgnlInstdAmt");
                    xmlWriter.WriteString(dr["sgst_total"].ToString());//Actual reported amount in challan 
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ReqdExctnDt");
                    xmlWriter.WriteString("txdttm");//date at which the challan amount was credited in RBI //txttm
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of undrlyg

                    xmlWriter.WriteStartElement("Justfn");
                    if (string.Compare(dr["ERRORCODE"].ToString(), "8") == 0)
                    {

                        xmlWriter.WriteStartElement("MssngOrIncrrctlInf");
                        xmlWriter.WriteString("Reported Amount is GREATER than the Actual amount");//Indicates reason why the case is created 
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("IncrrctInf");
                        xmlWriter.WriteString("MEIN02");//indicates incorrect inf (MEIN01,MEIN02,MEIN03)
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("PssblDplctInstr");
                        xmlWriter.WriteString("false");//if possible entry is dublicate or not 
                        xmlWriter.WriteEndElement();

                    }
                    else if (string.Compare(dr["ERRORCODE"].ToString(), "9") == 0)
                    {

                        xmlWriter.WriteStartElement("MssngOrIncrrctlInf");
                        xmlWriter.WriteString("Reported Amount is LESS than the Actual amount");//Indicates reason why the case is created 
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("IncrrctInf");
                        xmlWriter.WriteString("MEIN03");//indicates incorrect inf (MEIN01,MEIN02,MEIN03)
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("PssblDplctInstr");
                        xmlWriter.WriteString("false");//if possible entry is dublicate or not 
                        xmlWriter.WriteEndElement();

                    }

                    else if (string.Compare(dr["ERRORCODE"].ToString(), "7") == 0)
                    {

                        xmlWriter.WriteStartElement("MssngOrIncrrctlInf");
                        xmlWriter.WriteString("Invalid End To End Id(CIN) ");//Indicates reason why the case is created 
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("IncrrctInf");
                        xmlWriter.WriteString("MEIN01");//indicates incorrect inf (MEIN01,MEIN02,MEIN03)
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("PssblDplctInstr");
                        xmlWriter.WriteString("true");//if possible entry is dublicate or not 
                        xmlWriter.WriteEndElement();

                    }




                    xmlWriter.WriteEndElement();//end of justfn


                    xmlWriter.WriteStartElement("SplmtryData");
                    xmlWriter.WriteStartElement("Envlp");
                    xmlWriter.WriteStartElement("AddtlTxInf");
                    xmlWriter.WriteString(dr["sgst_total"].ToString());//additional value like expected value  
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();//end of SplmtryData

                    xmlWriter.WriteEndElement();//end of case

                    xmlWriter.WriteEndElement();//end of UblToApply

                }
                xmlWriter.WriteEndDocument();//end of requestPayload


                xmlWriter.Close();
                return (bizmsgidr);
            }

            Log("end of moe_UTA_xml");
            return bizmsgidr;
        }//used

    }
}

