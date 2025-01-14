﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using DAL;
using Models;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksHelper
{
    public class KCJDB800AutoDrawing : IAutoDrawing
    {
        KCJDB800Service objKCJDB800Service = new KCJDB800Service();
        public void AutoDrawing(SldWorks swApp, ModuleTree tree, string projectPath)
        {
            //创建项目模型存放地址
            string itemPath = projectPath + @"\" + tree.Module + "-" + tree.CategoryName;
            if (!Directory.Exists(itemPath))
            {
                Directory.CreateDirectory(itemPath);
            }
            else
            {
                Common.ShowMsg show = new ShowMsg();
                DialogResult result = show.ShowMessageBoxTimeout("模型文件夹" + itemPath + "存在，如果之前pack已经执行过，将不执行pack过程而是直接修改模型，如果要中断作图点击YES，继续作图请点击No或者3s后窗口会自动消失", "提示信息", MessageBoxButtons.YesNo, 3000);
                if (result == DialogResult.Yes) return;
            }
            //Pack的后缀
            string suffix = tree.Module + "-" + tree.ODPNo.Substring(tree.ODPNo.Length - 6);
            //判断文件是否存在，如果存在将不执行pack，如果不存在则执行pack
            //packango后需要接收打包完成的地址，参数为后缀
            string packedAssyPath = itemPath + @"\" + tree.CategoryName.ToLower() + "_" + suffix + ".sldasm";
            if (!File.Exists(packedAssyPath)) packedAssyPath = CommonFunc.PackAndGoFunc(suffix, swApp, tree.ModelPath, itemPath);

            //查询参数
            KCJDB800 item = (KCJDB800)objKCJDB800Service.GetModelByModuleTreeId(tree.ModuleTreeId.ToString());

            swApp.CommandInProgress = true; //告诉SolidWorks，现在是用外部程序调用命令
            int warnings = 0;
            int errors = 0;
            suffix = "_" + suffix;//后缀
            ModelDoc2 swModel = default(ModelDoc2);
            ModelDoc2 swPart = default(ModelDoc2);
            AssemblyDoc swAssy = default(AssemblyDoc);
            Component2 swComp;
            Feature swFeat = default(Feature);
            object configNames = null;
            ModelDocExtension swModelDocExt = default(ModelDocExtension);
            bool status = false;
            string compReName = string.Empty;
            //打开Pack后的模型
            swModel = swApp.OpenDoc6(packedAssyPath, (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref errors, ref warnings) as ModelDoc2;
            swAssy = swModel as AssemblyDoc;//装配体
            string assyName = swModel.GetTitle().Substring(0, swModel.GetTitle().Length - 7);//获取装配体名称
            swModelDocExt = (ModelDocExtension)swModel.Extension;
            //打开装配体后必须重建，使Pack后的零件名都更新到带后缀的状态，否则程序出错
            swModel.ForceRebuild3(true);
            //TopOnly参数设置成true，只重建顶层，不重建零件内部
            /*注意SolidWorks单位是m，计算是应当/1000m
             * 整形与整形运算得出的结果仍然时整形，1640 / 1000m结果为0，因此必须将其中一个转化成decimal型，使用后缀m就可以了
             * (int)不进行四舍五入，Convert.ToInt32会四舍五入
            */
            //-----------计算中间值，----------
            int fcNo = (int)((item.Length - item.FCSideLeft - item.FCSideRight) / 499m) - item.FCBlindNo;

            try
            {
                //----------Top Level----------
                //判断FC数量，FC侧板长度
                if (item.FCBlindNo > 0)
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0107[BP-500]{500}-3"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0107[BP-500]{500}-7"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩
                    swFeat = swAssy.FeatureByName("LocalLPattern4");
                    swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                    swModel.Parameter("D1@LocalLPattern4").SystemValue = item.FCBlindNo; //D1阵列数量,D3阵列距离
                    swModel.Parameter("D1@Distance31").SystemValue = item.FCSideLeft / 1000m;
                }
                else
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0107[BP-500]{500}-3"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0107[BP-500]{500}-7"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩
                    swFeat = swAssy.FeatureByName("LocalLPattern4");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                }
                //判断FC/KSA
                if (item.FCType == "KSA")
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "5202040401-1"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "5202040401-7"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern5");
                    swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                    swModel.Parameter("D1@LocalLPattern5").SystemValue = fcNo; //D1阵列数量,D3阵列距离
                    swModel.Parameter("D1@Distance34").SystemValue = (item.FCSideLeft + 500m * item.FCBlindNo) / 1000m;
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "KCJ FC FILTER-1"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "KCJ FC FILTER-9"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern3");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                }
                else
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "KCJ FC FILTER-1"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "KCJ FC FILTER-9"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern3");
                    swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                    swModel.Parameter("D1@LocalLPattern3").SystemValue = fcNo; //D1阵列数量,D3阵列距离
                    swModel.Parameter("D1@Distance33").SystemValue = (item.FCSideLeft + 500m * item.FCBlindNo) / 1000m;
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "5202040401-1"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "5202040401-7"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern5");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                }
                //----------油网侧板----------
                switch (item.FCSide)
                {
                    case "LEFT":
                        //重命名装配体内部
                        if (item.FCType == "KSA") compReName = "FNCE0108[BP-" + tree.Module + "]{" + (int)(item.FCSideLeft + fcNo * 2.5m) + "}";
                        else compReName = "FNCE0108[BP-" + tree.Module + "]{" + (int)(item.FCSideLeft - 4m) + "}";
                        status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-3") + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        if (status) swModelDocExt.RenameDocument(compReName);
                        swModel.ClearSelection2(true);
                        status = swModelDocExt.SelectByID2(compReName + "-3" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-3");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                            swPart = swComp.GetModelDoc2(); //打开零件
                            if (item.FCType == "KSA") swPart.Parameter("D2@草图1").SystemValue = (item.FCSideLeft + fcNo * 2.5m) / 1000m;
                            else swPart.Parameter("D2@草图1").SystemValue = (item.FCSideLeft - 4m) / 1000m;
                        }
                        status = swModelDocExt.SelectByID2(compReName + "-4" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-4");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                        }
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-3"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-4"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        break;
                    case "RIGHT":
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-3"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-4"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        //重命名装配体内部
                        if (item.FCType == "KSA") compReName = "FNCE0109[BP-" + tree.Module + "]{" + (int)(item.FCSideRight + fcNo * 2.5m) + "}";
                        else compReName = "FNCE0109[BP-" + tree.Module + "]{" + (int)(item.FCSideRight - 4m) + "}";
                        status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-3") + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        if (status) swModelDocExt.RenameDocument(compReName);
                        swModel.ClearSelection2(true);
                        status = swModelDocExt.SelectByID2(compReName + "-3" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-3");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                            swPart = swComp.GetModelDoc2(); //打开零件
                            if (item.FCType == "KSA") swPart.Parameter("D2@草图1").SystemValue = (item.FCSideRight + fcNo * 2.5m) / 1000m;
                            else swPart.Parameter("D2@草图1").SystemValue = (item.FCSideRight - 4m) / 1000m;
                        }
                        status = swModelDocExt.SelectByID2(compReName + "-4" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-4");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                        }
                        break;
                    case "BOTH":
                        //重命名装配体内部
                        if (item.FCType == "KSA") compReName = "FNCE0108[BP-" + tree.Module + ".1]{" + (int)(item.FCSideLeft + fcNo * 1.25m) + "}";
                        else compReName = "FNCE0108[BP-" + tree.Module + "]{" + (int)(item.FCSideLeft - 2m) + "}";
                        status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-3") + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        if (status) swModelDocExt.RenameDocument(compReName);
                        swModel.ClearSelection2(true);
                        status = swModelDocExt.SelectByID2(compReName + "-3" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-3");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                            swPart = swComp.GetModelDoc2(); //打开零件
                            if (item.FCType == "KSA") swPart.Parameter("D2@草图1").SystemValue = (item.FCSideLeft + fcNo * 1.25m) / 1000m;
                            else swPart.Parameter("D2@草图1").SystemValue = (item.FCSideLeft - 2m) / 1000m;
                        }
                        status = swModelDocExt.SelectByID2(compReName + "-4" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-4");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                        }
                        //重命名装配体内部
                        if (item.FCType == "KSA") compReName = "FNCE0109[BP-" + tree.Module + ".2]{" + (int)(item.FCSideRight + fcNo * 1.25m) + "}";
                        else compReName = "FNCE0109[BP-" + tree.Module + "]{" + (int)(item.FCSideRight - 2m) + "}";
                        status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-3") + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        if (status) swModelDocExt.RenameDocument(compReName);
                        swModel.ClearSelection2(true);
                        status = swModelDocExt.SelectByID2(compReName + "-3" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-3");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                            swPart = swComp.GetModelDoc2(); //打开零件
                            if (item.FCType == "KSA") swPart.Parameter("D2@草图1").SystemValue = (item.FCSideRight + fcNo * 1.25m) / 1000m;
                            else swPart.Parameter("D2@草图1").SystemValue = (item.FCSideRight - 2m) / 1000m;
                        }
                        status = swModelDocExt.SelectByID2(compReName + "-4" + "@" + assyName, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swModel.ClearSelection2(true);
                        if (status)
                        {
                            swComp = swAssy.GetComponentByName(compReName + "-4");
                            swComp.SetSuppression2(2); //2解压缩，0压缩.
                        }
                        break;
                    default:
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-3"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0108[BP-]{}-4"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-3"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0109[BP-]{}-4"));
                        swComp.SetSuppression2(0); //2解压缩，0压缩.
                        break;
                }
                //----------日本项目需要压缩零件----------
                if (item.Japan == "YES")
                {
                    //吊装垫片
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0070-9"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern1");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    //排风脖颈
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "EXSPIGOT-1"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    //排风滑门
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "EXDOOR-1"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    //排风腔
                    //重命名装配体内部
                    compReName = "FNCE0115[KCJDB800-" + tree.Module + "]{" + (int)item.Length + "}";
                    status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0115-1") + "@" + assyName,
                        "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    if (status) swModelDocExt.RenameDocument(compReName);
                    swModel.ClearSelection2(true);
                    status = swModelDocExt.SelectByID2(compReName + "-1" + "@" + assyName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swModel.ClearSelection2(true);
                    if (status)
                    {
                        swComp = swAssy.GetComponentByName(compReName + "-1");
                        swPart = swComp.GetModelDoc2(); //打开零件
                        swPart.Parameter("D1@Linear austragen1").SystemValue = item.Length / 1000m;
                        swFeat = swComp.FeatureByName("EX");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("Cut-Extrude4");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANSULSIDE RIGHT");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANSULSIDE LEFT");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANDTEC1");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANDTEC2");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANDTEC3");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANDTEC4");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("ANDTEC5");
                        swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        if (item.MARVEL == "YES")
                        {
                            swFeat = swComp.FeatureByName("MA-NTC");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("MA-TAB");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                        }
                        else
                        {
                            swFeat = swComp.FeatureByName("MA-NTC");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("MA-TAB");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩 
                        }
                    }
                }
                else
                {
                    //吊装垫片
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0070-9"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swFeat = swAssy.FeatureByName("LocalLPattern1");
                    swFeat.SetSuppression2(2, 2, configNames); //参数1：1解压，0压缩
                    //排风脖颈
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "EXSPIGOT-1"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0019-1"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D2@基体-法兰1").SystemValue = (item.ExLength + 50m) / 1000m;
                    swPart.Parameter("D3@草图1").SystemValue = item.ExHeight / 1000m;
                    swFeat = swComp.FeatureByName("ANSUL");
                    if (item.ANSUL == "YES") swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                    else swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0020-1"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D2@基体-法兰1").SystemValue = (item.ExLength + 50m) / 1000m;
                    swPart.Parameter("D3@草图1").SystemValue = item.ExHeight / 1000m;
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0047-1"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D2@基体-法兰1").SystemValue = item.ExWidth / 1000m;
                    swPart.Parameter("D3@草图1").SystemValue = item.ExHeight / 1000m;
                    swFeat = swComp.FeatureByName("ANDTEC");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0048-2"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D2@基体-法兰1").SystemValue = item.ExWidth / 1000m;
                    swPart.Parameter("D3@草图1").SystemValue = item.ExHeight / 1000m;
                    swFeat = swComp.FeatureByName("ANDTEC");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    //排风滑门
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "EXDOOR-1"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D1@Distance3").SystemValue = (item.ExWidth + 20m) / 1000m;
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0018-1"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D2@Base-Flange1").SystemValue = (item.ExLength * 2m + 100m) / 1000m;
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0013-1"));
                    swPart = swComp.GetModelDoc2(); //打开零件
                    swPart.Parameter("D1@Sketch1").SystemValue = (item.ExLength / 2m + 10m) / 1000m;
                    swPart.Parameter("D2@Sketch1").SystemValue = (item.ExWidth + 20m) / 1000m;
                    //排风腔
                    //重命名装配体内部
                    compReName = "FNCE0115[KCJDB800-" + tree.Module + "]{" + (int)item.Length + "}";
                    status = swModelDocExt.SelectByID2(CommonFunc.AddSuffix(suffix, "FNCE0115-1") + "@" + assyName,
                        "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    if (status) swModelDocExt.RenameDocument(compReName);
                    swModel.ClearSelection2(true);
                    status = swModelDocExt.SelectByID2(compReName + "-1" + "@" + assyName, "COMPONENT", 0, 0, 0, false,
                        0, null, 0);
                    swModel.ClearSelection2(true);
                    if (status)
                    {
                        swComp = swAssy.GetComponentByName(compReName + "-1");
                        swPart = swComp.GetModelDoc2(); //打开零件
                        swPart.Parameter("D1@Linear austragen1").SystemValue = item.Length / 1000m;
                        swFeat = swComp.FeatureByName("EX");
                        swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                        swFeat = swComp.FeatureByName("Cut-Extrude4");
                        swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                        swPart.Parameter("D4@Sketch1").SystemValue = item.ExRightDis / 1000m;
                        swPart.Parameter("D2@Sketch1").SystemValue = item.ExLength / 1000m;
                        swPart.Parameter("D1@Sketch1").SystemValue = item.ExWidth / 1000m;
                        if (item.ANSUL == "YES")
                        {
                            //侧喷
                            if (item.ANSide == "LEFT")
                            {
                                swFeat = swComp.FeatureByName("ANSULSIDE RIGHT");
                                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                                swFeat = swComp.FeatureByName("ANSULSIDE LEFT");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                            }
                            else if (item.ANSide == "RIGHT")
                            {
                                swFeat = swComp.FeatureByName("ANSULSIDE RIGHT");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swFeat = swComp.FeatureByName("ANSULSIDE LEFT");
                                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            }
                            else
                            {
                                swFeat = swComp.FeatureByName("ANSULSIDE RIGHT");
                                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                                swFeat = swComp.FeatureByName("ANSULSIDE LEFT");
                                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            }
                            //探测器
                            swFeat = swComp.FeatureByName("ANDTEC1");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC2");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC3");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC4");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC5");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            if (item.ANDetectorNo > 0)
                            {
                                swFeat = swComp.FeatureByName("ANDTEC1");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swPart.Parameter("D2@Sketch9").SystemValue = item.ANDetectorDis1 / 1000m;
                                if (item.ANDetectorEnd == "RIGHT" ||
                                    (item.ANDetectorEnd == "LEFT" && item.ANDetectorNo == 1))
                                    swPart.Parameter("D1@Sketch9").SystemValue = 195m / 1000m;
                                else swPart.Parameter("D1@Sketch9").SystemValue = 175m / 1000m;
                            }
                            if (item.ANDetectorNo > 1)
                            {
                                swFeat = swComp.FeatureByName("ANDTEC2");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swPart.Parameter("D1@Sketch10").SystemValue = item.ANDetectorDis2 / 1000m;
                                if (item.ANDetectorEnd == "LEFT" && item.ANDetectorNo == 2)
                                    swPart.Parameter("D2@Sketch10").SystemValue = 195m / 1000m;
                                else swPart.Parameter("D2@Sketch10").SystemValue = 175m / 1000m;
                            }
                            if (item.ANDetectorNo > 2)
                            {
                                swFeat = swComp.FeatureByName("ANDTEC3");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swPart.Parameter("D3@Sketch11").SystemValue = item.ANDetectorDis3 / 1000m;
                                if (item.ANDetectorEnd == "LEFT" && item.ANDetectorNo == 3)
                                    swPart.Parameter("D1@Sketch11").SystemValue = 195m / 1000m;
                                else swPart.Parameter("D1@Sketch11").SystemValue = 175m / 1000m;
                            }
                            if (item.ANDetectorNo > 3)
                            {
                                swFeat = swComp.FeatureByName("ANDTEC4");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swPart.Parameter("D2@Sketch12").SystemValue = item.ANDetectorDis4 / 1000m;
                                if (item.ANDetectorEnd == "LEFT" && item.ANDetectorNo == 4)
                                    swPart.Parameter("D3@Sketch12").SystemValue = 195m / 1000m;
                                else swPart.Parameter("D3@Sketch12").SystemValue = 175m / 1000m;
                            }
                            if (item.ANDetectorNo > 4)
                            {
                                swFeat = swComp.FeatureByName("ANDTEC5");
                                swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                                swPart.Parameter("D3@Sketch13").SystemValue = item.ANDetectorDis5 / 1000m;
                                if (item.ANDetectorEnd == "LEFT" && item.ANDetectorNo == 5)
                                    swPart.Parameter("D1@Sketch13").SystemValue = 195m / 1000m;
                                else swPart.Parameter("D1@Sketch13").SystemValue = 175m / 1000m;
                            }

                        }
                        else
                        {
                            swFeat = swComp.FeatureByName("ANSULSIDE RIGHT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANSULSIDE LEFT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC1");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC2");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC3");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC4");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("ANDTEC5");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        }

                        if (item.MARVEL == "YES")
                        {
                            swFeat = swComp.FeatureByName("MA-NTC");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("MA-TAB");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                        }
                        else
                        {
                            swFeat = swComp.FeatureByName("MA-NTC");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("MA-TAB");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩 
                        }

                        if (item.LightCable == "LEFT")
                        {
                            swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        }
                        else if (item.LightCable == "RIGHT")
                        {
                            swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                            swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                        }
                        else
                        {
                            swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                            swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                            swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                        }
                    }
                }
                //----------灯腔----------
                swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0116-1"));
                swPart = swComp.GetModelDoc2();//打开零件
                swPart.Parameter("D1@Linear austragen1").SystemValue = item.Length / 1000m;
                swFeat = swComp.FeatureByName("FC SUPPORT");
                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                swFeat = swComp.FeatureByName("FC SUPPORT B");
                swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                swFeat = swComp.FeatureByName("JAP LED M8");
                if (item.Japan == "YES") swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                else swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                swFeat = swComp.FeatureByName("LIGHT T8");
                if (item.LightType == "T8") swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                else swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                if (item.LightCable == "LEFT")
                {
                    swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                    swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                    swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                }
                else if (item.LightCable == "RIGHT")
                {
                    swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                    swFeat.SetSuppression2(1, 2, configNames); //参数1：1解压，0压缩
                }
                else
                {
                    swFeat = swComp.FeatureByName("LIGHT HOLE LEFT");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                    swFeat = swComp.FeatureByName("LIGHT HOLE RIGHT");
                    swFeat.SetSuppression2(0, 2, configNames); //参数1：1解压，0压缩
                }
                swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0056-1"));
                swPart = swComp.GetModelDoc2();//打开零件
                swPart.Parameter("D1@Skizze1").SystemValue = item.Length / 1000m;

                //----------SSP灯板支撑条----------
                if (item.SSPType == "DOME")
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0036-4"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0036-5"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0035-6"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0035-7"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swPart = swComp.GetModelDoc2();//打开零件
                    swPart.Parameter("D2@Sketch1").SystemValue = item.Length / 1000m;
                    if (item.Gutter == "YES")
                    {
                        swModel.Parameter("D1@Distance27").SystemValue = item.GutterWidth / 1000m;
                        swModel.Parameter("D1@Distance36").SystemValue = item.GutterWidth / 1000m;
                    }
                    else
                    {
                        swModel.Parameter("D1@Distance27").SystemValue = 0.5m / 1000m;
                        swModel.Parameter("D1@Distance36").SystemValue = 0.5m / 1000m;
                    }
                }
                else
                {
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0036-4"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0036-5"));
                    swComp.SetSuppression2(2); //2解压缩，0压缩.
                    swPart = swComp.GetModelDoc2();//打开零件
                    swPart.Parameter("D2@Sketch1").SystemValue = item.Length / 1000m;
                    if (item.Gutter == "YES")
                    {
                        swModel.Parameter("D1@Distance28").SystemValue = item.GutterWidth / 1000m;
                        swModel.Parameter("D1@Distance37").SystemValue = item.GutterWidth / 1000m;
                    }
                    else
                    {
                        swModel.Parameter("D1@Distance28").SystemValue = 0.5m / 1000m;
                        swModel.Parameter("D1@Distance37").SystemValue = 0.5m / 1000m;
                    }
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0035-6"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                    swComp = swAssy.GetComponentByName(CommonFunc.AddSuffix(suffix, "FNCE0035-7"));
                    swComp.SetSuppression2(0); //2解压缩，0压缩.
                }

                swModel.ForceRebuild3(true);//设置成true，直接更新顶层，速度很快，设置成false，每个零件都会更新，很慢
                swModel.Save();//保存，很耗时间
                swApp.CloseDoc(packedAssyPath);//关闭，很快
            }
            catch (Exception ex)
            {
                throw new Exception(packedAssyPath + "作图过程发生异常，详细：" + ex.Message);
            }
            finally
            {
                swApp.CommandInProgress = false; //及时关闭外部命令调用，否则影响SolidWorks的使用
            }

        }
    }
}
