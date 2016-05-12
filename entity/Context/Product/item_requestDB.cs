﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entity
{
    public partial class item_requestDB : BaseDB
    {
        public override int SaveChanges()
        {
            validate_Item_Request();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            validate_Item_Request();
            return base.SaveChangesAsync();
        }

        private void validate_Item_Request()
        {
            foreach (item_request item_request in base.item_request.Local)
            {
                if (item_request.IsSelected)
                {
                    if (item_request.State == EntityState.Added)
                    {
                        item_request.timestamp = DateTime.Now;
                        item_request.State = EntityState.Unchanged;
                        Entry(item_request).State = EntityState.Added;
                    }
                    else if (item_request.State == EntityState.Modified)
                    {
                        item_request.timestamp = DateTime.Now;
                        item_request.State = EntityState.Unchanged;
                        Entry(item_request).State = EntityState.Modified;
                    }
                    else if (item_request.State == EntityState.Deleted)
                    {
                        item_request.timestamp = DateTime.Now;
                        item_request.State = EntityState.Unchanged;
                        base.item_request.Remove(item_request);
                    }
                }
                else if (item_request.State > 0)
                {
                    if (item_request.State != EntityState.Unchanged)
                    {
                        Entry(item_request).State = EntityState.Unchanged;
                    }
                }
            }
        }

        public void Approve()
        {

            foreach (item_request item_request in base.item_request.Local.Where(x =>
                                                x.status != Status.Documents_General.Approved
                                                        && x.IsSelected))
            {
                item_transfer item_transfer = new item_transfer();


                item_transfer.status = Status.Documents_General.Pending;


                entity.Properties.Settings setting = new Properties.Settings();
                item_transfer.user_requested = base.security_user.Where(x => x.id_user == CurrentSession.Id_User).FirstOrDefault();
                item_transfer.id_item_request = item_request.id_item_request;
                if (base.app_department.FirstOrDefault()!=null)
                {
                    item_transfer.id_department = base.app_department.FirstOrDefault().id_department;
                    
                }
                
                if (base.app_document_range.Where(x => x.app_document.id_application == App.Names.Movement).FirstOrDefault() != null)
                {
                    item_transfer.id_range = base.app_document_range.Where(x => x.app_document.id_application == App.Names.Movement).FirstOrDefault().id_range;
                }




                purchase_tender purchase_tender = new purchase_tender();
                purchase_tender.status = Status.Documents_General.Pending;
                purchase_tender.id_department = item_request.id_department;

                purchase_tender.name = item_request.name;
                purchase_tender.code = 000;
                purchase_tender.trans_date = item_request.request_date;
                purchase_tender.comment = item_request.comment;


                production_order production_order = new production_order();
                production_order.status = Status.Production.InProcess;
                production_order.name = item_request.name;
                if (production_line.FirstOrDefault() != null)
                {
                    production_order.id_production_line = production_line.FirstOrDefault().id_production_line;
                }


                foreach (item_request_detail item_request_detail in item_request.item_request_detail)
                {
                    foreach (item_request_decision item in item_request_detail.item_request_decision)
                    {
                        //if (item.IsSelected == true)
                        //{
                        if (item.decision == entity.item_request_decision.Decisions.Movement)
                        {
                            ProductTransferDB ProductTransferDB = new entity.ProductTransferDB();
                            item_transfer _item_transfer = new entity.item_transfer();
                            _item_transfer.status = Status.Documents_General.Pending;
                            _item_transfer.IsSelected = true;

                            _item_transfer.State = EntityState.Added;
                            _item_transfer.user_requested = ProductTransferDB.security_user.Where(x => x.id_user == CurrentSession.Id_User).FirstOrDefault();
                            _item_transfer.id_item_request = item_request.id_item_request;
                            if (base.app_department.FirstOrDefault() != null)
                            {
                                _item_transfer.id_department = ProductTransferDB.app_department.FirstOrDefault().id_department;
                            }
                            if (ProductTransferDB.app_document_range.Where(x => x.app_document.id_application == App.Names.Movement).FirstOrDefault() != null)
                            {
                                _item_transfer.id_range = ProductTransferDB.app_document_range.Where(x => x.app_document.id_application == App.Names.Movement).FirstOrDefault().id_range;
                            }

                            int id_location = (int)item.id_location;
                            _item_transfer.app_location_origin = ProductTransferDB.app_location.Where(x => x.id_location == id_location).FirstOrDefault();
                            _item_transfer.app_branch_origin = ProductTransferDB.app_location.Where(x => x.id_location == id_location).FirstOrDefault().app_branch;
                            _item_transfer.comment = "Transfer item Request from " + item.decision.ToString();


                            //Create Transfer Detail in DB.
                            item_transfer_detail item_transfer_detail = new item_transfer_detail();
                            item_transfer_detail.id_item_product = item_request_detail.item.item_product.FirstOrDefault().id_item_product;
                            item_transfer_detail.item_product = ProductTransferDB.item_product.Where(x => x.id_item_product == item_transfer_detail.id_item_product).FirstOrDefault();
                            item_transfer_detail.quantity_origin = item.quantity;
                            item_transfer_detail.quantity_destination = item.quantity;


                            if (item_request_detail.id_project_task != null)
                            {
                                //Transfer related to Project because there is a Project.
                                item_transfer_detail.id_project_task = item_request_detail.project_task.id_project_task;

                                int id_branch = (int)ProductTransferDB.projects.Where(x => x.id_project == item_request_detail.project_task.id_project).FirstOrDefault().id_branch;
                                _item_transfer.app_location_destination = ProductTransferDB.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault().app_location.Where(x => x.is_default).FirstOrDefault();
                                _item_transfer.app_branch_destination = ProductTransferDB.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault();
                                _item_transfer.id_project = item_request_detail.project_task.id_project;
                            }

                            if (item_request_detail.id_sales_order_detail != null)
                            {
                                _item_transfer.app_location_destination = ProductTransferDB.app_branch.Where(x => x.id_branch == item_request.sales_order.app_branch.id_branch).FirstOrDefault().app_location.Where(x => x.is_default).FirstOrDefault();
                                _item_transfer.app_branch_destination = ProductTransferDB.app_branch.Where(x => x.id_branch == item_request.sales_order.app_branch.id_branch).FirstOrDefault();
                            }

                            if (item_request_detail.id_order_detail != null)
                            {
                                //Get Production Line
                                int id_production_line = ProductTransferDB.production_order_detail.Where(x => x.id_order_detail == item_request_detail.id_order_detail).FirstOrDefault().production_order.id_production_line;
                                //Get Location based on Line
                                app_location app_location = ProductTransferDB.production_line.Where(x => x.id_production_line == id_production_line).FirstOrDefault().app_location;
                                _item_transfer.app_location_destination = app_location;
                                //Get Branch based on Location
                                _item_transfer.app_branch_destination = ProductTransferDB.app_branch.Where(x => x.id_branch == app_location.id_branch).FirstOrDefault();
                            }

                            _item_transfer.item_transfer_detail.Add(item_transfer_detail);
                            _item_transfer.transfer_type = entity.item_transfer.Transfer_type.movemnent;

                            ProductTransferDB.item_transfer.Add(_item_transfer);
                            ProductTransferDB.SaveChanges();
                        }

                        else if (item.decision == entity.item_request_decision.Decisions.Transfer)
                        {
                            int id_location = (int)item.id_location;
                            item_transfer.app_location_origin = base.app_location.Where(x => x.id_location == id_location).FirstOrDefault();
                            item_transfer.app_branch_origin = base.app_location.Where(x => x.id_location == id_location).FirstOrDefault().app_branch;
                            item_transfer.comment = "Transfer item Request from " + item.decision.ToString();
                            if (item_request_detail.id_project_task != null)
                            {
                                int id_branch = (int)base.projects.Where(x => x.id_project == item_request_detail.project_task.id_project).FirstOrDefault().id_branch;
                                item_transfer.app_location_destination = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault().app_location.Where(x => x.is_default).FirstOrDefault();
                                item_transfer.app_branch_destination = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault();
                                item_transfer.id_project = item_request_detail.project_task.id_project;
                            }
                            if (item_request_detail.id_sales_order_detail != null)
                            {

                                item_transfer.app_location_destination = item_request.sales_order.app_branch.app_location.Where(x => x.is_default).FirstOrDefault();
                                item_transfer.app_branch_destination = item_request.sales_order.app_branch;
                            }
                            if (item_request_detail.id_order_detail != null)
                            {
                                int id_project = base.production_order_detail.Where(x => x.id_order_detail == item_request_detail.id_order_detail).FirstOrDefault().project_task.id_project;
                                int id_branch = (int)base.projects.Where(x => x.id_project == id_project).FirstOrDefault().id_branch;
                                item_transfer.app_location_destination = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault().app_location.Where(x => x.is_default).FirstOrDefault();
                                item_transfer.app_branch_destination = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault();
                            }




                            item_transfer_detail item_transfer_detail = new item_transfer_detail();
                            item_transfer_detail.id_item_product = item_request_detail.item.item_product.FirstOrDefault().id_item_product;
                            item_transfer_detail.id_project_task = item_request_detail.project_task.id_project_task;
                            item_transfer_detail.quantity_origin = item.quantity;
                            item_transfer_detail.quantity_destination = item.quantity;
                            item_transfer.item_transfer_detail.Add(item_transfer_detail);

                            item_transfer.transfer_type = entity.item_transfer.Transfer_type.transfer;

                        }
                        else if (item.decision == entity.item_request_decision.Decisions.Production)
                        {
                            production_order_detail production_order_detail = new production_order_detail();
                            production_order_detail.name = item_request_detail.item.name;
                            production_order_detail.quantity = item.quantity;
                            production_order_detail.status = Status.Project.InProcess;
                            production_order_detail.is_input = false;

                            production_order_detail.id_item = item_request_detail.item.id_item;
                            foreach (item_request_dimension item_request_dimension in item_request_detail.item_request_dimension)
                            {
                                production_order_dimension production_order_dimension = new production_order_dimension();
                                production_order_dimension.id_dimension = item_request_dimension.id_dimension;
                                production_order_dimension.id_measurement = item_request_dimension.id_measurement;
                                production_order_dimension.value = item_request_dimension.value;
                                production_order_detail.production_order_dimension.Add(production_order_dimension);
                            }
                            production_order.production_order_detail.Add(production_order_detail);


                        }
                        else
                        {

                            if (item_request_detail.id_project_task != null)
                            {
                                if (base.projects.Where(x => x.id_project == item_request_detail.project_task.id_project).FirstOrDefault().id_branch != null)
                                {
                                    int id_branch = (int)base.projects.Where(x => x.id_project == item_request_detail.project_task.id_project).FirstOrDefault().id_branch;
                                    purchase_tender.app_branch = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault();
                                }
                                else
                                {
                                    purchase_tender.app_branch = base.app_branch.Where(x => x.can_invoice == true && x.can_stock == true).FirstOrDefault();
                                }

                            }
                            if (item_request_detail.id_sales_order_detail != null)
                            {

                                purchase_tender.app_branch = item_request.sales_order.app_branch;
                            }
                            if (item_request_detail.id_order_detail != null)
                            {
                                int id_project = base.production_order_detail.Where(x => x.id_order_detail == item_request_detail.id_order_detail).FirstOrDefault().project_task.id_project;
                                int id_branch = (int)base.projects.Where(x => x.id_project == id_project).FirstOrDefault().id_branch;
                                purchase_tender.app_branch = base.app_branch.Where(x => x.id_branch == id_branch).FirstOrDefault();
                            }


                            purchase_tender.id_project = item_request_detail.project_task.id_project;
                            purchase_tender_item purchase_tender_item = new purchase_tender_item();
                            purchase_tender_item.id_item = item_request_detail.id_item;
                            purchase_tender_item.item_description = item_request_detail.comment;
                            purchase_tender_item.quantity = item.quantity;


                            foreach (item_request_dimension item_request_dimension in item_request_detail.item_request_dimension)
                            {
                                purchase_tender_dimension purchase_tender_dimension = new purchase_tender_dimension();
                                purchase_tender_dimension.id_dimension = item_request_dimension.id_dimension;
                                purchase_tender_dimension.id_measurement = item_request_dimension.id_measurement;
                                purchase_tender_dimension.value = item_request_dimension.value;
                                purchase_tender_item.purchase_tender_dimension.Add(purchase_tender_dimension);
                            }

                            purchase_tender.purchase_tender_item_detail.Add(purchase_tender_item);




                        }
                        item_request.status = Status.Documents_General.Approved;
                        //}
                    }
                }
                if (purchase_tender.purchase_tender_item_detail.Count() > 0)
                {
                    base.purchase_tender.Add(purchase_tender);
                }
                if (item_transfer.item_transfer_detail.Count() > 0)
                {
                    base.item_transfer.Add(item_transfer);
                }
                if (production_order.production_order_detail.Count() > 0)
                {
                    base.production_order.Add(production_order);
                }
            }
            SaveChanges();
        }
    }
}
