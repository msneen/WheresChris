using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Contacts;
using Plugin.Contacts.Abstractions;
using StayTogether.Classes;


namespace StayTogether
{
    public class ContactsHelper
    {

        public async Task<List<GroupMemberVm>> GetContacts()
        {
            if (!await CrossContacts.Current.RequestPermission()) return null;

            var contacts = new List<GroupMemberVm>();
            CrossContacts.Current.PreferContactAggregation = true;

            List<Contact> contactList = null;
            var contacts1 = contacts;
            await Task.Run(() =>
            {
                if (CrossContacts.Current == null || CrossContacts.Current.Contacts == null) return;

                contactList = CrossContacts.Current.Contacts.ToList();

                if (contactList == null) return;

                contactList = contactList.OrderBy(c => c.LastName).ToList();

                //for some reason we can't use linq
                foreach (var contact in CrossContacts.Current.Contacts)
                {
                    var cleanedName = CleanName(contact);
                    if (string.IsNullOrWhiteSpace(cleanedName) || contact.Phones == null) continue;

                    contacts1.AddRange(from phone in contact.Phones
                        let cleanedPhone = CleanPhoneNumber(phone.Number)
                        where phone.Type == PhoneType.Mobile && !string.IsNullOrWhiteSpace(cleanedPhone)
                        select new GroupMemberVm
                        {
                            Name = cleanedName, PhoneNumber = 
                            cleanedPhone
                        });
                }
            });

            var sortedcontacts = contacts1.OrderBy(c => c.Name).ToList();
            return sortedcontacts;


            ////for some reason we can't use linq
            //foreach (var contact in CrossContacts.Current.Contacts)
            //{
            //    var cleanedName = CleanName(contact);
            //    if (string.IsNullOrWhiteSpace(cleanedName) || contact.Phones == null) continue;
            //    foreach (var phone in contact.Phones)
            //    {
            //        var cleanedPhone = CleanPhoneNumber(phone.Number);
            //        if (phone.Type != PhoneType.Mobile || string.IsNullOrWhiteSpace(cleanedPhone)) continue;

            //        var contactSynopsis = new GroupMemberVm
            //        {
            //            Name = cleanedName,
            //            PhoneNumber = cleanedPhone
            //        };
            //        contacts.Add(contactSynopsis);
            //    }
            //}

            //var sortedcontacts = contacts.OrderBy(c => c.Name).ToList();
            //contacts = sortedcontacts;
            //return contacts;           
        }

        public static string CleanName(Contact contact)
        {
            var cleanedName = "";
            if (!string.IsNullOrWhiteSpace(contact.LastName))
            {
                cleanedName = contact.LastName;
            }
            if (string.IsNullOrWhiteSpace(contact.FirstName)) return cleanedName;

            if (!string.IsNullOrWhiteSpace(contact.LastName))
            {
                cleanedName += ", ";
            }
            cleanedName += contact.FirstName;

            return cleanedName;
           
        }


        public static string CleanPhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number)) return "";

            var cleanNumber =  number.Where(char.IsDigit).Aggregate("", (current, character) => current + character);

            if (string.IsNullOrWhiteSpace(cleanNumber)) return "";

            return cleanNumber.Length >= 10 ? cleanNumber.Substring(cleanNumber.Length - 10) : "";
        }

        public static string NameOrPhone(string phoneNumber, string name)
        {
            return string.IsNullOrEmpty(name) ? phoneNumber : name;
        }
    }
}