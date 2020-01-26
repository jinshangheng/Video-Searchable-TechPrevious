using System;
using System.Collections.Generic;
using System.Text;

namespace VSTP.Subtitles
{
    public class XunFeiResult
    {
        public int Ok { get; set; }
        public int Err_no { get; set; }
        public string Failed { get; set; }
        public string Data { get; set; }
        public string Task_id { get; set; }
    }

    public class StatusResult
    {
        public string Desc { get; set; }
        public int Status { get; set; }
    }
}
