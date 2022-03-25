using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treasury_MOE
{
    class MOE_BLL
    {
        MOE_DAL _dal = new MOE_DAL();

        public int update_moeerror(string moe_dt, string CNRid, string UTAid)//used
        {
            return (_dal.update_moeerror(moe_dt, CNRid, UTAid));
        }

        public DataSet get_moe_date()//used
        {
            return (_dal.get_moe_date());
        }
        public DataTable getdata_UA(string moe_dt, string type)//used
        {
            return (_dal.getdata_UA(moe_dt, type));
        }

        public DataTable getdata_CNR(string moe_dt, string type)//used
        {
            return (_dal.getdata_CNR(moe_dt, type));
        }

        public DataSet select_moe_data(string dt)
        {
            return _dal.select_moe_data(dt);
        }

        public DataTable get_master_data()
        {
            return _dal.get_master_data();
        }

        public string get_Running_Seq_bizmsgidr()
        {
            return _dal.get_Running_Seq_bizmsgidr();
        }
        public string get_Running_caseSequence()
        {
            return (_dal.get_Running_caseSequence());
        }
        public string get_Running_UASequence()
        {
            return (_dal.get_Running_UASequence());
        }
    }
}
