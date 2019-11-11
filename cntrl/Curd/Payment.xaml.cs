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
    public partial class Payment : UserControl
    {
        public PaymentDB PaymentDB { get; set; }

        public enum Modes
        {
            Recievable,
            Payable
        }

        private Modes Mode;
        private CollectionViewSource paymentpayment_detailViewSource;
        private CollectionViewSource paymentViewSource;
        private CollectionViewSource app_accountViewSource;
        public List<payment_schedual> payment_schedualList { get; set; }
        public bool clean_balance { get; set; }

        public Payment(Modes App_Mode, List<payment_schedual> _payment_schedualList, ref PaymentDB PaymentDB)
        {
            InitializeComponent();

            //Setting the Mode for this Window. Result of this variable will determine logic of the certain Behaviours.
            Mode = App_Mode;
            this.PaymentDB = PaymentDB;
            paymentViewSource = FindResource("paymentViewSource") as CollectionViewSource;
            paymentpayment_detailViewSource = FindResource("paymentpayment_detailViewSource") as CollectionViewSource;
            payment_schedualList = _payment_schedualList;

            payment payment = new payment();
            payment = (Mode == Modes.Recievable) ? PaymentDB.New(true) : PaymentDB.New(false);

            PaymentDB.payments.Add(payment);
            paymentViewSource.Source = PaymentDB.payments.Local;

            int id_contact = payment_schedualList.Select(x => x.id_contact).FirstOrDefault();

            sbxReturn.ContactID = id_contact;
            sbxPurchaseReturn.ContactID = id_contact;
            entity.contact contacts = PaymentDB.contacts.Find(id_contact);

            if (contacts != null)
            {
                payment.id_contact = contacts.id_contact;
                payment.contact = contacts;
            }
            var list = payment_schedualList.Where(x => x.payment_approve_detail != null)
                .GroupBy(x => new
                {
                    payment_type = x.payment_approve_detail.id_payment_type,
                    Account = x.payment_approve_detail.id_account,
                    Currency = x.payment_approve_detail.id_currency,
                }).Select(x => new { x.Key.payment_type, x.Key.Account, x.Key.Currency });
            foreach (var id in list)
            {

                Add_PaymentDetail(id.Currency, id.payment_type, id.Account);
                // }
                //else
                //{
                //    decimal buy_value = id.CurrencyFx.buy_value;
                //    decimal sell_value = id.CurrencyFx.sell_value;
                //    app_currencyfx app_currencyfx = PaymentDB.app_currencyfx.Where(x => x.buy_value == buy_value && x.sell_value == sell_value && x.id_company == CurrentSession.Id_Company).FirstOrDefault();
                //    if (app_currencyfx != null)
                //    {
                //        Add_PaymentDetail(app_currencyfx.id_currency, id.payment_type, id.Account);
                //    }
                //    else
                //    {
                //        using (db db = new db())
                //        {
                //            app_currency app_currencyNew = new app_currency();
                //            app_currency.name = id.CurrencyFx.app_currency.name;
                //            app_currency.is_priority = id.CurrencyFx.app_currency.is_priority;
                //            app_currency.has_rounding = id.CurrencyFx.app_currency.has_rounding;
                //            app_currency.code = id.CurrencyFx.app_currency.code;

                //            app_currencyfx currencyfx = new app_currencyfx();
                //            currencyfx.id_currency = app_currencyNew.id_currency;
                //            currencyfx.app_currency = app_currencyNew;
                //            currencyfx.buy_value = id.CurrencyFx.buy_value;
                //            currencyfx.sell_value = id.CurrencyFx.sell_value;
                //            currencyfx.is_reverse = id.CurrencyFx.is_reverse;
                //            app_currency.app_currencyfx.Add(currencyfx);

                //            db.app_currency.Add(app_currencyNew);
                //            db.SaveChanges();

                //            Add_PaymentDetail(app_currencyNew.id_currency, id.payment_type, id.Account);

                //        }




                //    }
                //}

            }

            var paymentlistCurrency = payment_schedualList.Where(x => x.payment_approve_detail == null)
                .GroupBy(x => x.app_currencyfx.app_currency).Select(x => new { x.Key.id_currency });
            foreach (var id in paymentlistCurrency)
            {
                //Get list by Currency, not CurrencyFX as Rates can change. You can buy at 65 INR but pay at 67.
                Add_PaymentDetail(id.id_currency, null, null);
            }

            app_document_range app_document_range = PaymentDB.app_document_range.Where(x => x.id_range == payment.id_range).FirstOrDefault();
            entity.Brillo.Document.Start.Automatic(payment, app_document_range);

            payment.RaisePropertyChanged("GrandTotal");
            payment.RaisePropertyChanged("GrandTotalDetail");

            paymentViewSource.View.MoveCurrentTo(payment);
            paymentpayment_detailViewSource.View.MoveCurrentToFirst();

        }



        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource payment_typeViewSource = this.FindResource("payment_typeViewSource") as CollectionViewSource;
            await PaymentDB.payment_type.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).LoadAsync();

            //Fix if Payment Type not inserted.
            if (PaymentDB.payment_type.Local.Count == 0)
            {
                entity.payment_type payment_type = new entity.payment_type()
                {
                    name = "Cash",
                    is_active = true,
                    is_default = true
                };

                PaymentDB.payment_type.Add(payment_type);
            }
            payment_typeViewSource.Source = PaymentDB.payment_type.Local;

            app_accountViewSource = this.FindResource("app_accountViewSource") as CollectionViewSource;

            await PaymentDB.app_account
                .Where(a => a.is_active && a.id_company == CurrentSession.Id_Company &&
                    (a.id_account_type == app_account.app_account_type.Bank ||
                    a.id_terminal == CurrentSession.Id_Terminal))
            .LoadAsync();

            //Fix if Payment Type not inserted.
            if (PaymentDB.app_account.Local.Count == 0)
            {
                app_account app_account = new app_account()
                {
                    name = "CashBox",
                    code = "Generic",
                    id_account_type = entity.app_account.app_account_type.Terminal,
                    id_terminal = CurrentSession.Id_Terminal,
                    is_active = true
                };
                PaymentDB.app_account.Add(app_account);
            }
            app_accountViewSource.Source = PaymentDB.app_account.Local;

            CollectionViewSource salesRepViewSourceCollector = this.FindResource("salesRepViewSourceCollector") as CollectionViewSource;
            salesRepViewSourceCollector.Source = await PaymentDB.sales_rep.Where(a => a.enum_type == sales_rep.SalesRepType.Collector && a.is_active && a.id_company == CurrentSession.Id_Company).ToListAsync();

            if (Mode == Modes.Recievable)
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PaymentDB, App.Names.PaymentUtility, CurrentSession.Id_Branch, CurrentSession.Id_Company);
                stackDocument.Visibility = Visibility.Visible;
            }

            if (paymentViewSource.View.CurrentItem is payment payment)
            {
                if (app_accountViewSource.View.CurrentItem is app_account app_account)
                {
                    foreach (payment_detail payment_detail in payment.payment_detail)
                    {
                        payment_detail.id_account = app_account.id_account;
                    }
                }
            }
        }

        #region Events

        private void Cancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        #endregion Events

        private void SaveChanges(object sender, EventArgs e)
        {
            paymentpayment_detailViewSource.View.Refresh();
            payment payment = paymentViewSource.View.CurrentItem as payment;

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
                foreach (payment_detail payment_detail in payment.payment_detail.Where(x => x.app_currencyfx.app_currency.id_currency == id.id_currency).ToList())
                {
                    TotalPaid += entity.Brillo.Currency.convert_Values(payment_detail.value, payment_detail.id_currencyfx, payment_detail.Default_id_currencyfx, App.Modules.Sales);
                }
                if (Math.Round(TotalPaid, 2) > Math.Round(TotalPayable, 2))
                {
                    String Currency = PaymentDB.app_currency.Where(x => x.id_currency == id.id_currency).FirstOrDefault().name;
                    MessageBoxResult MsgBoxResult = MessageBox.Show("Your amount is higher than : -" + TotalPayable + " " + Currency + ". Do You Want To Continue..", "Cognitivo", MessageBoxButton.YesNo);
                    if (MsgBoxResult == MessageBoxResult.No)
                    {
                        return;
                    }

                }
            }

            bool IsRecievable = Mode == Modes.Recievable ? true : false;
            bool IsPrintable = Mode == Modes.Recievable ? true : false;
            PaymentDB.Approve(payment_schedualList, IsRecievable, IsPrintable, clean_balance);

            Cancel_MouseDown(null, null);
        }

        private void PamentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;
            if (cbxPamentType.SelectedItem != null)
            {
                entity.payment_type payment_type = cbxPamentType.SelectedItem as entity.payment_type;
                if (payment_type != null)
                {
                    typeNumber.Visibility = Visibility.Collapsed;

                    if (payment_type.payment_behavior == global::entity.payment_type.payment_behaviours.WithHoldingVAT)
                    {
                        //If payment behaviour is WithHoldingVAT, hide everything.
                        stpaccount.Visibility = Visibility.Collapsed;
                        stpcreditpurchase.Visibility = Visibility.Collapsed;
                        stpcreditsales.Visibility = Visibility.Collapsed;
                        stptransdate.Visibility = Visibility.Visible;
                    }
                    else if (payment_type.payment_behavior == global::entity.payment_type.payment_behaviours.CreditNote)
                    {
                        //If payment behaviour is Credit Note, then hide Account.
                        stpaccount.Visibility = Visibility.Collapsed;
                        stptransdate.Visibility = Visibility.Visible;
                        //Check Mode.
                        if (Mode == Modes.Payable)
                        {
                            //If Payable, then Hide->Sales and Show->Payment
                            stpcreditsales.Visibility = Visibility.Collapsed;
                            stpcreditpurchase.Visibility = Visibility.Visible;

                            CollectionViewSource purchase_returnViewSource = this.FindResource("purchase_returnViewSource") as CollectionViewSource;
                            PaymentDB.purchase_return.Where(x => x.id_contact == payment.id_contact).Include(x => x.payment_schedual).Load();
                            purchase_returnViewSource.Source =
                                PaymentDB.purchase_return.Local
                                .Where(x =>
                                (x.payment_schedual.Sum(y => y.debit) < x.payment_schedual.Sum(y => y.credit)));
                        }
                        else
                        {
                            //If Recievable, then Hide->Payment and Show->Sales
                            stpcreditpurchase.Visibility = Visibility.Collapsed;
                            stpcreditsales.Visibility = Visibility.Visible;

                            CollectionViewSource sales_returnViewSource = this.FindResource("sales_returnViewSource") as CollectionViewSource;
                            PaymentDB.sales_return.Where(x => x.id_contact == payment.id_contact).Include(x => x.payment_schedual).Load();
                            sales_returnViewSource.Source = PaymentDB.sales_return.Local.Where(x => (x.payment_schedual.Sum(y => y.credit) < x.payment_schedual.Sum(y => y.debit)));
                        }
                    }
                    else
                    {
                        //If payment type is not direct, like check or transfer, then show number. Show both in Payments and Receivables.
                        if (payment_type.is_direct == false)
                        {
                            typeNumber.Visibility = Visibility.Visible;
                        }

                        //If paymentbehaviour is not WithHoldingVAT & CreditNote, it must be Normal, so only show Account.
                        stpaccount.Visibility = Visibility.Visible;
                        stptransdate.Visibility = payment_type.is_direct ? Visibility.Collapsed : Visibility.Visible;
                        stpcreditpurchase.Visibility = Visibility.Collapsed;
                        stpcreditsales.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #region Purchase and Sales Returns

        private void PurchaseReturn_Select(object sender, RoutedEventArgs e)
        {
            if (sbxPurchaseReturn.ReturnID > 0)
            {
                CollectionViewSource paymentpayment_detailViewSource = FindResource("paymentpayment_detailViewSource") as CollectionViewSource;
                payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
                purchase_return purchase_return = PaymentDB.purchase_return.Find(sbxPurchaseReturn.ReturnID);
                decimal return_value = sbxPurchaseReturn.Balance;
                payment_detail.value = payment_detail.value >= return_value ? return_value : payment_detail.value;
                payment_detail.id_purchase_return = purchase_return.id_purchase_return;
                payment_detail.Max_Value = return_value;
                sbxPurchaseReturn.Text = purchase_return.number + " - " + purchase_return.trans_date; ;
            }
        }

        private void Return_Select(object sender, RoutedEventArgs e)
        {
            if (sbxReturn.ReturnID > 0)
            {
                CollectionViewSource paymentpayment_detailViewSource = FindResource("paymentpayment_detailViewSource") as CollectionViewSource;
                payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
                if (payment_detail != null)
                {
                    sales_return sales_return = PaymentDB.sales_return.Find(sbxReturn.ReturnID);
                    decimal return_value = sbxReturn.Balance;
                    payment_detail.id_sales_return = sales_return.id_sales_return;
                    payment_detail.value = payment_detail.value >= return_value ? return_value : payment_detail.value;
                    payment_detail.Max_Value = return_value;
                    sbxReturn.Text = sales_return.number + " - " + sales_return.trans_date;
                    sbxReturn.RaisePropertyChanged("Text");
                }
            }
        }

        #endregion Purchase and Sales Returns

        private void AddDetail_Click(object sender, RoutedEventArgs e)
        {
            payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
            if (payment_detail != null)
            {
                Add_PaymentDetail(payment_detail.app_currencyfx.id_currency, null, null);
            }
        }

        private void Add_PaymentDetail(int CurrencyID, int? PaymentTypeID, int? AccountID)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;

            if (payment != null)
            {
                payment_detail payment_detail = new payment_detail();
                payment_detail.payment = payment;
                //Get current Active Rate of selected Currency.
                int OldCurrencyID = CurrencyID;
                app_currency app_currency = PaymentDB.app_currency.Where(x => x.id_currency == CurrencyID && x.id_company == CurrentSession.Id_Company).FirstOrDefault();
                app_currencyfx app_currencyfx = PaymentDB.app_currencyfx.Where(x => x.id_currency == CurrencyID).FirstOrDefault();
                if (app_currency == null)
                {
                    decimal buy_value = app_currencyfx.buy_value;
                    decimal sell_value = app_currencyfx.sell_value;
                    app_currencyfx app_currencyfxNew = PaymentDB.app_currencyfx.Where(x => x.buy_value == buy_value && x.sell_value == sell_value && x.id_company == CurrentSession.Id_Company).FirstOrDefault();
                    if (app_currencyfxNew != null)
                    {
                        app_currencyfx = app_currencyfxNew;
                        CurrencyID = app_currencyfxNew.id_currency;
                    }
                    else
                    {
                        using (db db = new db())
                        {
                            app_currency app_currencyNew = new app_currency();
                            app_currency.name = app_currencyfx.app_currency.name;
                            app_currency.is_priority = app_currencyfx.app_currency.is_priority;
                            app_currency.has_rounding = app_currencyfx.app_currency.has_rounding;
                            app_currency.code = app_currencyfx.app_currency.code;

                            app_currencyfx currencyfx = new app_currencyfx();
                            currencyfx.id_currency = app_currencyNew.id_currency;
                            currencyfx.app_currency = app_currencyNew;
                            currencyfx.buy_value = app_currencyfx.buy_value;
                            currencyfx.sell_value = app_currencyfx.sell_value;
                            currencyfx.is_reverse = app_currencyfx.is_reverse;
                            app_currency.app_currencyfx.Add(currencyfx);

                            db.app_currency.Add(app_currencyNew);
                            db.SaveChanges();

                            CurrencyID = app_currencyNew.id_currency;
                            app_currencyfx = currencyfx;
                        }
                    }
                }
                else
                {
                    app_currencyfx = PaymentDB.app_currencyfx.Where(x => x.id_currency == CurrencyID && x.id_company == CurrentSession.Id_Company && x.is_active).FirstOrDefault();
                }

                if (app_currencyfx != null)
                {
                    payment_detail.Default_id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_detail.id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_detail.payment.id_currencyfx = app_currencyfx.id_currencyfx;
                    payment_detail.app_currencyfx = app_currencyfx;
                }

                //Always get total value of Accounts Receivable from a particular Currency, and not Currency Rate. This is very important when Currency Fluctates.
                if (Mode == Modes.Recievable)
                {
                    payment_detail.value = payment_schedualList.Where(x => x.app_currencyfx.id_currency == OldCurrencyID).Sum(x => x.AccountReceivableBalance)
                                                                           - payment.payment_detail.Where(x => x.app_currencyfx.id_currency == CurrencyID).Sum(x => x.value);
                }
                else
                {
                    payment_detail.value = payment_schedualList.Where(x => x.app_currencyfx.id_currency == OldCurrencyID && x.payment_approve_detail == null).Sum(x => x.AccountPayableBalance)
                                                                            - payment.payment_detail.Where(x => x.app_currencyfx.id_currency == CurrencyID && x.IsLocked == false).Sum(x => x.value);
                }

                //If PaymentTypeID is not null, then this transaction has a PaymentApproval
                if (AccountID != null && PaymentTypeID != null)
                {
                    payment_detail.IsLocked = true;
                    payment_detail.id_account = (int)AccountID;
                    payment_detail.id_payment_type = (int)PaymentTypeID;
                    //Over wright Detail Value with Approved Value
                    payment_detail.value = payment_schedualList
                        .Where(x => x.payment_approve_detail != null &&
                                    x.payment_approve_detail.id_currency == CurrencyID &&
                                    x.payment_approve_detail.id_account == (int)AccountID &&
                                    x.payment_approve_detail.id_payment_type == PaymentTypeID)
                        .Sum(x => x.payment_approve_detail.value) - payment_schedualList
                        .Where(x => x.payment_approve_detail != null &&
                                    x.payment_approve_detail.id_currency == CurrencyID &&
                                    x.payment_approve_detail.id_account == (int)AccountID &&
                                    x.payment_approve_detail.id_payment_type == PaymentTypeID)
                        .Sum(x => (x.child.Count() > 0 ? x.child.Sum(y => y.debit) : 0));
                }
                else
                {
                    payment_detail.IsLocked = false;
                    payment_detail.id_account = CurrentSession.Id_Account;
                }

                payment_detail.IsSelected = true;

                payment.payment_detail.Add(payment_detail);
                paymentpayment_detailViewSource.View.Refresh();
            }
        }

        private void DeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;
            if (payment != null)
            {
                payment_detail payment_detail = paymentpayment_detailViewSource.View.CurrentItem as payment_detail;
                if (payment_detail != null)
                {
                    PaymentDB.payment_detail.Remove(payment_detail);
                    paymentpayment_detailViewSource.View.Refresh();
                }
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtptransdate.SelectedDate != null)
            {
                List<int> app_account_sessionList = PaymentDB.app_account_session
                    .Where(y => y.is_active
                    && y.cl_date != null
                    && y.op_date < dtptransdate.SelectedDate)
                    .Select(x => x.id_account)
                    .ToList();
                List<app_account> app_accountList = PaymentDB.app_account.Where(x => app_account_sessionList.Contains(x.id_account)).ToList();

                if (app_accountList.Count() > 0)
                {
                    app_accountViewSource.Source = app_accountList;
                }
            }
        }

        private void cbxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            app_account app_account = app_accountViewSource.View.CurrentItem as app_account;
            if (app_account != null)
            {
                if (app_account.id_currency != null || app_account.id_currency > 0)
                {
                    sbxCurrency.id_currency = (int)app_account.id_currency;
                }
            }
        }
    }
}