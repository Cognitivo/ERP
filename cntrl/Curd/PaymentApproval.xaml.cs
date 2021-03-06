﻿using entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace cntrl.Curd
{
    public partial class PaymentApproval : UserControl
    {
        public PaymentDB PaymentDB { get; set; }

        public enum Modes
        {
            Recievable,
            Payable
        }

        private Modes Mode;
        private CollectionViewSource payment_approvepayment_approve_detailViewSource;
        private CollectionViewSource payment_approveViewSource;
        public List<payment_schedual> payment_schedualList { get; set; }

        public PaymentApproval(Modes App_Mode, List<payment_schedual> _payment_schedualList, ref PaymentDB PaymentDB)
        {
            InitializeComponent();

            //Setting the Mode for this Window. Result of this variable will determine logic of the certain Behaviours.
            Mode = App_Mode;
            this.PaymentDB = PaymentDB;
            payment_approveViewSource = (CollectionViewSource)FindResource("payment_approveViewSource");
            payment_approvepayment_approve_detailViewSource = (CollectionViewSource)FindResource("payment_approvepayment_approve_detailViewSource");
            payment_schedualList = _payment_schedualList;
            cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PaymentDB, App.Names.PaymentOrder, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
            payment_approve payment_approve = new payment_approve();

            payment_approve.status = Status.Documents_General.Pending;
            payment_approve.State = EntityState.Added;

            payment_approve.app_document_range = entity.Brillo.Logic.Range.List_Range(PaymentDB, App.Names.PaymentOrder, CurrentSession.Id_Branch, CurrentSession.Id_Terminal).FirstOrDefault();

            payment_approve.IsSelected = true;

            PaymentDB.payment_approve.Add(payment_approve);
            payment_approveViewSource.Source = PaymentDB.payment_approve.Local;

            int id_contact = payment_schedualList.FirstOrDefault().id_contact;
            sbxReturn.ContactID = id_contact;

            entity.contact contacts = PaymentDB.contacts.Find(id_contact);
            if (contacts != null)
            {
                payment_approve.id_contact = contacts.id_contact;
                payment_approve.contact = contacts;
            }

            foreach (payment_schedual payment_schedual in payment_schedualList)
            {
                //Get list by Currency, not CurrencyFX as Rates can change. You can buy at 65 INR but pay at 67.
                Add_PaymentDetail(payment_schedual);
            }

            payment_approve.RaisePropertyChanged("GrandTotal");
            payment_approve.RaisePropertyChanged("GrandTotalDetail");

            payment_approveViewSource.View.MoveCurrentTo(payment_approve);
            payment_approvepayment_approve_detailViewSource.View.MoveCurrentToFirst();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource payment_typeViewSource = (CollectionViewSource)this.FindResource("payment_typeViewSource");
            await PaymentDB.payment_type.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).LoadAsync();

            //Fix if Payment Type not inserted.
            if (PaymentDB.payment_type.Local.Count == 0)
            {
                entity.payment_type payment_type = new entity.payment_type();
                payment_type.name = "Cash";
                payment_type.is_active = true;
                payment_type.is_default = true;

                PaymentDB.payment_type.Add(payment_type);
            }
            payment_typeViewSource.Source = PaymentDB.payment_type.Local;

            CollectionViewSource app_accountViewSource = (CollectionViewSource)this.FindResource("app_accountViewSource");
            await PaymentDB.app_account.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company &&
                (a.id_account_type == app_account.app_account_type.Bank || a.id_account == CurrentSession.Id_Account)).LoadAsync();

            //Fix if Payment Type not inserted.
            if (PaymentDB.app_account.Local.Count == 0)
            {
                app_account app_account = new app_account();
                app_account.name = "CashBox";
                app_account.code = "Generic";
                app_account.id_account_type = entity.app_account.app_account_type.Terminal;
                app_account.id_terminal = CurrentSession.Id_Terminal;
                app_account.is_active = true;

                PaymentDB.app_account.Add(app_account);
            }
            app_accountViewSource.Source = PaymentDB.app_account.Local;

            CollectionViewSource salesRepViewSourceCollector = (CollectionViewSource)this.FindResource("salesRepViewSourceCollector");
            salesRepViewSourceCollector.Source = await PaymentDB.sales_rep.Where(a => a.enum_type == sales_rep.SalesRepType.Collector && a.is_active && a.id_company == CurrentSession.Id_Company).ToListAsync();

            if (Mode == Modes.Recievable)
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PaymentDB, App.Names.PaymentUtility, CurrentSession.Id_Branch, CurrentSession.Id_Company);
                stackDocument.Visibility = Visibility.Visible;
            }

            payment_approve payment_approve = payment_approveViewSource.View.CurrentItem as payment_approve;
            if (payment_approve != null)
            {
                app_account app_account = app_accountViewSource.View.CurrentItem as app_account;
                if (app_account != null)
                {
                    foreach (payment_approve_detail payment_approve_detail in payment_approve.payment_approve_detail)
                    {
                        payment_approve_detail.id_account = app_account.id_account;
                    }
                }
            }
        }

        #region Events

        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        #endregion Events

        private void SaveChanges(object sender, EventArgs e)
        {
            payment_approvepayment_approve_detailViewSource.View.Refresh();
            payment_approve payment_approve = payment_approveViewSource.View.CurrentItem as payment_approve;
            foreach (var id in payment_schedualList.GroupBy(x => x.app_currencyfx).Select(x => new { x.Key.id_currency }))
            {
                Decimal TotalPayable = 0;
                if (Mode == Modes.Recievable)
                {
                    TotalPayable = payment_schedualList.Where(x => x.app_currencyfx.id_currency == id.id_currency).Sum(x => x.AccountReceivableBalance);
                }
                else
                {
                    TotalPayable = payment_schedualList.Where(x => x.app_currencyfx.id_currency == id.id_currency).Sum(x => x.AccountPayableBalance);
                }

                Decimal TotalPaid = 0;
                foreach (payment_approve_detail payment_approve_detail in payment_approve.payment_approve_detail.Where(x => x.id_currency == id.id_currency).ToList())
                {
                    TotalPaid += entity.Brillo.Currency.convert_Values(payment_approve_detail.value, payment_approve_detail.id_currencyfx, payment_approve_detail.Default_id_currencyfx, App.Modules.Sales);
                }
                //if (Math.Round(TotalPaid) > Math.Round(TotalPayable))
                //{
                //    String Currency = PaymentDB.app_currency.Where(x => x.id_currency == id.id_currency).FirstOrDefault().name;

                //    MessageBox.Show("Your Amount Is Higher Than :-" + TotalPayable + Currency);
                //    return;
                //}
            }
            try
            {
                app_document_range app_document_range = PaymentDB.app_document_range.Where(x => x.id_range == payment_approve.id_range).FirstOrDefault();
                payment_approve.number = entity.Brillo.Logic.Range.calc_Range(app_document_range, true);
                PaymentDB.SaveChanges();
                
                if (app_document_range != null)
                {
                    entity.Brillo.Document.Start.Automatic(payment_approve, app_document_range);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            lblCancel_MouseDown(null, null);
        }

        private void cbxPamentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewSource purchase_returnViewSource = this.FindResource("purchase_returnViewSource") as CollectionViewSource;
            payment_approve payment_approve = payment_approveViewSource.View.CurrentItem as payment_approve;
            if (cbxPamentType.SelectedItem != null)
            {
                entity.payment_type payment_type = cbxPamentType.SelectedItem as entity.payment_type;
                if (payment_type != null)
                {
                    if (payment_type.payment_behavior == global::entity.payment_type.payment_behaviours.WithHoldingVAT)
                    {
                        //If payment behaviour is WithHoldingVAT, hide everything.
                        stpaccount.Visibility = Visibility.Collapsed;
                        stpcreditpurchase.Visibility = Visibility.Collapsed;
                        stpcreditsales.Visibility = Visibility.Collapsed;
                    }
                    else if (payment_type.payment_behavior == global::entity.payment_type.payment_behaviours.CreditNote)
                    {
                        //If payment behaviour is Credit Note, then hide Account.
                        stpaccount.Visibility = Visibility.Collapsed;

                        //Check Mode.
                        if (Mode == Modes.Payable)
                        {
                            //If Payable, then Hide->Sales and Show->Payment
                            stpcreditsales.Visibility = Visibility.Collapsed;
                            stpcreditpurchase.Visibility = Visibility.Visible;

                            PaymentDB.purchase_return.Where(x => x.id_contact == payment_approve.id_contact).Include(x => x.payment_schedual).Load();
                            purchase_returnViewSource.Source = PaymentDB.purchase_return.Local.Where(x => (x.payment_schedual.Sum(y => y.debit) < x.payment_schedual.Sum(y => y.credit)));
                        }
                        else
                        {
                            //If Recievable, then Hide->Payment and Show->Sales
                            stpcreditpurchase.Visibility = Visibility.Collapsed;
                            stpcreditsales.Visibility = Visibility.Visible;

                            CollectionViewSource sales_returnViewSource = this.FindResource("sales_returnViewSource") as CollectionViewSource;
                            PaymentDB.sales_return.Where(x => x.id_contact == payment_approve.id_contact).Include(x => x.payment_schedual).Load();
                            sales_returnViewSource.Source = PaymentDB.sales_return.Local.Where(x => (x.payment_schedual.Sum(y => y.credit) < x.payment_schedual.Sum(y => y.debit)));
                        }
                    }
                    else
                    {
                        //If paymentbehaviour is not WithHoldingVAT & CreditNote, it must be Normal, so only show Account.
                        stpaccount.Visibility = Visibility.Visible;
                        stpcreditpurchase.Visibility = Visibility.Collapsed;
                        stpcreditsales.Visibility = Visibility.Collapsed;
                    }

                    //If PaymentType has Document to print, then show Document. Example, Checks or Bank Transfers.
                    //if (payment_type.id_document > 0 && paymentpayment_detailViewSource != null && paymentpayment_detailViewSource.View != null)
                    //{
                    //    stpDetailDocument.Visibility = Visibility.Visible;
                    //    payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;

                    //    app_document_range app_document_range = PaymentDB.app_document_range.Where(d => d.id_document == payment_type.id_document && d.is_active == true).FirstOrDefault();
                    //    if (app_document_range != null && payment_detail != null)
                    //    {
                    //        payment_detail.id_range = app_document_range.id_range;
                    //    }
                    //}
                    //else
                    //{
                    //    stpDetailDocument.Visibility = Visibility.Collapsed;
                    //}
                }
            }
        }

        #region Purchase and Sales Returns

        private void sbxPurchaseReturn_Select(object sender, RoutedEventArgs e)
        {
            if (sbxPurchaseReturn.ReturnID > 0)
            {
                CollectionViewSource payment_approvepayment_approve_detailViewSource = (CollectionViewSource)this.FindResource("payment_approvepayment_approve_detailViewSource");
                payment_approve_detail payment_approve_detail = this.payment_approvepayment_approve_detailViewSource.View.CurrentItem as payment_approve_detail;
                purchase_return purchase_return = PaymentDB.purchase_return.Find(sbxPurchaseReturn.ReturnID);
                decimal return_value = sbxPurchaseReturn.Balance;
                payment_approve_detail.value = return_value;
                payment_approve_detail.id_purchase_return = purchase_return.id_purchase_return;
                payment_approve_detail.Max_Value = return_value;
                sbxPurchaseReturn.Text = purchase_return.number + "-" + purchase_return.trans_date; ;
            }
        }

        private void sbxReturn_Select(object sender, RoutedEventArgs e)
        {
            if (sbxReturn.ReturnID > 0)
            {
                CollectionViewSource payment_approvepayment_approve_detailViewSource = (CollectionViewSource)this.FindResource("payment_approvepayment_approve_detailViewSource");
                payment_approve_detail payment_approve_detail = payment_approvepayment_approve_detailViewSource.View.CurrentItem as payment_approve_detail;
                if (payment_approve_detail != null)
                {
                    sales_return sales_return = PaymentDB.sales_return.Find(sbxReturn.ReturnID);
                    decimal return_value = sbxReturn.Balance;
                    payment_approve_detail.id_sales_return = sales_return.id_sales_return;
                    payment_approve_detail.value = return_value;
                    payment_approve_detail.Max_Value = return_value;
                    sbxReturn.Text = sales_return.number + "-" + sales_return.trans_date;
                    sbxReturn.RaisePropertyChanged("Text");
                }
            }
        }

        #endregion Purchase and Sales Returns

        private void Add_PaymentDetail(payment_schedual payment_schedual)
        {
            payment_approve payment_approve = payment_approveViewSource.View.CurrentItem as payment_approve;
            int CurrencyID = payment_schedual.app_currencyfx.id_currency;
            if (payment_approve != null)
            {
                payment_approve_detail payment_approve_detail = new payment_approve_detail();
                payment_approve_detail.payment_approve = payment_approve;

                //Get current Active Rate of selected Currency.
                app_currencyfx app_currencyfx = PaymentDB.app_currencyfx.Where(x => x.id_currency == CurrencyID && x.id_company == CurrentSession.Id_Company && x.is_active).FirstOrDefault();

                if (app_currencyfx != null)
                {
                    payment_approve_detail.Default_id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_approve_detail.id_currency = app_currencyfx.id_currency;
                    payment_approve_detail.id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_approve_detail.payment_approve.id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_approve_detail.payment_schedual = payment_schedual;
                    // payment_approve_detail.app_currencyfx = app_currencyfx;
                }

                payment_approve_detail.IsSelected = true;

                //Always get total value of Accounts Receivable from a particular Currency, and not Currency Rate. This is very important when Currency Fluctates.
                if (Mode == Modes.Recievable)
                {
                    payment_approve_detail.value = payment_schedual.debit;
                }
                else
                {
                    payment_approve_detail.value = payment_schedual.credit;
                }

                payment_schedual.payment_approve_detail = payment_approve_detail;
                payment_schedual.status = Status.Documents_General.Approved;
                payment_approve.payment_approve_detail.Add(payment_approve_detail);
                payment_approvepayment_approve_detailViewSource.View.Refresh();
            }
        }
    }
}