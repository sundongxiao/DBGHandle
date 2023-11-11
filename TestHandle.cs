using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDSCommon.Interfaces;
using YDSCommon.Models;
using YDSCommon;
using System.Windows;
using System.Xml;
using System.Net;

namespace DBGHandle
{
    public class TestHandle : ITestHandle
    {
        public TestHandle() { }

        /// <summary>
        /// MES校验，拿到镭雕码后调用去校验
        /// </summary>
        /// <param name="sn">PCBA镭雕码</param>
        /// <param name="dataDict">测试过程保存的数据，如 Final_Test_Guest_SN="L5047DLXXXXXXX"</param>
        /// <param name="addLog">日志打印</param>
        /// <returns></returns>
        public TestHandleResult TestStart(string sn, Dictionary<string, string> dataDict, Action<string> addLog)
        {
            addLog("进入Mes对接");//对应日志打印 进入Mes对接
            DBGSN = sn;
            //***********************************************镭雕码还原**************************************************************
            /*镭雕码 20位不用还原，16、17位需按截取规则还原
              根据镭雕码前三位判断进行还原
              比如进来的是17位的 33011303PB0600567（这个把H8和A截取掉了）
              然后需要把H8和A加回去
            */
            if (DBGSN.Length == 17)//长度17
            {
                //       H8      330                    A    1303PB0600567
                DBGSN = "H8" + DBGSN.Substring(0, 3) + "J" + DBGSN.Substring(3);// 
            }
            if (DBGSN.Length == 16)//长度16
            {
                //16的截取的、方式 去掉H8 ，A和倒数第五位    比如    H8 330 A 1303PB060 0 0567 去H8 A 0后得 330 1303PB060 0567
                //然后把它还原成20为如下
                //               H8      330                    A              1303PB060                     0                 0567
                DBGSN = DBGSN = "H8" + DBGSN.Substring(0, 3) + "A" + DBGSN.Substring(3, DBGSN.Length - 7) + "0" + DBGSN.Substring(12, 4);//
            }
            //**********************************************镭雕码还原***************************************************************
            addLog($"DBGSN:{DBGSN}");//软件界面打印加回去的镭雕码
            #region 不用修改
            var result = new TestHandleResult();
            if (string.IsNullOrEmpty(DBGSN) || string.IsNullOrEmpty(dataDict.GetDictValue("WorkUnitCode")))
            {
                addLog("DBGSN为空，检查镭雕码是否已扫描，镭雕码空：" + string.IsNullOrEmpty(sn).ToString());
                addLog(dataDict.GetDictValue("WorkUnitCode"));
                return result;
            }
            var dbg = new DBGWebService();
            addLog($"WorkUnitCode:{dataDict.GetDictValue("WorkUnitCode")}");
            result.Code = Enums.TestHandleCode.Success;
            //GP12站 一开始不校验
            if (dataDict.GetDictValue("WorkUnitCode") != "C5_GP12")
            {
                var resultXML = dbg.TestIn_SNCheck(DBGSN, dataDict.GetDictValue("WorkUnitCode"));
                addLog("SNCHeck完成");
                addLog($"TestIn_SNCheck 返回ErrorCode:{resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText.ToString()} ErrorMsg:" + resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText);
                if (Convert.ToInt32(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText.ToString()) == 0)
                {
                    result.Code = Enums.TestHandleCode.Success;
                }
                else
                {
                    result.Code = Enums.TestHandleCode.Fail;
                    throw new Exception(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText.ToString());
                }
            }
            return result;
            #endregion
        }
        /// <summary>
        /// 测试失败时候执行这个方法
        /// </summary>
        /// <param name="sn"> 镭雕码</param>
        /// <param name="dataDict">测试数据的字典</param>
        /// <param name="addLog">日志打印</param>
        /// <returns></returns>
        public TestHandleResult TestFail(string sn, Dictionary<string, string> dataDict, Action<string> addLog)
        {
            addLog("前面有测试项未通过");
            DBGSN = sn;
            //**********************************************镭雕码还原***************************************************************
            /*镭雕码 20位不用还原，16、17位需按截取规则还原
               根据镭雕码前三位判断进行还原
               比如进来的是17位的 33011303PB0600567（这个把H8和A截取掉了）
               然后需要把H8和A加回去
             */
            if (DBGSN.Length == 17)//长度17位进入还原，如果16位的改一下17成16
            {
                //       H8      330                    A    1303PB0600567
                DBGSN = "H8" + DBGSN.Substring(0, 3) + "J" + DBGSN.Substring(3);

            }
            if (DBGSN.Length == 16)//长度16
            {
                //16的截取的、方式 去掉H8 ，A和倒数第五位    比如    H8 330 A 1303PB060 0 0567 去H8 A 0后得 330 1303PB060 0567
                //然后把它还原成20为如下
                //               H8      330                    A              1303PB060                     0                 0567
                DBGSN = DBGSN = "H8" + DBGSN.Substring(0, 3) + "A" + DBGSN.Substring(3, DBGSN.Length - 7) + "0" + DBGSN.Substring(12, 4);//
            }
            //**********************************************镭雕码还原***************************************************************
            addLog($"DBGSN:{DBGSN}");//软件界面打印加回去的镭雕码
            #region  不用修改
            var result = new TestHandleResult();
            if (string.IsNullOrEmpty(DBGSN))
            {
                return result;
            }
            var dbg = new DBGWebService();
            var failReason = "";
            failReason = "测试ID: " + dataDict.GetDictValue("测试ID") + " 测试名: " + dataDict.GetDictValue("测试名") + " 测试结果: " + dataDict.GetDictValue("测试结果");
            addLog($"TestFailInfo:{failReason}");

            var resultXML = dbg.TestOut_ResultCollect(DBGSN, dataDict.GetDictValue("WorkUnitCode"), "FAIL", failReason, Utility.GetMacByNetworkInterface(), System.Environment.UserName);

            addLog($"请求测试失败结果 TestOut_ResultCollect 返回ErrorCode:{resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText} ErrorMsg:" + resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText);
            if (Convert.ToInt32(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText.ToString()) == 0)
            {
                result.Code = Enums.TestHandleCode.Success;
            }
            else
            {
                result.Code = Enums.TestHandleCode.Fail;
                var errorMsg = resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText;
                throw new Exception(errorMsg);
            }
            return result;
            #endregion
        }
        #region 属性
        string DBGSN;
        string Version;
        string ISP_INFO;
        string Sensor_Info;
        string SEGMENT1;
        string SEGMENT2;
        string SEGMENT3;
        string SEGMENT4;
        string SEGMENT5;
        string SEGMENT6;
        string SEGMENT7;
        string SEGMENT8;
        string SEGMENT9;
        string SEGMENT10;
        string user;
        #endregion
        /// <summary>
        /// 测试成功后执行，包含上传数据到MES
        /// </summary>
        /// <param name="sn">镭雕码</param>
        /// <param name="dataDict">数据字典</param>
        /// <param name="addLog">日志打印</param>
        /// <returns></returns>
        public TestHandleResult TestSuccess(string sn, Dictionary<string, string> dataDict, Action<string> addLog)
        {
            #region 数据初始化/获取
            Version = "";
            ISP_INFO = "";
            Sensor_Info = "";
            SEGMENT1 = "test";
            SEGMENT2 = "test";
            SEGMENT3 = "test";
            SEGMENT4 = "test";
            SEGMENT5 = "test";
            SEGMENT6 = "test";
            SEGMENT7 = "test";
            SEGMENT8 = "test";
            SEGMENT9 = "test";
            SEGMENT10 = "";
            user = "";
            var scancode = "";
            #endregion
            var result = new TestHandleResult();
            DBGSN = sn;
            //***********************************************镭雕码还原**************************************************************
            /*镭雕码 20位不用还原，16、17位需按截取规则还原
              根据镭雕码前三位判断进行还原
              比如进来的是17位的 33011303PB0600567（这个把H8和A截取掉了）
              然后需要把H8和A加回去
            */
            if (DBGSN.Length == 17)//长度17位进入还原，如果16位的改一下17成16
            {
                //       H8      330                    A    1303PB0600567
                DBGSN = "H8" + DBGSN.Substring(0, 3) + "J" + DBGSN.Substring(3);
            }
            if (DBGSN.Length == 16)//长度16
            {
                //16的截取的、方式 去掉H8 ，A和倒数第五位    比如    H8 330 A 1303PB060 0 0567 去H8 A 0后得 330 1303PB060 0567
                //然后把它还原成20为如下
                //               H8      330                    A              1303PB060                     0                 0567
                DBGSN = DBGSN = "H8" + DBGSN.Substring(0, 3) + "A" + DBGSN.Substring(3, DBGSN.Length - 7) + "0" + DBGSN.Substring(12, 4);//
            }
            //************************************************镭雕码还原*************************************************************
            addLog($"校验PCBA:{DBGSN}");//日志打印  

            scancode = dataDict.GetDictValue("PCBANumber");//把扫描框的值给到变量 scancode
            var dbg = new DBGWebService();
            System.Xml.XmlNode resultXML = null;
            if (dataDict.GetDictValue("WorkUnitCode") == "FCT#2th")//成品站数据上传
            {
                if (!string.IsNullOrEmpty(dataDict.GetDictValue("Final_Test_Guest_SN")) || !string.IsNullOrEmpty(dataDict.GetDictValue("Final_Test_YDS_SN")) || !string.IsNullOrEmpty(dataDict.GetDictValue("Final_SN")))
                {
                    SEGMENT10 = dataDict.GetDictValue("Final_Test_Guest_SN");
                    if (DBGSN.Contains("ZD2") || DBGSN.Contains("D18") || DBGSN.Contains("E31") || DBGSN.Contains("E40") || DBGSN.Contains("EV3") || DBGSN.Contains("GSE") || DBGSN.Contains("GEV"))
                    {
                        SEGMENT10 = dataDict.GetDictValue("Final_Test_YDS_SN");
                    }
                    else if (DBGSN.Contains("Z02"))
                    {
                        //SEGMENT1 = dataDict.GetDictValue("Final_Test_IMEI");
                        SEGMENT1 = dataDict.GetDictValue("Final_Test_Guest_SN");//dataDict.GetDictValue("Final_Test_QRCode");
                    }
                    else if (DBGSN.Contains("B4V1"))
                    {
                        SEGMENT10 = dataDict.GetDictValue("Final_SN");
                    }
                    else if (DBGSN.Contains("E12") || DBGSN.Contains("402"))
                    {
                        SEGMENT1 = dataDict.GetDictValue("Final_Test_IMEI");
                        SEGMENT10 = dataDict.GetDictValue("Final_Test_QRCode");
                    }
                    addLog("上传成品标签内容:" + SEGMENT10);
                }
                else
                {
                    addLog("上传成品标签内容为空");
                }
                //************************************************上传数据到MES*************************************************************
                addLog("上传测试数据");
                if (DBGSN.Contains("E12"))//EP12
                {
                    addLog("%EP12_UploadInfo%");
                    resultXML = dbg.UploadInfo(DBGSN, "", "", " ", SEGMENT1, SEGMENT10, DBGSN, DBGSN, "", "", "", "", "", "", SEGMENT1);
                }
                else if (DBGSN.Contains("Z02"))//EP12Z02
                {
                    addLog("%EP12-Z02_UploadInfo%");
                    resultXML = dbg.UploadInfo(DBGSN, "", "", " ", SEGMENT1, "", "", "", "", "", "", "", "", SEGMENT1, "");
                }
                else
                {
                    addLog("******");//一般情况上传镭雕码 和 10字段
                    resultXML = dbg.UploadInfo(DBGSN, "", "", " ", "", "", "", "", "", "", "", "", "", SEGMENT10, "");
                }
                //************************************************上传数据到MES*************************************************************              
                if (Convert.ToInt32(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText.ToString()) == 0)
                {
                    result.Code = Enums.TestHandleCode.Success;
                    addLog("数据上传成功，feild1:" + DBGSN + " feid10:" + SEGMENT10);//
                }
                else
                {
                    result.Code = Enums.TestHandleCode.Fail;
                    addLog("数据上传失败");//
                    throw new Exception(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText);
                }
            }

            bool result1 = false;
            try
            {
                if (dataDict.GetDictValue("WorkUnitCode") == "FCT#2th")//成品站过站
                {
                    if (DBGSN.Contains("D18") || DBGSN.Contains("E31") || DBGSN.Contains("E40") || DBGSN.Contains("EV3") || DBGSN.Contains("GSE") || DBGSN.Contains("GEV"))
                    {
                        resultXML = dbg.TestOut_ResultCollect(dataDict.GetDictValue("Final_Test_YDS_SN"), dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                    }
                    else if (DBGSN.Contains("Z02"))
                    {
                        addLog("Z02-PASS:" + SEGMENT10);
                        resultXML = dbg.TestOut_ResultCollect(SEGMENT10, dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                        //resultXML = dbg.TestOut_ResultCollect(dataDict.GetDictValue("Final_Test_QRCode"), dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                    }
                    //else if (DBGSN.Contains("402"))//2023.9.27注释 调试E300N
                    //{
                    //    resultXML = dbg.TestOut_ResultCollect(DBGSN, dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                    //}
                    else if (DBGSN.Contains("B4V1"))
                    {
                        resultXML = dbg.TestOut_ResultCollect(dataDict.GetDictValue("Final_SN"), dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                    }
                    else
                    {
                        resultXML = dbg.TestOut_ResultCollect(SEGMENT10, dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);
                    }

                }
                else if (dataDict.GetDictValue("WorkUnitCode") == "C5_GP12")//GP12站过站
                {
                    addLog("GP12站位校验");
                    SEGMENT10 = dataDict.GetDictValue("Final_Test_Guest_SN");
                    if (DBGSN.Contains("Z02"))
                    {
                        addLog("*Z02-GP12*");
                        SEGMENT10 = scancode;
                        //SEGMENT10 = dataDict.GetDictValue("Final_Test_QRCode");
                    }
                    else if (DBGSN.Contains("L50579L"))
                    {

                        SEGMENT10 = dataDict.GetDictValue("PCBANumber");
                        addLog("JH11图牛：" + SEGMENT10);
                    }
                    else if (DBGSN.Contains("B4V1"))
                    {
                        SEGMENT10 = dataDict.GetDictValue("Final_SN");
                    }
                    else if (DBGSN.Contains("ZD2") || DBGSN.Contains("D18") || DBGSN.Contains("E31") || DBGSN.Contains("E40") || DBGSN.Contains("EV3") || DBGSN.Contains("GSE") || DBGSN.Contains("GEV"))
                    {
                        addLog("*Final_Test_YDS_SN*");
                        SEGMENT10 = dataDict.GetDictValue("Final_Test_YDS_SN");
                    }
                    else if (DBGSN.Contains("L50357DL"))
                    {
                        addLog("*JH11低配*");
                        SEGMENT10 = scancode;
                    }
                    else if (DBGSN.Contains("L50437DL"))
                    {
                        addLog("*JX65PHEV*");
                        SEGMENT10 = scancode;
                    }
                    else if (DBGSN.Contains("8ZD30N"))
                    {

                        addLog("*E300N*");
                        SEGMENT10 = scancode.Substring(0, 15);// dataDict.GetDictValue("Final_Test_PCBA");
                    }
                    else if (scancode.Contains("9YD"))
                    {
                        addLog("*EP12-GP12*");
                        SEGMENT10 = scancode.Substring(scancode.Length - 15, 15);
                    }
                    addLog("%DebugParams%" + SEGMENT10 + "%" + dataDict.GetDictValue("WorkUnitCode") + "%");
                    resultXML = dbg.TestOut_ResultCollect(SEGMENT10, dataDict.GetDictValue("WorkUnitCode"), "PASS", "", "", "");
                }
                else//烧录与半成品过站
                {
                    resultXML = dbg.TestOut_ResultCollect(DBGSN, dataDict.GetDictValue("WorkUnitCode"), "PASS", "", Utility.GetMacByNetworkInterface(), System.Environment.UserName);

                }
                result1 = true;
                addLog($"调用TestOut_ResultCollect,工站：" + dataDict.GetDictValue("WorkUnitCode"));
                addLog($"请求测试成功结果 TestOut_ResultCollect 返回ErrorCode:{resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText} ErrorMsg:" + resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText);
            }
            catch { }

            if (result1)
            {
                if (Convert.ToInt32(resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorCode").InnerText.ToString()) == 0)
                {
                    result.Code = Enums.TestHandleCode.Success;
                }
                else
                {
                    result.Code = Enums.TestHandleCode.Fail;
                    addLog("错误：" + resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText.ToString());
                    // resultXML.SelectSingleNode("Message").SelectSingleNode("ErrorMsg").InnerText.ToString()
                    // throw new Exception(resultXML.SelectNodes("DBGWEBSERVICE/Message/ErrorMsg")[0].InnerText);
                }
            }
            addLog($"测试结果：result.Code：" + result.Code);

            return result;
        }

    }
}
