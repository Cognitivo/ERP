﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using entity;

namespace cntrl.Curd
{
    /// <summary>
    /// Interaction logic for payment_type.xaml
    /// </summary>
    public partial class payment_type : UserControl
    {
        private entity.dbContext mydb = new entity.dbContext();
        private CollectionViewSource myViewSource = new CollectionViewSource();
        public bool isExternalCall { get; set; }

        private CollectionViewSource _MainViewSource = null;
        public CollectionViewSource MainViewSource { get { return _MainViewSource; } set { _MainViewSource = value; } }

        public object curObject { get; set; }

        public Class.clsCommon.Mode operationMode { get; set; }

        private entity.payment_type _payment_typeObject = null;
        public entity.payment_type payment_typeObject { get { return _payment_typeObject; } set { _payment_typeObject = value; } }

        private CollectionViewSource ItemsSource = null;
        public CollectionViewSource objCollectionViewSource { get { return ItemsSource; } set { ItemsSource = value; } }

        private entity.dbContext _entity = null;
        public entity.dbContext entity { get { return _entity; } set { _entity = value; } }

        public payment_type()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))

            {
                CollectionViewSource app_documentViewSource = ((CollectionViewSource)(FindResource("app_documentViewSource")));
             
                app_documentViewSource.Source = entity.db.app_document.Local;
              
                cbxbehaviour.ItemsSource = Enum.GetValues(typeof(entity.payment_type.payment_behaviours));
                if (!isExternalCall)
                {
                    stackFields.DataContext = objCollectionViewSource;
                }
                else
                {
                    MainViewSource.View.MoveCurrentTo(curObject);
                    if (operationMode == Class.clsCommon.Mode.Add)
                    {
                        entity.payment_type newPaymentType = new entity.payment_type();
                        mydb.db.payment_type.Add(newPaymentType);
                        myViewSource.Source = mydb.db.payment_type.Local;
                        myViewSource.View.Refresh();
                        cbxPrint.IsChecked = false;
                        myViewSource.View.MoveCurrentTo(newPaymentType);
                        stackFields.DataContext = myViewSource;
                    }
                    else if (operationMode == Class.clsCommon.Mode.Edit)
                    {
                        objCollectionViewSource.View.MoveCurrentTo(payment_typeObject);
                        cbxPrint.IsChecked = payment_typeObject.app_document != null ? true : false;
                        stackFields.DataContext = objCollectionViewSource;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isExternalCall)
                {
                    entity.CancelChanges();
                    objCollectionViewSource.View.Refresh();
                }
                else
                {
                    if (operationMode == Class.clsCommon.Mode.Add)
                        mydb.CancelChanges();
                }
                Grid parentGrid = (Grid)this.Parent;
                parentGrid.Children.Clear();
                parentGrid.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isExternalCall)
                {
                    IEnumerable<DbEntityValidationResult> validationresult = entity.db.GetValidationErrors();
                    if (validationresult.Count() == 0)
                    {
                        entity.db.SaveChanges();
                        btnCancel_Click(sender, e);
                    }
                }
                else
                {
                    IEnumerable<DbEntityValidationResult> validationresult = mydb.db.GetValidationErrors();
                    if (validationresult.Count() == 0)
                    {
                        if (operationMode == Class.clsCommon.Mode.Add)
                        {
                            mydb.SaveChanges();
                            entity.payment_type payment_type = myViewSource.View.CurrentItem as entity.payment_type;
                            mydb.db.Entry(payment_type).State = EntityState.Detached;
                            _entity.db.payment_type.Attach(payment_type);
                            objCollectionViewSource.View.Refresh();
                            objCollectionViewSource.View.MoveCurrentTo(payment_type);
                            MainViewSource.View.Refresh();
                            MainViewSource.View.MoveCurrentTo(curObject);
                            btnCancel_Click(sender, e);
                        }
                        else if (operationMode == Class.clsCommon.Mode.Edit)
                        {
                            btnCancel_Click(sender, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)cbxPrint.IsChecked)
            {
                entity.db.app_document.Where(x => x.id_application == global::entity.App.Names.PaymentType && x.id_company == CurrentSession.Id_Company).Load();
            }
            else
            {
                entity.payment_type payment_type = objCollectionViewSource.View.CurrentItem as entity.payment_type;
                payment_type.id_document = null;
                payment_type.app_document = null;
                objCollectionViewSource.View.Refresh();
            }
        }
    }
}