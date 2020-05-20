using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;
using SQLite;
using System;

namespace Xapp2.Models
{

    public class LoginViewModel // : BaseViewModel
    {
        #region Fields

        public string email { get; set; }

        public bool isInvalidEmail { get; set; }

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the property that bounds with an entry that gets the email ID from user in the login page.
        /// </summary>
        //public string Email
        //{
        //    get
        //    {
        //        return this.email;
        //    }

        //    set
        //    {
        //        if (this.email == value)
        //        {
        //            return;
        //        }

        //        this.email = value;
        //        this.NotifyPropertyChanged();
        //    }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether the entered email is valid or invalid.
        /// </summary>
        //public bool IsInvalidEmail
        //{
        //    get
        //    {
        //        return this.isInvalidEmail;
        //    }

        //    set
        //    {
        //        if (this.isInvalidEmail == value)
        //        {
        //            return;
        //        }

        //        this.isInvalidEmail = value;
        //        this.NotifyPropertyChanged();
        //    }
        //}

        #endregion


    }

}
