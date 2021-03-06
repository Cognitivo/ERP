namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class app_currency : AuditGeneric, IDataErrorInfo
    {
        public app_currency()
        {
            app_currency_denomination = new List<app_currency_denomination>();
            app_currencyfx = new List<app_currencyfx>();
            impex_expense = new List<impex_expense>();
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_active = true;
            is_priority = false;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_currency { get; set; }

        [Required]
        public string name { get; set; }

        public int? id_country { get; set; }

        [Required]
        public bool is_priority { get; set; }

        public bool is_active
        {
            get { return _is_active; }
            set
            {
                if (_is_active != value)
                {
                    _is_active = value;
                    RaisePropertyChanged("is_active");
                }
            }
        }

        private bool _is_active; public bool has_rounding { get; set; }
        public bool is_reverse { get; set; }

        public string code { get; set; }

        public virtual ICollection<app_currency_denomination> app_currency_denomination { get; set; }
        public virtual ICollection<app_currencyfx> app_currencyfx { get; set; }
        public virtual ICollection<item_request> item_request { get; set; }
        public virtual ICollection<impex_expense> impex_expense { get; set; }
        public virtual ICollection<item_asset> item_asset { get; set; }

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

                // iterate over all of the properties
                // of this object - aggregating any validation errors
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    String propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }

        public string this[string columnName]
        {
            get
            {
                // apply property level validation rules
                if (columnName == "name")
                {
                    if (string.IsNullOrEmpty(name))
                        return "Name needs to be filled";
                }
                if (columnName == "id_country")
                {
                    if (id_company == 0)
                        return "Country needs to bo selected";
                }
                return "";
            }
        }
    }
}