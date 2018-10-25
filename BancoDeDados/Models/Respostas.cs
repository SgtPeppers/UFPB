
using System.ComponentModel.DataAnnotations;

namespace BancoDeDados
{
    public class Respostas
    {
        public string UserImage { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }

        [Required]
        public string Texto { get; set; }
    }
}