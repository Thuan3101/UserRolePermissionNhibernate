using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Entities.Data
{
    public class RefreshToken
    {
        public virtual int Id { get; set; }

        public virtual int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        public virtual string? Token { get; set; }

        public virtual string? JwtId { get; set; }

        public virtual bool IsUsed { get; set; }
        public virtual bool IsRevoked { get; set; }
        public virtual DateTime IssuedAt { get; set; }
        public virtual DateTime ExpiredAt { get; set; }

        //public RefreshToken()
        //{
        //    IssuedAt = DateTime.UtcNow;
        //    ExpiredAt = DateTime.UtcNow.AddHours(8);
        //    IsUsed = false;
        //    IsRevoked = false;
        //}
    }
}
