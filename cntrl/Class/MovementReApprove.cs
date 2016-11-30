﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using entity;
using entity.Brillo;
using cntrl.PanelAdv;
using System.Windows;

namespace cntrl.Class
{
    public class MovementReApprove
    {
        public void Start(db db, int ID, entity.App.Names Application)
        {


            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            ValueChange(db, ID, Application);
            QuantityUP(db, ID, Application);
            QuantityDown(db, ID, Application);
            DateChange(db, ID, Application);
            NewMovement(db, ID, Application);

            List<item_movement> item_movementList = new List<item_movement>();
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                item_movementList.AddRange(sales_invoice_detail.item_movement);
            }
            List<item_movement> item_movementListOld = new List<item_movement>();
            foreach (sales_invoice_detail sales_invoice_detail in Oldsales_invoice.sales_invoice_detail)
            {
                item_movementListOld.AddRange(sales_invoice_detail.item_movement);
            }
            ActionPanel _ActionPanel = new ActionPanel();
            _ActionPanel.item_movementList = item_movementList;
            _ActionPanel.item_movementOldList = item_movementListOld;



            Window window = new Window
            {
                Title = "ReApprove",
                Content = _ActionPanel
            };

            window.ShowDialog();

        }
        public void ValueChange(db db, int ID, entity.App.Names Application)
        {
            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                sales_invoice_detail Oldsales_invoice_detail = Oldsales_invoice.sales_invoice_detail.Where(x => x == sales_invoice_detail).FirstOrDefault();
                if (Oldsales_invoice_detail != null)
                {
                    if (sales_invoice_detail.unit_price != Oldsales_invoice_detail.unit_price)
                    {
                        foreach (item_movement item_movement in sales_invoice_detail.item_movement)
                        {
                            item_movement_value item_movement_value = item_movement.item_movement_value.FirstOrDefault();
                            if (item_movement_value != null)
                            {
                                item_movement_value.unit_value = sales_invoice_detail.unit_price;
                            }
                        }
                    }
                }
            }

        }
        public void QuantityUP(db db, int ID, entity.App.Names Application)
        {
            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                sales_invoice_detail Oldsales_invoice_detail = Oldsales_invoice.sales_invoice_detail.Where(x => x == sales_invoice_detail).FirstOrDefault();
                if (Oldsales_invoice_detail != null)
                {
                    if (sales_invoice_detail.quantity != Oldsales_invoice_detail.quantity)
                    {
                        decimal Diff = sales_invoice_detail.quantity - Oldsales_invoice_detail.quantity;
                        foreach (item_movement item_movement in sales_invoice_detail.item_movement)
                        {

                            if (item_movement.parent == null)
                            {
                                item_movement.debit = sales_invoice_detail.quantity;
                            }
                            else
                            {
                                if ((item_movement.parent.credit - item_movement.parent.child.Sum(x => x.debit)) > Diff)
                                {
                                    item_movement.debit = sales_invoice_detail.quantity;
                                }
                                else
                                {
                                    item_movement.debit = sales_invoice_detail.quantity;
                                    Stock stock = new Stock();
                                    List<StockList> Items_InStockLIST = stock.List(sales_invoice_detail.app_location.id_branch, (int)sales_invoice_detail.id_location, sales_invoice_detail.item.item_product.FirstOrDefault().id_item_product);
                                    foreach (StockList StockList in Items_InStockLIST)
                                    {
                                        if (StockList.QtyBalance > item_movement.debit)
                                        {
                                            item_movement.parent = db.item_movement.Where(x => x.id_movement == StockList.MovementID).FirstOrDefault();
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }
        public void QuantityDown(db db, int ID, entity.App.Names Application)
        {
            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                sales_invoice_detail Oldsales_invoice_detail = Oldsales_invoice.sales_invoice_detail.Where(x => x == sales_invoice_detail).FirstOrDefault();
                if (Oldsales_invoice_detail != null)
                {
                    if (sales_invoice_detail.quantity != Oldsales_invoice_detail.quantity)
                    {
                        decimal Diff = sales_invoice_detail.quantity - Oldsales_invoice_detail.quantity;
                        foreach (item_movement item_movement in sales_invoice_detail.item_movement)
                        {

                            if (item_movement.child.Count() == 0)
                            {
                                item_movement.debit = sales_invoice_detail.quantity;
                            }
                            else
                            {
                                if ((item_movement.child.Sum(x => x.credit)) > Diff)
                                {
                                    item_movement.debit = sales_invoice_detail.quantity;
                                }
                                else
                                {
                                    item_movement.debit = sales_invoice_detail.quantity;
                                    foreach (item_movement item in item_movement.child)
                                    {
                                        Stock stock = new Stock();

                                        List<StockList> Items_InStockLIST = stock.DebitList(sales_invoice_detail.app_location.id_branch, (int)sales_invoice_detail.id_location, sales_invoice_detail.item.item_product.FirstOrDefault().id_item_product);
                                        foreach (StockList StockList in Items_InStockLIST)
                                        {
                                            if (StockList.QtyBalance > item.credit)
                                            {
                                                item_movement.parent = db.item_movement.Where(x => x.id_movement == StockList.MovementID).FirstOrDefault();
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

        }

        public void DateChange(db db, int ID, entity.App.Names Application)
        {
            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                sales_invoice_detail Oldsales_invoice_detail = Oldsales_invoice.sales_invoice_detail.Where(x => x == sales_invoice_detail).FirstOrDefault();
                if (Oldsales_invoice_detail != null)
                {
                    if (sales_invoice_detail.quantity != Oldsales_invoice_detail.quantity)
                    {
                        decimal Diff = sales_invoice_detail.quantity - Oldsales_invoice_detail.quantity;
                        foreach (item_movement item_movement in sales_invoice_detail.item_movement)
                        {
                            item_movement.trans_date = sales_invoice.trans_date;
                        }
                    }
                }
            }
        }
        public void NewMovement(db db, int ID, entity.App.Names Application)
        {
            sales_invoice Oldsales_invoice = db.sales_invoice.Where(x => x.id_sales_invoice == ID).FirstOrDefault();
            sales_invoice sales_invoice = db.sales_invoice.Find(ID);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                sales_invoice_detail Oldsales_invoice_detail = Oldsales_invoice.sales_invoice_detail.Where(x => x == sales_invoice_detail).FirstOrDefault();
                if (Oldsales_invoice_detail == null)
                {
                    Stock stock = new Stock();
                    entity.Brillo.Logic.Stock Stock = new entity.Brillo.Logic.Stock();
                    List<StockList> Items_InStockLIST = stock.List(sales_invoice_detail.app_location.id_branch, (int)sales_invoice_detail.id_location, sales_invoice_detail.item.item_product.FirstOrDefault().id_item_product);

                    db.item_movement.AddRange(Stock.DebitOnly_MovementLIST(db, Items_InStockLIST, Status.Stock.InStock,
                                             App.Names.SalesInvoice,
                                                            sales_invoice_detail.id_sales_invoice,
                                                            sales_invoice_detail.id_sales_invoice_detail,
                                                            sales_invoice.id_currencyfx,
                                                            sales_invoice_detail.item.item_product.FirstOrDefault(),
                                                            (int)sales_invoice_detail.id_location,
                                                            sales_invoice_detail.quantity,
                                                            sales_invoice.trans_date,
                                                         "Sales Invoice Fix"
                                                            ));
                }
            }
        }

    }
}
