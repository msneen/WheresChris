using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Contacts;
using Plugin.Contacts.Abstractions;
using StayTogether.Classes;
using WheresChris.Views;


namespace StayTogether
{
    public class ContactsHelper
    {
        public static ObservableCollection<ContactDisplayItemVm> GroupMemberListInstance { get; set; }

        public async Task<ObservableCollection<ContactDisplayItemVm>> GetContactsAsync(string characters = "")
        {
            //if (GroupMemberListInstance != null) return GroupMemberListInstance;

            if (!await CrossContacts.Current.RequestPermission()) return null;

            CrossContacts.Current.PreferContactAggregation = false;

            if (CrossContacts.Current == null || CrossContacts.Current.Contacts == null) return null;

            var contactList = CrossContacts.Current.Contacts.ToList();

            var itemList = contactList
                    .Where(contact => contact.HasValidNameAndPhone())
                    .Select(filteredContact => new ContactDisplayItemVm
                    {
                        Name = filteredContact.CleanName(),
                        PhoneNumber = filteredContact.FirstOrDefaultMobileNumber().CleanPhoneNumber()
                        
                    })
                    .OrderBy(groupMemberVm => groupMemberVm.Name)
                    .ToList();
            List<ContactDisplayItemVm> itemListFiltered;
            if(!string.IsNullOrWhiteSpace(characters) && characters.Length > 2)
            {
                if(characters.IsNumeric())
                {
                    itemListFiltered = itemList.Where(x => x.PhoneNumber.Contains(characters)).ToList<ContactDisplayItemVm>();
                }
                else
                {
                    itemListFiltered = itemList.Where(x => x.Name.ToLower().Contains(characters.ToLower())).ToList<ContactDisplayItemVm>();
                }
            }
            else
            {
                itemListFiltered = itemList;
            }

            GroupMemberListInstance = new ObservableCollection<ContactDisplayItemVm>(itemListFiltered);

            return GroupMemberListInstance;
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

        public static bool IsValidPhoneNumber(string number)
        {
            var cleanNumber = CleanPhoneNumber(number);
            return cleanNumber.Length == 10;
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

    public static class ContactExtensions
    {
        public static string CleanPhoneNumber(this string phoneNumer)
        {
            return ContactsHelper.CleanPhoneNumber(phoneNumer);
        }

        public static bool HasValidNameAndPhone(this Contact contact)
        {
            return contact.HasValidName() && contact.HasValidPhoneNumber();
        }

        public static string CleanName(this Contact contact)
        {
            return ContactsHelper.CleanName(contact);
        }

        public static bool HasValidPhoneNumber(this Contact contact)
        {
            return !string.IsNullOrWhiteSpace(contact?.Phones?.FirstOrDefaultMobileNumber());
        }

        public static string FirstOrDefaultMobileNumber(this Contact contact)
        {
            return contact?.Phones?.FirstOrDefaultMobilePhone()?.Number;
        }

        public static Phone FirstOrDefaultMobilePhone(this List<Phone> phones)
        {
            var phone = phones.FirstOrDefault(x => x.Type == PhoneType.Mobile) ?? phones.FirstOrDefault();
            return phone;
        }

        public static string FirstOrDefaultMobileNumber(this List<Phone> phones)
        {
            return phones.FirstOrDefaultMobilePhone()?.Number;
        }

        public static bool IsValidPhoneNumber(this string number)
        {
            var cleanNumber = ContactsHelper.CleanPhoneNumber(number);
            return cleanNumber.Length == 10;
        }

        public static bool HasValidName(this Contact contact)
        {
            return !string.IsNullOrWhiteSpace(ContactsHelper.CleanName(contact));
        }

        public static bool IsNumeric(this string characters)
        {
            return characters.All(char.IsDigit);
        }
    }
}