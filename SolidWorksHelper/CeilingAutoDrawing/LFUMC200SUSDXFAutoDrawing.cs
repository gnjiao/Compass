﻿using DAL;
using Models;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksHelper
{
    public class LFUMC200SUSDXFAutoDrawing : IAutoDrawing
    {
        LFUMC200SUSDXFService objLFUMC200SUSDXFService = new LFUMC200SUSDXFService();
        ProjectService objProjectService = new ProjectService();
        DXFCutListService objDxfCutListService = new DXFCutListService();
        HoodCutListService objHoodCutListService = new HoodCutListService();
        SubAssyService objSubAssyService = new SubAssyService();
        CeilingCutListService objCeilingCutListService = new CeilingCutListService();
        List<HoodCutList> hoodCutLists = new List<HoodCutList>();
        List<CeilingCutList> ceilingCutLists = new List<CeilingCutList>();
        public void AutoDrawing(SldWorks swApp, ModuleTree tree, string projectPath)
        {
            //创建下料图文件夹，默认在D盘MyProjects目录下（先判断文件夹是否存在）
            string dxfPath = projectPath + @"\DXF-CUTLIST";
            if (!Directory.Exists(dxfPath)) Directory.CreateDirectory(dxfPath);
            //创建dxf图文件夹
            string newPath = dxfPath + @"\" + tree.Item + "-" + tree.Module + @"\";
            if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
            //拷贝文件，调用通用函数
            if (!CommonFunc.CopyDxfFiles(tree.ModelPath, newPath)) return;
            //查询参数
            LFUMC200SUSDXF objLFUMC200SUSDXF = (LFUMC200SUSDXF)objLFUMC200SUSDXFService.GetModelByModuleTreeId(tree.ModuleTreeId.ToString());
            //查询标准DxfCutlist，根据item.categoryId查询dxfCutList对象列表
            List<DXFCutList> oldList = objDxfCutListService.GetDXFCutListsByCategoryId(tree.CategoryId.ToString());

            Project objProject = objProjectService.GetProjectByProjectId(tree.ProjectId.ToString());

            #region HoodCutList
            if (objProject.HoodType == "Hood")
            {
                //乘以数量，赋值moduletreeid
                foreach (var item in oldList)
                {
                    hoodCutLists.Add(new HoodCutList()
                    {
                        ModuleTreeId = objLFUMC200SUSDXF.ModuleTreeId,
                        PartDescription = item.PartDescription,
                        Length = item.Length,
                        Width = item.Width,
                        Thickness = item.Thickness,
                        Quantity = item.Quantity * objLFUMC200SUSDXF.Quantity,//多个UCP
                        Materials = item.Materials,
                        PartNo = item.PartNo,
                        UserId = 1
                    });
                }
                //基于事务hoodCutLists提交SQLServer
                if (hoodCutLists.Count == 0) return;
                try
                {
                    if (objHoodCutListService.ImportCutList(hoodCutLists)) hoodCutLists.Clear();
                }
                catch (Exception ex)
                {
                    throw new Exception("LFUMC200SUSDXF的HoodCutlist导入数据库失败" + ex.Message);
                }
            }
            #endregion


            #region CeilingCutList

            if (objProject.HoodType == "Ceiling")
            {
                //添加SubAssy，获得SubAssyId
                int subAssyId = objSubAssyService.AddSubAssy(new SubAssy()
                {
                    ProjectId = objProject.ProjectId,
                    SubAssyName = tree.Module
                });
                //乘以数量，赋值moduletreeid
                foreach (var item in oldList)
                {
                    ceilingCutLists.Add(new CeilingCutList()
                    {
                        SubAssyId = subAssyId,
                        PartDescription = item.PartDescription,
                        Length = item.Length,
                        Width = item.Width,
                        Thickness = item.Thickness,
                        Quantity = item.Quantity * objLFUMC200SUSDXF.Quantity,//多个UCP
                        Materials = item.Materials,
                        PartNo = item.PartNo,
                        UserId = 1
                    });
                }
                //基于事务CeilingCutLists提交SQLServer
                if (ceilingCutLists.Count == 0) return;
                try
                {
                    if (objCeilingCutListService.ImportCutList(ceilingCutLists)) ceilingCutLists.Clear();
                }
                catch (Exception ex)
                {
                    throw new Exception("LFUMC200SUSDXF的CeilingCutlist导入数据库失败" + ex.Message);
                }
            }
            #endregion
        }
    }
}
