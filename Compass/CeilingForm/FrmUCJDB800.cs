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
    public partial class FrmUCJDB800 : MetroFramework.Forms.MetroForm
    {
        CategoryService objCategoryService = new CategoryService();
        UCJDB800Service objUCJDB800Service = new UCJDB800Service();
        private UCJDB800 objUCJDB800 = null;
        public FrmUCJDB800()
        {
            InitializeComponent();
            IniCob();
            //管理员和技术部才能更新数据
            if (Program.ObjCurrentUser.UserGroupId == 1 || Program.ObjCurrentUser.UserGroupId == 2) btnEditData.Visible = true;
            else btnEditData.Visible = false;
        }
        public FrmUCJDB800(Drawing drawing, ModuleTree tree) : this()
        {
            objUCJDB800 = (UCJDB800)objUCJDB800Service.GetModelByModuleTreeId(tree.ModuleTreeId.ToString());
            if (objUCJDB800 == null) return;
            this.Text = drawing.ODPNo + " / Item: " + drawing.Item + " / Module: " + tree.Module + " - " + tree.CategoryName;
            Category objCategory = objCategoryService.GetCategoryByCategoryId(tree.CategoryId.ToString());
            pbModelImage.Image = objCategory.ModelImage.Length == 0
                ? Image.FromFile("NoPic.png")
                : (Image)new SerializeObjectToString().DeserializeObject(objCategory.ModelImage);
            FillData();
        }
        private void IniCob()
        {
            //ANSUL腔
            cobGutter.Items.Add("YES");
            cobGutter.Items.Add("NO");
            cobGutter.SelectedIndex = 1;
            //ANSUL
            cobANSUL.Items.Add("YES");
            cobANSUL.Items.Add("NO");
            cobANSUL.SelectedIndex = 1;
            //ANSUL侧喷
            cobANSide.Items.Add("LEFT");
            cobANSide.Items.Add("RIGHT");
            cobANSide.Items.Add("NO");
            cobANSide.SelectedIndex = 2;
            //ANSUL末端探测器
            cobANDetectorEnd.Items.Add("LEFT");
            cobANDetectorEnd.Items.Add("RIGHT");
            cobANDetectorEnd.Items.Add("NO");
            cobANDetectorEnd.SelectedIndex = 2;
            //ANSUL探测器数量
            cobANDetectorNo.Items.Add("0");
            cobANDetectorNo.Items.Add("1");
            cobANDetectorNo.Items.Add("2");
            cobANDetectorNo.Items.Add("3");
            cobANDetectorNo.Items.Add("4");
            cobANDetectorNo.Items.Add("5");
            cobANDetectorNo.SelectedIndex = 0;
            //MARVEL
            cobMARVEL.Items.Add("YES");
            cobMARVEL.Items.Add("NO");
            cobMARVEL.SelectedIndex = 1;
            //灯具类型
            cobLightType.Items.Add("LED");
            cobLightType.Items.Add("T8");
            cobLightType.Items.Add("HCL");
            //灯腔出线孔位置
            cobLightCable.Items.Add("LEFT");
            cobLightCable.Items.Add("RIGHT");
            cobLightCable.Items.Add("NO");
            cobLightCable.SelectedIndex = 2;
            //UV灯长短
            cobUVType.Items.Add("SHORT");
            cobUVType.Items.Add("LONG");
            cobUVType.SelectedIndex = 0;
            //SSP灯板类型
            cobSSPType.Items.Add("DOME");
            cobSSPType.Items.Add("FLAT");
            //日本项目
            cobJapan.Items.Add("YES");
            cobJapan.Items.Add("NO");
            cobJapan.SelectedIndex = 1;
            //油网左右
            cobFCSide.Items.Add("LEFT");
            cobFCSide.Items.Add("RIGHT");
            cobFCSide.Items.Add("BOTH");
            cobFCSide.Items.Add("NO");
            //盲板数量
            cobFCBlindNo.Items.Add("0");
            cobFCBlindNo.Items.Add("1");
            cobFCBlindNo.Items.Add("2");
            cobFCBlindNo.Items.Add("3");
            cobFCBlindNo.SelectedIndex = 0;
        }

        private void txtLength_TextChanged(object sender, EventArgs e)
        {
            if (!DataValidate.IsDecimal(txtLength.Text.Trim()) || txtLength.Text.Trim().Length == 0) return;
            txtExRightDis.Text = (Convert.ToDecimal(txtLength.Text.Trim()) / 2).ToString();
        }

        private void FillData()
        {
            if (objUCJDB800 == null) return;
            pbModelImage.Tag = objUCJDB800.UCJDB800Id;

            cobGutter.Text = objUCJDB800.Gutter;
            cobANSUL.Text = objUCJDB800.ANSUL;
            cobANSide.Text = objUCJDB800.ANSide;
            cobANDetectorEnd.Text = objUCJDB800.ANDetectorEnd;
            cobANDetectorNo.Text = objUCJDB800.ANDetectorNo.ToString();
            cobMARVEL.Text = objUCJDB800.MARVEL;
            cobSSPType.Text = objUCJDB800.SSPType;
            cobJapan.Text = objUCJDB800.Japan;
            cobFCSide.Text = objUCJDB800.FCSide;
            cobUVType.Text = objUCJDB800.UVType;
            cobFCBlindNo.Text = objUCJDB800.FCBlindNo.ToString();
            cobLightType.Text = objUCJDB800.LightType;
            cobLightCable.Text = objUCJDB800.LightCable;

            txtLength.Text = objUCJDB800.Length.ToString();
            txtExRightDis.Text = objUCJDB800.ExRightDis.ToString();
            txtExLength.Text = objUCJDB800.ExLength.ToString();
            txtExWidth.Text = objUCJDB800.ExWidth.ToString();
            txtExHeight.Text = objUCJDB800.ExHeight.ToString();
            txtGutterWidth.Text = objUCJDB800.GutterWidth.ToString();
            txtFCSideLeft.Text = objUCJDB800.FCSideLeft.ToString();
            txtFCSideRight.Text = objUCJDB800.FCSideRight.ToString();
            txtANDetectorDis1.Text = objUCJDB800.ANDetectorDis1.ToString();
            txtANDetectorDis2.Text = objUCJDB800.ANDetectorDis2.ToString();
            txtANDetectorDis3.Text = objUCJDB800.ANDetectorDis3.ToString();
            txtANDetectorDis4.Text = objUCJDB800.ANDetectorDis4.ToString();
            txtANDetectorDis5.Text = objUCJDB800.ANDetectorDis5.ToString();
        }
        private void btnEditData_Click(object sender, EventArgs e)
        {
            #region 数据验证
            //必填项目
            if (pbModelImage.Tag.ToString().Length == 0) return;
            if (!DataValidate.IsDecimal(txtLength.Text.Trim()) || Convert.ToDecimal(txtLength.Text.Trim()) < 100m)
            {
                MessageBox.Show("请认真检查烟罩长度", "提示信息");
                txtLength.Focus();
                txtLength.SelectAll();
                return;
            }
            //ANSUL腔
            if (cobGutter.SelectedIndex == -1)
            {
                MessageBox.Show("请选择是否右ANSUL腔", "提示信息");
                cobGutter.Focus();
                return;
            }
            else if (cobGutter.SelectedIndex == 0 && (!DataValidate.IsDecimal(txtGutterWidth.Text.Trim()) || Convert.ToDecimal(txtGutterWidth.Text.Trim()) < 30m))
            {
                MessageBox.Show("请认真检查ANSUL腔宽度", "提示信息");//当脖颈大于2时需要填写脖颈间距
                txtGutterWidth.Focus();
                txtGutterWidth.SelectAll();
                return;
            }

            //脖颈
            if (!DataValidate.IsDecimal(txtExLength.Text.Trim()) || Convert.ToDecimal(txtExLength.Text.Trim()) < 50m)
            {
                MessageBox.Show("请填写脖颈长度", "提示信息");
                txtExLength.Focus();
                txtExLength.SelectAll();
                return;
            }
            if (!DataValidate.IsDecimal(txtExWidth.Text.Trim()) || Convert.ToDecimal(txtExWidth.Text.Trim()) < 50m)
            {
                MessageBox.Show("请填写脖颈宽度", "提示信息");
                txtExWidth.Focus();
                txtExWidth.SelectAll();
                return;
            }
            if (!DataValidate.IsDecimal(txtExHeight.Text.Trim()) || Convert.ToDecimal(txtExHeight.Text.Trim()) < 20m)
            {
                MessageBox.Show("请填写脖颈高度", "提示信息");
                txtExHeight.Focus();
                txtExHeight.SelectAll();
                return;
            }
            //ANSUL
            if (cobANSUL.SelectedIndex == -1)
            {
                MessageBox.Show("是否带ANSUL", "提示信息");
                cobANSUL.Focus();
                return;
            }

            if (cobMARVEL.SelectedIndex == -1)
            {
                MessageBox.Show("是否带MARVEL", "提示信息");
                cobMARVEL.Focus();
                return;
            }
            if (cobANSUL.SelectedIndex == 0)
            {
                if (cobANSide.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择ANSUL侧喷位置", "提示信息");
                    cobANSide.Focus();
                    return;
                }
                //ANSUL探测器
                if (cobANDetectorEnd.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择ANSUL末端探测器位置", "提示信息");
                    cobANDetectorEnd.Focus();
                    return;
                }
                if (cobANDetectorNo.SelectedIndex == -1)
                {
                    MessageBox.Show("请检查探测器数量", "提示信息");
                    cobANDetectorNo.Focus();
                    return;
                }
                if (cobANDetectorNo.SelectedIndex > 0)
                {
                    if (!DataValidate.IsDecimal(txtANDetectorDis1.Text.Trim()) || Convert.ToDecimal(txtANDetectorDis1.Text.Trim()) < 30m)
                    {
                        MessageBox.Show("请检查探测器间距1", "提示信息");
                        txtANDetectorDis1.Focus();
                        txtANDetectorDis1.SelectAll();
                        return;
                    }
                }
                if (cobANDetectorNo.SelectedIndex > 1)
                {
                    if (!DataValidate.IsDecimal(txtANDetectorDis2.Text.Trim()) || Convert.ToDecimal(txtANDetectorDis2.Text.Trim()) < 30m)
                    {
                        MessageBox.Show("请检查探测器间距2", "提示信息");
                        txtANDetectorDis2.Focus();
                        txtANDetectorDis2.SelectAll();
                        return;
                    }
                }
                if (cobANDetectorNo.SelectedIndex > 2)
                {
                    if (!DataValidate.IsDecimal(txtANDetectorDis3.Text.Trim()) || Convert.ToDecimal(txtANDetectorDis3.Text.Trim()) < 30m)
                    {
                        MessageBox.Show("请检查探测器间距3", "提示信息");
                        txtANDetectorDis3.Focus();
                        txtANDetectorDis3.SelectAll();
                        return;
                    }
                }
                if (cobANDetectorNo.SelectedIndex > 3)
                {
                    if (!DataValidate.IsDecimal(txtANDetectorDis4.Text.Trim()) || Convert.ToDecimal(txtANDetectorDis4.Text.Trim()) < 30m)
                    {
                        MessageBox.Show("请检查探测器间距4", "提示信息");
                        txtANDetectorDis4.Focus();
                        txtANDetectorDis4.SelectAll();
                        return;
                    }
                }
                if (cobANDetectorNo.SelectedIndex > 4)
                {
                    if (!DataValidate.IsDecimal(txtANDetectorDis5.Text.Trim()) || Convert.ToDecimal(txtANDetectorDis5.Text.Trim()) < 30m)
                    {
                        MessageBox.Show("请检查探测器间距5", "提示信息");
                        txtANDetectorDis5.Focus();
                        txtANDetectorDis5.SelectAll();
                        return;
                    }
                }
            }
            //其他配置
            if (cobSSPType.SelectedIndex == -1)
            {
                MessageBox.Show("请选择SSP灯板类型", "提示信息");
                cobSSPType.Focus();
                return;
            }
            if (cobJapan.SelectedIndex == -1)
            {
                MessageBox.Show("请选择是否为日本项目", "提示信息");
                cobJapan.Focus();
                return;
            }
            if (cobLightType.SelectedIndex == -1)
            {
                MessageBox.Show("请选择灯具类型", "提示信息");
                cobLightType.Focus();
                return;
            }
            if (cobLightCable.SelectedIndex == -1)
            {
                MessageBox.Show("请选择灯腔出线孔位置", "提示信息");
                cobLightCable.Focus();
                return;
            }
            if (cobUVType.SelectedIndex == -1)
            {
                MessageBox.Show("请选择UV灯类型", "提示信息");
                cobUVType.Focus();
                return;
            }
            //油网侧板
            if (cobFCSide.SelectedIndex == -1)
            {
                MessageBox.Show("请选择油网侧板", "提示信息");
                cobFCSide.Focus();
                return;
            }
            if ((cobFCSide.SelectedIndex == 0 || cobFCSide.SelectedIndex == 2) && (!DataValidate.IsDecimal(txtFCSideLeft.Text.Trim()) || Convert.ToDecimal(txtFCSideLeft.Text.Trim()) < 10m))
            {
                MessageBox.Show("请认真检查左油网侧板长度", "提示信息");//当脖颈大于2时需要填写脖颈间距
                txtFCSideLeft.Focus();
                txtFCSideLeft.SelectAll();
                return;
            }
            if ((cobFCSide.SelectedIndex == 1 || cobFCSide.SelectedIndex == 2) && (!DataValidate.IsDecimal(txtFCSideRight.Text.Trim()) || Convert.ToDecimal(txtFCSideRight.Text.Trim()) < 10m))
            {
                MessageBox.Show("请认真检查右油网侧板长度", "提示信息");//当脖颈大于2时需要填写脖颈间距
                txtFCSideRight.Focus();
                txtFCSideRight.SelectAll();
                return;
            }
            #endregion
            //封装对象
            UCJDB800 objUCJDB800 = new UCJDB800()
            {
                UCJDB800Id = Convert.ToInt32(pbModelImage.Tag),
                ANSUL = cobANSUL.Text,
                ANSide = cobANSide.Text.Trim().Length == 0 ? "NO" : cobANSide.Text,
                ANDetectorEnd = cobANDetectorEnd.Text.Trim().Length == 0 ? "NO" : cobANDetectorEnd.Text,
                ANDetectorNo = Convert.ToInt32(cobANDetectorNo.Text.Trim()),
                MARVEL = cobMARVEL.Text,
                SSPType = cobSSPType.Text,
                Japan = cobJapan.Text,
                Gutter = cobGutter.Text,
                FCSide = cobFCSide.Text,
                UVType = cobUVType.Text,
                FCBlindNo = Convert.ToInt32(cobFCBlindNo.Text.Trim()),
                LightType = cobLightType.Text,
                LightCable = cobLightCable.Text,

                Length = Convert.ToDecimal(txtLength.Text.Trim()),
                ExRightDis = Convert.ToDecimal(txtExRightDis.Text.Trim()),
                ExLength = Convert.ToDecimal(txtExLength.Text.Trim()),
                ExWidth = Convert.ToDecimal(txtExWidth.Text.Trim()),
                ExHeight = Convert.ToDecimal(txtExHeight.Text.Trim()),
                GutterWidth = Convert.ToDecimal(txtGutterWidth.Text.Trim()),
                FCSideLeft = Convert.ToDecimal(txtFCSideLeft.Text.Trim()),
                FCSideRight = Convert.ToDecimal(txtFCSideRight.Text.Trim()),
                ANDetectorDis1 = Convert.ToDecimal(txtANDetectorDis1.Text.Trim()),
                ANDetectorDis2 = Convert.ToDecimal(txtANDetectorDis2.Text.Trim()),
                ANDetectorDis3 = Convert.ToDecimal(txtANDetectorDis3.Text.Trim()),
                ANDetectorDis4 = Convert.ToDecimal(txtANDetectorDis4.Text.Trim()),
                ANDetectorDis5 = Convert.ToDecimal(txtANDetectorDis5.Text.Trim())
            };
            //提交修改
            try
            {
                if (objUCJDB800Service.EditModel(objUCJDB800) == 1)
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
        private void cobGutter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cobGutter.Text.Trim() == "YES")
            {
                lblGutterWidth.Visible = true;
                txtGutterWidth.Visible = true;
            }
            else
            {
                lblGutterWidth.Visible = false;
                txtGutterWidth.Visible = false;
            }
        }

        private void cobFCSide_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cobFCSide.Text.Trim())
            {
                case "LEFT":
                    lblFCSideLeft.Visible = true;
                    lblFCSideRight.Visible = false;
                    txtFCSideLeft.Visible = true;
                    txtFCSideRight.Visible = false;
                    break;
                case "RIGHT":
                    lblFCSideLeft.Visible = false;
                    lblFCSideRight.Visible = true;
                    txtFCSideLeft.Visible = false;
                    txtFCSideRight.Visible = true;
                    break;
                case "BOTH":
                    lblFCSideLeft.Visible = true;
                    lblFCSideRight.Visible = true;
                    txtFCSideLeft.Visible = true;
                    txtFCSideRight.Visible = true;
                    break;
                default:
                    lblFCSideLeft.Visible = false;
                    lblFCSideRight.Visible = false;
                    txtFCSideLeft.Visible = false;
                    txtFCSideRight.Visible = false;
                    break;
            }
        }

        private void cobANSUL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cobANSUL.Text.Trim() == "YES")
            {
                lblANSide.Visible = true;
                lblANDetectorEnd.Visible = true;
                lblANDetectorNo.Visible = true;
                cobANSide.Visible = true;
                cobANDetectorEnd.Visible = true;
                cobANDetectorNo.Visible = true;
            }
            else
            {
                lblANSide.Visible = false;
                lblANDetectorEnd.Visible = false;
                lblANDetectorNo.Visible = false;
                cobANSide.Visible = false;
                cobANDetectorEnd.Visible = false;
                cobANDetectorNo.Visible = false;
            }
        }
        /// <summary>
        /// 探测器数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cobDetectorNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cobANDetectorNo.SelectedIndex == 0)
            {
                lblANDetectorDis1.Visible = false;
                lblANDetectorDis2.Visible = false;
                lblANDetectorDis3.Visible = false;
                lblANDetectorDis4.Visible = false;
                lblANDetectorDis5.Visible = false;
                txtANDetectorDis1.Visible = false;
                txtANDetectorDis2.Visible = false;
                txtANDetectorDis3.Visible = false;
                txtANDetectorDis4.Visible = false;
                txtANDetectorDis5.Visible = false;
            }
            else if (cobANDetectorNo.SelectedIndex == 1)
            {
                lblANDetectorDis1.Visible = true;
                txtANDetectorDis1.Visible = true;
                lblANDetectorDis2.Visible = false;
                lblANDetectorDis3.Visible = false;
                lblANDetectorDis4.Visible = false;
                lblANDetectorDis5.Visible = false;
                txtANDetectorDis2.Visible = false;
                txtANDetectorDis3.Visible = false;
                txtANDetectorDis4.Visible = false;
                txtANDetectorDis5.Visible = false;
            }
            else if (cobANDetectorNo.SelectedIndex == 2)
            {
                lblANDetectorDis1.Visible = true;
                txtANDetectorDis1.Visible = true;
                lblANDetectorDis2.Visible = true;
                txtANDetectorDis2.Visible = true;
                lblANDetectorDis3.Visible = false;
                lblANDetectorDis4.Visible = false;
                lblANDetectorDis5.Visible = false;
                txtANDetectorDis3.Visible = false;
                txtANDetectorDis4.Visible = false;
                txtANDetectorDis5.Visible = false;
            }
            else if (cobANDetectorNo.SelectedIndex == 3)
            {
                lblANDetectorDis1.Visible = true;
                txtANDetectorDis1.Visible = true;
                lblANDetectorDis2.Visible = true;
                txtANDetectorDis2.Visible = true;
                lblANDetectorDis3.Visible = true;
                txtANDetectorDis3.Visible = true;
                lblANDetectorDis4.Visible = false;
                lblANDetectorDis5.Visible = false;
                txtANDetectorDis4.Visible = false;
                txtANDetectorDis5.Visible = false;
            }
            else if (cobANDetectorNo.SelectedIndex == 4)
            {
                lblANDetectorDis1.Visible = true;
                txtANDetectorDis1.Visible = true;
                lblANDetectorDis2.Visible = true;
                txtANDetectorDis2.Visible = true;
                lblANDetectorDis3.Visible = true;
                txtANDetectorDis3.Visible = true;
                lblANDetectorDis4.Visible = true;
                txtANDetectorDis4.Visible = true;
                lblANDetectorDis5.Visible = false;
                txtANDetectorDis5.Visible = false;
            }
            else if (cobANDetectorNo.SelectedIndex == 5)
            {

                lblANDetectorDis1.Visible = true;
                txtANDetectorDis1.Visible = true;
                lblANDetectorDis2.Visible = true;
                txtANDetectorDis2.Visible = true;
                lblANDetectorDis3.Visible = true;
                txtANDetectorDis3.Visible = true;
                lblANDetectorDis4.Visible = true;
                txtANDetectorDis4.Visible = true;
                lblANDetectorDis5.Visible = true;
                txtANDetectorDis5.Visible = true;
            }
        }
    }
}
