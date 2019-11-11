﻿using entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cognitivo.Commercial
{
    public partial class AccountsRecievable : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        #endregion INotifyPropertyChanged

        cntrl.Curd.Refinance Refinance = new cntrl.Curd.Refinance(cntrl.Curd.Refinance.Mode.AccountReceivable);

        private CollectionViewSource payment_schedualViewSource, contactViewSource;
        private PaymentDB PaymentDB = new PaymentDB();

        public AccountsRecievable()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (contactViewSource.View != null)
            {


                contact contact = contactViewSource.View.CurrentItem as contact;
               
                if (contact!=null && payment_schedualViewSource != null)
                {
                    payment_schedualViewSource.View.Filter = i =>
                    {
                        payment_schedual payment_schedual = i as payment_schedual;
                        if (payment_schedual.id_contact == contact.id_contact && payment_schedual.AccountReceivableBalance > 0)
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            app_company app_company = PaymentDB.app_company.Find(CurrentSession.Id_Company);
            if (app_company.app_company_interest != null)
            {
                if (app_company.app_company_interest.is_forced == false)
                {
                    BtnInterest.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnInterest.Visibility = Visibility.Collapsed;
                }
            }

            if (Cognitivo.Properties.Settings.Default.PaymentLoadData == true)
            {
                load_Schedual();
            }
        

            foreach (app_condition app_condition in CurrentSession.Conditions)
            {
                System.Windows.Controls.Label lbl = new System.Windows.Controls.Label();
                lbl.Content = app_condition.name;
                lbl.Tag = app_condition.id_condition;
                lbl.Foreground = SystemColors.HighlightBrush;
                lbl.MouseUp += lblCondition_MouseUp;
                stckFilter.Children.Add(lbl);
            }
        }

        private void lblCondition_MouseUp(object sender, EventArgs e)
        {
            contact contact = contactViewSource.View.CurrentItem as contact;

            if (payment_schedualViewSource != null && contact != null)
            {
                System.Windows.Controls.Label lbl = sender as System.Windows.Controls.Label;
                int ConditionID = Convert.ToInt32(lbl.Tag);

                if (contact.id_contact > 0 && payment_schedualViewSource.View != null && ConditionID > 0)
                {
                    payment_schedualViewSource.View.Filter = i =>
                    {
                        payment_schedual payment_schedual = i as payment_schedual;
                        if (payment_schedual.id_contact == contact.id_contact &&
                            payment_schedual.AccountReceivableBalance > 0 &&
                            payment_schedual.sales_invoice.id_condition == ConditionID)
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
        }

        private void load_Schedual()
        {
            payment_schedualViewSource = (CollectionViewSource)FindResource("payment_schedualViewSource");
            if (payment_schedualViewSource != null)
            {
                PaymentDB.payment_schedual
                    .Where(x => x.id_payment_detail == null && x.id_company == CurrentSession.Id_Company
                        && (x.id_sales_invoice > 0 || x.id_sales_order > 0)
                        && (x.debit - (x.child.Count() > 0 ? x.child.Sum(y => y.credit) : 0)) > 0)
                        .Include(x => x.sales_invoice)
                        .Include(x => x.contact)
                        .OrderBy(x => x.expire_date)
                        .Load();
                payment_schedualViewSource.Source = PaymentDB.payment_schedual.Local;
            }

            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            List<contact> contactLIST = new List<contact>();
            if (PaymentDB.payment_schedual.Local.Count() > 0)
            {
                foreach (payment_schedual payment in PaymentDB.payment_schedual.Local.ToList())
                {
                    if (contactLIST.Contains(payment.contact) == false)
                    {
                        contact contact = new contact();
                        contact = payment.contact;
                        contactLIST.Add(contact);
                    }
                }

                contactViewSource.Source = contactLIST;
            }
        }

        private void load_Schedual(string query)
        {
            payment_schedualViewSource = (CollectionViewSource)FindResource("payment_schedualViewSource");
            if (payment_schedualViewSource != null)
            {
                PaymentDB.payment_schedual
                    .Where(x => x.id_payment_detail == null && x.id_company == CurrentSession.Id_Company
                        && (x.id_sales_invoice > 0 || x.id_sales_order > 0)
                        && (x.debit - (x.child.Count() > 0 ? x.child.Sum(y => y.credit) : 0)) > 0
                        && (x.contact.name.Contains(query) || x.contact.gov_code.Contains(query) || x.contact.code.Contains(query)))

                        .Include(x => x.sales_invoice)
                        .Include(x => x.contact)
                        .OrderBy(x => x.expire_date)
                        .Load();
                payment_schedualViewSource.Source = PaymentDB.payment_schedual.Local;
            }

            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            List<contact> contactLIST = new List<contact>();
            if (PaymentDB.payment_schedual.Local.Count() > 0)
            {
                foreach (payment_schedual payment in PaymentDB.payment_schedual.Local.ToList())
                {
                    if (contactLIST.Contains(payment.contact) == false)
                    {
                        contact contact = new contact();
                        contact = payment.contact;
                        contactLIST.Add(contact);
                    }
                }

                contactViewSource.Source = contactLIST;
            }
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            List<payment_schedual> PaymentSchedualList = new List<payment_schedual>();

            if (payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList().Count > 0)
            {
                PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList();
            }
            else if (payment_schedualViewSource.View.OfType<payment_schedual>().ToList().Count > 0)
            {
                PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().ToList();
            }
            else
            {
                //If nothing found, then exit.
                return;
            }
            app_company app_company = PaymentDB.app_company.Find(CurrentSession.Id_Company);
            if (app_company.app_company_interest != null)
            {
                if (app_company.app_company_interest.is_forced)
                {
                    IntCalculation(app_company, PaymentSchedualList);
                }
            }

           
            cntrl.Curd.Payment Payment = new cntrl.Curd.Payment(cntrl.Curd.Payment.Modes.Recievable, PaymentSchedualList, ref PaymentDB);

            crud_modal.Visibility = Visibility.Visible;
            crud_modal.Children.Add(Payment);
        }

        public void IntCalculation(app_company app_company, List<payment_schedual> PaymentSchedualList)
        {
            foreach (payment_schedual payment_schedual in PaymentSchedualList.Where(x => x.is_interest == false))
            {
                decimal delta = 0;
                decimal amount = 0;

                //If there is Interest calculated for that day, then skip.
                if (payment_schedual.child.Where(x => x.is_interest && x.trans_date.Date == DateTime.Now.Date).Count() > 0)
                {
                    continue;
                }

                //If Child Exist = Date Diff of Date. If Child does not Exist, take Expiry.
                if (payment_schedual.child.Where(x => x.is_interest).Count() == 0)
                {
                    delta = Convert.ToDecimal((DateTime.Now - payment_schedual.expire_date).TotalDays);
                    amount = payment_schedual.AccountReceivableBalance;
                }
                else


                {
                    payment_schedual _payment_schedual = payment_schedual.child.Where(x => x.is_interest).LastOrDefault();
                    amount = payment_schedual.AccountReceivableBalance + payment_schedual.child.Where(x => x.is_interest).Sum(x => x.AccountReceivableBalance);

                    if (_payment_schedual != null)
                    {
                        delta = Convert.ToDecimal((DateTime.Now - _payment_schedual.trans_date).TotalDays);
                    }
                }

                if (delta > app_company.app_company_interest.grace_period)
                {
                    decimal dailyinterstrate = app_company.app_company_interest.InterestDaily;

                    decimal Totaldailyinterest = amount * dailyinterstrate;

                    decimal totalint = (Totaldailyinterest * delta);
                    payment_schedual Intpayment_schedual = new payment_schedual();
                    Intpayment_schedual.credit = 0;
                    Intpayment_schedual.debit = totalint;
                    Intpayment_schedual.id_currencyfx = payment_schedual.id_currencyfx;
                    Intpayment_schedual.sales_invoice = payment_schedual.sales_invoice;
                    Intpayment_schedual.trans_date = DateTime.Now;
                    Intpayment_schedual.expire_date = payment_schedual.expire_date;
                    Intpayment_schedual.status = Status.Documents_General.Approved;
                    Intpayment_schedual.id_contact = payment_schedual.id_contact;
                    Intpayment_schedual.is_interest = true;
                    payment_schedual.child.Add(Intpayment_schedual);
                }
            }
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            if (Cognitivo.Properties.Settings.Default.PaymentLoadData == true)
            {
                if (!string.IsNullOrEmpty(query) && contactViewSource != null)
                {
                    try
                    {
                        if (contactViewSource != null)
                        {
                            if (contactViewSource.View != null)
                            {
                                contactViewSource.View.Filter = i =>
                                {
                                    contact contact = i as contact;
                                    if (contact != null)
                                    {
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
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                };
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        toolbar.msgError(ex);
                    }
                }
                else
                {
                    if (contactViewSource != null)
                    {
                        if (contactViewSource.View != null)
                        {
                            if (contactViewSource.View.Filter != null)
                            {
                                contactViewSource.View.Filter = null;
                            }

                        }
                    }
                }
            }

        }

        #region PrefSettings

        private void tbCustomize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            popupCustomize.StaysOpen = false;
            popupCustomize.IsOpen = true;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            Commercial.PaymentSetting _pref_PaymentSetting = new Commercial.PaymentSetting();
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            Commercial.PaymentSetting.Default.Save();
            Cognitivo.Properties.Settings.Default.Save();
            _pref_PaymentSetting = Commercial.PaymentSetting.Default;
            popupCustomize.IsOpen = false;
        }

        #endregion PrefSettings

        private void Refince_Click(object sender, RoutedEventArgs e)
        {
            payment_schedual PaymentSchedual = payment_schedualViewSource.View.CurrentItem as payment_schedual;

            if (payment_schedualViewSource != null)
            {
                if (payment_schedualViewSource.View != null)
                {
                    payment_schedualViewSource.View.Filter = i =>
                    {
                        payment_schedual payment_schedual = (payment_schedual)i;
                        if (payment_schedual.IsSelected == true)
                            return true;
                        else
                            return false;
                    };
                }
            }
            payment_schedualViewSource.View.MoveCurrentToLast();
            Refinance.objEntity = PaymentDB;
            Refinance.payment_schedualList = payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected).ToList();
            Refinance.id_contact = PaymentSchedual.id_contact;
            Refinance.id_currency = PaymentSchedual.app_currencyfx.id_currency;
            Refinance.btnSave_Click += SaveRefinance_Click;
            crud_modal.Visibility = Visibility.Visible;
            crud_modal.Children.Add(Refinance);
        }

        public void SaveRefinance_Click(object sender)
        {
            foreach (payment_schedual payment_schedual in Refinance.payment_schedualList)
            {
                if (payment_schedual.id_payment_schedual == 0)
                {
                    PaymentDB.payment_schedual.Add(payment_schedual);
                }
                else
                {
                    payment_schedual _payment_schedual = PaymentDB.payment_schedual.Find(payment_schedual.id_payment_schedual);
                    _payment_schedual.debit = payment_schedual.debit;
                }
            }

            IEnumerable<DbEntityValidationResult> validationresult = PaymentDB.GetValidationErrors();
            if (validationresult.Count() == 0)
            {
                PaymentDB.SaveChanges();
                crud_modal.Children.Clear();
                crud_modal.Visibility = Visibility.Collapsed;
            }
            load_Schedual();
        }

        private void btnWithholding_Click(object sender, RoutedEventArgs e)
        {
            List<payment_schedual> PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList();

            if (PaymentSchedualList.Count > 0)
            {
                sales_invoice sales_invoice = PaymentSchedualList.FirstOrDefault().sales_invoice;

                if (sales_invoice.payment_withholding_detail.Count() == 0)
                {
                    cntrl.VATWithholding VATWithholding = new cntrl.VATWithholding();

                    VATWithholding.invoiceList = new List<object>();
                    VATWithholding.invoiceList.Add(sales_invoice);
                    VATWithholding.PaymentDB = PaymentDB;
                    VATWithholding.payment_schedual = PaymentSchedualList.FirstOrDefault();
                    VATWithholding.percentage = sales_invoice.vatwithholdingpercentage;
                    crud_modal.Visibility = Visibility.Visible;
                    crud_modal.Children.Add(VATWithholding);
                }
                else
                {
                    toolbar.msgWarning("Linked With Vat Holding...");
                }
            }
        }

        private void crud_modal_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PaymentDB = new PaymentDB();
            load_Schedual();
            ListBox_SelectionChanged(sender, null);
        }

        private void Interest_Click(object sender, RoutedEventArgs e)
        {
            List<payment_schedual> PaymentSchedualList = new List<payment_schedual>();

            if (payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList().Count > 0)
            {
                PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList();
            }
            else if (payment_schedualViewSource.View.OfType<payment_schedual>().ToList().Count > 0)
            {
                PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().ToList();
            }
            else
            {
                //If nothing found, then exit.
                return;
            }
            app_company app_company = PaymentDB.app_company.Find(CurrentSession.Id_Company);
            if (app_company.app_company_interest != null)
            {
                IntCalculation(app_company, PaymentSchedualList);
            }
            PaymentDB.SaveChanges();
        }

        private void PaymentGroup_Click(object sender, RoutedEventArgs e)
        {
            cntrl.Curd.PaymentGroup Payment = new cntrl.Curd.PaymentGroup(ref PaymentDB);

            crud_modal.Visibility = Visibility.Visible;
            crud_modal.Children.Add(Payment);
        }

        private void Toolbar_btnSearchInSource_Click(object sender, System.Windows.Input.KeyEventArgs e, string query)
        {
            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            contactViewSource.Source = null;
            if (Cognitivo.Properties.Settings.Default.PaymentLoadData == false)
            {
                load_Schedual(query);
            }
        }

        private void Rearrange_Click(object sender, RoutedEventArgs e)
        {
            PaymentDB.Rearrange_Payment();
        }

        private void Excel_Create(object sender, RoutedEventArgs e)
        {

        }

        private void Excel_Drop(object sender, DragEventArgs e)
        {
           entity.Brillo.AccountReceivable2Excel Rec2Excel = new entity.Brillo.AccountReceivable2Excel();

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (Rec2Excel.Read(file.FirstOrDefault()))
                {
                    load_Schedual();
                }
            }
        }



    }
}