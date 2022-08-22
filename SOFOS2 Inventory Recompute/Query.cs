using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOFOS2_Inventory_Recompute
{
    public class Query
    {
        public static StringBuilder GetAllItems()
        {
            return new StringBuilder(@"SELECT
	                    distinct x.itemcode as items
                    FROM (

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM prv00 a INNER JOIN prv10 b ON a.transnum = b.transnum

	                    UNION all

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM prg00 a INNER JOIN prg10 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue * -1 as transvalue
	                    FROM sapt0 a INNER JOIN sapt1 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM sapr0 a INNER JOIN sapr1 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iir00 a INNER JOIN iir10 b ON a.transnum = b.transnum WHERE a.transtype != 'BB'

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iii00 a INNER JOIN iii10 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iia00 a INNER JOIN iia10 b ON a.transnum = b.transnum

                    ) as x ORDER BY itemcode;");
        }

        public static StringBuilder GetAllItemsBatchTwo(string items)
        {
            return new StringBuilder($@"SELECT
	                    distinct x.itemcode as items
                    FROM (

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM prv00 a INNER JOIN prv10 b ON a.transnum = b.transnum

	                    UNION all

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM prg00 a INNER JOIN prg10 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue * -1 as transvalue
	                    FROM sapt0 a INNER JOIN sapt1 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM sapr0 a INNER JOIN sapr1 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iir00 a INNER JOIN iir10 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iii00 a INNER JOIN iii10 b ON a.transnum = b.transnum

	                    UNION ALL

	                    SELECT
		                    a.transdate, a.reference, b.itemcode, b.transvalue
	                    FROM iia00 a INNER JOIN iia10 b ON a.transnum = b.transnum

                    ) as x WHERE x.itemcode IN ({items})ORDER BY itemcode;");
        }

        public static StringBuilder GetItemLedger()
        {
            return new StringBuilder(@"SET @runval := 0;
                SET @itemcode = @item;
                SELECT
	                x.transdate, x.reference, x.itemcode, x. x.transvalue, @runval := @runval + x.transvalue as RunningValue 
                FROM (

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.price as 'cost', b.quantity, b.transvalue
	                FROM prv00 a INNER JOIN prv10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION all

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.price as 'cost', b.quantity, b.transvalue
	                FROM prg00 a INNER JOIN prg10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.cost as 'cost', b.quantity, b.transvalue * -1 as transvalue
	                FROM sapt0 a INNER JOIN sapt1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.cost as 'cost', b.quantity, b.transvalue
	                FROM sapr0 a INNER JOIN sapr1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.price as 'cost', b.quantity, b.transvalue
	                FROM iir00 a INNER JOIN iir10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transdate, a.reference, b.itemcode, b.price as 'cost', b.quantity, b.transvalue
	                FROM iii00 a INNER JOIN iii10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transdate, a.reference, b.itemcode,b.price as 'cost', b.variance as quantity, b.transvalue
	                FROM iia00 a INNER JOIN iia10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

                ) as x ORDER BY x.transdate;");
        }

        public static StringBuilder UpdateTransactionValuePerTransaction(Process p)
        {
            var sb = new StringBuilder();

            switch (p)
            {
                case Process.Sales:
                    sb.Append(@"UPDATE sapt1 SET transvalue = ROUND(cost * quantity, 2);");
                    break;
                case Process.ReturnFromCustomer:
                    sb.Append(@"UPDATE sapr1 SET transvalue = ROUND(cost * quantity, 2);");
                    break;
                case Process.ReceiveFromVendor:
                    sb.Append(@"UPDATE prv10 SET transvalue = ROUND(price * quantity, 2);");
                    break;
                case Process.ReturnGoods:
                    sb.Append(@"UPDATE prg10 SET transvalue = ROUND(price * quantity, 2);");
                    break;
                case Process.Issuance:
                    sb.Append(@"UPDATE iii10 SET transvalue = ROUND(price * quantity, 2);");
                    break;
                case Process.Receiving:
                    sb.Append(@"UPDATE iir10 SET transvalue = ROUND(price * quantity, 2);");
                    break;
                case Process.InventoryAdjustment:
                    sb.Append(@"UPDATE iia10 SET transvalue = ROUND(ABS(price * variance), 2);");
                    break;
                default:
                    break;
            }

            return sb;
        }

        public static StringBuilder GetMultipleItemInOneTransaction()
        {
            return new StringBuilder(@"SELECT
	                                        x.module, x.transnum, x.itemcode, x.count
                                        FROM(
                                        SELECT 'Sales' as module, transnum, itemcode, count(itemcode) 'count' FROM sapt1
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'ReturnFromCustomer' as module, transnum, itemcode, count(itemcode) 'count' FROM sapr1
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'ReceiveFromVendor' as module, transnum, itemcode, count(itemcode) 'count' FROM prv10
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'ReturnGoods' as module, transnum, itemcode, count(itemcode) 'count' FROM prg10
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'Issuance' as module,transnum, itemcode, count(itemcode) 'count' FROM iii10
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'Receiving' as module,transnum, itemcode, count(itemcode) 'count' FROM iir10
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        UNION ALL
                                        SELECT 'InventoryAdjustment' as module,transnum, itemcode, count(itemcode) 'count' FROM iia10
                                        GROUP BY transnum, itemcode
                                        HAVING COUNT(itemcode) > 1
                                        ) as x;
                                        ");
        }

        public static StringBuilder GetItemMultiple(Process p)
        {
            var sb = new StringBuilder();
            string table = string.Empty;

            switch (p)
            {
                case Process.Sales:
                    table = "sapt1";
                    break;
                case Process.ReturnFromCustomer:
                    table = "sapr1";
                    break;
                case Process.ReceiveFromVendor:
                    table = "prv10";
                    break;
                case Process.ReturnGoods:
                    table = "prg10";
                    break;
                case Process.Issuance:
                    table = "iii10";
                    break;
                case Process.Receiving:
                    table = "iir10";
                    break;
                case Process.InventoryAdjustment:
                    table = "iia10";
                    break;
                default:
                    break;
            }

            sb.Append($@"SELECT detailnum, itemcode, uomcode, transValue FROM {table} where transnum = @transnum and itemcode = @itemcode ORDER BY detailnum asc");

            return sb;
        }

        public static StringBuilder UpdateMultipleUomTransValue(Process p)
        {
            var sb = new StringBuilder();
            string table = string.Empty;

            switch (p)
            {
                case Process.Sales:
                    table = "sapt1";
                    break;
                case Process.ReturnFromCustomer:
                    table = "sapr1";
                    break;
                case Process.ReceiveFromVendor:
                    table = "prv10";
                    break;
                case Process.ReturnGoods:
                    table = "prg10";
                    break;
                case Process.Issuance:
                    table = "iii10";
                    break;
                case Process.Receiving:
                    table = "iir10";
                    break;
                case Process.InventoryAdjustment:
                    table = "iia10";
                    break;
                default:
                    break;
            }

            sb.Append($@"UPDATE {table} SET transvalue = @transvalue where detailnum = @detailnum and itemcode = @itemcode;");

            return sb;
        }

        public static StringBuilder UpdateMasterData()
        {
            return new StringBuilder(@"UPDATE ii000 SET transvalue = @transVal, runningValue = @runningValue WHERE itemcode = @itemCode");
        }

        public static StringBuilder UpdateRunningValueZeroRunningQty()
        {
            return new StringBuilder(@"UPDATE ii000 SET runningvalue = 0 where runningquantity = 0 and runningvalue != 0;");
        }

        public static StringBuilder GetItemsFromMasterData(string items)
        {
            return new StringBuilder($"SELECT itemcode as items FROM ii000 WHERE itemcode IN ({items})");
        }

        public static StringBuilder UpdateCostMasterData()
        {
            return new StringBuilder(@"UPDATE iiuom SET cost = ROUND(@cost * conversion, 2) WHERE itemcode = @itemcode;");
        }

        public static StringBuilder UpdateCostPerTransaction(Process p)
        {
            var sb = new StringBuilder();

            switch (p)
            {
                case Process.Sales:
                    sb.Append(@"UPDATE sapt1 SET cost = @cost, transvalue = ROUND(@cost * quantity, 2) WHERE transnum = @transnum AND itemcode = @itemcode;");
                    break;
                case Process.ReturnFromCustomer:
                    sb.Append(@"UPDATE sapr1 SET cost = @cost, transvalue = ROUND(@cost * quantity, 2) WHERE transnum = @transnum AND itemcode = @itemcode;");
                    break;
                //case Process.ReceiveFromVendor:
                //    sb.Append(@"UPDATE prv10 SET transvalue = ROUND(price * quantity, 2);");
                //    break;
                case Process.ReturnGoods:
                    sb.Append(@"UPDATE prg10 SET price = @cost, transvalue = ROUND(@cost * quantity, 2) WHERE transnum = @transnum AND itemcode = @itemcode;");
                    break;
                case Process.Issuance:
                    sb.Append(@"UPDATE iii10 SET price = @cost, transvalue = ROUND(@cost * quantity, 2) WHERE transnum = @transnum AND itemcode = @itemcode;");
                    break;
                //case Process.Receiving:
                //    sb.Append(@"UPDATE iir10 SET transvalue = ROUND(price * quantity, 2);");
                //    break;
                case Process.InventoryAdjustment:
                    sb.Append(@"UPDATE iia10 SET price = @cost, transvalue = ROUND(ABS(@cost * variance), 2) WHERE transnum = @transnum AND itemcode = @itemcode;");
                    break;
                default:
                    break;
            }

            return sb;
        }

        public static StringBuilder GetItemDetails()
        {
            return new StringBuilder(@"SET @runval := 0;
                SET @itemcode = @item;
                SELECT
	                x.systemdate,x.transnum,x.transdate, x.reference, x.itemcode, x.quantity,x.cost, x.transvalue, @runval := @runval + x.transvalue as RunningValue, x.module, x.conversion
                FROM (

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'ReceiveFromVendor' as module,b.conversion
	                FROM prv00 a INNER JOIN prv10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION all

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price  / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity, b.quantity * -1) as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'ReturnGoods' as module,b.conversion
	                FROM prg00 a INNER JOIN prg10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.cost / b.conversion as 'cost',IF(a.transtype = 'CD', b.quantity, b.quantity * -1) as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'Sales' as module,b.conversion
	                FROM sapt0 a INNER JOIN sapt1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.cost / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'ReturnFromCustomer' as module,b.conversion
	                FROM sapr0 a INNER JOIN sapr1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'Receiving' as module,b.conversion
	                FROM iir00 a INNER JOIN iir10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity, b.quantity * -1)  as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'Issuance' as module,b.conversion
	                FROM iii00 a INNER JOIN iii10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode,b.price / b.conversion as 'cost', b.variance as quantity, b.transvalue, 'InventoryAdjustment' as module,b.conversion
	                FROM iia00 a INNER JOIN iia10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

                ) as x ORDER BY x.transdate, x.systemdate asc;");
        }

        public static StringBuilder GetItemDetailsBatchTwo()
        {
            return new StringBuilder(@"SET @runval := 0;
                SET @itemcode = @item;
                SELECT
	                x.systemdate,x.transnum,x.transdate, x.reference, x.itemcode, x.quantity,x.cost, x.transvalue, @runval := @runval + x.transvalue as RunningValue, x.module, x.conversion
                FROM (

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'ReceiveFromVendor' as module,b.conversion
	                FROM prv00 a INNER JOIN prv10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION all

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price  / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity, b.quantity * -1) as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'ReturnGoods' as module,b.conversion
	                FROM prg00 a INNER JOIN prg10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.cost / b.conversion as 'cost',IF(a.transtype = 'CD', b.quantity, b.quantity * -1) as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'Sales' as module,b.conversion
	                FROM sapt0 a INNER JOIN sapt1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.cost / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'ReturnFromCustomer' as module,b.conversion
	                FROM sapr0 a INNER JOIN sapr1 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity * -1, b.quantity) as quantity, IF(a.transtype = 'CD', b.transvalue * -1, b.transvalue) as transvalue, 'Receiving' as module,b.conversion
	                FROM iir00 a INNER JOIN iir10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode, b.price / b.conversion as 'cost', IF(a.transtype = 'CD', b.quantity, b.quantity * -1)  as quantity, IF(a.transtype = 'CD', b.transvalue, b.transvalue * -1) as transvalue, 'Issuance' as module,b.conversion
	                FROM iii00 a INNER JOIN iii10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

	                UNION ALL

	                SELECT
		                a.transnum,a.systemdate,a.transdate, a.reference, b.itemcode,b.price / b.conversion as 'cost', b.variance as quantity, b.transvalue, 'InventoryAdjustment' as module,b.conversion
	                FROM iia00 a INNER JOIN iia10 b ON a.transnum = b.transnum
	                WHERE b.itemcode = @itemcode

                ) as x ORDER BY x.systemdate,x.transdate asc;");
        }

        public static StringBuilder GetLedgerSOFOS2()
        {
            return new StringBuilder(@"set @csum := 0; 
                set @runvalue:=0;
                select systemDate,idUser,transNum,module,transDate,reference,crossreference,outQuantity,inQuantity,round(price,2) 'price',round(transValue,2) 'transValue',round(runningQuantity,3) 'runningQuantity',if(runningquantity = 0, @runvalue:=0, round(@runvalue:=@runvalue + transvalue,2))  'runningvalue',runningQty from
                (select x.systemDate, x.idUser,x.transNum,x.module,x.transDate,x.reference,x.crossreference,x.outQuantity,x.inQuantity,
                x.price as price, if(x.inQuantity > 0, transValue, transValue * -1) 'transValue', (@csum:= @csum + IF(x.outQuantity > 0, x.outQuantity * -1 , x.inQuantity)) as runningQuantity, x.runningvalue, x.runningQty as runningQty 
                from(
                
                #ST
                SELECT  a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference, IF(left(reference, 2) = 'CD', 0, 
                CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as outQuantity, IF(left(reference, 2) = 'CD', CONVERT(SUM(b.quantity * b.conversion),
                DECIMAL(12,3)), 0) as inQuantity, CONVERT(b.price/b.conversion, DECIMAL(12,2)) as price, b.itemCode, 'Issuance' as module, runningQty, MIN(runningValue) as runningValue, MAX(transValue) as transValue,
                b.price as cost, b.quantity, b.conversion 
                FROM iii00 a inner join iii10 b on a.transNum = b.transNum  where b.itemCode = @itemCode GROUP BY a.reference
                
                UNION ALL
                
                #RR
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference, IF(left(reference, 2) = 'CD',
                CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3)), 0) as outQuantity, IF(left(reference, 2) = 'CD', 0, 
                CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as inQuantity, 
                CASE 
                    WHEN left(reference, 2) = 'TR' AND b.conversion = 1 THEN
                        round(b.price,2)
                    ELSE
                        round(SUM(b.price/b.conversion), 2) 
                    END as price,
                b.itemCode, 'Receiving' as module, runningQty, runningValue, transValue, b.price as cost, b.quantity, b.conversion 
                FROM iir00 a inner join iir10 b on a.transNum = b.transNum   where b.itemCode = @itemCode GROUP BY a.reference

                UNION ALL
                
                #IA
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference, 
	                CASE WHEN a.transtype <> 'CD' THEN
		                IF(b.variance < 0, CONVERT(abs(b.variance * b.conversion), DECIMAL(12,3)), 0)
	                ELSE
		                IF(b.variance < 0, CONVERT(abs(b.variance * b.conversion), DECIMAL(12,3)), 0)
	                END as outQuantity,
	                CASE WHEN a.transtype <> 'CD' THEN
		                IF(b.variance > 0, CONVERT(abs(b.variance  * b.conversion), DECIMAL(12,3)), 0)
	                ELSE	
		                IF(b.variance > 0, CONVERT(abs(b.variance  * b.conversion), DECIMAL(12,3)), 0)
	                END as inQuantity, 
	                CONVERT(price / conversion, DECIMAL(12,2)) as price, b.itemCode, 'Adjustment' as module, runningQuantity, abs(runningValue) as runningValue, transValue, 0 as cost, 0 as quantity, b.conversion 
                FROM iia00 a inner join iia10 b on a.transNum = b.transNum    where b.itemCode = @itemCode GROUP BY a.reference                             

                UNION ALL
                    
                #RR
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference,
                IF(left(reference, 2) = 'CD', CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3)), 0) as outQuantity, 
                IF(left(reference, 2) = 'CD', 0, CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as inQuantity, 
                CONVERT(SUM(b.price/b.conversion), DECIMAL(12,2))  as price, b.itemCode, 'Purchasing(RV)' as module, 
                runningQty, runningValue, transValue, ROUND(b.price/b.conversion,2) as cost, ROUND(b.quantity * b.conversion, 2) as quantity, b.conversion 
                FROM prv00 a inner join prv10 b on a.transNum = b.transNum  where b.itemCode = @itemCode GROUP BY a.reference

                UNION ALL

                #RG
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference, IF(left(reference, 2) = 'CD', 0, CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as outQuantity, IF(left(reference, 2) = 'CD', CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3)), 0) as inQuantity, CONVERT(SUM(b.price/b.conversion), DECIMAL(12,2)) as price, b.itemCode, 'Purchasing(RG)' as module, runningQty, runningValue, transValue, 0 as cost, 0 as quantity, b.conversion FROM prg00 a inner join prg10 b on a.transNum = b.transNum  where b.itemCode = @itemCode  GROUP BY a.reference

                UNION ALL

                #CG/CI/SI
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, IF(left(reference, 2) = 'CD',a.crossreference,''), IF(left(reference, 2) = 'CD', 0, CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as outQuantity, IF(left(reference, 2) = 'CD', CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3)), 0) as inQuantity, CONVERT(b.cost/b.conversion, DECIMAL(12,2)) as price, b.itemCode, 
                CASE 
                    WHEN LEFT(a.reference,2) = 'CG' OR LEFT(a.reference,2) = 'VS' THEN 'Paiwi'
                    WHEN LEFT(a.crossreference,2) = 'CG' OR LEFT(a.crossreference,2) = 'VS' THEN 'Paiwi'
                ELSE 'POS' END as module, runningQuantity, MIN(runningValue) as runningValue, MAX(transValue) as transValue, 0 as cost, 0 as quantity, b.conversion FROM sapt0 a inner join sapt1 b on a.transNum = b.transNum  where b.itemCode = @itemCode and a.NoEffectOnInventory = false GROUP BY a.reference

                UNION ALL

                #RC
                SELECT a.systemDate, a.idUser, a.transNum, a.transDate, a.reference, a.crossreference, IF(left(reference, 2) = 'CD', CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3)), 0) as inQuantity, IF(left(reference, 2) = 'CD', 0, CONVERT(SUM(b.quantity * b.conversion), DECIMAL(12,3))) as outQuantity, CONVERT(b.cost/b.conversion, DECIMAL(12,2)) as price, b.itemCode, 'Return' as module, runningQty, (select runningValue from sapr1 where transnum=a.transNum and itemcode=b.itemcode order by detailNum desc limit 1) as runningValue, MAX(transValue) 'transValue', 0 as cost, 0 as quantity, b.conversion   
                FROM sapr0 a 
                inner join sapr1 b on a.transNum = b.transNum  where b.itemCode = @itemCode  and a.NoEffectOnInventory = false GROUP BY a.reference
                                
                UNION ALL

                #revaluation
				SELECT a.systemdate, a.iduser, a.transnum, a.transdate, a.reference, null as crossReference, 0 as outQuantity, 0 as inQuantity, b.variance as price, b.itemcode, 'Inventory Revaluation' as module, b.runningQuantity, b.runningvalue, transValue, b.variance as cost, b.runningquantity as quantity, 1 as conversion 
				FROM irev0 a inner join irev1 b ON a.transnum = b.transnum where itemcode = @itemCode group by a.reference
                ) as x order by x.transDate, x.systemDate ASC) a;");
        }

        public static StringBuilder GetRemainingWrongRunVal()
        {
            return new StringBuilder(@"SELECT itemcode FROM ii000 where runningValue < 0
                UNION ALL
                SELECT itemcode FROM ii000 where runningquantity = 0 and runningvalue != 0
                UNION ALL
                SELECT itemcode FROM ii000 where runningquantity > 0 and runningvalue <= 0;");
        }
    }

    public enum Process
    {
        Sales,
        ReturnFromCustomer,
        ReceiveFromVendor,
        ReturnGoods,
        Issuance,
        Receiving,
        InventoryAdjustment
    }
}
