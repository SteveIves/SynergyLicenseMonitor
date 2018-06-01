using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

enum LoginStatus
{
    OK = 0,
    NOTINS = 1,
    MAXUSR = 2,
    EXPDEMO = 7
}

enum LogoutStatus
{
    OK = 0,
    BADTOK = 3
}

namespace SynergyLicenseMonitor
{
    class LicensingToolkit
    {
        //PInvoke requires full security permissions because we are calling unmanaged code
        [DllImport("syncli_api.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern int win_lm_stat(string product, string application, [MarshalAs(UnmanagedType.LPArray)]int[] count, [MarshalAs(UnmanagedType.LPArray)]int[] syserr);

        [DllImport("syncli_api.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern int win_lm_login(string product, string application, int a3, int seat, [MarshalAs(UnmanagedType.LPArray)]int[] token, [MarshalAs(UnmanagedType.LPArray)]int[] syserr);

        [DllImport("syncli_api.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern int win_lm_logout(string product, string application, int seat, int token, [MarshalAs(UnmanagedType.LPArray)]int[] syserr);


        private static string producerCode = "999";

        public static bool Login(string licenseCode, int seat, ref int licenseToken, ref string statusMessage)
        {
            bool success = false;
            int status = 0;
            int checkLicense = 0;
            int[] token = new int[1];   //Arrays required for * (returned) parameters because of interop???
            int[] error = new int[1];

            status = win_lm_login(producerCode, licenseCode, checkLicense, seat, token, error);

            switch ((LoginStatus)status)
            {
                case LoginStatus.OK:
                    success = true;
                    licenseToken = token[0];
                    statusMessage = string.Format("License for application {0} was allocated.", licenseCode);
                    break;
                case LoginStatus.NOTINS:
                    statusMessage = string.Format("No license installed for application {0}.", licenseCode);
                    break;
                case LoginStatus.MAXUSR:
                    statusMessage = string.Format("Maximum users exceeded for application {0}.", licenseCode);
                    break;
                case LoginStatus.EXPDEMO:
                    statusMessage = string.Format("Temporary license for application {0} has expired.", licenseCode);
                    //Demo period expired, but the license will still have been allocated.
                    int logoutStatus = 0;
                    logoutStatus = win_lm_logout(producerCode, licenseCode, seat, token[0], error);
                    break;
                default:
                    statusMessage = getErrorText(status);
                    break;
            }

            return success;
        }

        public static bool Logout(string licenseCode, int seat, int token, ref string statusMessage)
        {
            bool success = false;
            int status = 0;
            int[] error = new int[1];

            status = win_lm_logout(producerCode, licenseCode, seat, token, error);

            switch ((LogoutStatus)status)
            {
                case LogoutStatus.OK:
                    success = true;
                    statusMessage = statusMessage = string.Format("License for application {0} was deallocated.", licenseCode);
                    break;
                case LogoutStatus.BADTOK:
                    statusMessage = "Invalid login token";
                    break;
                default:
                    statusMessage = getErrorText(status);
                    break;
            }
            return success;
        }

        public static int LicenseUsage(string licenseCode)
        {
            int[] error = new int[1];
            int rslt;
            int[] count = new int[1];
            if ((rslt = win_lm_stat(producerCode, licenseCode, count, error)) != 0)
                throw new ApplicationException(getErrorText(rslt));
            return count[0];
        }

        private static string getErrorText(int errorNumber)
        {
            string txt = "";
            switch (errorNumber)
            {
                case 1:
                    txt = "License not configured.";
                    break;
                case 2:
                    txt = "Exceeded concurrent user maximum.";
                    break;
                case 3:
                    txt = "Bad logout token.";
                    break;
                case 5:
                    txt = "Unexpected failure in UNIX License Server.";
                    break;
                case 6:
                    txt = "LM internal consistency failure.";
                    break;
                case 7:
                    txt = "Demo period has expired.";
                    break;
                case 171:
                    txt = "Cannot access license file (or Windows registry).";
                    break;
                case 172:
                    txt = "LM communications error (msgsnd/msgrsv). Likely config problem.";
                    break;
                case 173:
                    txt = "LM communications error (msgsnd/msgrsv). Likely config problem.";
                    break;
                case 174:
                    txt = "LM communications timeout.";
                    break;
                case 175:
                    txt = "Cannot access license file (or Windows registry).";
                    break;
                case 176:
                    txt = "Cannot access Network License Server.";
                    break;
                case 177:
                    txt = "Old License Manager version–install latest License Manager.";
                    break;
                case 179:
                    txt = "Network License Server is busy.";
                    break;
                case 180:
                    txt = "LM internal consistency failure.";
                    break;
                case 181:
                    txt = "LM communications error (msgsnd/msgrsv). Likely config problem.";
                    break;
                case 182:
                    txt = "LM communications error (msgsnd/msgrsv). Likely config problem.";
                    break;
                case 183:
                    txt = "Cannot access LM server (not running or MSGWAIT timeout).";
                    break;
                case 184:
                    txt = "LM internal consistency failure.";
                    break;
                case 185:
                    txt = "Cannot access Network License Server.";
                    break;
                case 186:
                    txt = "Exceeded concurrent user maximum.";
                    break;
                case 188:
                    txt = "Network License Server is busy.";
                    break;
                default:
                    txt = string.Format("Unknown license manager error {0}.", errorNumber);
                    break;
            }
            return txt;
        }
    }
}
