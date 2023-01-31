using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp
{
    public class Order
    {
        public string Order_code { get; set; }
        public System.DateTime Creation_Date { get; set; }
        public string Exception_Message { get; set; }
        public string User_Steps { get; set; }
        public int Id_User { get; set; }
        public int Id_Status { get; set; }
        public Nullable<System.DateTime> Date_of_Accepted { get; set; }
    }
}
