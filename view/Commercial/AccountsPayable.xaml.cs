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
    public partial class AccountsPayable : Page, INotifyPropertyChanged
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

        // List<payment_schedual> ListPayments = new List<entity.payment_schedual>();

        private PaymentDB PaymentDB = new PaymentDB();

        private CollectionViewSource contactViewSource;
        private CollectionViewSource payment_schedualViewSource;

        private cntrl.Curd.Refinance Refinance = new cntrl.Curd.Refinance(cntrl.Curd.Refinance.Mode.AccountPayable);

        public AccountsPayable()
        {
            InitializeComponent();
        }


        private void toolBar_btnApprove_Click(object sender)
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

            cntrl.Curd.PaymentApproval PaymentApproval = new cntrl.Curd.PaymentApproval(cntrl.Curd.PaymentApproval.Modes.Payable, PaymentSchedualList, ref PaymentDB);

            crud_modal.Visibility = Visibility.Visible;
            crud_modal.Children.Add(PaymentApproval);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (contactViewSource.View != null)
            {
                contact contact = contactViewSource.View.CurrentItem as contact;
                if (contact != null && payment_schedualViewSource != null)
                {
                    payment_schedualViewSource.View.Filter = i =>
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
           
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Cognitivo.Properties.Settings.Default.PaymentLoadData == true)
            {
                load_Schedual();
            }

        }

        private async void load_Schedual()
        {
            payment_schedualViewSource = (CollectionViewSource)FindResource("payment_schedualViewSource");
            if (payment_schedualViewSource != null)
            {
                payment_schedualViewSource.Source = await PaymentDB.payment_schedual
                                                        .Where(x => x.payment_detail.id_payment == null && x.id_company == CurrentSession.Id_Company
                                                           && (x.id_purchase_invoice > 0 || x.id_purchase_order > 0) && x.id_note == null
                                                           && (x.credit - (x.child.Count() > 0 ? x.child.Sum(y => y.debit) : 0)) > 0)
                                                           .Include(y => y.purchase_invoice)
                                                           .Include(z => z.contact)
                                                           .OrderBy(x => x.expire_date)
                                                        .ToListAsync();
            }

            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            List<contact> contactLIST = new List<contact>();

            if (PaymentDB.payment_schedual.Local.Count() > 0)
            {
                foreach (payment_schedual payment in PaymentDB.payment_schedual.Local.OrderBy(x => x.contact.name).ToList())
                {
                    if (contactLIST.Contains(payment.contact) == false)
                    {
                        contact contact = new contact();
                        contact = payment.contact;
                        contactLIST.Add(contact);
                    }
                }
            }

            contactViewSource.Source = contactLIST;
        }

        private async void load_Schedual(string query)
        {
            payment_schedualViewSource = (CollectionViewSource)FindResource("payment_schedualViewSource");
            if (payment_schedualViewSource != null)
            {
                payment_schedualViewSource.Source = await PaymentDB.payment_schedual
                                                        .Where(x => x.payment_detail.id_payment == null && x.id_company == CurrentSession.Id_Company
                                                           && (x.id_purchase_invoice > 0 || x.id_purchase_order > 0) && x.id_note == null
                                                           && (x.credit - (x.child.Count() > 0 ? x.child.Sum(y => y.debit) : 0)) > 0
                                                           && (x.contact.name.Contains(query) || x.contact.gov_code.Contains(query) || x.contact.code.Contains(query)))
                                                           .Include(y => y.purchase_invoice)
                                                           .Include(z => z.contact)
                                                           .OrderBy(x => x.expire_date)
                                                        .ToListAsync();
            }

            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            List<contact> contactLIST = new List<contact>();

            if (PaymentDB.payment_schedual.Local.Count() > 0)
            {
                foreach (payment_schedual payment in PaymentDB.payment_schedual.Local.OrderBy(x => x.contact.name).ToList())
                {
                    if (contactLIST.Contains(payment.contact) == false)
                    {
                        contact contact = new contact();
                        contact = payment.contact;
                        contactLIST.Add(contact);
                    }
                }
            }

            contactViewSource.Source = contactLIST;
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
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

            cntrl.Curd.Payment Payment = new cntrl.Curd.Payment(cntrl.Curd.Payment.Modes.Payable, PaymentSchedualList, ref PaymentDB);

            crud_modal.Visibility = Visibility.Visible;
            crud_modal.Children.Add(Payment);
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            try
            {
                if (Cognitivo.Properties.Settings.Default.PaymentLoadData == true)
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

            }
            catch (Exception ex)
            {
                toolbar.msgError(ex);
            }
        }

        private void btnWithholding_Click(object sender, RoutedEventArgs e)
        {
            List<payment_schedual> PaymentSchedualList = payment_schedualViewSource.View.OfType<payment_schedual>().Where(x => x.IsSelected == true).ToList();

            if (PaymentSchedualList.Count > 0)
            {
                purchase_invoice purchase_invoice = PaymentSchedualList.FirstOrDefault().purchase_invoice;
                if (purchase_invoice.payment_withholding_detail.Count() == 0)
                {
                    cntrl.VATWithholding VATWithholding = new cntrl.VATWithholding();
                    VATWithholding.invoiceList = new List<object>();
                    VATWithholding.invoiceList.Add(purchase_invoice);
                    VATWithholding.PaymentDB = PaymentDB;
                    VATWithholding.payment_schedual = PaymentSchedualList.FirstOrDefault();
                    if (purchase_invoice.vatwithholdingpercentage == 0)
                    {
                        purchase_invoice.vatwithholdingpercentage = Commercial.PaymentSetting.Default.vatwithholdingpercent;
                    }
                    VATWithholding.percentage = purchase_invoice.vatwithholdingpercentage;
                    crud_modal.Visibility = System.Windows.Visibility.Visible;
                    crud_modal.Children.Add(VATWithholding);
                }
                else
                {
                    toolbar.msgWarning("Alerady Link With Vat Holding...");
                }
            }
        }

        private void Refince_Click(object sender, RoutedEventArgs e)
        {
            payment_schedual PaymentSchedual = payment_schedualViewSource.View.CurrentItem as payment_schedual;

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
            IEnumerable<DbEntityValidationResult> validationresult = PaymentDB.GetValidationErrors();
            if (validationresult.Count() == 0)
            {
                PaymentDB.SaveChanges();
                crud_modal.Children.Clear();
                crud_modal.Visibility = Visibility.Collapsed;
            }
            load_Schedual();
        }

        private void crud_modal_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PaymentDB = new PaymentDB();
            load_Schedual();
            ListBox_SelectionChanged(sender, null);
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

        private void Toolbar_btnSearchInSource_Click(object sender, KeyEventArgs e, string query)
        {

            contactViewSource = (CollectionViewSource)FindResource("contactViewSource");
            contactViewSource.Source = null;
            if (Cognitivo.Properties.Settings.Default.PaymentLoadData == false)
            {
                load_Schedual(query);
            }
        }
    }
}