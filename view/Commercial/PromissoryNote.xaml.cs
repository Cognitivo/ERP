﻿using entity;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Commercial
{
    public partial class PromissoryNote : Page
    {
        private PromissoryNoteDB PromissoryNoteDB = new PromissoryNoteDB();
        private CollectionViewSource payment_promissory_noteViewSource;

        public PromissoryNote()
        {
            InitializeComponent();
        }

        private void toolBar_btnApprove_Click(object sender)
        {
            PromissoryNoteDB.Approve();
            payment_promissory_noteViewSource.View.Refresh();
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            PromissoryNoteDB.Anull();
            payment_promissory_noteViewSource.View.Refresh();
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            if (!string.IsNullOrEmpty(query) && payment_promissory_noteViewSource != null)
            {
                payment_promissory_noteViewSource.View.Filter = i =>
                {
                    payment_promissory_note payment_promissory_note = i as payment_promissory_note;

                    if (payment_promissory_note != null)
                    {
                        if ((payment_promissory_note.contact != null ? payment_promissory_note.contact.name.ToLower().Contains(query.ToLower()) : false)
                            || payment_promissory_note.note_number.Contains(query))
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
            else
            {
                payment_promissory_noteViewSource.View.Filter = null;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            payment_promissory_noteViewSource = ((CollectionViewSource)(FindResource("payment_promissory_noteViewSource")));
            payment_promissory_noteViewSource.Source = PromissoryNoteDB.payment_promissory_note.Where(x => x.id_company == CurrentSession.Id_Company).ToList();

            cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PromissoryNoteDB, entity.App.Names.PromissoryNote, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
        }

        private void set_ContactPref(object sender, EventArgs e)
        {
            if (sbxContact.ContactID > 0)
            {
                contact contact = PromissoryNoteDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                payment_promissory_note payment_promissory_note = (payment_promissory_note)payment_promissory_noteViewSource.View.CurrentItem;
                payment_promissory_note.id_contact = contact.id_contact;
                payment_promissory_note.contact = contact;
            }
        }

        private void toolbar_btnEdit_Click(object sender)
        {
            payment_promissory_note payment_promissory_note = (payment_promissory_note)payment_promissory_noteViewSource.View.CurrentItem;
            if (payment_promissory_note != null)
            {
                payment_promissory_note.State = System.Data.Entity.EntityState.Modified;
            }
            payment_promissory_noteViewSource.View.Refresh();
        }

        private void toolbar_btnSave_Click(object sender)
        {
            PromissoryNoteDB.SaveChanges();
            payment_promissory_note payment_promissory_note = (payment_promissory_note)payment_promissory_noteViewSource.View.CurrentItem;
            if (payment_promissory_note != null)
            {
                payment_promissory_note.State = System.Data.Entity.EntityState.Unchanged;
            }
            payment_promissory_noteViewSource.View.Refresh();
        }

        private void toolbar_btnCancel_Click(object sender)
        {
            payment_promissory_note payment_promissory_note = (payment_promissory_note)payment_promissory_noteViewSource.View.CurrentItem;
            if (payment_promissory_note != null)
            {
                payment_promissory_note.State = System.Data.Entity.EntityState.Unchanged;
            }
            payment_promissory_noteViewSource.View.Refresh();
        }
    }
}