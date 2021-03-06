﻿using entity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Configs
{
    /// <summary>
    /// Interaction logic for ImpexIncotermCondition.xaml
    /// </summary>
    public partial class IncotermCondition : Page
    {
        private entity.dbContext entity = new entity.dbContext();
        private CollectionViewSource impex_incoterm_conditionViewSource;

        public IncotermCondition()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                impex_incoterm_conditionViewSource = this.FindResource("impex_incoterm_conditionViewSource") as CollectionViewSource;
                entity.db.impex_incoterm_condition.Where(x => x.id_company == CurrentSession.Id_Company).Load();
                impex_incoterm_conditionViewSource.Source = entity.db.impex_incoterm_condition.Local;
            }
            catch (Exception)
            {
                //throw ex;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //New
            crud_modal.Visibility = System.Windows.Visibility.Visible;
            cntrl.Curd.incoterm_condition objCon = new cntrl.Curd.incoterm_condition();
            impex_incoterm_condition impex_incoterm_condition = new impex_incoterm_condition();
            entity.db.impex_incoterm_condition.Add(impex_incoterm_condition);
            impex_incoterm_conditionViewSource.View.MoveCurrentToLast();
            objCon.conditionViewSource = impex_incoterm_conditionViewSource;
            objCon.entity = entity;
            crud_modal.Children.Add(objCon);
        }

        private void pnl_IncotermCondition_linkEdit_click(object sender, int intIncotermConditionId)
        {
            //Edit
            crud_modal.Visibility = System.Windows.Visibility.Visible;
            cntrl.Curd.incoterm_condition objCon = new cntrl.Curd.incoterm_condition();
            impex_incoterm_conditionViewSource.View.MoveCurrentTo(entity.db.impex_incoterm_condition.Where(x => x.id_incoterm_condition == intIncotermConditionId).FirstOrDefault());
            objCon.conditionViewSource = impex_incoterm_conditionViewSource;
            objCon.entity = entity;
            crud_modal.Children.Add(objCon);
        }
    }
}