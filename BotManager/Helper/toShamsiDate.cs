using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreManager.Helper
{
    public static class toShamsiDate
    {
        /// <summary>
        /// Convert DateTime to Shamsi Date (YYYY/MM/DD)
        /// </summary>
        public static string ToShamsiDateYMD(this DateTime date)
        {
            System.Globalization.PersianCalendar PC = new System.Globalization.PersianCalendar();
            int intYear = PC.GetYear(date);
            int intMonth = PC.GetMonth(date);
            int intDay = PC.GetDayOfMonth(date);
            return (intYear.ToString() + "/" + intMonth.ToString() + "/" + intDay.ToString());
        }

        /// <summary>
        /// Convert DateTime to Shamsi Date (DD/MM/YYYY)
        /// </summary>
        public static string ToShamsiDateDMY(this DateTime date)
        {
            System.Globalization.PersianCalendar PC = new System.Globalization.PersianCalendar();
            int intYear = PC.GetYear(date);
            int intMonth = PC.GetMonth(date);
            int intDay = PC.GetDayOfMonth(date);
            return (intDay.ToString() + "/" + intMonth.ToString() + "/" + intYear.ToString());
        }

        /// <summary>
        /// Convert DateTime to Shamsi String 
        /// </summary>
        public static string ToShamsiDateString(this DateTime date)
        {
            System.Globalization.PersianCalendar PC = new System.Globalization.PersianCalendar();
            int intYear = PC.GetYear(date);
            int intMonth = PC.GetMonth(date);
            int intDayOfMonth = PC.GetDayOfMonth(date);
            DayOfWeek enDayOfWeek = PC.GetDayOfWeek(date);
            string strMonthName, strDayName;
            switch (intMonth)
            {
                case 1:
                    strMonthName = "فروردین";
                    break;
                case 2:
                    strMonthName = "اردیبهشت";
                    break;
                case 3:
                    strMonthName = "خرداد";
                    break;
                case 4:
                    strMonthName = "تیر";
                    break;
                case 5:
                    strMonthName = "مرداد";
                    break;
                case 6:
                    strMonthName = "شهریور";
                    break;
                case 7:
                    strMonthName = "مهر";
                    break;
                case 8:
                    strMonthName = "آبان";
                    break;
                case 9:
                    strMonthName = "آذر";
                    break;
                case 10:
                    strMonthName = "دی";
                    break;
                case 11:
                    strMonthName = "بهمن";
                    break;
                case 12:
                    strMonthName = "اسفند";
                    break;
                default:
                    strMonthName = "";
                    break;
            }

            switch (enDayOfWeek)
            {
                case DayOfWeek.Friday:
                    strDayName = "جمعه";
                    break;
                case DayOfWeek.Monday:
                    strDayName = "دوشنبه";
                    break;
                case DayOfWeek.Saturday:
                    strDayName = "شنبه";
                    break;
                case DayOfWeek.Sunday:
                    strDayName = "یکشنبه";
                    break;
                case DayOfWeek.Thursday:
                    strDayName = "پنجشنبه";
                    break;
                case DayOfWeek.Tuesday:
                    strDayName = "سه شنبه";
                    break;
                case DayOfWeek.Wednesday:
                    strDayName = "چهارشنبه";
                    break;
                default:
                    strDayName = "";
                    break;
            }

            return (string.Format("{0} {1} {2} {3}", strDayName, intDayOfMonth, strMonthName, intYear));
        }
        /// <summary>
        /// get Datetime day of week
        /// </summary>
        public static string ToShamsiDayOfWeek(this DateTime date)
        {
            System.Globalization.PersianCalendar PC = new System.Globalization.PersianCalendar();
            DayOfWeek enDayOfWeek = PC.GetDayOfWeek(date);
            string strDayName;
            switch (enDayOfWeek)
            {
                case DayOfWeek.Friday:
                    strDayName = "جمعه";
                    break;
                case DayOfWeek.Monday:
                    strDayName = "دوشنبه";
                    break;
                case DayOfWeek.Saturday:
                    strDayName = "شنبه";
                    break;
                case DayOfWeek.Sunday:
                    strDayName = "یکشنبه";
                    break;
                case DayOfWeek.Thursday:
                    strDayName = "پنجشنبه";
                    break;
                case DayOfWeek.Tuesday:
                    strDayName = "سه شنبه";
                    break;
                case DayOfWeek.Wednesday:
                    strDayName = "چهارشنبه";
                    break;
                default:
                    strDayName = "";
                    break;
            }

            return strDayName;
        }
    }
}
