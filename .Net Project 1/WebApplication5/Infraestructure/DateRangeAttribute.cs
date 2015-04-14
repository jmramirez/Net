﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication5.Models;
using WebApplication5.ViewModels;

namespace WebApplication5.Infraestructure
{
    public class DateRangeAttribute : ValidationAttribute, IClientValidatable, IMetadataAware
    {
        protected static ApplicationDbContext dc;
        


        private const string DateFormat = "yyyy/MM/dd";
        private static class DefaultErrorMessages
        {
            public const string Range = "'{0}' must be a date between {1:d} and {2:d}.";
            public const string Min = "'{0}' must be a date after {1:d}";
            public const string Max = "'{0}' must be a date before {2:d}.";
        }
        private DateTime _minDate = DateTime.Now.Date;
        private DateTime _maxDate = DateTime.MaxValue;

        /// <summary>
        /// String representation of the Min Date (yyyy/MM/dd)
        /// </summary>
        public string Min
        {
            get { return FormatDate(_minDate, DateTime.MinValue); }
            set { _minDate = DateTime.Now.Date; }
        }
        /// <summary>
        /// String representation of the Max Date (yyyy/MM/dd)
        /// </summary>
        
        
        public string Max
        {  
            
            get { return FormatDate(_maxDate, DateTime.MaxValue); }
            set { _maxDate = ParseDate(value, DateTime.MaxValue); }
        }

        public static DateTime getDate()
        {
            DateTime result;
            
            dc = new ApplicationDbContext();
            dc.Configuration.ProxyCreationEnabled = false;
            dc.Configuration.LazyLoadingEnabled = false;

            Course co = dc.Courses.First();
            result = co.Scheduled;
            return result;
        }
        public bool SuppressDataTypeUpdate { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null || !(value is DateTime))
            {
                return true;
            }
            DateTime dateValue = (DateTime)value;
            return _minDate <= dateValue && dateValue <= _maxDate;
        }
        public override string FormatErrorMessage(string name)
        {
            EnsureErrorMessage();
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minDate, _maxDate);
        }

        private void EnsureErrorMessage()
        {
            //  normally we'd pass the default error message in the constructor
            // but here the default message depends on whether we have one or both of the range ends
            // This method is used to inject a default error message if none has been set
            if (string.IsNullOrEmpty(ErrorMessage)
                && string.IsNullOrEmpty(ErrorMessageResourceName)
                && ErrorMessageResourceType == null)
            {
                string message;
                if (_minDate == DateTime.MinValue)
                {
                    if (_maxDate == DateTime.MaxValue)
                    {
                        throw new ArgumentException("Must set at least one of Min and Max");
                    }
                    message = DefaultErrorMessages.Max;
                }
                else
                {
                    if (_maxDate == DateTime.MaxValue)
                    {
                        message = DefaultErrorMessages.Min;
                    }
                    else
                    {
                        message = DefaultErrorMessages.Range;
                    }
                }
                ErrorMessage = message;
            }
        }

        private static DateTime ParseDate(string dateValue, DateTime defaultValue)
        {
            if (string.IsNullOrWhiteSpace(dateValue))
            {
                return defaultValue;
            }
            else if (dateValue == "course")
            {
                DateTime date;
                date = getDate();
                return date;
            }
            else
            {
                return DateTime.ParseExact(dateValue, DateFormat, CultureInfo.InvariantCulture);
            }
        
        }
        private static string FormatDate(DateTime dateTime, DateTime defaultValue)
        {
            if (dateTime == defaultValue)
            {
                return "";
            }
            return dateTime.ToString(DateFormat);
        }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]
                       {
                           new ModelClientValidationRangeDateRule(FormatErrorMessage(metadata.GetDisplayName()), _minDate,
                                                              _maxDate)
                       };
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (!SuppressDataTypeUpdate)
            {
                metadata.DataTypeName = "Date";
            }
        }
    }

}