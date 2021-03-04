using System.ComponentModel.DataAnnotations;

namespace FabricMQ.Broker.Models
{
    public class AuthenticateModel
    {
        [Required]
        [MaxLength(30)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Password { get; set; }
    }
}
