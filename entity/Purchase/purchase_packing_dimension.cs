namespace entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class purchase_packing_dimension : Audit
    {
        public purchase_packing_dimension()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id_packing_property { get; set; }

    
        public int id_dimension { get; set; }
        public decimal value { get; set; }
        public int id_measurement { get; set; }

        public virtual purchase_packing_detail purchase_packing_detail { get; set; }
        public virtual app_dimension app_dimension { get; set; }
        public virtual app_measurement app_measurement { get; set; }
    }
}