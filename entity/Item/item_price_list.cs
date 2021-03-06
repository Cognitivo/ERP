namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class item_price_list : AuditGeneric, IDataErrorInfo
    {
        public item_price_list()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            item_price_rel = new List<item_price>();
            is_active = true;
        }

        public enum PercentOverTypes
        {
            OverCost,
            OverPriceList
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_price_list { get; set; }

        [Required]
        public string name { get; set; }

        public bool is_default { get; set; }
        public bool is_active { get; set; }

        public PercentOverTypes percent_type { get; set; }
        public decimal percent_over { get; set; }

        public virtual item_price_list ref_price_list { get; set; }
        public virtual IEnumerable<item_price> item_price_rel { get; set; }

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

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
                return "";
            }
        }
    }
}