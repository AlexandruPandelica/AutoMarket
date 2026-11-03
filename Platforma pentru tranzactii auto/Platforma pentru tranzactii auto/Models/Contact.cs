using System;
using System.ComponentModel.DataAnnotations;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nume { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Subiect { get; set; }

        [Required]
        public string Mesaj { get; set; }

        public DateTime DataTrimiterii { get; set; } = DateTime.UtcNow; 

        public bool EsteCitit { get; set; } = false; 
    }
}