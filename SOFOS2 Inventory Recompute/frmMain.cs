using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOFOS2_Inventory_Recompute
{
    public partial class frmMain : Form
    {
        Recompute controller = null;
        Stopwatch watch = null;

        public frmMain()
        {
            InitializeComponent();

            this.Load += (o, e) =>
            {
                List<string> items = new List<string>();
                tsDB.Text = $"Connected @ {Recompute.db}";

                items = Properties.Settings.Default.ITEMS.Split(',').ToList();

                //dataGridView1.DataSource = items.Select(x => new { ItemCode = x }).ToList();
            };

            btnUpdateTransValue.Click += (o, e) =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    controller = new Recompute();

                    var updates = controller.UpdateTransactionValue();

                    btnUpdateTransValue.Enabled = false;

                    foreach (KeyValuePair<Process, int> item in updates)
                    {
                        sb.Append($"{item.Key} - {item.Value.ToString("N0")} rows affected.{Environment.NewLine}");

                    }

                    if (updates.Count > 0)
                        btnUpdateCost.Enabled = true;

                    if (sb.Length < 1)
                        sb.Append("No rows affected in any transaction.");

                    MessageBox.Show(this, sb.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception er)
                {
                    MessageBox.Show(this, er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnUpdateTransValue.Enabled = true;
                }
            };


            btnUpdateCost.Click += (o, e) =>
            {
                try
                {
                    Task.Run(() =>
                    {
                        StartUpdateCost();
                    }).ContinueWith((t) =>
                    {
                        if (t.IsCompleted)
                        {
                            ThreadHelper.SetControlState(this, btnUpdateCost, true);
                        }
                        else
                        {
                            throw t.Exception;
                        }
                    });
                }
                catch (Exception er)
                {
                    MessageBox.Show(this, er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnUpdateCostBatchTwo.Click += (o, e) =>
            {
                Task.Run(() =>
                {
                    StartUpdateCostBatchTwo();
                }).ContinueWith((t) =>
                {
                    if (t.IsCompleted)
                    {
                        ThreadHelper.SetControlState(this, btnUpdateCostBatchTwo, true);
                    }
                    else
                    {
                        throw t.Exception;
                    }
                });
            };

            btnLast.Click += (o, e) =>
            {
                Task.Run(() =>
                {
                    StartUpdateCostBatchThree();
                }).ContinueWith((t) =>
                {
                    if (t.IsCompleted)
                    {
                        ThreadHelper.SetControlState(this, btnLast, true);
                    }
                    else
                    {
                        throw t.Exception;
                    }
                });
            };
        }

        private void StartRecompute()
        {
            try
            {
                controller = new Recompute(this);
                watch = new Stopwatch();

                //ThreadHelper.SetControlState(this, btnRecompute, false);

                //controller.UpdateMasterData();  
            }
            catch
            {
                throw;
            }
        }

        private void StartUpdateCost()
        {
            try
            {
                controller = new Recompute(this);
                watch = new Stopwatch();

                ThreadHelper.SetControlState(this, btnUpdateCost, false);

                controller.UpdateCostPerTransaction();
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private void StartUpdateCostBatchTwo()
        {
            try
            {
                controller = new Recompute(this);
                watch = new Stopwatch();

                ThreadHelper.SetControlState(this, btnUpdateCostBatchTwo, false);

                ThreadHelper.SetLabel(this, label1, string.Empty);


                controller.UpdateCostPerTransactionBatchTwo();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private void StartUpdateCostBatchThree()
        {
            try
            {
                controller = new Recompute(this);
                watch = new Stopwatch();

                ThreadHelper.SetControlState(this, btnLast, false);

                ThreadHelper.SetLabel(this, label1, string.Empty);


                controller.UpdateRemainingItems();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }
    }
}
