﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationPropertyAttribute : Attribute
    {
        private readonly string propertyName;
        private readonly ConfigurationPropertyNamingConvention namingConvention;
        private string section;
        private Type scopeType;

        public ConfigurationPropertyAttribute()
        {

        }

        public ConfigurationPropertyAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public ConfigurationPropertyAttribute(ConfigurationPropertyNamingConvention namingConvention)
        {
            this.namingConvention = namingConvention;
        }

        public string PropertyName
        {
            get { return this.propertyName; }
        }
        
        public string ScopeTypeName
        {
            get
            {
                if (this.scopeType != null)
                    return this.scopeType.AssemblyQualifiedName;
                return string.Empty;
            }
            set
            {
                this.scopeType = Type.GetType(value);
            }
        }

        public Type ScopeType
        {
            get { return this.scopeType ?? typeof(ConfigurationBase); }
            set { this.scopeType = value; }
        }

        public ConfigurationPropertyNamingConvention NamingConvention
        {
            get { return this.namingConvention; }
        }

        public string Section
        {
            get { return this.section; }
            set { this.section = value; }
        }

        internal string GetPropertyName(string descriptorName)
        {
            var items = new List<string>();
            if (this.section != null)
                items.Add(this.section);
            if (this.propertyName != null)
                items.Add(this.propertyName);
            else if (this.namingConvention == ConfigurationPropertyNamingConvention.None)
                items.Add(descriptorName);
            else if (this.namingConvention == ConfigurationPropertyNamingConvention.CamelCase)
                items.Add(StringUtility.ToCamelCase(descriptorName));
            return string.Join(".", items);
        }
    }
}
