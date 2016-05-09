﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.Entity;
using entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace cntrl.Curd
{
    public partial class payment_quick : UserControl
    {
        PaymentDB PaymentDB = new PaymentDB();

        public enum Modes
        {
            Recievable,
            Payable
        }

        private Modes Mode;
        CollectionViewSource paymentpayment_detailViewSource;
        CollectionViewSource paymentViewSource;
        public List<payment_schedual> payment_schedualList { get; set; }
        //public int id_payment_schedual { get; set; } 
        //public payment_detail payment_detail { get; set; }

        public payment_quick(Modes App_Mode, int? ContactID, List<payment_schedual> _payment_schedualList)
        {
            InitializeComponent();

            //Setting the Mode for this Window. Result of this variable will determine logic of the certain Behaviours.
            Mode = App_Mode;

            paymentViewSource = (CollectionViewSource)this.FindResource("paymentViewSource");
            paymentpayment_detailViewSource = (CollectionViewSource)this.FindResource("paymentpayment_detailViewSource");
            payment_schedualList = _payment_schedualList;
            payment payment = PaymentDB.New();
            PaymentDB.payments.Add(payment);
            paymentViewSource.Source = PaymentDB.payments.Local;
            if (Mode == Modes.Recievable)
            {
                payment.GrandTotal = _payment_schedualList.Sum(x => x.AccountReceivableBalance);
            }
            else
            {
                payment.GrandTotal = _payment_schedualList.Sum(x => x.AccountPayableBalance);

            }
            int id_contact = _payment_schedualList.FirstOrDefault().id_contact;
            if (PaymentDB.contacts.Where(x => x.id_contact == id_contact).FirstOrDefault() != null)
            {
                payment.id_contact = id_contact;
                payment.contact = PaymentDB.contacts.Where(x => x.id_contact == id_contact).FirstOrDefault();
            }

            payment_detail payment_detail = new payment_detail();
            payment_detail.payment = payment;
            payment.payment_detail.Add(payment_detail);

            if (Mode == Modes.Recievable)
            {
                payment_detail.value = _payment_schedualList.Sum(x => x.AccountReceivableBalance);
            }
            else
            {
                payment_detail.value = _payment_schedualList.Sum(x => x.AccountPayableBalance);
            }

            int id_currencyfx = _payment_schedualList.FirstOrDefault().id_currencyfx;
            if (PaymentDB.app_currencyfx.Where(x => x.id_currencyfx == id_currencyfx).FirstOrDefault() != null)
            {
                payment_detail.id_currencyfx = id_currencyfx;
                payment_detail.app_currencyfx = PaymentDB.app_currencyfx.Where(x => x.id_currencyfx == id_currencyfx).FirstOrDefault();
            }







            paymentViewSource.View.MoveCurrentTo(payment);
            paymentpayment_detailViewSource.View.MoveCurrentToFirst();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource payment_typeViewSource = (CollectionViewSource)this.FindResource("payment_typeViewSource");
            PaymentDB.payment_type.Where(a => a.is_active).Load();
            payment_typeViewSource.Source = PaymentDB.payment_type.Local;

            CollectionViewSource app_accountViewSource = (CollectionViewSource)this.FindResource("app_accountViewSource");
            PaymentDB.app_account.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).Load();
            app_accountViewSource.Source = PaymentDB.app_account.Local;

            //CollectionViewSource paymentpayment_detailViewSource = (CollectionViewSource)this.FindResource("paymentpayment_detailViewSource");
            //List<payment_detail> payment_detailList = new List<payment_detail>();
            //payment_detailList.Add(payment_detail);
            //paymentpayment_detailViewSource.Source = payment_detailList;

            cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(App.Names.PaymentUtility, CurrentSession.Id_Branch, CurrentSession.Id_Company);

            //paymentpayment_detailViewSource.View.Refresh();
            //paymentpayment_detailViewSource.View.MoveCurrentToLast();



            paymentViewSource.View.Refresh();
            paymentpayment_detailViewSource.View.Refresh();
        }

        #region Events



        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        #endregion

        private void SaveChanges()
        {
            paymentpayment_detailViewSource.View.Refresh();
            payment payment = paymentViewSource.View.CurrentItem as payment;
            //entity.Brillo.Logic.AccountReceivable AccountReceivable = new entity.Brillo.Logic.AccountReceivable();

            decimal total = payment.GrandTotal;

            foreach (payment_schedual payment_schedual in payment_schedualList)
            {
                if (total > 0)
                {
                   
                    PaymentDB.Approve(payment_schedual.id_payment_schedual, true);

                    total = total - payment.payment_detail.FirstOrDefault().value;
                }
            }
           

        }

        private void cbxPamentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;
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

                            CollectionViewSource purchase_returnViewSource = this.FindResource("purchase_returnViewSource") as CollectionViewSource;
                            PaymentDB.purchase_return.Where(x => x.id_contact == payment.id_contact).Load();
                            purchase_returnViewSource.Source = PaymentDB.purchase_return.Local;
                        }
                        else
                        {
                            //If Recievable, then Hide->Payment and Show->Sales
                            stpcreditpurchase.Visibility = Visibility.Collapsed;
                            stpcreditsales.Visibility = Visibility.Visible;

                            CollectionViewSource sales_returnViewSource = this.FindResource("sales_returnViewSource") as CollectionViewSource;
                            PaymentDB.sales_return.Where(x => x.id_contact == payment.id_contact && (x.sales_invoice.GrandTotal - x.GrandTotal) > 0).Load();
                            sales_returnViewSource.Source = PaymentDB.sales_return.Local;
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
                    if (payment_type.id_document > 0 && paymentpayment_detailViewSource != null && paymentpayment_detailViewSource.View != null)
                    {
                        stpDetailDocument.Visibility = Visibility.Visible;
                        payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
                        payment_detail.id_range = PaymentDB.app_document_range.Where(d => d.id_document == payment_type.id_document && d.is_active == true).Include(i => i.app_document).FirstOrDefault().id_range;
                    }
                    else
                    {
                        stpDetailDocument.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #region Purchase and Sales Returns

        private void purchasereturnComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                purchasereturnComboBox_MouseDoubleClick(null, null);
            }
        }

        private void purchasereturnComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (purchasereturnComboBox.Data != null)
                {
                    purchase_return purchase_return = (purchase_return)purchasereturnComboBox.Data;
                    purchasereturnComboBox.Text = purchase_return.number;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void salesreturnComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                salesreturnComboBox_MouseDoubleClick(null, null);
            }
        }

        private void salesreturnComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                if (salesreturnComboBox.Data != null)
                {
                    CollectionViewSource paymentpayment_detailViewSource = (CollectionViewSource)this.FindResource("paymentpayment_detailViewSource");
                    payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
                    sales_return sales_return = (sales_return)salesreturnComboBox.Data;
                    salesreturnComboBox.Text = sales_return.number;
                    decimal return_value = (sales_return.GrandTotal - sales_return.payment_schedual.Where(x => x.id_sales_return == sales_return.id_sales_return).Sum(x => x.credit));
                    payment_detail.id_sales_return = sales_return.id_sales_return;

                    if (payment_detail.value > return_value)
                    {

                        payment_detail.value = return_value;
                        payment_detail.RaisePropertyChanged("value");
                    }
                    else
                    {
                        payment_detail.value = payment_detail.value;
                        payment_detail.RaisePropertyChanged("value");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            lblCancel_MouseDown(sender, null);
        }




    }
}
