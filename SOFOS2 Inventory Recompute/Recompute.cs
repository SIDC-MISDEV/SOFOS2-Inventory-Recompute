using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOFOS2_Inventory_Recompute
{
    public class Recompute
    {
        public static string server = Properties.Settings.Default.SERVER;
        public static string db = Properties.Settings.Default.DATABASE;
        private static string user = Properties.Settings.Default.USER;
        private static string pass = Properties.Settings.Default.PASSWORD;

        private string itemForUpdate = Properties.Settings.Default.ITEMS;

        string connString = $"Server={server};Database={db};Username={user};Password={pass};Allow User Variables=True;";
        frmMain frm;

        public Recompute(frmMain _frm = null)
        {
            frm = _frm;
        }

        public List<string> GetAllItems(MySQLHelper conn, string items)
        {
            try
            {
                var result = new List<string>();

                var _items = items.Split(',');

                var forUpdate = $"'{string.Join("','", _items)}'";

                if(items.Length > 0)
                    conn.ArgSQLCommand = Query.GetItemsFromMasterData(forUpdate);
                else
                    conn.ArgSQLCommand = Query.GetAllItems();


                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        result.Add(dr["items"].ToString());
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        public List<string> GetAllItemsBatchTwo(MySQLHelper conn, string items)
        {
            try
            {
                var result = new List<string>();

                var _items = items.Split(',');

                var forUpdate = $"'{string.Join("','", _items)}'";
                
                conn.ArgSQLCommand = Query.GetAllItemsBatchTwo(forUpdate);
                //conn.ArgSQLCommand = Query.GetItemsFromMasterData(forUpdate);

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        result.Add(dr["items"].ToString());
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        private Item GetLatestTransaction(MySQLHelper conn, string itemCode)
        {
            try
            {
                var item = new Item();

                conn.ArgSQLCommand = Query.GetItemDetails();
                conn.ArgSQLParam = new Dictionary<string, object>() { { "@item", itemCode } };

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        item.ItemCode = dr["itemcode"].ToString();
                        item.TransValue = Convert.ToDecimal(dr["transvalue"]);
                        item.RunningValue = Convert.ToDecimal(dr["runningvalue"]);
                    }
                }

                return item;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Item GetLatestTransactionSOFOS2(MySQLHelper conn, string itemCode)
        {
            try
            {
                var item = new Item();

                conn.ArgSQLCommand = Query.GetLedgerSOFOS2();
                conn.ArgSQLParam = new Dictionary<string, object>() { { "@itemCode", itemCode } };

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        item.ItemCode = itemCode;
                        item.RunningValue = Convert.ToDecimal(dr["runningvalue"]);
                    }
                }

                return item;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Dictionary<Process, int> UpdateTransactionValue()
        {
            try
            {
                var result = new Dictionary<Process, int>();
                var modules = Enum.GetValues(typeof(Process)).Cast<Process>().Select(v => v.ToString()).ToList();
                int updated = 0;

                using (var conn = new MySQLHelper(connString))
                {
                    conn.BeginTransaction();

                    foreach (var module in modules)
                    {
                        Process p;

                        Enum.TryParse(module, out p);

                        updated = UpdateTransactionValuePerTransaction(conn, p);

                        if (updated > 0)
                            result.Add(p, updated);

                    }

                    int s = UpdateMultipleItemInOneTransaction(conn);

                    if (result.Count > 0)
                        conn.CommitTransaction();
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        public int UpdateTransactionValuePerTransaction(MySQLHelper conn, Process module)
        {
            try
            {
                int result = 0;

                conn.ArgSQLCommand = Query.UpdateTransactionValuePerTransaction(module);
                result = conn.ExecuteMySQL();

                return result;
            }
            catch
            {

                throw;
            }
        }

        public List<Item> GetMultipleItemInOneTransaction(MySQLHelper conn)
        {
            try
            {
                var result = new List<Item>();

                conn.ArgSQLCommand = Query.GetMultipleItemInOneTransaction();

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        result.Add(new Item
                        {
                            Module = dr["module"].ToString(),
                            TransNum = Convert.ToInt32(dr["transnum"]),
                            ItemCode = dr["itemcode"].ToString()
                        });
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        public List<Item> GetItemMultiple(MySQLHelper conn, Process p, Item item)
        {
            try
            {
                var result = new List<Item>();

                conn.ArgSQLCommand = Query.GetItemMultiple(p);
                conn.ArgSQLParam = new Dictionary<string, object>()
                {
                    { "@itemcode", item.ItemCode },
                    { "@transnum", item.TransNum }
                };

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        result.Add(new Item
                        {
                            DetailNum = Convert.ToInt32(dr["detailnum"]),
                            ItemCode = dr["itemcode"].ToString(),
                            UomCode = dr["uomcode"].ToString(),
                            TransValue = Convert.ToDecimal(dr["transvalue"])
                        });
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        public int UpdateMultipleItemInOneTransaction(MySQLHelper conn)
        {
            try
            {
                List<Item> listItem = new List<Item>(),
                    specificItem = new List<Item>();
                int result = 0;

                listItem = GetMultipleItemInOneTransaction(conn);

                if (listItem.Count > 1)
                {
                    foreach (var item in listItem)
                    {
                        Process p;
                        Enum.TryParse(item.Module, out p);

                        specificItem = GetItemMultiple(conn, p, item);

                        result += UpdateTransValueMultipleItem(conn, specificItem, p);
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        private int UpdateTransValueMultipleItem(MySQLHelper conn, List<Item> items, Process p)
        {
            try
            {
                int result = 0,
                    multipleUom = 0;
                decimal tempVal = 0;

                multipleUom = items.Select(r => r.UomCode).Distinct().Count();

                foreach (var item in items)
                {
                    tempVal += item.TransValue;
                }

                conn.ArgSQLCommand = Query.UpdateMultipleUomTransValue(p);
                conn.ArgSQLParam = new Dictionary<string, object>()
                {
                    { "@itemcode", items[0].ItemCode },
                    { "@transvalue", tempVal },
                    { "@detailnum", items[0].DetailNum }
                };

                result = conn.ExecuteMySQL();

                return result;
            }
            catch
            {

                throw;
            }
        }


        public void UpdateRemainingItems()
        {
            try
            {
                Item forUpdateItem = null;
                int updatedItems = 0;
                int updatedItemsZero = 0;
                var items = new List<string>();
                var forUpdateItems = itemForUpdate;

                using (var conn = new MySQLHelper(connString))
                {
                    conn.BeginTransaction();

                    items = GetRemainingItems(conn);

                    if (items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            forUpdateItem = new Item();

                            forUpdateItem = GetLatestTransaction(conn, item);

                            if (!string.IsNullOrEmpty(forUpdateItem.ItemCode))
                            {
                                updatedItems += UpdateMasterData(conn, forUpdateItem);
                                ThreadHelper.SetLabel(frm, frm.label1, $"Updating item {item}. {Environment.NewLine}{updatedItems} out of {items.Count}.");
                            }
                        }

                        if (updatedItems > 0)
                            conn.CommitTransaction();

                        
                    }
                }
            }
            catch
            {

                throw;
            }
        }

        public void UpdateCostPerTransaction()
        {
            Item forUpdateItem = new Item();
            List<Logs> updateItem = new List<Logs>();
            try
            {
                List<Item> itemDetails = null;
                decimal runningQty = 0, runningValue = 0, cost = 0, transValue = 0;
                var items = new List<string>();
                int forUpdateCostItem = 0;
                int countItemForUpdate = 0;
                int updatedCost = 0;
                int updateItemRunValMasterData = 0;
                int updateCostUom = 0;
                decimal prevCost = 0;
                int updatedItemsZero = 0;

                string errorItems = string.Empty;
                string[] listItems = new string[] { };
                string[] listItems2 = new string[] { };
                string itemsBatchTwo = string.Empty;

                if (File.Exists(Path.Combine(Application.StartupPath, "updateitem.txt")))
                {
                    errorItems = File.ReadAllText(Path.Combine(Application.StartupPath, "updateitem.txt"));
                    listItems = errorItems.Replace("\n", ",").Replace("\r", "").Split(',');
                    listItems2 = listItems.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    itemsBatchTwo = string.Join(",", listItems2);
                }

                if (File.Exists(Path.Combine(Application.StartupPath, "error.txt")))
                    File.Delete(Path.Combine(Application.StartupPath, "error.txt"));

                using (var conn = new MySQLHelper(connString))
                {
                    conn.BeginTransaction();

                    items = GetAllItems(conn, itemsBatchTwo);

                    if(items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            try
                            {
                                itemDetails = new List<Item>();
                                countItemForUpdate += 1;

                                itemDetails = GetItemLedgerDetails(conn, item);

                                if (itemDetails.Count > 0)
                                {
                                    runningQty = 0;
                                    runningValue = 0;
                                    cost = 0;
                                    transValue = 0;
                                    updatedCost = 0;
                                    prevCost = 0;

                                    foreach (var details in itemDetails)
                                    {
                                        try
                                        {

                                            forUpdateItem = new Item();
                                            forUpdateItem.ItemCode = details.ItemCode;
                                            forUpdateItem.Reference = details.Reference;
                                            forUpdateItem.Cost = details.Cost;

                                            Process p;
                                            Enum.TryParse(details.Module, out p);

                                            forUpdateItem.Module = p.ToString();

                                            if (p == Process.ReceiveFromVendor || p == Process.Receiving || (p == Process.InventoryAdjustment && details.Qty > 0))
                                            {
                                                transValue = Math.Round(details.Cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);



                                                runningQty += details.Qty * details.Conversion;
                                                runningValue += transValue;

                                                if (runningQty <= 0 || runningValue <= 0)
                                                {
                                                    runningQty = 0;
                                                    runningValue = 0;
                                                    cost = prevCost;

                                                }
                                                else
                                                    cost = Math.Round(runningValue / runningQty, 2, MidpointRounding.AwayFromZero);

                                                updatedCost += 1;
                                            }
                                            else
                                            {


                                                if (cost != details.Cost)
                                                {
                                                    transValue = Math.Round(cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);

                                                    forUpdateItem.Cost = cost;

                                                    if (cost > 0)
                                                    {
                                                        var count = updateItem.Count(x => x.ItemCode == details.ItemCode);

                                                        if(count < 1)
                                                        {
                                                            File.AppendAllText(Path.Combine(Application.StartupPath, "updateitem2.txt"), $"{forUpdateItem.ItemCode}{Environment.NewLine}");
                                                        }

                                                        updateItem.Add(new Logs
                                                        {
                                                            ItemCode = details.ItemCode,
                                                            OldCost = details.Cost,
                                                            NewCost = cost,
                                                            Reference = details.Reference
                                                        });

                                                        conn.ArgSQLCommand = Query.UpdateCostPerTransaction(p);
                                                        conn.ArgSQLParam = new Dictionary<string, object>()
                                                        {
                                                            { "@transnum", details.TransNum },
                                                            { "@itemcode", details.ItemCode },
                                                            { "@cost", cost * details.Conversion}
                                                        };

                                                        forUpdateCostItem += conn.ExecuteMySQL();

                                                        updatedCost += 1;
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("0 Cost computed or wrong transdate saved");

                                                    }
                                                }
                                                else
                                                {
                                                    forUpdateItem.Cost = details.Cost;
                                                    transValue = Math.Round(details.Cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);
                                                    updatedCost += 1;
                                                }

                                                prevCost = cost;

                                                runningQty += details.Qty * details.Conversion;

                                                if (runningQty == 0)
                                                    runningValue = 0;
                                                else
                                                    runningValue += transValue;
                                            }

                                            ThreadHelper.SetLabel(frm, frm.lblCostProgress, $"Updating item {item}. {Environment.NewLine}{updatedCost} out of {itemDetails.Count} transaction(s) for checking.{Environment.NewLine}{forUpdateCostItem} item cost transaction updated.{Environment.NewLine}Progress:{countItemForUpdate} out of {items.Count}.");

                                        }
                                        catch (Exception er)
                                        {
                                            throw er;
                                            //updateItem.Add(new Logs
                                            //{
                                            //    ItemCode = details.ItemCode,
                                            //    OldCost = details.Cost,
                                            //    NewCost = cost,
                                            //    Reference = details.Reference,
                                            //    Remarks = er.Message
                                            //});

                                            //WriteLogs(updateItem);


                                            //File.AppendAllText(Path.Combine(Application.StartupPath, "error.txt"), $"{details.ItemCode}{Environment.NewLine}");

                                            //continue;
                                        }
                                    }

                                    if (updatedCost > 0)
                                    {
                                        forUpdateItem.TransValue = transValue;
                                        forUpdateItem.RunningValue = runningValue;

                                        updateItemRunValMasterData += UpdateMasterData(conn, forUpdateItem);
                                        updateCostUom += UpdateCostUom(conn, forUpdateItem);
                                        ThreadHelper.SetLabel(frm, frm.lblUpdateCostMasterData, $"Item running value update count: {updateItemRunValMasterData}.{Environment.NewLine}Item UOM cost update {updateCostUom}.");
                                    }

                                }
                            }
                            catch (Exception er)
                            {
                                string query = string.Empty;
                                Process mod;
                                Enum.TryParse(forUpdateItem.Module, out mod);

                                switch (mod)
                                {
                                    case Process.Sales:
                                        query = $"UPDATE sapt0 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.ReturnFromCustomer:
                                        query = $"UPDATE sapr0 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.ReceiveFromVendor:
                                        query = $"UPDATE prv00 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.ReturnGoods:
                                        query = $"UPDATE prg00 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.Issuance:
                                        query = $"UPDATE iii00 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.Receiving:
                                        query = $"UPDATE iir00 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    case Process.InventoryAdjustment:
                                        query = $"UPDATE iia00 SET transdate = systemdate WHERE reference = '{forUpdateItem.Reference}';";
                                        break;
                                    default:
                                        break;
                                }

                                updateItem.Add(new Logs
                                {
                                    ItemCode = forUpdateItem.ItemCode,
                                    OldCost = forUpdateItem.Cost,
                                    NewCost = forUpdateItem.Cost,
                                    Reference = forUpdateItem.Reference,
                                    Remarks = er.Message,
                                    Script = query
                                });

                                WriteLogs(updateItem);

                                File.AppendAllText(Path.Combine(Application.StartupPath, "error.txt"), $"{forUpdateItem.ItemCode}{Environment.NewLine}");

                                continue;
                            }
                        }

                        updatedItemsZero = UpdateRunValItemsRqtyZero(conn);
                        ThreadHelper.SetLabel(frm, frm.lblUpdateZero, $"{updatedItemsZero} items with zero running quantiy but wrong running value updated successfully.");

                        if (forUpdateCostItem > 0 || updatedCost > 0)
                            conn.CommitTransaction();
                    }
                }
            }
            catch(Exception er)
            {

                throw er;
            }
            finally
            {
                WriteLogs(updateItem);
            }
        }

        public void UpdateCostPerTransactionBatchTwo()
        {
            Item forUpdateItem = new Item();
            List<Logs> updateItem = new List<Logs>();
            try
            {
                List<Item> itemDetails = null;
                List<string> itemsForNextUpdate = new List<string>();

                string errorItems = File.ReadAllText(Path.Combine(Application.StartupPath, "error.txt"));

                var listItems = errorItems.Replace("\n", ",").Replace("\r", "").Split(',');

                var listItems2 = listItems.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                string itemsBatchTwo = string.Join(",", listItems2);

                decimal runningQty = 0, runningValue = 0, cost = 0, transValue = 0;
                var items = new List<string>();
                int forUpdateCostItem = 0;
                int countItemForUpdate = 0;
                int updatedCost = 0;
                int updateItemRunValMasterData = 0;
                int updateCostUom = 0;
                decimal prevCost = 0;
                int updatedItemsZero = 0;

                using (var conn = new MySQLHelper(connString))
                {
                    conn.BeginTransaction();

                    items = GetAllItemsBatchTwo(conn, itemsBatchTwo);

                    if (items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            itemDetails = new List<Item>();
                            countItemForUpdate += 1;

                            itemDetails = GetItemLedgerDetailsBatchTwo(conn, item);

                            if (itemDetails.Count > 0)
                            {
                                runningQty = 0;
                                runningValue = 0;
                                cost = 0;
                                transValue = 0;
                                updatedCost = 0;
                                prevCost = 0;

                                foreach (var details in itemDetails)
                                {
                                    try
                                    {
                                        forUpdateItem = new Item();
                                        forUpdateItem.ItemCode = details.ItemCode;
                                        forUpdateItem.Reference = details.Reference;
                                        forUpdateItem.Cost = details.Cost;

                                        Process p;
                                        Enum.TryParse(details.Module, out p);

                                        if (p == Process.ReceiveFromVendor || p == Process.Receiving || (p == Process.InventoryAdjustment && details.Qty > 0))
                                        {
                                            transValue = Math.Round(details.Cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);



                                            runningQty += details.Qty * details.Conversion;
                                            runningValue += transValue;

                                            if (runningQty <= 0 || runningValue <= 0)
                                            {
                                                runningQty = 0;
                                                runningValue = 0;
                                                cost = prevCost;
                                            }

                                            cost = Math.Round(runningValue / runningQty, 2, MidpointRounding.AwayFromZero);
                                        }
                                        else
                                        {

                                            if (cost != details.Cost)
                                            {
                                                //forUpdateItem = new Item();

                                                transValue = Math.Round(cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);
                                                forUpdateItem.Cost = cost;


                                                if (cost > 0)
                                                {
                                                    updateItem.Add(new Logs
                                                    {
                                                        ItemCode = details.ItemCode,
                                                        OldCost = details.Cost,
                                                        NewCost = cost,
                                                        Reference = details.Reference
                                                    });

                                                    conn.ArgSQLCommand = Query.UpdateCostPerTransaction(p);
                                                    conn.ArgSQLParam = new Dictionary<string, object>()
                                                    {
                                                        { "@transnum", details.TransNum },
                                                        { "@itemcode", details.ItemCode },
                                                        { "@cost", cost * details.Conversion}
                                                    };

                                                    forUpdateCostItem += conn.ExecuteMySQL();

                                                    updatedCost += 1;
                                                }
                                                else
                                                {
                                                    throw new Exception("0 Cost computed or wrong transdate saved");
                                                    //updateItem.Add(new Logs
                                                    //{
                                                    //    ItemCode = details.ItemCode,
                                                    //    OldCost = details.Cost,
                                                    //    NewCost = cost,
                                                    //    Reference = details.Reference,
                                                    //    Remarks = "0 Cost computed"
                                                    //});
                                                }
                                            }
                                            else
                                            {
                                                updatedCost += 1;
                                                transValue = Math.Round(details.Cost * (details.Qty * details.Conversion), 2, MidpointRounding.AwayFromZero);
                                            }

                                            prevCost = cost;

                                            runningQty += details.Qty * details.Conversion;

                                            if (runningQty == 0)
                                                runningValue = 0;
                                            else
                                                runningValue += transValue;
                                        }

                                        ThreadHelper.SetLabel(frm, frm.lblCostProgress, $"Updating item {item}. {Environment.NewLine}{updatedCost} out of {itemDetails.Count} transaction(s) for checking.{Environment.NewLine}{forUpdateCostItem} item cost transaction updated.{Environment.NewLine}Progress:{countItemForUpdate} out of {items.Count}.");

                                    }
                                    catch (Exception er)
                                    {
                                        updateItem.Add(new Logs
                                        {
                                            ItemCode = details.ItemCode,
                                            OldCost = details.Cost,
                                            NewCost = cost,
                                            Reference = details.Reference,
                                            Remarks = er.Message
                                        });

                                        WriteLogs(updateItem);

                                        continue;
                                    }
                                }

                                if (updatedCost > 0)
                                {
                                    forUpdateItem.TransValue = transValue;
                                    forUpdateItem.RunningValue = runningValue;

                                    updateItemRunValMasterData += UpdateMasterData(conn, forUpdateItem);
                                    updateCostUom += UpdateCostUom(conn, forUpdateItem);
                                    ThreadHelper.SetLabel(frm, frm.lblUpdateCostMasterData, $"Item running value update count: {updateItemRunValMasterData}.{Environment.NewLine}Item UOM cost update {updateCostUom}.");
                                }
                            }
                        }

                        updatedItemsZero = UpdateRunValItemsRqtyZero(conn);
                        ThreadHelper.SetLabel(frm, frm.lblUpdateZero, $"{updatedItemsZero} items with zero running quantiy but wrong running value updated successfully.");

                        if (forUpdateCostItem > 0 || updatedCost > 0)
                            conn.CommitTransaction();
                    }
                }
            }
            catch (Exception er)
            {

                throw er;
            }
            finally
            {
                WriteLogs(updateItem);
            }
        }

        private List<string> GetRemainingItems(MySQLHelper conn)
        {
            try
            {
                var item = new List<string>();

                conn.ArgSQLCommand = Query.GetRemainingWrongRunVal();

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        item.Add(dr["itemcode"].ToString());
                    }
                }

                return item;
            }
            catch
            {

                throw;
            }
        }

        private int UpdateMasterData(MySQLHelper conn, Item item)
        {
            try
            {
                int result = 0;

                conn.ArgSQLCommand = Query.UpdateMasterData();
                conn.ArgSQLParam = new Dictionary<string, object>()
                {
                    { "@itemcode", item.ItemCode },
                    { "@transval", Math.Abs(item.TransValue) },
                    { "@runningvalue", item.RunningValue }
                };

                result = conn.ExecuteMySQL();

                return result;
            }
            catch
            {

                throw;
            }
        }

        private int UpdateCostUom(MySQLHelper conn, Item item)
        {
            try
            {
                int result = 0;

                conn.ArgSQLCommand = Query.UpdateCostMasterData();
                conn.ArgSQLParam = new Dictionary<string, object>()
                {
                    { "@itemcode", item.ItemCode },
                    { "@cost", item.Cost }
                };

                result = conn.ExecuteMySQL();

                return result;
            }
            catch
            {

                throw;
            }
        }

        private int UpdateRunValItemsRqtyZero(MySQLHelper conn)
        {
            try
            {
                int result = 0;

                conn.ArgSQLCommand = Query.UpdateRunningValueZeroRunningQty();

                result = conn.ExecuteMySQL();

                return result;
            }
            catch
            {

                throw;
            }
        }

        private List<Item> GetItemLedgerDetails(MySQLHelper conn, string itemCode)
        {
            try
            {
                var item = new List<Item>();

                conn.ArgSQLCommand = Query.GetItemDetails();
                conn.ArgSQLParam = new Dictionary<string, object>() { { "@item", itemCode } };

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        item.Add(new Item
                        {
                            ItemCode = dr["itemcode"].ToString(),
                            Reference = dr["reference"].ToString(),
                            Cost = Convert.ToDecimal(dr["cost"]),
                            Module = dr["module"].ToString(),
                            Qty = Convert.ToDecimal(dr["quantity"]),
                            TransNum = Convert.ToInt32(dr["transnum"]),
                            Conversion = Convert.ToDecimal(dr["conversion"])
                        });
                    }
                }

                return item;
            }
            catch
            {

                throw;
            }
        }

        private List<Item> GetItemLedgerDetailsBatchTwo(MySQLHelper conn, string itemCode)
        {
            try
            {
                var item = new List<Item>();

                conn.ArgSQLCommand = Query.GetItemDetailsBatchTwo();
                conn.ArgSQLParam = new Dictionary<string, object>() { { "@item", itemCode } };

                using (var dr = conn.MySQLExecuteReaderBeginTransaction())
                {
                    while (dr.Read())
                    {
                        item.Add(new Item
                        {
                            ItemCode = dr["itemcode"].ToString(),
                            Reference = dr["reference"].ToString(),
                            Cost = Convert.ToDecimal(dr["cost"]),
                            Module = dr["module"].ToString(),
                            Qty = Convert.ToDecimal(dr["quantity"]),
                            TransNum = Convert.ToInt32(dr["transnum"]),
                            Conversion = Convert.ToDecimal(dr["conversion"])
                        });
                    }
                }

                return item;
            }
            catch
            {

                throw;
            }
        }

        private void WriteLogs(List<Logs> items)
        {
            string fileName = string.Format($"Items Update History-{DateTime.Now.ToString("ddMMyyyyHHmmss")}.csv");
            string dropSitePath = Application.StartupPath;

            ObjectToCSV<Logs> receiveFromVendorObjectToCSV = new ObjectToCSV<Logs>();
            string filename = Path.Combine(dropSitePath, fileName);
            receiveFromVendorObjectToCSV.SaveToCSV(items, filename);
        }

        private void WriteLogs(List<ItemLedger> items)
        {
            string fileName = string.Format($"Item with Latest Running Value in Ledger-{DateTime.Now.ToString("ddMMyyyyHHmmss")}.csv");
            string dropSitePath = Application.StartupPath;

            ObjectToCSV<ItemLedger> receiveFromVendorObjectToCSV = new ObjectToCSV<ItemLedger>();
            string filename = Path.Combine(dropSitePath, fileName);
            receiveFromVendorObjectToCSV.SaveToCSV(items, filename);
        }

        public void GetAllItemsWithLatestRunningValue()
        {
            try
            {
                var result = new List<ItemLedger>();
                int count = 0;

                using (var conn = new MySQLHelper(connString))
                {
                    var list = GetAllItems(conn, string.Empty);

                    if(list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            Item forGenerate = new Item();
                            forGenerate = GetLatestTransactionSOFOS2(conn, item);

                            if(!string.IsNullOrEmpty(forGenerate.ItemCode))
                            {
                                result.Add(new ItemLedger
                                {
                                    ItemCode = forGenerate.ItemCode,
                                    RunningValue = forGenerate.RunningValue
                                });

                                count += 1;

                                ThreadHelper.SetLabel(frm, frm.label2, $"{item} - {count} of {list.Count}");
                            }
                        }
                    }
                }

                WriteLogs(result);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }

    public class ObjectToCSV<T>
    {
        public void SaveToCSV(List<T> reportData, string path)
        {
            var lines = new List<string>();
            IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();
            var header = string.Join(",", props.ToList().Select(x => x.Name));
            lines.Add(header);
            var valueLines = reportData.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
            lines.AddRange(valueLines);
            File.WriteAllLines(path, lines.ToArray());
        }
    }

    public class Item
    {
        public int TransNum { get; set; }
        public int DetailNum { get; set; }
        public string ItemCode { get; set; }
        public string UomCode { get; set; }
        public decimal TransValue { get; set; }
        public decimal RunningValue { get; set; }
        public string Module { get; set; }
        public decimal Cost { get; set; }
        public string Reference { get; set; }
        public decimal Qty { get; set; }
        public decimal Conversion { get; set; }
    }

    public class Logs
    {
        public string Reference { get; set; }
        public string ItemCode { get; set; }
        public decimal OldCost { get; set; }
        public decimal NewCost { get; set; }
        public string Remarks { get; set; }
        public string Script { get; set; }
    }

    public class ItemLedger
    {
        public string ItemCode { get; set; }
        public decimal RunningValue { get; set; }
    }
}
