﻿using System;
using System.Collections.Generic;
using System.Linq;
using WPFLocalizeExtension.Extensions;

namespace entity.Brillo.Logic
{
    public class Stock
    {
        /// <summary>
        /// Will take the Entity, check child rows, validate, and insert the necesary transactions into Item_Movement table.
        /// </summary>
        /// <param name="obj_entity">Pass Table Entity such as Sales Invoice or Purchase Order</param>
        /// <returns></returns>
        public List<item_movement> insert_Stock(db db, object obj_entity)
        {
            List<item_movement> item_movementList = new List<item_movement>();

            //SALES INVOICE
            if (obj_entity.GetType().BaseType == typeof(sales_invoice) || obj_entity.GetType() == typeof(sales_invoice))
            {
                sales_invoice sales_invoice = (sales_invoice)obj_entity;

                foreach (sales_invoice_detail detail in sales_invoice.sales_invoice_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {
                    item_product item_product = FindNFix_ItemProduct(detail.item);
                    detail.id_location = FindNFix_Location(item_product, detail.app_location, sales_invoice.app_branch);
                    detail.app_location = db.app_location.Where(x => x.id_location == detail.id_location).FirstOrDefault();
                    //Add Logic for removing Reserved Stock 
                    if (item_product != null && detail.sales_order_detail != null)
                    {
                        //Adding into List
                        item_movementList.Add(Debit_Movement(entity.Status.Stock.Reserved,
                                            App.Names.SalesInvoice,
                                            detail.id_sales_invoice,
                                            item_product,
                                            detail.app_location,
                                            detail.quantity,
                                            sales_invoice.trans_date,
                                            comment_Generator(App.Names.SalesInvoice, sales_invoice.number, sales_invoice.contact.name)
                                            ));
                    }

                    item_movementList = Debit_MovementLIST(db,entity.Status.Stock.InStock,
                                             App.Names.SalesInvoice,
                                             detail.id_sales_invoice,
                                             sales_invoice.app_currencyfx,
                                             item_product,
                                             detail.app_location,
                                             detail.quantity,
                                             sales_invoice.trans_date,
                                             comment_Generator(App.Names.SalesInvoice, sales_invoice.number, sales_invoice.contact.name)
                                             );
                }
                //Return List so we can save into context.
                return item_movementList;
            }

            //SALES RETURN
            else if (obj_entity.GetType().BaseType == typeof(sales_return) || obj_entity.GetType() == typeof(sales_return))
            {
                sales_return sales_return = (sales_return)obj_entity;
                foreach (sales_return_detail sales_return_detail in sales_return.sales_return_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {
                    item_product item_product = FindNFix_ItemProduct(sales_return_detail.item);
                    sales_return_detail.id_location = FindNFix_Location(item_product, sales_return_detail.app_location, sales_return.app_branch);
                    sales_return_detail.app_location = db.app_location.Where(x => x.id_location == sales_return_detail.id_location).FirstOrDefault();
                    item_movementList.Add(Credit_Movement(entity.Status.Stock.InStock,
                                             App.Names.SalesReturn,
                                             sales_return_detail.id_sales_return,
                                             sales_return.id_currencyfx,
                                             item_product,
                                             (int)sales_return_detail.id_location,
                                             sales_return_detail.quantity,
                                             sales_return.trans_date,
                                             comment_Generator(App.Names.SalesReturn, sales_return.number, sales_return.contact.name)
                                             ));
                }
                //Return List so we can save into context.
                return item_movementList;

            }

            //PURCHASE RETURN
            else if (obj_entity.GetType().BaseType == typeof(purchase_return) || obj_entity.GetType() == typeof(purchase_return))
            {
                purchase_return purchase_return = (purchase_return)obj_entity;
                List<purchase_return_detail> Listpurchase_return_detail = purchase_return.purchase_return_detail.Where(x => x.id_item > 0).ToList();
                foreach (purchase_return_detail purchase_return_detail in Listpurchase_return_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {

                    item_product item_product = FindNFix_ItemProduct(purchase_return_detail.item);
                    purchase_return_detail.id_location = FindNFix_Location(item_product, purchase_return_detail.app_location, purchase_return.app_branch);
                    purchase_return_detail.app_location = db.app_location.Where(x => x.id_location == purchase_return_detail.id_location).FirstOrDefault();


                    item_movementList = Debit_MovementLIST(db, entity.Status.Stock.InStock,
                                             App.Names.PurchaseReturn,
                                             purchase_return_detail.id_purchase_return,
                                             purchase_return.app_currencyfx,
                                             item_product,
                                             purchase_return_detail.app_location,
                                             purchase_return_detail.quantity,
                                             purchase_return.trans_date,
                                             comment_Generator(App.Names.PurchaseReturn, purchase_return.number, purchase_return.contact.name)
                                             );
                }
                //Return List so we can save into context.
                return item_movementList;
            }

            //SALES ORDER
            else if (obj_entity.GetType().BaseType == typeof(sales_order) || obj_entity.GetType() == typeof(sales_order))
            {
                sales_order sales_order = (sales_order)obj_entity;
                foreach (sales_order_detail sales_order_detail in sales_order.sales_order_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {

                    item_product item_product = FindNFix_ItemProduct(sales_order_detail.item);
                    sales_order_detail.id_location = FindNFix_Location(item_product, sales_order_detail.app_location, sales_order.app_branch);
                    sales_order_detail.app_location = db.app_location.Where(x => x.id_location == sales_order_detail.id_location).FirstOrDefault();
                    //Add Logic for removing Reserved Stock 
                    if (item_product != null && sales_order_detail.sales_budget_detail != null)
                    {
                        //Adding into List
                        item_movementList.Add(Debit_Movement(entity.Status.Stock.Reserved,
                                            App.Names.SalesOrder,
                                            sales_order_detail.id_sales_order,
                                            item_product,
                                            sales_order_detail.app_location,
                                            sales_order_detail.quantity,
                                            sales_order.trans_date,
                                            comment_Generator(App.Names.SalesOrder, sales_order.number, sales_order.contact.name)
                                            ));
                    }

                    item_movementList = Debit_MovementLIST(db, entity.Status.Stock.InStock,
                                             App.Names.SalesOrder,
                                            sales_order_detail.id_sales_order,
                                              sales_order.app_currencyfx,
                                             item_product,
                                             sales_order_detail.app_location,
                                             sales_order_detail.quantity,
                                              sales_order.trans_date,
                                             comment_Generator(App.Names.SalesOrder, sales_order.number, sales_order.contact.name)
                                             );
                }

                //Return List so we can save into context.
            }

            //PURCHASE INVOICE
            else if (obj_entity.GetType().BaseType == typeof(purchase_invoice) || obj_entity.GetType() == typeof(purchase_invoice))
            {
                purchase_invoice purchase_invoice = (purchase_invoice)obj_entity;
                List<purchase_invoice_detail> Listpurchase_invoice_detail = purchase_invoice.purchase_invoice_detail.Where(x => x.id_item > 0).ToList();
                foreach (purchase_invoice_detail purchase_invoice_detail in Listpurchase_invoice_detail
                        .Where(x => x.item.item_product.Count() > 0))
                {

                    item_product item_product = FindNFix_ItemProduct(purchase_invoice_detail.item);
                    purchase_invoice_detail.id_location = FindNFix_Location(item_product, purchase_invoice_detail.app_location, purchase_invoice.app_branch);
                    purchase_invoice_detail.app_location = db.app_location.Where(x => x.id_location == purchase_invoice_detail.id_location).FirstOrDefault();
                    if (purchase_invoice_detail.purchase_order_detail != null)
                    {
                        item_movementList = Debit_MovementLIST(db, entity.Status.Stock.OnTheWay,
                                             App.Names.PurchaseInvoice,
                                             purchase_invoice_detail.id_purchase_invoice,
                                             purchase_invoice.app_currencyfx,
                                             item_product,
                                             purchase_invoice_detail.app_location,
                                             purchase_invoice_detail.quantity,
                                              purchase_invoice.trans_date,
                                             comment_Generator(App.Names.PurchaseInvoice, purchase_invoice.number, purchase_invoice.contact.name)
                                             );

                        //Adding into List
                      // item_movementList.Add(mov_OnTheWay);
                    }

                    //Improve Comment. More standarized.
                    item_movementList.Add(Credit_Movement(entity.Status.Stock.InStock,
                                              App.Names.PurchaseInvoice,
                                              purchase_invoice_detail.id_purchase_invoice,
                                              purchase_invoice.id_currencyfx,
                                              item_product,
                                              (int)purchase_invoice_detail.id_location,
                                              purchase_invoice_detail.quantity,
                                              purchase_invoice.trans_date,
                                              comment_Generator(App.Names.PurchaseInvoice, purchase_invoice.number, purchase_invoice.contact.name)
                                              ));
                }
                //Return List so we can save into context.
                return item_movementList;
            }

            //PURCHASE ORDER
            else if (obj_entity.GetType().BaseType == typeof(purchase_order) || obj_entity.GetType() == typeof(purchase_order))
            {
                purchase_order purchase_order = (purchase_order)obj_entity;
                List<purchase_order_detail> Listpurchase_order_detail = purchase_order.purchase_order_detail.Where(x => x.id_item > 0).ToList();
                foreach (purchase_order_detail purchase_order_detail in Listpurchase_order_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {
                    item_product item_product = FindNFix_ItemProduct(purchase_order_detail.item);
                    purchase_order_detail.id_location = FindNFix_Location(item_product, purchase_order_detail.app_location, purchase_order.app_branch);
                    purchase_order_detail.app_location = db.app_location.Where(x => x.id_location == purchase_order_detail.id_location).FirstOrDefault();
                    if (item_product != null)
                    {
                        item_movementList.Add(Credit_Movement(entity.Status.Stock.InStock,
                                                App.Names.PurchaseOrder,
                                                purchase_order_detail.id_purchase_order,
                                                purchase_order.id_currencyfx,
                                                item_product,
                                                (int)purchase_order_detail.id_location,
                                                purchase_order_detail.quantity,
                                                purchase_order.trans_date,
                                                comment_Generator(App.Names.PurchaseOrder, purchase_order.number, purchase_order.contact.name)
                                                ));
                    }
                }
                //Return List so we can save into context.
                return item_movementList;
            }
            //production Execustion
            if (obj_entity.GetType().BaseType == typeof(production_execution) || obj_entity.GetType() == typeof(production_execution))
            {
                production_execution production_execution = (production_execution)obj_entity;

                foreach (production_execution_detail detail in production_execution.production_execution_detail
                    .Where(x => x.item.item_product.Count() > 0))
                {
                    item_product item_product = FindNFix_ItemProduct(detail.item);

                            if (detail.is_input)
                            {
                                item_movementList = Debit_MovementLIST(db, entity.Status.Stock.InStock,
                                                        App.Names.ProductionExecution,
                                               detail.id_production_execution,
                                            Currency.get_Default(CurrentSession.Id_Company).app_currencyfx.Where(x => x.is_active).FirstOrDefault(),
                                                        item_product,
                                                        production_execution.production_line.app_location,
                                               detail.quantity,
                                                        production_execution.trans_date,
                                                        comment_Generator(App.Names.ProductionExecution,
                                                        production_execution.id_production_execution.ToString(), "")
                                               );

                            }
                            else
                            {
                        item_movementList.Add(Credit_Movement(entity.Status.Stock.InStock,
                                                App.Names.ProductionExecution,
                                                detail.id_production_execution,
                                            Currency.get_Default(CurrentSession.Id_Company).app_currencyfx.Where(x => x.is_active).FirstOrDefault().id_currencyfx,
                                                item_product,
                                               production_execution.production_line.app_location.id_location,
                                               detail.quantity,
                                                production_execution.trans_date,
                                           comment_Generator(App.Names.ProductionExecution,
                                                        production_execution.id_production_execution.ToString(), "")
                                            ));

                    }

                    // }

                }
                //Return List so we can save into context.
                return item_movementList;
            }

            return null;
        }

        /// <summary>
        /// Will Delete all records found matching Application ID and Transaction ID from Database.
        /// </summary>
        /// <param name="Application_ID">ID of Source Application. OfType app.Applications</param>
        /// <param name="Transaction_ID">ID of Source Transaction from DB.</param>
        /// <returns></returns>
        public List<item_movement> revert_Stock(db db, App.Names Application_ID, int Transaction_ID)
        {
            List<item_movement> item_movementList = new List<item_movement>();

            //using (db db = new db())
            //{
            item_movementList = db.item_movement
                                        .Where(x => x.id_application == Application_ID
                                            && x.transaction_id == Transaction_ID).ToList();
            if (item_movementList != null)
            {
                return item_movementList;
            }
            // }

            return null;
        }

        public string comment_Generator(App.Names AppName, string TransNumber, string ContactName)
        {
            string strAPP = LocExtension.GetLocalizedValue<string>("Cognitivo:local:" + AppName.ToString());
            return string.Format(strAPP + " {0} / {1}", TransNumber, ContactName);
        }


        public List<item_movement> Debit_MovementLIST(db db, entity.Status.Stock Status, App.Names ApplicationID, int TransactionID,
                                                       app_currencyfx app_currencyfx, item_product item_product, app_location app_location,
                                                       decimal Quantity, DateTime TransDate,
                                          string Comment)
        {

            List<item_movement> Items_InStockLIST = new List<item_movement>();
            List<item_movement> Final_ItemMovementLIST = new List<item_movement>();

            int id_location = app_location.id_location;
            int id_item_product = item_product.id_item_product;
            //using (db db = new db())
            //{
                
                Items_InStockLIST = db.item_movement.Where(x => x.id_location == id_location
                                                                      && x.id_item_product == id_item_product
                                                                      && x.status == entity.Status.Stock.InStock
                                                                      && (x.credit - (x._child.Count() > 0 ? x._child.Sum(y => y.debit) : 0)) > 0).ToList();

                if (item_product.cogs_type == item_product.COGS_Types.LIFO && Items_InStockLIST != null)
                {
                    Items_InStockLIST = Items_InStockLIST.OrderBy(x => x.trans_date).ToList();
                }
                else if (Items_InStockLIST != null)
                {
                    Items_InStockLIST = Items_InStockLIST.OrderByDescending(x => x.trans_date).ToList();
                }

                decimal qty_SalesDetail = Quantity;
                
                ///Will create new Item Movements 
                ///if split from Parents is needed.
                foreach (item_movement parent_Movement in Items_InStockLIST)
                {
                    if (qty_SalesDetail > 0)
                    {
                        item_movement item_movement = new item_movement();

                        decimal movement_debit_quantity = qty_SalesDetail;

                        //If Parent Movement is lesser than Quantity, then only take total value of Parent.
                        if (parent_Movement.credit <= qty_SalesDetail)
                        {
                            movement_debit_quantity = parent_Movement.credit;
                        }

                        item_movement.comment = Comment;
                        item_movement.id_item_product = item_product.id_item_product;
                        item_movement.debit = movement_debit_quantity;
                        item_movement.credit = 0;
                        item_movement.status = Status;
                        item_movement.id_location = app_location.id_location;
                        item_movement._parent = null;
                        item_movement.id_application = ApplicationID;
                        item_movement.transaction_id = TransactionID;
                        item_movement.trans_date = TransDate;

                        item_movement._parent = parent_Movement;

                        //Logic for Value
                        item_movement_value item_movement_value = new item_movement_value();

                        item_movement_value.unit_value = parent_Movement.GetValue_ByCurrency(app_currencyfx.app_currency);
                        item_movement_value.id_currencyfx = app_currencyfx.id_currencyfx;
                        item_movement_value.comment = Brillo.Localize.StringText("DirectCost");
                        item_movement.item_movement_value.Add(item_movement_value);

                        //Adding into List
                        Final_ItemMovementLIST.Add(item_movement);
                        qty_SalesDetail = qty_SalesDetail - parent_Movement.credit;
                    }
                }

                ///In case Parent does not exist, will enter this code.
                if (qty_SalesDetail > 0)
                {
                    item_movement item_movement = new item_movement();
                    //Adding into List if Movement List for this Location is empty.
                    item_movement.comment = Comment;
                    item_movement.id_item_product = item_product.id_item_product;
                    item_movement.debit = Quantity;
                    item_movement.credit = 0;
                    item_movement.status = Status;
                    item_movement.id_location = app_location.id_location;
                    item_movement._parent = null;
                    item_movement.id_application = ApplicationID;
                    item_movement.transaction_id = TransactionID;
                    item_movement.trans_date = TransDate;

                    item_movement._parent = null;

                    //Logic for Value in case Parent does not Exist, we will take from 
                    item_movement_value item_movement_value = new item_movement_value();
                    item_movement_value.unit_value = (decimal)item_product.item.unit_cost;
                    item_movement_value.id_currencyfx = app_currencyfx.id_currencyfx;
                    item_movement_value.comment = Brillo.Localize.StringText("DirectCost");
                    item_movement.item_movement_value.Add(item_movement_value);
                    //Adding into List
                    Final_ItemMovementLIST.Add(item_movement);
                }
            //}
            return Final_ItemMovementLIST;
        }

     
        public item_movement Credit_Movement( entity.Status.Stock Status, App.Names ApplicationID, int TransactionID,
                                              int CurrencyFXID, item_product item_product, int LocationID,
                                              decimal Quantity, DateTime TransDate, string Comment)
        {
                if (Quantity > 0)
                {
                    item_movement item_movement = new item_movement();
                    //Adding into List if Movement List for this Location is empty.
                    item_movement.comment = Comment;
                    item_movement.id_item_product = item_product.id_item_product;
                    item_movement.debit = 0;
                    item_movement.credit = Quantity;
                    item_movement.status = Status;
                    item_movement.id_location = LocationID;
                    item_movement._parent = null;
                    item_movement.id_application = ApplicationID;
                    item_movement.transaction_id = TransactionID;
                    item_movement.trans_date = TransDate;

                    item_movement._parent = null;

                    //Logic for Value in case Parent does not Exist, we will take from 
                    item_movement_value item_movement_value = new item_movement_value();
                    item_movement_value.unit_value = (decimal)item_product.item.unit_cost;
                    item_movement_value.id_currencyfx = CurrencyFXID;
                    item_movement_value.comment = Brillo.Localize.StringText("DirectCost");
                    item_movement.item_movement_value.Add(item_movement_value);

                    return item_movement;
                }

                return null;
        }

        public List<item_movement> DebitCredit_MovementList(db db, entity.Status.Stock Status, App.Names ApplicationID, int TransactionID,
                                              app_currencyfx app_currencyfx, item_product item_product, app_location app_location,
                                              decimal Quantity, DateTime TransDate, string Comment, decimal unit_price)
        {
            List<item_movement> Final_ItemMovementLIST = new List<item_movement>();

            //Bring Debit Function form above. IT should handle child and parent values.
            List<item_movement> debit_movementLIST = new List<item_movement>();
            debit_movementLIST = Debit_MovementLIST(db, Status, ApplicationID, TransactionID, app_currencyfx, 
                                                    item_product, app_location, Quantity, TransDate, Comment);

            List<item_movement> credit_movementLIST = new List<item_movement>();
            foreach (item_movement debit_movement in debit_movementLIST)
            {
                item_movement credit_movement = new item_movement();
                credit_movement = Credit_Movement(Status, ApplicationID, TransactionID, app_currencyfx.id_currencyfx,
                                              item_product, app_location.id_location, debit_movement.debit, TransDate,
                                              Comment);
                credit_movement._parent = debit_movement;
                credit_movementLIST.Add(credit_movement);
            }

            if (credit_movementLIST.Count > 0)
            {
                Final_ItemMovementLIST.AddRange(credit_movementLIST);
            }
            
            return Final_ItemMovementLIST;
        }

        public item_movement Debit_Movement( entity.Status.Stock Status, App.Names ApplicationID, int TransactionID,
                                             item_product item_product, app_location app_location, decimal Quantity,
                                             DateTime TransDate, string Comment)
        {
            item_movement item_movement = new item_movement();
            item_movement.comment = Comment;
            item_movement.id_item_product = item_product.id_item_product;
            item_movement.debit = Quantity;
            item_movement.credit = 0;
            item_movement.status = Status;
            item_movement.id_location = app_location.id_location;
            item_movement.id_application = ApplicationID;
            item_movement.transaction_id = TransactionID;
            item_movement.trans_date = TransDate;
            return item_movement;
        }


        public item_movement credit_Movement(
          entity.Status.Stock Status,
          App.Names ApplicationID,
          int TransactionID,
          int Item_ProductID,
          int LocationID,
          decimal Quantity,
          DateTime TransDate,
          string Comment)
        {
            item_movement item_movement = new item_movement();
            item_movement.comment = Comment;
            item_movement.id_item_product = Item_ProductID;
            item_movement.debit = 0;
            item_movement.credit = Quantity;
            item_movement.status = Status;
            item_movement.id_location = LocationID;
            item_movement.id_application = ApplicationID;
            item_movement.transaction_id = TransactionID;
            item_movement.trans_date = TransDate;
            return item_movement;
        }

        #region
        public item_product FindNFix_ItemProduct(item item)
        {
            if (item.item_product == null)
            {
                using (db db = new db())
                {
                    item_product item_product = new item_product();
                    item.item_product.Add(item_product);
                    db.items.Attach(item);
                    db.SaveChangesAsync();
                    return item_product;
                }
            }
            return item.item_product.FirstOrDefault();
        }

        public int FindNFix_Location(item_product item_product, app_location app_location, app_branch app_branch)
        {
            if (app_location == null && app_branch != null)
            {
                Location Location = new Location();
                return Location.get_Location(item_product, app_branch);
            }

            return app_location.id_location;
        }
        #endregion
    }
}
