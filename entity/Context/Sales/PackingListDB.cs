﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace entity
{
    public partial class PackingListDB : BaseDB
    {
        public sales_packing New()
        {
            sales_packing sales_packing = new sales_packing();
            sales_packing.State = EntityState.Added;
            sales_packing.app_document_range = Brillo.Logic.Range.List_Range(this, App.Names.PackingList, CurrentSession.Id_Branch, CurrentSession.Id_Terminal).FirstOrDefault();
            sales_packing.IsSelected = true;

            return sales_packing;
        }

        public override int SaveChanges()
        {
            validate_PackingList();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            validate_PackingList();
            return base.SaveChangesAsync();
        }

        private void validate_PackingList()
        {
            foreach (sales_packing sales_packing in base.sales_packing.Local)
            {
                if (sales_packing.IsSelected && sales_packing.Error == null)
                {
                    if (sales_packing.State == EntityState.Added)
                    {
                        sales_packing.timestamp = DateTime.Now;
                        sales_packing.State = EntityState.Unchanged;
                        Entry(sales_packing).State = EntityState.Added;
                        add_CRM(sales_packing);
                    }
                    else if (sales_packing.State == EntityState.Modified)
                    {
                        sales_packing.timestamp = DateTime.Now;
                        sales_packing.State = EntityState.Unchanged;
                        Entry(sales_packing).State = EntityState.Modified;
                    }
                    else if (sales_packing.State == EntityState.Deleted)
                    {
                        sales_packing.timestamp = DateTime.Now;
                        sales_packing.is_head = false;
                        sales_packing.State = EntityState.Deleted;
                        Entry(sales_packing).State = EntityState.Modified;
                    }
                }
                if (sales_packing.State > 0)
                {
                    if (sales_packing.State != EntityState.Unchanged)
                    {
                        Entry(sales_packing).State = EntityState.Unchanged;
                    }
                }
            }
        }

        private void add_CRM(sales_packing sales_packing)
        {
            if (sales_packing.id_sales_packing == 0 || sales_packing == null)
            {
                crm_opportunity crm_opportunity = new crm_opportunity();
                crm_opportunity.id_contact = sales_packing.id_contact;

                crm_opportunity.sales_packing.Add(sales_packing);
                base.crm_opportunity.Add(crm_opportunity);
            }
            else
            {
                crm_opportunity crm_opportunity = sales_order.Find(sales_packing.id_sales_packing).crm_opportunity;
                crm_opportunity.sales_packing.Add(sales_packing);
                base.crm_opportunity.Attach(crm_opportunity);
            }
        }

        public void Approve()
        {
            NumberOfRecords = 0;

            foreach (sales_packing sales_packing in base.sales_packing.Local)
            {
                if (sales_packing.IsSelected && sales_packing.Error == null)
                {
                    if (sales_packing.id_sales_packing == 0)
                    {
                        SaveChanges();
                    }

                    if (sales_packing.status != Status.Documents_General.Approved)
                    {
                        CurrentItems.getProducts_InStock(sales_packing.id_branch, DateTime.Now, true);

                        Brillo.Logic.Stock _Stock = new Brillo.Logic.Stock();
                        List<item_movement> item_movementList = new List<item_movement>();
                        item_movementList = _Stock.SalesPacking_Approve(this, sales_packing);

                        if (item_movementList != null && item_movementList.Count > 0)
                        {
                            item_movement.AddRange(item_movementList);
                        }

                        if (sales_packing.number == null && sales_packing.id_range > 0)
                        {
                            Brillo.Logic.Range.branch_Code = CurrentSession.Branches.Where(x => x.id_branch == sales_packing.id_branch).FirstOrDefault().code;
                            Brillo.Logic.Range.terminal_Code = CurrentSession.Terminals.Where(x => x.id_terminal == sales_packing.id_terminal).FirstOrDefault().code;

                            app_document_range app_document_range = base.app_document_range.Find(sales_packing.id_range);
                            sales_packing.number = Brillo.Logic.Range.calc_Range(app_document_range, true);
                            sales_packing.RaisePropertyChanged("number");
                            sales_packing.is_issued = true;

                            Brillo.Document.Start.Automatic(sales_packing, app_document_range);
                        }
                        else
                        {
                            sales_packing.is_issued = false;
                        }

                        sales_packing.status = Status.Documents_General.Approved;
                        SaveChanges();
                    }

                    NumberOfRecords += 1;
                }

                if (sales_packing.Error != null)
                {
                    sales_packing.HasErrors = true;
                }
            }
        }

        public void Annull()
        {
            foreach (sales_packing sales_packing in base.sales_packing.Local)
            {
                if (sales_packing.IsSelected && sales_packing.Error == null)
                {
                    Brillo.Logic.Stock _Stock = new Brillo.Logic.Stock();
                    List<item_movement> item_movementList = new List<item_movement>();
                    item_movementList = _Stock.revert_Stock(this, App.Names.PackingList, sales_packing);



                    if (item_movementList != null && item_movementList.Count > 0)
                    {
                        foreach (item_movement item_movement in item_movementList)
                        {
                            if (item_movement.id_sales_invoice_detail == null)
                            {
                                base.item_movement.Remove(item_movement);
                            }
                            else
                            {
                                item_movement.id_sales_packing_detail = null;
                            }
                        }
                       
                    }

                    foreach (sales_packing_detail sales_packing_detail in sales_packing.sales_packing_detail)
                    {
                        sales_packing_detail.id_sales_order_detail = null;
                    }

                    sales_packing.status = Status.Documents_General.Annulled;
                    SaveChanges();
                }
            }
        }
    }
}