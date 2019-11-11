﻿
using entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Configs
{
    public partial class AccountUtility : INotifyPropertyChanged
    {
        #region Load and Initilize

        private db db = new db();

        #region NotifyPropertyChange

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion NotifyPropertyChange

        private CollectionViewSource app_accountViewSource
            , app_account_listViewSource
            , app_accountapp_account_detailViewSource
            , app_account_detailViewSource
            , app_accountDestViewSource
            , app_accountOriginViewSource
            , app_currencyViewSource
            , app_currencydestViewSource
            , payment_typeViewSource
            , amount_transferViewSource = null;

        private List<Class.clsTransferAmount> listTransferAmt = null;

        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; RaisePropertyChanged("IsActive"); }
        }

        private bool _IsActive;

        public DateTime LastUsed
        {
            get { return _LastUsed; }
            set { _LastUsed = value; RaisePropertyChanged("LastUsed"); }
        }

        private DateTime _LastUsed;

        private void dataPager_OnDemandLoading(object sender, Syncfusion.UI.Xaml.Controls.DataPager.OnDemandLoadingEventArgs e)
        {
            OnDemandLoading(e.StartIndex, e.PageSize);
        }

        private void OnDemandLoading(int StartIndex, int PageSize)
        {
            app_account app_account = app_accountDataGrid.SelectedItem as app_account;
            if (app_account != null)
            {
                app_account_session app_account_session = app_account.app_account_session.LastOrDefault();
                if (app_account_session != null)
                {
                    IsActive = app_account_session.is_active;
                    LastUsed = app_account_session.app_account_detail.Select(x => x.trans_date).LastOrDefault();

                    int SessionID = 0;
                    //Sets the SessionID.
                    if (app_account_session.is_active)
                    {
                        SessionID = app_account_session.id_session;
                        //app_account.app_account_session
                        //.OrderByDescending(x => x.op_date)
                        //.Select(x => x.id_session)
                        //.FirstOrDefault();
                    }
                    
                    List<app_account_detail> ListDetails = app_account_session.app_account_detail.AsQueryable().Include(x => x.app_currencyfx.app_currency)
                                          .Where
                                           (x => x.id_session == SessionID &&
                                          x.status == Status.Documents_General.Approved)
                    .OrderByDescending(y => y.trans_date)
                    .Skip(StartIndex)
                    .Take(PageSize).ToList();

                    //if (ListDetails.Count() > 0)
                    //{
                    dataPager.LoadDynamicItems(StartIndex, ListDetails);
                    // }


                }
            }
        }

        public AccountUtility()
        {
            InitializeComponent();
        }

        private void TextAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            int dest_currid = Convert.ToInt32(cbxDestinationCurrency.SelectedValue);
            int origin_currid = Convert.ToInt32(cbxOriginCurrency.SelectedValue);
            app_currencyfx originapp_currencyfx = CurrentSession.CurrencyFX_ActiveRates.Where(x => x.id_currency == origin_currid).FirstOrDefault();
            app_currencyfx destinapp_currencyfx = CurrentSession.CurrencyFX_ActiveRates.Where(x => x.id_currency == dest_currid).FirstOrDefault();
            app_currency dest_appcurrency = CurrentSession.Currencies.Where(x => x.id_currency == dest_currid).FirstOrDefault();
            app_currency origin_appcurrency = CurrentSession.Currencies.Where(x => x.id_currency == origin_currid).FirstOrDefault();
            decimal fxrate = TextFxRate.Text != "" ? Convert.ToDecimal(TextFxRate.Text) : 1;
            decimal amount = TextAmount.Text != "" ? Convert.ToDecimal(TextAmount.Text) : 0;
            if (destinapp_currencyfx != null)
            {
                if (origin_appcurrency != null)
                {
                    TextDestAmount.Text = Convert.ToString(Math.Round(entity.Brillo.Currency.convert_Values(amount, destinapp_currencyfx.id_currencyfx, origin_appcurrency, fxrate, entity.App.Modules.Sales), 2));

                }

            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            app_account_detailViewSource = this.FindResource("app_account_detailViewSource") as CollectionViewSource;
            await db.app_account.Where(a => a.id_company == CurrentSession.Id_Company && a.is_active).LoadAsync();

            //Main Account DataGrid.
            app_accountViewSource = FindResource("app_accountViewSource") as CollectionViewSource;
            app_accountViewSource.Source = db.app_account.Local;
            app_accountapp_account_detailViewSource = this.FindResource("app_accountapp_account_detailViewSource") as CollectionViewSource;

            app_account_listViewSource = this.FindResource("app_account_listViewSource") as CollectionViewSource;
            app_account_listViewSource.Source = db.app_account.Local.Where(a => a.id_account_type == app_account.app_account_type.Terminal && a.id_account_type == app_account.app_account_type.Bank).ToList();

            app_accountDestViewSource = this.FindResource("app_accountDestViewSource") as CollectionViewSource;
            app_accountOriginViewSource = this.FindResource("app_accountOriginViewSource") as CollectionViewSource;
            app_accountDestViewSource.Source = db.app_account.Local;
            app_accountOriginViewSource.Source = db.app_account.Local;

            //Payment Type
            payment_typeViewSource = this.FindResource("payment_typeViewSource") as CollectionViewSource;
            payment_typeViewSource.Source = db.payment_type.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).ToList();

            //CurrencyFx
            app_currencyViewSource = this.FindResource("app_currencyViewSource") as CollectionViewSource;
            app_currencydestViewSource = this.FindResource("app_currencydestViewSource") as CollectionViewSource;
            app_currencyViewSource.Source = CurrentSession.Currencies;
            app_currencydestViewSource.Source = CurrentSession.Currencies;

            //List of 100 Latest Transactions.
            dataPager.OnDemandLoading += dataPager_OnDemandLoading;

            //Transfer
            listTransferAmt = new List<Class.clsTransferAmount>();
            amount_transferViewSource = this.FindResource("amount_transferViewSource") as CollectionViewSource;
            amount_transferViewSource.Source = listTransferAmt;
        }

        private void app_accountDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Account detail.
            app_account app_account = app_accountDataGrid.SelectedItem as app_account;

            if (app_account != null)
            {
                //Get the Very Last Session of this Account.
                app_account_session app_account_session = app_account.app_account_session.LastOrDefault();

                ///Gets the Current
                if (app_account_session != null)
                {
                    IsActive = app_account_session.is_active;
                    LastUsed = app_account_session.app_account_detail.Select(x => x.trans_date).LastOrDefault();

                    int SessionID = 0;
                    //Sets the SessionID.
                    if (app_account_session.is_active)
                    {
                        SessionID = app_account_session.id_session;
                        //app_account.app_account_session
                        //.OrderByDescending(x => x.op_date)
                        //.Select(x => x.id_session)
                        //.FirstOrDefault();
                    }
                    if (app_account.id_account_type == app_account.app_account_type.Bank)
                    {
                        app_account_detailDataGrid.ItemsSource =
                                              app_account_session.app_account_detail
                                          .Where
                                           (x => x.id_session == SessionID &&
                                          x.status == Status.Documents_General.Approved) //Gets only Approved Items into view.
                                          .GroupBy(ad => new { ad.app_currencyfx.id_currency })
                                          .Select(s => new
                                          {
                                              cur = s.Max(ad => ad.app_currencyfx.app_currency.name),
                                              payType = s.Max(ad => ad.payment_type.name),
                                              amount = s.Sum(ad => ad.credit) - s.Sum(ad => ad.debit)
                                          }).ToList();
                    }
                    else
                    {
                        app_account_detailDataGrid.ItemsSource =
                                          app_account_session.app_account_detail
                                      .Where
                                      (x => x.id_session == SessionID && //Gets Current Session Items Only.
                                      (x.status == Status.Documents_General.Approved || x.status == Status.Documents_General.Pending)) //Gets only Approved Items into view.
                                      .GroupBy(ad => new { ad.app_currencyfx.id_currency, ad.id_payment_type })
                                      .Select(s => new
                                      {
                                          cur = s.Max(ad => ad.app_currencyfx.app_currency.name),
                                          payType = s.Max(ad => ad.payment_type.name),
                                          amount = s.Sum(ad => ad.credit) - s.Sum(ad => ad.debit)
                                      }).ToList();
                    }

                }
                else
                {
                    IsActive = false;
                    app_account_detailDataGrid.ItemsSource = null;
                }

                //This code will change AccountID of Current Session and Can cause Serious Problems.
                // CurrentSession.Id_Account = app_account.id_account;

                if (frmActive.Children.Count > 0)
                {
                    frmActive.Children.RemoveAt(0);
                }

                AccountActive AccountActive = new AccountActive(app_account.id_account)
                {
                    db = db,
                    app_accountViewSource = app_accountViewSource
                };

                frmActive.Children.Add(AccountActive);

                var count = app_account.app_account_detail.Count() / dataPager.PageSize;

                if (app_account.app_account_detail.Count() % dataPager.PageSize == 0)
                {
                    dataPager.PageCount = count;
                }
                else
                {
                    dataPager.PageCount = count + 1;
                }

                OnDemandLoading(0, dataPager.PageSize);
                dataPager.MoveToFirstPage();
            }
        }

        #endregion Load and Initilize

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            app_accountDataGrid_SelectionChanged(null, null);

        }

        private void app_account_detailDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnDemandLoading(0, dataPager.PageSize);
        }

        private void cbxOriginAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            app_account app_accountorigin = app_accountOriginViewSource.View.CurrentItem as app_account;
            if (app_accountorigin != null)
            {

                if (app_currencyViewSource != null)
                {
                    if (app_currencyViewSource.View != null)
                    {
                        app_currencyViewSource.View.Filter = null;
                        if (app_accountorigin.id_currency != null)
                        {
                            app_currencyViewSource.View.Filter = i =>
                            {
                                app_currency app_currency = i as app_currency;
                                if (app_currency.id_currency == app_accountorigin.id_currency)
                                {
                                    return true;
                                }

                                return false;
                            };
                        }
                    }
                }


            }
        }

        private void btnAdjust_Click(object sender, RoutedEventArgs e)
        {
            app_account app_account = app_accountDataGrid.SelectedItem as app_account;
            if (app_account != null)
            {
                if (app_account.app_account_session.Where(x => x.is_active).Count() > 0)
                {
                    app_account_detail app_account_detail = new app_account_detail();
                    app_account_detail.id_account = app_account.id_account;
                    app_account_session app_account_session = app_account.app_account_session.Where(x => x.is_active).FirstOrDefault();
                    if (app_account_session != null)
                    {
                        app_account_detail.id_session = app_account_session.id_session;
                    }



                    if (cmbpayment.SelectedItem != null)
                    {
                        payment_type payment_type = db.payment_type.Find((int)cmbpayment.SelectedValue);
                        if (payment_type != null)
                        {
                            app_account_detail.id_payment_type = payment_type.id_payment_type;
                            app_account_detail.payment_type = payment_type;

                            //If payment type is Direct, then Detail should be Approved Status.
                            if (payment_type.is_direct)
                            {
                                app_account_detail.status = Status.Documents_General.Approved;
                            }
                            else //Else it should be pending to be fixed with Bank Reconcilization.
                            {
                                app_account_detail.status = Status.Documents_General.Pending;
                            }
                        }
                    }

                    if (cmbcurrency.SelectedItem != null)
                    {
                        int id_curreny = (int)cmbcurrency.SelectedValue;
                        app_currencyfx app_currencyfx = db.app_currencyfx.Where(x => x.is_active && x.id_currency == id_curreny).FirstOrDefault();

                        if (app_currencyfx != null)
                        {
                            app_account_detail.id_currencyfx = app_currencyfx.id_currencyfx;
                        }
                    }
                    if (txtdebit.Text == "")
                    {
                        app_account_detail.debit = 0;
                    }
                    else
                    {
                        app_account_detail.debit = Convert.ToDecimal(txtdebit.Text);
                    }

                    if (txtcredit.Text == "")
                    {
                        app_account_detail.credit = 0;
                    }
                    else
                    {
                        app_account_detail.credit = Convert.ToDecimal(txtcredit.Text);
                    }

                    app_account_detail.comment = tbxCommentAdjust.Text;
                    app_account_detail.id_account = app_account.id_account;
                    db.app_account_detail.Add(app_account_detail);
                    db.SaveChanges();

                    cmbpayment.Text = "";
                    cmbcurrency.Text = "";
                    txtcredit.Text = "";
                    txtdebit.Text = "";
                    listTransferAmt.Clear();
                    amount_transferViewSource.View.Refresh();
                    app_accountViewSource.View.Refresh();
                    app_accountapp_account_detailViewSource.View.Refresh();
                    toolBar.msgSaved(1);
                }
                else
                {
                    toolBar.msgWarning("Open Your Session...");
                }
            }
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            //Run through all Transfer in list.
            //  foreach (Class.clsTransferAmount Transfer in listTransferAmt)
            //{
            payment_type payment_type = payment_typeViewSource.View.CurrentItem as payment_type;
            app_account dest_account = cbxDestinationAccount.SelectedItem as app_account;
            app_account origin_account = cbxOriginAccount.SelectedItem as app_account;

            if (dest_account != null && origin_account != null && payment_type != null)
            {
                //Fix this code. Allow use of manual FX Rate and create into table.
                int dest_currid = Convert.ToInt32(cbxDestinationCurrency.SelectedValue);
                int origin_currid = Convert.ToInt32(cbxOriginCurrency.SelectedValue);
                int DestinationRate_ID = CurrentSession.CurrencyFX_ActiveRates.Where(x => x.id_currency == dest_currid).FirstOrDefault().id_currencyfx;
                int OriginRate_ID = CurrentSession.CurrencyFX_ActiveRates.Where(x => x.id_currency == origin_currid).FirstOrDefault().id_currencyfx;

                app_currencyfx Destapp_currencyfxold =
                    CurrentSession
                    .CurrencyFX_ActiveRates
                    .Where(x => x.id_currency == DestinationRate_ID).FirstOrDefault();

                decimal SellRate = Destapp_currencyfxold != null ? Destapp_currencyfxold.sell_value : 0;

                if (SellRate != Convert.ToDecimal(TextFxRate.Text))
                {
                    using (db _db = new db())
                    {
                        app_currencyfx fx = new app_currencyfx()
                        {
                            sell_value = Convert.ToDecimal(TextFxRate.Text),
                            buy_value = Convert.ToDecimal(TextFxRate.Text),
                            id_company = CurrentSession.Id_Company,
                            is_active = false,
                            id_currency = dest_currid
                        };

                        _db.app_currencyfx.Add(fx);
                        _db.SaveChanges();

                        DestinationRate_ID = fx.id_currencyfx;
                    }
                }
                else
                {
                    DestinationRate_ID = Destapp_currencyfxold.id_currencyfx;
                }


                app_currencyfx originapp_currencyfx =
                  db.app_currencyfx
                 .Where(x => x.id_currencyfx == OriginRate_ID).FirstOrDefault();
                //Set up Origin Data.
                app_account_detail Origin_AccountTransaction = new app_account_detail()
                {
                    id_account = origin_account.id_account,
                    id_currencyfx = OriginRate_ID,
                    app_currencyfx = originapp_currencyfx,
                    id_payment_type = (int)cbxPaymentType.SelectedValue,
                    credit = 0,
                    tran_type = app_account_detail.tran_types.Transaction,
                    debit = Convert.ToDecimal(TextAmount.Text),
                    comment = tbxCommentTransfer.Text + "-> Transfered to " + origin_account.name,
                    trans_date = DateTime.Now
                };

                int SessionID_Origin = db.app_account_session.Where(x => x.id_account == origin_account.id_account && x.is_active).Select(y => y.id_session).FirstOrDefault();
                if (SessionID_Origin > 0)
                {
                    Origin_AccountTransaction.id_session = SessionID_Origin;
                }


                // decimal Amount_AfterFXExchange = entity.Brillo.Currency.convert_Values(Transfer.amount, OriginRate_ID, DestinationRate_ID, entity.App.Modules.Sales);

                app_currencyfx Destapp_currencyfx =
                  db.app_currencyfx
                  .Where(x => x.id_currencyfx == DestinationRate_ID).FirstOrDefault();

                app_account_detail Destination_AccountTransaction = new app_account_detail()
                {
                    id_account = dest_account.id_account,
                    id_currencyfx = DestinationRate_ID,
                    app_currencyfx = Destapp_currencyfx,
                    id_payment_type = (int)cbxPaymentType.SelectedValue,
                    tran_type = app_account_detail.tran_types.Transaction,
                    credit = Convert.ToDecimal(TextDestAmount.Text),
                    debit = 0,
                    comment = tbxCommentTransfer.Text + "-> Transfered from " + dest_account.name,
                    trans_date = DateTime.Now
                };

                int SessionID_Destination = db.app_account_session.Where(x => x.id_account == dest_account.id_account && x.is_active).Select(y => y.id_session).FirstOrDefault();
                if (SessionID_Destination > 0)
                {
                    Destination_AccountTransaction.id_session = SessionID_Destination;
                }

                if (payment_type.is_direct)
                {
                    Origin_AccountTransaction.status = Status.Documents_General.Approved;
                    Destination_AccountTransaction.status = Status.Documents_General.Approved;
                }
                else
                {
                    Origin_AccountTransaction.status = Status.Documents_General.Pending;
                    Destination_AccountTransaction.status = Status.Documents_General.Pending;
                }

                db.app_account_detail.Add(Origin_AccountTransaction);
                db.app_account_detail.Add(Destination_AccountTransaction);
                db.SaveChanges();

            }
            // }

            toolBar.msgSaved(1);

            listTransferAmt.Clear();
            amount_transferViewSource.View.Refresh();
            app_accountViewSource.View.Refresh();
            app_accountapp_account_detailViewSource.View.Refresh();
            //app_account_detail_adjustViewSource.View.Refresh();

            app_accountDataGrid_SelectionChanged(null, null);
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                app_accountViewSource.View.Filter = i =>
                {
                    app_account app_account = i as app_account;
                    if (app_account.name.ToLower().Contains(query.ToLower()))
                    {
                        return true;
                    }

                    return false;
                };
            }
            else
            {
                app_accountViewSource.View.Filter = null;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabAccount.SelectedIndex == 2)
            {
                app_account_listViewSource.Source = db.app_account.Where(a => a.is_active == true && a.id_account_type == app_account.app_account_type.Terminal && a.id_company == CurrentSession.Id_Company).ToList();
                app_account_listViewSource.View.Refresh();
            }
            if (app_accountViewSource != null)
            {
                app_accountViewSource.View.Refresh();
            }
            if (app_accountapp_account_detailViewSource != null)
            {
                app_accountapp_account_detailViewSource.View.Refresh();
            }
        }
    }
}