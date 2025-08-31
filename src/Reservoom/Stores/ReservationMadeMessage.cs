using CommunityToolkit.Mvvm.Messaging.Messages;
using Reservoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservoom.Stores
{
    public class ReservationMadeMessage : ValueChangedMessage<Reservation>
    {
        public ReservationMadeMessage(Reservation value) : base(value)
        {
            
        }
    }
}
