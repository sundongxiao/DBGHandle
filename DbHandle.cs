using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDSCommon.Interfaces;
using YDSCommon.Models;
using YDSCommon;
using System.Windows;

namespace DBGHandle
{
    public  class DbHandle:IDbHandle
    {
        public DbHandle()
        { }
        public TestHandleResult TestStart(string sn,  Action<string> addLog)
        {
            


            addLog("进入数据对接");
            var result = new TestHandleResult();
            var dbg= new WebService1.WebService1();
            addLog($"sn:{sn}");

            var resultXML = dbg.DataAdd(sn, "","");
          
            if (resultXML .ToString () == "")
            {
                result.Code = Enums.TestHandleCode.Success;
            }
            else
            {
                string resultstr = "";
                if (resultXML.ToString() != "数据传输错误")
                {
                   
                    if (resultXML.ToString().Contains("0"))
                    {
                        resultstr = result + "pcba重复;";
                    }
                    if (resultXML.ToString().Contains("1"))
                    {
                        resultstr = result + "客户SN重复;";
                    }
                    if (resultXML.ToString().Contains("2"))
                    {
                        resultstr = result + "整机SN重复;";
                    }
                    if (resultXML.ToString().Contains("3"))
                    {
                        resultstr = result + "iccid重复;";
                    }
                    if (resultXML.ToString().Contains("4"))
                    {
                        resultstr = result + "imei重复;";
                    }
                    if (resultXML.ToString().Contains("5"))
                    {
                        resultstr = result + "imsi重复;";
                    }
                    result.Code = Enums.TestHandleCode.Fail;
                    addLog(resultstr);
                    //throw new Exception(resultstr);
                }
                else
                {
                    result.Code = Enums.TestHandleCode.Fail;
                    addLog(resultstr);
                    //throw new Exception(resultXML.ToString());
                  
                }
               

               
            }
            return result;
        }
    }
}
