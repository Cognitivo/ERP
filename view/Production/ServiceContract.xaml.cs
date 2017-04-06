﻿using entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Production
{
    public partial class ServiceContract : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion INotifyPropertyChanged

        private CollectionViewSource contactViewSource, production_service_accountViewSource;
        db db = new db();

        public ServiceContract()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            contact contact = contactViewSource.View.CurrentItem as contact;
            if (contact != null && production_service_accountViewSource != null)
            {
                production_service_accountViewSource.View.Filter = i =>
                {
                    payment_schedual payment_schedual = i as payment_schedual;
                    if (payment_schedual.id_contact == contact.id_contact)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }
            else
            {
                contactViewSource.View.Filter = null;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            load_Schedual();
        }

        private async void load_Schedual()
        {
            production_service_accountViewSource = (CollectionViewSource)FindResource("production_service_accountViewSource");
            if (production_service_accountViewSource != null)
            {
                contactViewSource = FindResource("contactViewSource") as CollectionViewSource;
                List<contact> contactLIST = new List<contact>();

                production_service_accountViewSource.Source = await db.production_service_account
                                    .Where(
                                        x => x.id_company == CurrentSession.Id_Company
                                        && x.id_purchase_order_detail != null
                                        && x.credit > x.child.Where(z => z.id_purchase_invoice_detail == null).Sum(y => y.debit)
                                        )
                                        .Include(y => y.purchase_order_detail)
                                        .Include(z => z.contact)
                                        .OrderByDescending(x => x.trans_date)
                                    .ToListAsync();

                foreach (production_service_account service_account in db.production_service_account.Local.OrderBy(x => x.contact.name).ToList())
                {
                    if (contactLIST.Contains(service_account.contact) == false)
                    {
                        contact contact = new contact();
                        contact = service_account.contact;
                        contactLIST.Add(contact);
                    }
                }

                contactViewSource.Source = contactLIST;
            }
        }

        private void btnPurchaseInvoice_Click(object sender, RoutedEventArgs e)
        {
            List<purchase_order> Order_List = new List<purchase_order>();
            List<purchase_order_detail> OrderDetail_List = new List<purchase_order_detail>();
            List<purchase_invoice> Invoice_List = new List<purchase_invoice>();

            foreach (production_service_account ParentAccount in db.production_service_account.Local.Where(x => x.IsSelected))
            {
                if (ParentAccount.purchase_order_detail != null)
                {
                    if (ParentAccount.purchase_order_detail.purchase_order != null)
                    {
                        purchase_order order = ParentAccount.purchase_order_detail.purchase_order;
                        
                        if (Order_List.Contains(ParentAccount.purchase_order_detail.purchase_order) == false)
                        {
                            Order_List.Add(ParentAccount.purchase_order_detail.purchase_order);
                            
                            if (order != null)
                            {
                                purchase_invoice invoice = new purchase_invoice();

                                invoice.status = Status.Documents_General.Pending;
                                invoice.id_purchase_order = order.id_purchase_order;
                                invoice.id_contact = order.id_contact;
                                invoice.contact = order.contact;
                                invoice.id_project = order.id_project;
                                invoice.id_currencyfx = order.id_currencyfx; //Bad
                                invoice.id_contract = order.id_contract;
                                invoice.id_condition = order.id_condition;
                                invoice.comment = order.comment;
                                invoice.is_impex = order.is_impex;
                                invoice.trans_date = DateTime.Now;
                                invoice.id_branch = order.id_branch;
                                invoice.id_terminal = order.id_terminal;
                                invoice.id_department = order.id_department;

                                db.purchase_invoice.Add(invoice);
                                Invoice_List.Add(invoice);
                            }
                        }
                    }
                }
            }

            foreach (production_service_account ParentAccount in db.production_service_account.Local.Where(x => x.IsSelected))
            {
                if (ParentAccount.purchase_order_detail != null)
                {
                    if (OrderDetail_List.Contains(ParentAccount.purchase_order_detail) == false)
                    {
                        OrderDetail_List.Add(ParentAccount.purchase_order_detail);
                        purchase_order_detail purchase_order_detail = ParentAccount.purchase_order_detail;

                        purchase_invoice_detail detail = new purchase_invoice_detail();
                        detail.id_item = purchase_order_detail.id_item;
                        detail.id_vat_group = purchase_order_detail.id_vat_group;
                        detail.app_vat_group = purchase_order_detail.app_vat_group;
                        detail.batch_code = purchase_order_detail.batch_code;
                        detail.expire_date = purchase_order_detail.expire_date;

                        detail.id_purchase_order_detail = ParentAccount.id_purchase_order_detail;
                        detail.quantity = ParentAccount.child.Where(x => x.purchase_invoice_detail == null).Sum(x => x.debit);
                        detail.unit_cost = purchase_order_detail.unit_cost;

                        if (ParentAccount.purchase_order_detail != null)
                        {
                            if (ParentAccount.purchase_order_detail.purchase_order != null)
                            {
                                if (Order_List.Contains(ParentAccount.purchase_order_detail.purchase_order) == false)
                                {
                                    purchase_invoice invoice = Invoice_List.Where(x => x.id_purchase_order == ParentAccount.purchase_order_detail.id_purchase_order).FirstOrDefault();
                                    invoice.purchase_invoice_detail.Add(detail);
                                }
                            }
                        }

                        foreach (production_service_account child in ParentAccount.child.Where(x => x.purchase_invoice_detail == null))
                        {
                            child.purchase_invoice_detail = detail;
                        }
                    }
                }
            }

            db.SaveChangesAsync();
            load_Schedual();
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            try
            {
                if (!string.IsNullOrEmpty(query))
                {
                    contactViewSource.View.Filter = i =>
                    {
                        contact contact = i as contact;
                        string name = "";
                        string code = "";
                        string gov_code = "";

                        if (contact.name != null)
                        {
                            name = contact.name.ToLower();
                        }

                        if (contact.code != null)
                        {
                            code = contact.code.ToLower();
                        }

                        if (contact.gov_code != null)
                        {
                            gov_code = contact.gov_code.ToLower();
                        }

                        if (name.Contains(query.ToLower())
                            || code.Contains(query.ToLower())
                            || gov_code.Contains(query.ToLower()))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    };
                }
                else
                {
                    contactViewSource.View.Filter = null;
                }
            }
            catch (Exception ex)
            {
                toolbar.msgError(ex);
            }
        }
    }
}