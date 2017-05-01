using System;
using System.Collections.Generic;
using System.Text;

namespace StayTogether.Models
{
    public class InvitationList : List<InvitationVm>
    {
        //Todo:  at the time we call to retrieve, clean the list

        public void AddInvitation(InvitationVm item, int hours = 3)
        {
            Clean(hours);
            Add(item);
        }

        /// <summary>
        /// Remove stale invitations
        /// </summary>
        public void Clean(int hours = 3)
        {
            for (var i = this.Count - 1; i >= 0; i--)
            {
                if (DateTime.Now > this[i].ReceivedTime.AddHours(hours))
                {
                    this.RemoveAt(i);
                }
            }
        }
    }
}
