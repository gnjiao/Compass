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
using Models;
using DAL;

namespace Compass
{
    public partial class FrmProjectTracking : Form
    {
        private SqlDataPager objSqlDataPager = null;
        private ProjectTrackingService objProjectTrackingService = new ProjectTrackingService();
        private ProjectStatusService objProjectStatusService = new ProjectStatusService();
        private ProjectService objProjectService = new ProjectService();
        public FrmProjectTracking()
        {
            InitializeComponent();
            toolTip.SetToolTip(cobQueryYear, "按照项目完工日期年度查询");
            IniProjectStatus(cobProjectStatus);
            IniODPNo(cobODPNo);
            dgvProjectTracking.AutoGenerateColumns = false;
            grbEditProjectTracking.Visible = false;

            //查询年度初始化
            int currentYear = DateTime.Now.Year;
            cobQueryYear.Items.Add(currentYear + 1);//先添加下一年
            for (int i = 0; i <= currentYear - 2020; i++)
            {
                cobQueryYear.Items.Add(currentYear - i);
            }
            cobQueryYear.SelectedIndex = 1;//默认定位当前年份
            //设置默认的显示条数
            this.cobRecordList.SelectedIndex = 1;
            //初始无数据禁用相关按钮,考虑用户体验
            this.btnToPage.Enabled = false;
            this.btnFirst.Enabled = false;
            this.btnPre.Enabled = false;
            this.btnNext.Enabled = false;
            this.btnLast.Enabled = false;
            StringBuilder innerJoin = new StringBuilder("inner join ProjectStatus on ProjectStatus.ProjectStatusId=ProjectTracking.ProjectStatusId");
            innerJoin.Append(" inner join Projects on ProjectTracking.ProjectId=Projects.ProjectId");
            innerJoin.Append(" inner join Users on Projects.UserId=Users.UserId");
            innerJoin.Append(" left join (select ProjectId,max(DrReleaseTarget)as DrReleaseTarget from DrawingPlan group by ProjectId) as PlanList on PlanList.ProjectId=Projects.ProjectId");

            //初始化分页对象
            objSqlDataPager = new SqlDataPager()
            {
                PrimaryKey = "ProjectTrackingId",
                TableName = "ProjectTracking",
                InnerJoin1 = innerJoin.ToString(),
                InnerJoin2 = "inner join Projects on ProjectTracking.ProjectId=Projects.ProjectId",
                FiledName = "ProjectTrackingId,ODPNo,ProjectStatusName,DrReleaseTarget,DrReleaseActual,ShippingTime,ProdFinishActual,DeliverActual,ProjectName,KickOffStatus,UserAccount",
                CurrentPage = 1,
                Sort = "ShippingTime desc",
            };
            btnQueryByYear_Click(null, null);

            //初始化下拉框后关联事件委托
            this.cobProjectStatus.SelectedIndexChanged += new System.EventHandler(this.cobProjectStatus_SelectedIndexChanged);

            SetPermissions();
        }
        /// <summary>
        /// 设置权限
        /// </summary>
        private void SetPermissions()
        {
            //管理员才能编辑跟踪信息
            if (Program.ObjCurrentUser.UserGroupId == 1)
            {
                tsmiEditProjectTracking.Visible = true;
                this.dgvProjectTracking.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProjectTracking_CellDoubleClick);
            }
            else
            {
                tsmiEditProjectTracking.Visible = false;
                this.dgvProjectTracking.CellDoubleClick -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProjectTracking_CellDoubleClick);
            }
        }
        /// <summary>
        /// 执行查询的公共方法
        /// </summary>
        private void Query()
        {
            //开启所有的按钮
            this.btnToPage.Enabled = true;
            this.btnFirst.Enabled = true;
            this.btnPre.Enabled = true;
            this.btnNext.Enabled = true;
            this.btnLast.Enabled = true;
            //【1】设置分页查询的条件
            //objSqlDataPager.Condition = string.Format("ShippingTime>='{0}/01/01' and ShippingTime<='{0}/12/31'", this.cobQueryYear.Text);
            //【2】设置每页显示的条数
            //objSqlDataPager.PageSize = Convert.ToInt32(this.cobRecordList.Text.Trim());
            //【3】执行查询
            this.dgvProjectTracking.DataSource = objSqlDataPager.GetPagedData();
            //【4】显示记录总数，显示总页数，显示当前页码
            this.lblRecordsCound.Text = objSqlDataPager.RecordCount.ToString();
            this.lblPageCount.Text = objSqlDataPager.TotalPages.ToString();
            if (objSqlDataPager.RecordCount == 0)
            {
                this.lblCurrentPage.Text = "0";
            }
            else
            {
                this.lblCurrentPage.Text = objSqlDataPager.CurrentPage.ToString();
            }
            //禁用按钮的情况
            if (this.lblPageCount.Text == "0" || this.lblPageCount.Text == "1")
            {
                this.btnToPage.Enabled = false;
                this.btnFirst.Enabled = false;
                this.btnPre.Enabled = false;
                this.btnNext.Enabled = false;
                this.btnLast.Enabled = false;
            }
            else
            {
                this.btnToPage.Enabled = true;
            }
        }

        private void QueryByYear()
        {
            objSqlDataPager.Condition = string.Format("ShippingTime>='{0}/01/01' and ShippingTime<='{0}/12/31'", this.cobQueryYear.Text);
            objSqlDataPager.PageSize = Convert.ToInt32(this.cobRecordList.Text.Trim());
            Query();
        }

        private void QureyAll()
        {
            objSqlDataPager.Condition = "ShippingTime>='2020/01/01'";
            objSqlDataPager.PageSize = 10000;
            Query();
        }




        /// <summary>
        /// 初始化项目状态下拉框
        /// </summary>
        /// <param name="cobItem"></param>
        private void IniProjectStatus(ComboBox cobItem)
        {
            cobItem.DataSource = objProjectStatusService.GetAllProjectStatus();
            cobItem.DisplayMember = "ProjectStatusName";
            cobItem.ValueMember = "ProjectStatusId";
            cobItem.SelectedIndex = -1;//默认不要选中
        }
        /// <summary>
        /// 初始化ODPNo下拉框
        /// </summary>
        /// <param name="cobItem"></param>
        private void IniODPNo(ComboBox cobItem)
        {
            //绑定ODPNo下拉框
            cobItem.DataSource = objProjectService.GetProjectsByWhereSql("");
            cobItem.DisplayMember = "ODPNo";
            cobItem.ValueMember = "ProjectId";
            cobItem.SelectedIndex = -1;//默认不要选中
        }
        /// <summary>
        /// dgv添加行号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvProjectTracking_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DataGridViewStyle.DgvRowPostPaint(this.dgvProjectTracking, e);
            if (e.RowIndex > -1)
            {
                string projectStatus = this.dgvProjectTracking.Rows[e.RowIndex].Cells["ProjectStatusName"].Value.ToString();
                switch (projectStatus)
                {
                    case "DrawingMaking":
                        dgvProjectTracking.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(178, 252, 255);
                        break;
                    case "InProduction":
                        dgvProjectTracking.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(94, 223, 255);
                        break;
                    case "ProductionCompleted":
                        dgvProjectTracking.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0, 206, 209);
                        break;
                    case "ProjectCompleted":
                        dgvProjectTracking.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(95, 158, 160);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 添加项目跟踪信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddProjectTracking_Click(object sender, EventArgs e)
        {
            #region 数据验证
            if (cobODPNo.SelectedIndex == -1)
            {
                MessageBox.Show("请选择或者输入项目编号，如果没有，请到项目列表中添加后再选择", "验证信息");
                cobODPNo.Focus();
                return;
            }
            //验证日期顺序的正确性

            #endregion
            //封装项目跟踪对象
            ProjectTracking objProjectTracking = new ProjectTracking()
            {
                ProjectId = Convert.ToInt32(cobODPNo.SelectedValue),
                ProjectStatusId = 3,
                //(默认)因为dtp最小日期限制
                DrReleaseActual = DateTime.MinValue,
                ProdFinishActual = DateTime.MinValue,
                DeliverActual = DateTime.MinValue
            };
            //提交添加
            try
            {
                int projectTrackingId = objProjectTrackingService.AddProjectTracking(objProjectTracking);
                if (projectTrackingId > 1)
                {
                    //提示添加成功
                    MessageBox.Show("项目跟踪条目添加成功", "提示信息");
                    //刷新显示
                    btnQueryAllProjectTracking_Click(null, null);
                    //清空内容
                    cobODPNo.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 根据项目状态查询跟踪记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQueryByProjectStatus_Click(object sender, EventArgs e)
        {
            if (cobProjectStatus.SelectedIndex == -1) return;
            objSqlDataPager.Condition = string.Format("ProjectTracking.ProjectStatusId = {0}", cobProjectStatus.SelectedValue.ToString());
            objSqlDataPager.PageSize = 10000;
            Query();
        }
        /// <summary>
        /// 选择项目状态后直接查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cobProjectStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnQueryByProjectStatus_Click(null, null);
        }
        /// <summary>
        /// 根据项目号查询跟踪记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQueryByProjectId_Click(object sender, EventArgs e)
        {
            if (cobODPNo.SelectedIndex == -1) return;
            objSqlDataPager.Condition = string.Format("ProjectTracking.ProjectId = {0}", cobODPNo.SelectedValue.ToString());
            objSqlDataPager.PageSize = 10000;
            Query();
        }
        /// <summary>
        /// 选中行回填项目号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvProjectTracking_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProjectTracking.RowCount == 0) return;
            if (dgvProjectTracking.CurrentRow == null) return;
            cobODPNo.Text = this.dgvProjectTracking.CurrentRow.Cells["ODPNo"].Value.ToString();
        }

        /// <summary>
        /// 显示全部项目跟踪记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQueryAllProjectTracking_Click(object sender, EventArgs e)
        {
            QureyAll();
        }
        /// <summary>
        /// 查询所有项目跟踪记录菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiQueryAllProjectTracking_Click(object sender, EventArgs e)
        {
            btnQueryAllProjectTracking_Click(null, null);
        }
        /// <summary>
        /// 修改跟踪记录菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiEditProjectTracking_Click(object sender, EventArgs e)
        {
            if (dgvProjectTracking.RowCount == 0)
            {
                return;
            }
            if (dgvProjectTracking.CurrentRow == null)
            {
                MessageBox.Show("请选中需要修改的跟踪记录", "提示信息");
                return;
            }
            string projectTrackingId = dgvProjectTracking.CurrentRow.Cells["ProjectTrackingId"].Value.ToString();
            ProjectTracking objProjectTracking = objProjectTrackingService.GetProjectTrackingById(projectTrackingId);
            //初始化修改信息
            grbEditProjectTracking.Visible = true;//显示修改框
            grbEditProjectTracking.Location = new Point(10, 9);
            txtEditProjectTrackingId.Text = objProjectTracking.ProjectTrackingId.ToString();
            IniODPNo(cobEditODPNo);
            IniProjectStatus(cobEditProjectStatus);
            cobEditKickOffStatus.Items.Clear();
            cobEditKickOffStatus.Items.Add("Yes");
            cobEditKickOffStatus.Items.Add("No");

            cobEditODPNo.Text = objProjectTracking.ODPNo;
            cobEditProjectStatus.Text = objProjectTracking.ProjectStatusName;
            cobEditKickOffStatus.Text = objProjectTracking.KickOffStatus;

            //断开事件委托
            this.dtpEditDrReleaseActual.ValueChanged -= new System.EventHandler(this.dtpEditDrReleaseActual_ValueChanged);
            this.dtpEditProdFinishActual.ValueChanged -= new System.EventHandler(this.dtpEditProdFinishActual_ValueChanged);
            this.dtpEditDeliverActual.ValueChanged -= new System.EventHandler(this.dtpEditDeliverActual_ValueChanged);

            dtpEditDrReleaseActual.Text = objProjectTracking.DrReleaseActual == DateTime.MinValue ?
                Convert.ToDateTime("1/1/2020").ToShortDateString() :
                objProjectTracking.DrReleaseActual.ToShortDateString();
            dtpEditProdFinishActual.Text = objProjectTracking.ProdFinishActual == DateTime.MinValue ?
                Convert.ToDateTime("1/1/2020").ToShortDateString() :
                objProjectTracking.ProdFinishActual.ToShortDateString();
            dtpEditDeliverActual.Text = objProjectTracking.DeliverActual == DateTime.MinValue ?
                Convert.ToDateTime("1/1/2020").ToShortDateString() :
                objProjectTracking.DeliverActual.ToShortDateString();

            //重新建立事件委托
            this.dtpEditDrReleaseActual.ValueChanged += new System.EventHandler(this.dtpEditDrReleaseActual_ValueChanged);
            this.dtpEditProdFinishActual.ValueChanged += new System.EventHandler(this.dtpEditProdFinishActual_ValueChanged);
            this.dtpEditDeliverActual.ValueChanged += new System.EventHandler(this.dtpEditDeliverActual_ValueChanged);
        }
        /// <summary>
        /// 双击单元格修改记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvProjectTracking_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            tsmiEditProjectTracking_Click(null, null);
        }
        /// <summary>
        /// 提交修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditProjectTracking_Click(object sender, EventArgs e)
        {
            #region 数据验证
            if (cobEditProjectStatus.SelectedIndex == -1)
            {
                MessageBox.Show("请选择项目状态", "验证信息");
                cobEditProjectStatus.Focus();
                return;
            }
            if (cobEditODPNo.SelectedIndex == -1)
            {
                MessageBox.Show("请选择项目编号，如果没有，请到项目列表中添加后再选择", "验证信息");
                cobEditODPNo.Focus();
                return;
            }
            if (cobEditKickOffStatus.SelectedIndex == -1)
            {
                MessageBox.Show("请选择Kick-Off状态", "验证信息");
                cobEditKickOffStatus.Focus();
                return;
            }
            //验证日期顺序的正确性
            if (dtpEditDrReleaseActual.Value.ToString("MM/dd/yyyy") != "01/01/2020" && dtpEditProdFinishActual.Value.ToString("MM/dd/yyyy") != "01/01/2020" && DateTime.Compare(dtpEditDrReleaseActual.Value, dtpEditProdFinishActual.Value) > 0)
            {
                MessageBox.Show("实际发图日期不能大于实际完工日期，请认真检查", "验证信息");
                return;
            }
            if (dtpEditProdFinishActual.Value.ToString("MM/dd/yyyy") != "01/01/2020" && dtpEditDeliverActual.Value.ToString("MM/dd/yyyy") != "01/01/2020" && DateTime.Compare(dtpEditProdFinishActual.Value, dtpEditDeliverActual.Value) > 0)
            {
                MessageBox.Show("实际完工日期不能大于实际发货日期，请认真检查", "验证信息");
                return;
            }
            #endregion
            int firstRowIndex = dgvProjectTracking.CurrentRow.Index;
            //封装项目跟踪对象
            ProjectTracking objProjectTracking = new ProjectTracking()
            {
                ProjectTrackingId = Convert.ToInt32(txtEditProjectTrackingId.Text.Trim()),
                ProjectId = Convert.ToInt32(cobEditODPNo.SelectedValue),
                ProjectStatusId = Convert.ToInt32(cobEditProjectStatus.SelectedValue),
                KickOffStatus = cobEditKickOffStatus.Text,
                DrReleaseActual = Convert.ToDateTime(dtpEditDrReleaseActual.Text) == Convert.ToDateTime("1/1/2020") ?
                    DateTime.MinValue : Convert.ToDateTime(dtpEditDrReleaseActual.Text),
                ProdFinishActual = Convert.ToDateTime(dtpEditProdFinishActual.Text) == Convert.ToDateTime("1/1/2020") ?
                    DateTime.MinValue : Convert.ToDateTime(dtpEditProdFinishActual.Text),
                DeliverActual = Convert.ToDateTime(dtpEditDeliverActual.Text) == Convert.ToDateTime("1/1/2020") ?
                    DateTime.MinValue : Convert.ToDateTime(dtpEditDeliverActual.Text)
            };
            //调用后台方法修改对象
            try
            {
                if (objProjectTrackingService.EditProjectTracing(objProjectTracking) == 1)
                {
                    MessageBox.Show("修改计划成功！", "提示信息");
                    grbEditProjectTracking.Visible = false;
                    btnQueryByYear_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            dgvProjectTracking.Rows[firstRowIndex].Selected = true;//将刚修改的行选中
            dgvProjectTracking.FirstDisplayedScrollingRowIndex = firstRowIndex;//将修改的行显示在第一行
        }
        /// <summary>
        /// 删除跟踪记录菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiDeleteProjectTracking_Click(object sender, EventArgs e)
        {
            if (dgvProjectTracking.RowCount == 0)
            {
                return;
            }
            if (dgvProjectTracking.CurrentRow == null)
            {
                MessageBox.Show("请选中需要删除的跟踪记录", "验证信息");
                return;
            }
            string projectTrackingId = dgvProjectTracking.CurrentRow.Cells["ProjectTrackingId"].Value.ToString();
            string odpNo = dgvProjectTracking.CurrentRow.Cells["ODPNo"].Value.ToString();
            //删除询问
            DialogResult result = MessageBox.Show("确定要删除（项目编号ODP： " + odpNo + " ）这条跟踪记录吗？", "删除询问", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            int firstRowIndex = dgvProjectTracking.CurrentRow.Index;
            try
            {
                if (objProjectTrackingService.DeleteProjectTracking(projectTrackingId) == 1)
                {
                    btnQueryAllProjectTracking_Click(null, null);//同步刷新显示数据
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            grbEditProjectTracking.Visible = false;
            dgvProjectTracking.Rows[firstRowIndex].Selected = true;//将刚修改的行选中
            dgvProjectTracking.FirstDisplayedScrollingRowIndex = firstRowIndex;//将修改的行显示在第一行
        }

        /// <summary>
        /// 更改实际发图日期时，项目状态自动切换成生产中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpEditDrReleaseActual_ValueChanged(object sender, EventArgs e)
        {
            cobEditProjectStatus.SelectedValue = 4;
        }
        /// <summary>
        /// 更改实际完工日期后，项目状态自动切换成生产完工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpEditProdFinishActual_ValueChanged(object sender, EventArgs e)
        {
            cobEditProjectStatus.SelectedValue = 5;
        }
        /// <summary>
        /// 更改实际发货日期后，项目状态自动切换成项目完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpEditDeliverActual_ValueChanged(object sender, EventArgs e)
        {
            cobEditProjectStatus.SelectedValue = 6;
        }

        private void dgvProjectTracking_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }
        /// <summary>
        /// 根据年份查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQueryByYear_Click(object sender, EventArgs e)
        {
            if (this.cobQueryYear.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要查询的年度", "提示信息");
                return;
            }
            objSqlDataPager.CurrentPage = 1;//每次执行查询都必须设置为第一页
            QueryByYear();
            //禁用上一页按钮
            this.btnFirst.Enabled = false;
            this.btnPre.Enabled = false;
        }
        /// <summary>
        /// 第一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFirst_Click(object sender, EventArgs e)
        {
            objSqlDataPager.CurrentPage = 1;//每次执行查询都必须设置为第一页
            QueryByYear();
            //禁用上一页按钮和第一页
            this.btnFirst.Enabled = false;
            this.btnPre.Enabled = false;
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPre_Click(object sender, EventArgs e)
        {
            objSqlDataPager.CurrentPage -= 1;//在当前页码上减一
            QueryByYear();
            //禁用下一页和最后一页按钮
            if (objSqlDataPager.CurrentPage == 1)
            {
                this.btnFirst.Enabled = false;
                this.btnPre.Enabled = false;
            }
        }
        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {
            objSqlDataPager.CurrentPage += 1;//在当前页码上加一
            QueryByYear();
            //禁用下一页和最后一页按钮
            if (objSqlDataPager.CurrentPage == objSqlDataPager.TotalPages)
            {
                this.btnLast.Enabled = false;
                this.btnNext.Enabled = false;
            }
        }
        /// <summary>
        /// 最后一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLast_Click(object sender, EventArgs e)
        {
            objSqlDataPager.CurrentPage = objSqlDataPager.TotalPages;//在当前页码上加一
            QueryByYear();
            //禁用下一页和最后一页按钮
            this.btnLast.Enabled = false;
            this.btnNext.Enabled = false;
        }
        /// <summary>
        /// 跳转到
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToPage_Click(object sender, EventArgs e)
        {
            int a = this.txtToPage.IsInteger("跳转的页码");
            if (a != 0)
            {
                int toPage = Convert.ToInt32(this.txtToPage.Text.Trim());
                if (toPage > objSqlDataPager.TotalPages)
                {
                    btnLast_Click(null, null);//直接为最后一页
                }
                else if (toPage == 0)
                {
                    btnFirst_Click(null, null);//第一页
                }
                else
                {
                    objSqlDataPager.CurrentPage = toPage;//跳转到给定页码
                    Query();
                    if (objSqlDataPager.CurrentPage == 1)
                    {
                        //禁用上一页按钮和第一页
                        this.btnFirst.Enabled = false;
                        this.btnPre.Enabled = false;
                    }
                    else if (objSqlDataPager.CurrentPage == objSqlDataPager.TotalPages)
                    {
                        //禁用下一页和最后一页按钮
                        this.btnLast.Enabled = false;
                        this.btnNext.Enabled = false;
                    }
                }
            }
        }
    }
}
