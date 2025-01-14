﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using DAL;
using Models;

namespace Compass
{
    public partial class FrmLPZ : MetroFramework.Forms.MetroForm
    {
        CategoryService objCategoryService = new CategoryService();
        LPZService objLPZService = new LPZService();
        private LPZ objLPZ = null;
        public FrmLPZ()
        {
            InitializeComponent();
            IniCob();
            //管理员和技术部才能更新数据
            if (Program.ObjCurrentUser.UserGroupId == 1 || Program.ObjCurrentUser.UserGroupId == 2) btnEditData.Visible = true;
            else btnEditData.Visible = false;
        }
        public FrmLPZ(Drawing drawing, ModuleTree tree) : this()
        {
            objLPZ = (LPZ)objLPZService.GetModelByModuleTreeId(tree.ModuleTreeId.ToString());
            if (objLPZ == null) return;
            this.Text = drawing.ODPNo + " / Item: " + drawing.Item + " / Module: " + tree.Module + " - " + tree.CategoryName;
            Category objCategory = objCategoryService.GetCategoryByCategoryId(tree.CategoryId.ToString());
            pbModelImage.Image = objCategory.ModelImage.Length == 0
                ? Image.FromFile("NoPic.png")
                : (Image)new SerializeObjectToString().DeserializeObject(objCategory.ModelImage);
            FillData();
        }
        private void IniCob()
        {
            //Z板数量
            cobZPanelNo.Items.Add("0");
            cobZPanelNo.Items.Add("1");
            cobZPanelNo.Items.Add("2");
            cobZPanelNo.Items.Add("3");
            cobZPanelNo.Items.Add("4");
            cobZPanelNo.Items.Add("5");
            cobZPanelNo.Items.Add("6");
            cobZPanelNo.Items.Add("7");
            cobZPanelNo.Items.Add("8");
            cobZPanelNo.Items.Add("9");
            cobZPanelNo.Items.Add("10");
            cobZPanelNo.Items.Add("11");
            cobZPanelNo.Items.Add("12");
            cobZPanelNo.Items.Add("13");
            cobZPanelNo.Items.Add("14");
            cobZPanelNo.Items.Add("15");
            cobZPanelNo.Items.Add("16");
            cobZPanelNo.Items.Add("17");
            cobZPanelNo.Items.Add("18");
            cobZPanelNo.Items.Add("19");
            cobZPanelNo.Items.Add("20");
        }
        /// <summary>
        /// 填数据
        /// </summary>
        private void FillData()
        {
            if (objLPZ == null) return;
            pbModelImage.Tag = objLPZ.LPZId;
            cobZPanelNo.Text = objLPZ.ZPanelNo.ToString();
            
            txtLength.Text = objLPZ.Length.ToString();
            txtWidth.Text = objLPZ.Width.ToString();
        }
        private void btnEditData_Click(object sender, EventArgs e)
        {
            //必填项目
            if (pbModelImage.Tag.ToString().Length == 0) return;
            if (!DataValidate.IsDecimal(txtLength.Text.Trim()) || Convert.ToDecimal(txtLength.Text.Trim()) < 50m)
            {
                MessageBox.Show("请认真检查LP板长度", "提示信息");
                txtLength.Focus();
                txtLength.SelectAll();
                return;
            }
            if (!DataValidate.IsDecimal(txtWidth.Text.Trim()) || Convert.ToDecimal(txtWidth.Text.Trim()) < 20m)
            {
                MessageBox.Show("请认真检查W板宽度", "提示信息");
                txtWidth.Focus();
                txtWidth.SelectAll();
                return;
            }
            if (cobZPanelNo.SelectedIndex == -1)
            {
                MessageBox.Show("请选择Z板阵列数量", "提示信息");
                cobZPanelNo.Focus();
                return;
            }
            
            //封装对象
            LPZ objLPZ = new LPZ()
            {
                LPZId = Convert.ToInt32(pbModelImage.Tag),

                Length = Convert.ToDecimal(txtLength.Text.Trim()),
                Width = Convert.ToDecimal(txtWidth.Text.Trim()),
                ZPanelNo = Convert.ToInt32(cobZPanelNo.Text.Trim())
            };
            //提交修改
            try
            {
                if (objLPZService.EditModel(objLPZ) == 1)
                {
                    MessageBox.Show("制图数据修改成功", "提示信息");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
