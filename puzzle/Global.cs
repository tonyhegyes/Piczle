using System;
using puzzle.Providers;
using DBreeze;
using Microsoft.Isam.Esent.Collections.Generic;
using puzzle.Providers.Security;
using System.Security;
using System.Runtime.InteropServices;


namespace puzzle
{
    public static class Global
    {
        public static DBreezeEngine userEngine;
        public static PersistentDictionary<string, MainWindow.rememberedUser> rememberedUsers_dictionary = new PersistentDictionary<string, MainWindow.rememberedUser>(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\resources\localData");

        public static RijndaelEncryption UserDataEncryption;

        public static DisplayLanguageHandler LanguageAgent = new DisplayLanguageHandler();
        public static bool isRunning = false;

        public static string SecureString_toString(SecureString password)
        {
            if (password == null)
                throw new ArgumentNullException("securePassword");

            IntPtr bstr = IntPtr.Zero;
            try
            {
                bstr = Marshal.SecureStringToBSTR(password);
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally { Marshal.ZeroFreeBSTR(bstr); }
        }
    } 
}
