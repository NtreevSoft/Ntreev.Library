//Released under the MIT License.
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

using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Ntreev.Library
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    [Serializable]
    public struct SignatureDate
    {
        private string id;
        private DateTime dateTime;

        public SignatureDate(string user, Guid token)
        {
            this.id = user ?? throw new ArgumentNullException(nameof(user));
            this.Token = token;
            this.dateTime = DateTime.UtcNow;
        }

        public SignatureDate(DateTime dateTime)
        {
            this.id = string.Empty;
            this.dateTime = dateTime;
            Token = default;
        }

        public SignatureDate(string user, DateTime dateTime) : this(user, default, dateTime)
        {
        }

        public SignatureDate(string user, Guid token, DateTime dateTime)
        {
            this.id = user ?? throw new ArgumentNullException(nameof(user));
            this.dateTime = dateTime;
            Token = token;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.id ?? "(null)", this.dateTime);
        }

        public SignatureDate ToLocalValue()
        {
#if DEBUG
            if (this.dateTime != DateTime.MinValue && this.dateTime.Kind != DateTimeKind.Utc)
            {
                throw new InvalidOperationException();
            }
#endif
            return new SignatureDate(this.id,  this.Token, this.dateTime.ToLocalTime());
        }

        public SignatureDate ToUniversalValue()
        {
#if DEBUG
            if (this.dateTime != DateTime.MinValue && this.dateTime.Kind == DateTimeKind.Utc)
            {
                throw new InvalidOperationException();
            }
#endif
            return new SignatureDate(this.id, this.Token, this.dateTime.ToUniversalTime());
        }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTime
        {
            get { return this.dateTime; }
            set { this.dateTime = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        public string ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [DataMember]
        public Guid Token { get; set; }

        public static bool operator ==(SignatureDate left, SignatureDate right)
        {
            return (left.id == right.id) && 
                   (left.Token == right.Token) &&
                   (left.dateTime == right.dateTime);
        }

        public static bool operator !=(SignatureDate left, SignatureDate right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is SignatureDate == false)
                return false;
            return this == (SignatureDate)obj;
        }

        public override int GetHashCode()
        {
            return HashUtility.GetHashCode(this.id) ^
                   HashUtility.GetHashCode(this.Token) ^
                   HashUtility.GetHashCode(this.dateTime);
        }

        public static readonly SignatureDate Empty = new SignatureDate(string.Empty, default, DateTime.MinValue);
    }
}
